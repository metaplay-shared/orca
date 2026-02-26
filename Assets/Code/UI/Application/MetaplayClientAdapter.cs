using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Metaplay.Core;
using Metaplay.Core.Message;
using Metaplay.Unity;
using Metaplay.Unity.ConnectionStates;
using Metaplay.Unity.DefaultIntegration;
using System.Threading;
using System.Threading.Tasks;
using Game.Logic;
using Game.Logic.TypeCodes;
using Metaplay.Core.Client;
using Zenject;

namespace Code.UI.Application {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class MetaplayClientAdapter : IMetaplayLifecycleDelegate, IInitializable, ITickable {
		private readonly IApplicationStateManager applicationStateManager;
		private readonly IEnvironmentConfigProvider environmentConfigProvider;
		private readonly CancellationToken cancellationToken;

		public MetaplayClientAdapter(
			IApplicationStateManager applicationStateManager,
			IEnvironmentConfigProvider environmentConfigProvider,
			CancellationToken cancellationToken
		) {
			this.applicationStateManager = applicationStateManager;
			this.environmentConfigProvider = environmentConfigProvider;
			this.cancellationToken = cancellationToken;
		}

		/// <summary>
		/// A session has been successfully negotiated with the server. At this point, we also have the
		/// relevant state initialized on the client, so we can move on to the game state.
		/// </summary>
		Task IMetaplayLifecycleDelegate.OnSessionStartedAsync() {
			// Switch to the in-game state.
			applicationStateManager.SwitchToState(
					ApplicationState.Game,
					cancellationToken
				)
				.Forget();

			// At this point, the player state is available. For example, the following are now valid:
			// Access player state members: MetaplayClient.PlayerModel.CurrentTime
			// Execute player actions: MetaplayClient.PlayerContext.ExecuteAction(..);
			
			return Task.CompletedTask;
		}

		/// <summary>
		/// The current logical session has been lost and can no longer be resumed. This can happen for multiple
		/// reasons, for example, if the network connection is dropped for a sufficient long time, or if the
		/// application has been in the background for a long time, or if the server is in a maintenance mode.
		///
		/// The application should react to this by showing a 'Connection Lost' dialog and present the player
		/// with a 'Reconnect' button.
		/// For some types of errors, it may be appropriate to omit the error popup, and auto-reconnect instead.
		/// </summary>
		/// <param name="connectionLost">Information about why the session loss happened.</param>
		void IMetaplayLifecycleDelegate.OnSessionLost(ConnectionLostEvent connectionLost) {
			if (connectionLost.AutoReconnectRecommended) {
				// For certain errors, we auto-reconnect straight away without
				// prompting the player. Note that AutoReconnectRecommended is
				// just a suggestion by the SDK and is based on the type of the
				// error. The game does not have to obey the suggestion.
				applicationStateManager.SwitchToState(
					ApplicationState.Initializing,
					cancellationToken
				).Forget();
			} else {
				// Otherwise, show the connection error popup, with info text
				// and a reconnect button.
				// Despite losing the session, the game scene will linger until
				// the player clicks on the reconnect button.
				// MetaplayClient.PlayerModel is still available so that the
				// game scene can continue to access it. It will remain available
				// until the reconnection starts.
				ShowConnectionErrorPopup(connectionLost);
			}
		}

		/// <summary>
		/// Metaplay failed to establish a session with the server. Show the connection error and 'Reconnect'
		/// button so the player can try again.
		/// </summary>
		/// <param name="connectionLost">Information about why the failure happened.</param>
		void IMetaplayLifecycleDelegate.OnFailedToStartSession(ConnectionLostEvent connectionLost) {
			// Show the connection error popup, with info text and a reconnect button.
			// Note that we're not in the game scene since the error occurred before
			// the session was started. Furthermore, MetaplayClient.PlayerModel is
			// unavailable.
			ShowConnectionErrorPopup(connectionLost);
		}

		/// <summary>
		/// Show a popup with the details of a connection/session error,
		/// and a reconnect button.
		/// </summary>
		/// <param name="connectionLost"></param>
		private void ShowConnectionErrorPopup(ConnectionLostEvent connectionLost) {
			if (connectionLost.TechnicalError is TerminalError.LogicVersionMismatch logicVersionMismatch) {
				ForcedUpdatePopup.ShowForcedUpdatePopup(connectionLost, logicVersionMismatch).Forget();
				return;
			}

			if (connectionLost.TechnicalError is TerminalError.InMaintenance inMaintenance) {
				MaintenanceModePopup.ShowMaintenanceModePopup(connectionLost, inMaintenance)
					.ContinueWith(
						() => applicationStateManager.SwitchToState(
							ApplicationState.Initializing,
							cancellationToken
						)
					).Forget();
				return;
			}

			ConnectionErrorPopup.ShowConnectionErrorPopup(connectionLost)
				.ContinueWith(
					() => applicationStateManager.SwitchToState(
						ApplicationState.Initializing,
						cancellationToken
					)
				).Forget();
		}

		public void Initialize() {
			MetaplayClient.Initialize(
				new MetaplayClientOptions {
					// Hook all the lifecycle and connectivity callbacks back to this class.
					LifecycleDelegate = this,
					IAPOptions = new MetaplayIAPOptions {
						EnableIAPManager = true,
					},
					AdditionalClients = new IMetaplaySubClient[]
					{
						new MatchmakingClient(),
						new OrcaLeagueClient(ClientSlotGame.OrcaLeague),
					},
					AnalyticsDelegate = null,
				}
			);
		}

		public void Tick() {
			// Update Metaplay connections and game logic
			MetaplayClient.Update();
		}
	}
}

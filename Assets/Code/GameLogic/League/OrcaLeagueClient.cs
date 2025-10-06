using Metaplay.Core.Client;
using Metaplay.Core.League;
using Metaplay.Core.Message;
using Metaplay.Core.Tasks;
using System;
using System.Threading;

namespace Game.Logic
{
    public class OrcaLeagueClient : LeagueClient<OrcaPlayerDivisionModel>
    {
        IMessageDispatcher _messageDispatcher;

        public OrcaLeagueClient(ClientSlot clientSlot) : base(clientSlot) { }
        public bool   LeagueJoinRequestInProgress { get; private set; }
        public string LeagueJoinRequestStatus     { get; private set; }

        CancellationTokenSource _joinRequestTimeoutCts;

        public override void Initialize(IMetaplaySubClientServices clientServices)
        {
            base.Initialize(clientServices);

            _messageDispatcher = clientServices.MessageDispatcher;
            _messageDispatcher.AddListener<PlayerJoinOrcaLeagueResponse>(HandleLeagueJoinResponse);
        }

        public override void Dispose()
        {
            base.Dispose();
            _messageDispatcher.RemoveListener<PlayerJoinOrcaLeagueResponse>(HandleLeagueJoinResponse);
        }

        public void TryJoinLeagues()
        {
            if (LeagueJoinRequestInProgress)
                return;

            LeagueJoinRequestInProgress = true;
            LeagueJoinRequestStatus     = "Trying to join...";
            _messageDispatcher.SendMessage(PlayerJoinOrcaLeagueRequest.Instance);

            _joinRequestTimeoutCts = new CancellationTokenSource();

            MetaTask.Delay(TimeSpan.FromSeconds(5), _joinRequestTimeoutCts.Token).ContinueWithCtx(t =>
            {
                LeagueJoinRequestInProgress = false;
                LeagueJoinRequestStatus     = "Timeout";
            }, _joinRequestTimeoutCts.Token);
        }

        void HandleLeagueJoinResponse(PlayerJoinOrcaLeagueResponse response)
        {
            LeagueJoinRequestInProgress = false;
            _joinRequestTimeoutCts.Cancel();

            if (!response.Success)
            {
                LeagueJoinRequestStatus = "Failed: " + response.FailureReason;
            }
            else
            {
                LeagueJoinRequestStatus = null;
            }
        }
    }
}

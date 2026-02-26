using JetBrains.Annotations;
using Metaplay.Core.Message;
using System.Collections.Generic;
using UnityEditor;

namespace Code.UI.Application {
	public interface IServerEndpointProvider {
		/// <summary>
		/// Returns the server endpoint where the client should try to connect. Only 'localhost' and Offline Mode
		/// are supported in this sample. The active server can be chosen by inspecting the 'ApplicationManager'
		/// object in the hierarchy.
		///
		/// For a cloud-enabled game, this needs to be expanded to support the cloud endpoints, too.
		/// </summary>
		/// <returns></returns>
		ServerEndpoint GetServerEndpoint();
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class ServerEndpointProvider : IServerEndpointProvider {
		private static readonly Dictionary<ActiveEnvironment, ServerEndpoint> ServerEndpoints =
			new() {
				// TODO: Fix URLs
				{ ActiveEnvironment.OfflineMode, new ServerEndpoint() }, {
					ActiveEnvironment.Localhost, new ServerEndpoint(
						"localhost",
					#if UNITY_WEBGL && !UNITY_EDITOR
						9380,
					#else
						9339,
					#endif
						false,
						"http://localhost:5552/"
					)
				}, {
					ActiveEnvironment.Develop, new ServerEndpoint(
						"orca-develop.p1.metaplay.io",
						9339,
						true,
						"https://orca-develop-assets.p1.metaplay.io/"
					)
				}, {
					ActiveEnvironment.Stable, new ServerEndpoint(
						"orca-stable.p1.metaplay.io",
						9339,
						true,
						"https://orca-stable-assets.p1.metaplay.io/"
					)
				}, {
					ActiveEnvironment.Production, new ServerEndpoint(
						"prod.orca.skunkworksgames.com",
						9339,
						true,
						"https://prod-assets.orca.skunkworksgames.com/"
					)
				}
			};

		#if UNITY_EDITOR

		public const string ACTIVE_ENVIRONMENT_KEY = "com.skunkworksgames.orca-activeEnvironment";
		private static ActiveEnvironment activeEnvironment;

		[InitializeOnLoadMethod]
		private static void Initialize() {
			activeEnvironment = (ActiveEnvironment)EditorPrefs.GetInt(
				ACTIVE_ENVIRONMENT_KEY,
				System.Convert.ToInt32(ActiveEnvironment.OfflineMode)
			);
		}

		public static ActiveEnvironment ActiveEnvironment {
			get => activeEnvironment;
			set {
				activeEnvironment = value;
				EditorPrefs.SetInt(ACTIVE_ENVIRONMENT_KEY, (int)value);
			}
		}

		#endif

		public ServerEndpoint GetServerEndpoint() {
			#if UNITY_EDITOR
			return ServerEndpoints[ActiveEnvironment];
			#elif UNITY_WEBGL
			return ServerEndpoints[ActiveEnvironment.Localhost];
			#else
			return ServerEndpoints[ActiveEnvironment.Stable];
			#endif
		}
	}
}

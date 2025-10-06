// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic.TypeCodes;
using Game.Logic.Matchmaking;
using Metaplay.Core;
using Metaplay.Core.Client;
using Metaplay.Core.Message;

namespace Game.Logic
{
    public class MatchmakingClient : IMetaplaySubClient
    {
        /// <inheritdoc />
        public ClientSlot ClientSlot => ClientSlotGame.Matchmaker;

        IMessageDispatcher _messageDispatcher;

        public OrcaMatchingResponse LatestResponse { get; private set; }

        #if UNITY_EDITOR
        public static MatchmakingClient EditorHookCurrent;
        #endif

        public MatchmakingClient()
        {
            #if UNITY_EDITOR
            EditorHookCurrent = this;
            #endif
        }

        public void Initialize(IMetaplaySubClientServices clientServices)
        {
            _messageDispatcher = clientServices.MessageDispatcher;
            _messageDispatcher.AddListener<OrcaMatchingResponse>(HandleMatchingResponse);
        }

        public void Dispose()
        {
            _messageDispatcher.RemoveListener<OrcaMatchingResponse>(HandleMatchingResponse);
        }

        public void HandleMatchingResponse(OrcaMatchingResponse response)
        {
            LatestResponse = response;
        }

        public void SendMatchmakingRequest()
        {
            _messageDispatcher.SendMessage(new OrcaMatchingRequest());
        }

        public void OnSessionStart(SessionProtocol.SessionStartSuccess successMessage, ClientSessionStartResources sessionStartResources) { }
        public void OnSessionStop() { }
        public void OnDisconnected() { }
        public void EarlyUpdate() { }
        public void UpdateLogic(MetaTime time) { }
        public void FlushPendingMessages() { }
    }
}


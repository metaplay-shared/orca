// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;

namespace Game.Logic.Matchmaking
{
    [MetaMessage(MessageCodes.OrcaMatchingRequest, MessageDirection.ClientToServer), MessageRoutingRuleSession]
    public class OrcaMatchingRequest : MetaMessage
    {
        public OrcaMatchingRequest() { }
    }

    [MetaMessage(MessageCodes.OrcaMatchingResponse, MessageDirection.ServerToClient)]
    public class OrcaMatchingResponse : MetaMessage
    {
        public bool IsSuccess { get; set; }

        OrcaMatchingResponse() { }
        public OrcaMatchingResponse(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}


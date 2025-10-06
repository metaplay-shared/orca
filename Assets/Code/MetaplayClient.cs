using Game.Logic;
using Game.Logic.TypeCodes;
using Metaplay.Core.Client;
using Metaplay.Unity.DefaultIntegration;

public class MetaplayClient : MetaplayClientBase<PlayerModel>
{
	public static OrcaLeagueClient LeagueClient => ClientStore.TryGetClient<OrcaLeagueClient>(ClientSlotGame.OrcaLeague);
}

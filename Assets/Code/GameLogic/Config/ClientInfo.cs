using System.Runtime.Serialization;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ClientInfo : GameConfigKeyValue<ClientInfo> {
		[MetaMember(1)] public float ItemVelocity { get; private set; }
		[MetaMember(2)] public float ItemVelocityOnBoard { get; private set; }
		[MetaMember(3)] public float FlightTimeOffset { get; private set; }
		[MetaMember(4)] public float BoardFlightTimeOffset { get; private set; }
		[MetaMember(5)] public int InfoMessageDelayMs { get; private set; }
		[MetaMember(6)] public ColorInfo InactiveItemColor { get; private set; }
		[MetaMember(7)] public bool ShowLockAreaLocks { get; private set; }

		public float GetItemFlightTime(float distance) {
			return distance / ItemVelocity + FlightTimeOffset;
		}

		public float GetItemOnBoardFlightTime(float distance) {
			return distance / ItemVelocityOnBoard + BoardFlightTimeOffset;
		}
	}

	[MetaSerializable]
	public class ColorInfo {
		[MetaMember(1)] public float Red { get; private set; }
		[MetaMember(2)] public float Green { get; private set; }
		[MetaMember(3)] public float Blue { get; private set; }
		[MetaMember(4)] public float Alpha { get; private set; }

		public ColorInfo() { }

		public ColorInfo(float r, float g, float b, float a) {
			Red = r;
			Green = g;
			Blue = b;
			Alpha = a;
		}
	}
}

using UnityEngine;

namespace Code.UI.Sound {
	[CreateAssetMenu(fileName = "SoundSettings", menuName = "Sound settings", order = 0)]
	public class SoundSettings : ScriptableObject {
		/*
		 requested sounds:
		 
		Island music
		Map music
		
		X Celebration sound(New character unlocked, level up, building piece completed)
		X Flying resources hitting the resource bars
		X Cloud reveal sound
		X Button sound (claim button, inventory)
		X Close button sound
		/ Merge sound (pitch change for every step so it goes higher a little bit when chain progresses like in match3 games)
		/ Map button sound
		/ Claim key in ui sound
		Enter island sound
		Tap to continue sound
		Hero icon created on board
		Claim item (key, orange)
		Fill order sound
		Skip timer sound
		Crate unloaded sound
		Enter Cafe orca building sound
		Click recipe icon sound
		Unlock island button sound
		Building piece placed sound
		Text spawning sound
		 */

		[Header("Sound effects")]
		public AudioClip ItemSelectionSound;
		public AudioClip ItemMergeSound;
		public AudioClip ItemSpawnSound;
		public AudioClip RewardCelebration;
		public AudioClip FlightHit;
		public AudioClip InventoryItemFlightHit;
		public AudioClip MergeBoardCloudOpened;
		public AudioClip ButtonClicked;
		public AudioClip EnterIsland;
		public AudioClip DialogueOpen;

		[Header("Background music")]
		public AudioClip IslandMusic;
		public AudioClip MapMusic;
	}
}

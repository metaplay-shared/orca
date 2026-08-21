using System;
using Game.Logic;
using UnityEngine;

namespace Code.UI.Dialogue.Commands {
	public class SpeakerCommand : CommandBase {
		private readonly DialoguePopup dialoguePopup;

		public SpeakerCommand(DialoguePopup dialoguePopup) {
			this.dialoguePopup = dialoguePopup;
		}

		public override CommandType Type => CommandType.Speaker;

		public override void Execute(string[] parameters) {
			RequireParameterCount(parameters.Length, 3);
			string speakerId = parameters[0];
			if (speakerId == "Unknown") {
				return;
			}

			Debug.Log($"Set character {speakerId} to expression {parameters[1]} and to {parameters[2]} side");
			SpeakerSide side = Enum.Parse<SpeakerSide>(parameters[2]);
			dialoguePopup.SetSpeaker(speakerId, parameters[1], side);
		}
	}
}

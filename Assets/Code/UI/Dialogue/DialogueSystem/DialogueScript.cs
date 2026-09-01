using System.Linq;
using Game.Logic;
using JetBrains.Annotations;

namespace Code.UI.Dialogue.DialogueSystem {
	public class DialogueScript {
		public readonly DialogueInfo Dialogue;
		private int currentEntry;

		public DialogueScript(DialogueInfo dialogue) {
			Dialogue = dialogue;
			Rewind();
		}

		public void Rewind() {
			currentEntry = -1;
		}

		public bool TryGetNext(out DialogueEntryInfo entry) {
			if (currentEntry + 1 < Dialogue.Dialogue.Entries.Count) {
				entry = Dialogue.Dialogue.Entries[++currentEntry];
				return true;
			}

			entry = null;
			return false;
		}

		[CanBeNull]
		public ChatDialogueEntryInfo GetFirstChatDialogue() {
			return (ChatDialogueEntryInfo)Dialogue.Dialogue.Entries.FirstOrDefault(
				entry => entry is ChatDialogueEntryInfo
			);
		}

		[CanBeNull]
		public CommandDialogueEntryInfo GetFirstSpeaker() {
			return (CommandDialogueEntryInfo)Dialogue.Dialogue.Entries.FirstOrDefault(
				entry => {
					if (entry is CommandDialogueEntryInfo command) {
						return command.Command == CommandType.Speaker;
					}

					return false;
				}
			);
		}
	}
}

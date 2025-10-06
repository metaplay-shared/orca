using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class DialogueInfo : IGameConfigData<DialogueId> {
		[MetaMember(1)] public DialogueId Id { get; private set; }
		[MetaMember(2)] public DialogueWrapperInfo Dialogue { get; private set; }

		public DialogueInfo() { }

		public DialogueInfo(DialogueId dialogueId, List<DialogueEntryInfo> entries) {
			Id = dialogueId;
			Dialogue = new DialogueWrapperInfo(entries);
		}

		public DialogueId ConfigKey => Id;
	}

	/**
	 * This class is added to make config parsing work correctly. Lists are handled separately in CsvReader which messes
	 * the type registry ("invalid type for cell in csv" error).
	 */
	[MetaSerializable]
	public class DialogueWrapperInfo {
		[MetaMember(1)] public List<DialogueEntryInfo> Entries { get; private set; }

		public DialogueWrapperInfo() { }

		public DialogueWrapperInfo(List<DialogueEntryInfo> entries) {
			Entries = entries;
		}
	}

	[MetaSerializable]
	public abstract class DialogueEntryInfo {
		[MetaMember(100)] public int Index { get; private set; }

		public DialogueEntryInfo() { }

		public DialogueEntryInfo(int index) {
			Index = index;
		}
	}

	[MetaSerializableDerived(1)]
	public class CommandDialogueEntryInfo : DialogueEntryInfo {
		[MetaMember(1)] public CommandType Command { get; private set; }
		[MetaMember(2)] public List<string> Parameters { get; private set; }

		public CommandDialogueEntryInfo() { }

		public CommandDialogueEntryInfo(int index, CommandType command, List<string> parameters) : base(index) {
			Command = command;
			Parameters = parameters;
		}
	}

	[MetaSerializableDerived(2)]
	public class ChatDialogueEntryInfo : DialogueEntryInfo {
		[MetaMember(1)] public string Speaker { get; private set; }
		[MetaMember(2)] public string LocalizationId { get; private set; }
		[MetaMember(3)] public string Text { get; private set; }
		[MetaMember(4)] public string InfoUrl { get; private set; }

		public ChatDialogueEntryInfo() { }

		public ChatDialogueEntryInfo(int index, string speaker, string localizationId, string text, string url) : base(index) {
			Speaker = speaker;
			LocalizationId = localizationId;
			Text = text;
			InfoUrl = url;
		}

		public string GetLocalizationKey(DialogueId id) {
			return "Dialogue." + id + "." + LocalizationId;
		}
	}

	[MetaSerializable]
	public enum CommandType {
		None,
		Speaker
	}
}

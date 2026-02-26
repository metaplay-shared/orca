using System.Collections.Generic;
using Code.UI.Dialogue.Commands;
using Game.Logic;
using UnityEngine;

namespace Code.UI.Dialogue.DialogueSystem {
	public class DialogueCommandRunner {
		//private Dictionary<CommandType, Action<string[]>> synchronousCommands = new();
		private readonly Dictionary<CommandType, CommandBase> synchronousCommands = new();

		public void RunCommand(CommandDialogueEntryInfo command) {
			if (!synchronousCommands.ContainsKey(command.Command)) {
				Debug.LogError($"Command \"{command.Command}\" not found");
			}

			synchronousCommands[command.Command].Execute(command.Parameters.ToArray());
		}

		public void RegisterCommand(CommandBase command) {
			synchronousCommands[command.Type] = command;
		}
	}
}

using System;
using System.Collections.Generic;
using Metaplay.Core;

namespace Game.Logic {
	public class DialogueParser {
		public static List<DialogueEntryInfo> ParseDialogue(string input) {
			string[] lines = input.Replace("\r", "").Split("\n");
			List<DialogueEntryInfo> entries = new List<DialogueEntryInfo>();
			int index = 1;
			foreach (string line in lines) {
				if (line.Trim().Length > 0) {
					entries.Add(ParseDialogueLine(index, line));
					index++;
				}
			}

			return entries;
		}

		private static DialogueEntryInfo ParseDialogueLine(int index, string line) {
			ConfigLexer lexer = new ConfigLexer(line);
			if (lexer.CurrentToken.Type == ConfigLexer.TokenType.ForwardSlash) {
				lexer.Advance();
				string rawCommand = lexer.ParseIdentifier();
				if (!Enum.TryParse(rawCommand, out CommandType command)) {
					throw new Exception($"Invalid command at index {index}: {rawCommand}");
				}
				List<string> paramList = new List<string>();
				while (!lexer.IsAtEnd) {
					paramList.Add(lexer.ParseIdentifier());
				}

				return new CommandDialogueEntryInfo(index, command, paramList);
			} else {
				string speaker = lexer.ParseIdentifier();
				lexer.ParseToken(ConfigLexer.TokenType.Colon);
				lexer.ParseToken(ConfigLexer.TokenType.Hash);
				string localizationId = lexer.ParseIdentifier();
				string text = lexer.ParseStringLiteral();
				string url = lexer.ParseStringLiteral();
				return new ChatDialogueEntryInfo(index, speaker, localizationId, text, url);
			}
		}
	}
}

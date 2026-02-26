using System.Collections.Generic;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class DialogueParserTest {
		[Test]
		public void ParseDialogue() {
			List<DialogueEntryInfo> dialogue = DialogueParser.ParseDialogue(
				@"
/Speaker TutorialGuy Happy
TutorialGuy: #Greeting_1 ""Greetings, Player!""

TutorialGuy: #Greeting_2 ""I see you have discovered the chat feature."""
			);
			Assert.AreEqual(3, dialogue.Count);
			Assert.AreEqual(CommandType.Speaker, ((CommandDialogueEntryInfo)dialogue[0]).Command);
			Assert.AreEqual("TutorialGuy", ((CommandDialogueEntryInfo)dialogue[0]).Parameters[0]);
		}
	}
}

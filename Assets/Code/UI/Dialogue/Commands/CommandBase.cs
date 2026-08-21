using System;
using Game.Logic;

namespace Code.UI.Dialogue.Commands {
	public abstract class CommandBase {
		public abstract CommandType Type { get; }

		public abstract void Execute(string[] parameters);

		protected void RequireParameterCount(int count, int required) {
			if (count != required) {
				throw new Exception(
					$"Wrong parameter count for command {Type.ToString()}. Got {count}, expected {required}"
				);
			}
		}
	}
}

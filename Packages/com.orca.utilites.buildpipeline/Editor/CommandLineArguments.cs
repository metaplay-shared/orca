using System;
using System.Linq;

namespace Orca.Utilities.BuildPipeline.Editor {
	public static class CommandLineArguments {
		public static bool HasFlag(string name) {
			string[] args = Environment.GetCommandLineArgs();
			return args.Contains(name);
		}

		public static string GetCommandLineArg(string name) {
			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++) {
				if (args[i] == name && args.Length > i + 1) {
					return args[i + 1];
				}
			}

			return string.Empty;
		}
	}
}
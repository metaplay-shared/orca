namespace Code.Editor.BuildPipeline {
	public static class BuildCallbackOrder {
		public static class PreprocessBuildCallbackOrder {
			public const int ActiveBuildType = Default - 1;
			public const int Default = 0;
			public const int BundleVersion = Default;
			public const int EditorUserBuildSettings = Default;
			public const int ScriptingDefineSymbols = Default;
			public const int Configs = Default;
			public const int Addressables = Configs + 1;
		}
	}
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Game.Logic;
using Metaplay.Core.Localization;
using Metaplay.Unity.DefaultIntegration;
using ModestTree;

namespace Code.UI.Utils {
	public static class Localizer {
		public const string NO_KEY = "#NO_KEY#";

		private static readonly HashSet<string> missingKeys = new();

		public static string Localize(string key, params object[] parameters) {
			var translations = MetaplayClient.ActiveLanguage.Translations;
			if (!translations.ContainsKey(TranslationId.FromString(key))) {
				missingKeys.Add(key);

				return NO_KEY + key;
			}

			var format = MetaplayClient.ActiveLanguage.Translations[TranslationId.FromString(key)];
			return string.Format(format, parameters);
		}

		public static string Localize(this ChainTypeId type) {
			return Localize($"Chain.{type.Value}");
		}

		public static string Localize(this ChainInfo info) {
			return Localize($"Chain.{info.Type}.{info.Level}");
		}
		
		public static string Localize(this LevelId<ChainTypeId> chainLevelId) {
			return Localize($"Chain.{chainLevelId.Type}.{chainLevelId.Level}");
		}

		public static string Localize(this IslandTypeId type) {
			return Localize($"Island.{type.Value}");
		}

		public static string Localize(this ShopCategoryId type) {
			return Localize($"MarketCategory.{type.Value}");
		}

		public static string Localize(this HeroTypeId type) {
			return Localize($"Hero.{type.Value}");
		}

		[Conditional("UNITY_EDITOR")]
		public static void PrintReport() {
			if (missingKeys.IsEmpty()) {
				return;
			}

			StringBuilder sb = new();
			sb.Append("Missing localization keys found:\n");

			foreach (string key in missingKeys) {
				sb.Append($"    {key}\n");
			}

			UnityEngine.Debug.LogError(sb.ToString());
		}
	}
}

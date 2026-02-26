// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Localization;

namespace Game.Logic
{
    public class GlobalOptions : IMetaplayCoreOptionsProvider
    {
        /// <summary>
        /// Game-specific constant options for core Metaplay SDK.
        /// </summary>
        public MetaplayCoreOptions Options => new MetaplayCoreOptions(
            projectName:            "Orca",
            projectId:              "Orca",
            gameMagic:              "ORCA",
            supportedLogicVersions: new MetaVersionRange(7, 7),
            clientLogicVersion:     7,
            guildInviteCodeSalt:    0x17,
            sharedNamespaces:       new string[] { "Game.Logic" },
            defaultLanguage:        LanguageId.FromString("en"),
            featureFlags: new MetaplayFeatureFlags
            {
                EnableLocalizations = true,
                EnableGuilds = true,
                EnablePlayerLeagues = true,
            });
    }
}

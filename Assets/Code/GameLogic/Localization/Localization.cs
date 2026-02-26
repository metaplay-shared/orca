using System;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Logic.LiveOpsEvents;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Localization;
using Metaplay.Core.Offers;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializableDerived(1)]
	public class OrcaLocalizationBuildParameters : LocalizationsBuildParameters {
		[MetaMember(1)]
		public bool GenerateDiffs { get; set; }

		public OrcaLocalizationBuildParameters() { }
	}

	public class LocalizationBuild : LocalizationsBuild {
		
		Random rand = 
#if NETCOREAPP
			Random.Shared;
#else
			new System.Random();
#endif
		public float NextSingle()
		{
#if NETCOREAPP
			return rand.NextSingle();
#else
			return (float)rand.NextDouble();
#endif
		}
		
		public LocalizationBuild() : base() { }
		
		protected override async Task<IEnumerable<LocalizationLanguage>> BuildAsync(LocalizationsBuildParameters buildParams, CancellationToken ct)
		{
			var orcaLocalizationBuildParameters = buildParams as OrcaLocalizationBuildParameters;
			SpreadsheetContent content = (SpreadsheetContent)await SourceFetcher.Fetch("Localizations").Get();
			return GameConfigHelper
				.SplitLanguageSheets(content, allowMissingTranslations: false)
				.Select(x=> {
					var spreadsheet = ModifyContent(x, orcaLocalizationBuildParameters.GenerateDiffs);
					return ParseLocalizationLanguageSheet(spreadsheet);
				});
		}

		private SpreadsheetContent ModifyContent(SpreadsheetContent content, bool generateDiffs) {
			var header = content.Cells[0];
			List<List<SpreadsheetCell>> rows = new List<List<SpreadsheetCell>>();
			rows.Add(header);

			for (var rowIndex = 0; rowIndex < content.Cells.Skip(1).ToList().Count; rowIndex++) {
				var row = content.Cells.Skip(1).ToList()[rowIndex];
				
				if (!generateDiffs || NextSingle() > .02f)
					rows.Add(row);
				else {
					List<SpreadsheetCell> rowCells = new List<SpreadsheetCell>();
					for (var i = 0; i < row.Count; i++) {
						rowCells.Add(row[i]);
					}

					var randVal = NextSingle();

					string newValue = rowCells[1].Value;

					if (randVal < .5f) {
						newValue += ((char)('A' + rand.Next(26))).ToString().ToLower();
					} else if (randVal < 1) {
						newValue = rows[rowIndex - 1][1].Value;
					}

					rowCells[1] = new SpreadsheetCell(newValue, rowCells[1].Row, rowCells[1].Column);

					rows.Add(rowCells);
				}
			}

			return new SpreadsheetContent(content.Name, rows, content.SourceInfo);
		}
	}
}
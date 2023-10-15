using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimTrust.Trade;
using RimWorld;
using Verse;

namespace RimTrust.Core.Interactive
{
    class CompUseEffect_AccessLegacyResearchCache : CompUseEffect
    {

		List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
					"DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
					"Smithing", "RecurveBow",
					"PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
					"LongBlades", "PlateArmor", "Greatbow",
					"Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
					"Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
					"Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurett",
					"MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
					"LongRangeMineralScanner", "ShieldBelt",
					"PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
					"MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
					"ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

		List<int> initResearchRecordValue = new List<int>() { 500, 1000, 400, 300, 400, 300, 600,
					500, 1000, 800, 800, 500,
					600, 300, 800, 700, 300, 400, 600, 600,
					700, 400,
					400, 600, 1000, 500,
					400, 600, 600,
					1600, 400, 700, 700, 400, 600, 500, 600, 700, 1000, 500, 600, 500, 3200, 600, 300,
					1000, 300, 600, 500, 1200, 2000, 500, 1000, 500, 800,
					3000, 2000, 1200, 1200, 1000, 1000, 1000, 1500, 2000, 1000,
					1400, 1600, 2600,
					4000, 2500, 4000, 4000, 2000, 6000, 6000, 2000, 3000, 3000,
					4000, 2800, 6000, 6000, 3000, 4000};

		public override void DoEffect(Pawn usedBy)
			{
				base.DoEffect(usedBy);
				this.AccessLegacyResearch(usedBy);
			}

			public override bool CanBeUsedBy(Pawn p, out string failReason)
			{
				if (1 == 2)
				{
					failReason = "NoActiveResearchProjectToFinish".Translate();
					return false;
				}
				failReason = null;
				return true;
			}

			private void AccessLegacyResearch(Pawn usedBy)
			{
				if (!Methods.LegacyResearch.Count.Equals(0))
				{
					int index = 0;
					foreach (string item in initResearchRecord)
						{

						float currentResearchProgress = Find.ResearchManager.GetProgress(ResearchProjectDef.Named(item));

						if (currentResearchProgress < initResearchRecordValue[index])
							{
							
							Find.ResearchManager.FinishProject(ResearchProjectDef.Named(item), false, null, true);
							Messages.Message("MessageResearchProjectFinishedByItem".Translate(ResearchProjectDef.Named(item).LabelCap), usedBy, MessageTypeDefOf.PositiveEvent, true);
							}
						}
				}
			}
	}
}


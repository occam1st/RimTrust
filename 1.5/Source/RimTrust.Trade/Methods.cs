using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Trade
{
    [DefOf]
    public class HediffDef_Neural : HediffDef
    {
        public static HediffDef NeuralOverdose;
        public static HediffDef NeuralFatigue;
        public static HediffDef NeuralImplant;
        public static ThoughtDef Ascension;

        static void HeDiffDefOf_NeuralOverdose()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
        static void HeDiffDefOf_NeuralFatigue()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
        static void HeDiffDefOf_NeuralImplant()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
        /*  static void HeDiffDefOf_Ascension()
          {
              DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
          }*/
    }
    /*public class ThoughtDef_Ascension : ThoughtDef
    { 
        public static ThoughtDef Ascension;

        static void ThoughtDefOf_Ascension()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef_Ascension));
        }
    }*/

    public static class Methods
    {

        public static bool debug = false;
        internal static List<Tradeable> cacheNotes = new List<Tradeable>();
        private static readonly FieldInfo tradeables = AccessTools.Field(typeof(TradeDeal), "tradeables");

        public static int TrustFunds;
        public static List<int> LegacySkills = new List<int>();
        public static List<float> LegacyResearch = new List<float>();
        public static int LegacyPower;
        public static int PawnXpTotal;
        public static Pawn LegacyPawn;
        public static List<Thing> TrusteeCollectorThings = new List<Thing>();

        public static bool DoExecute(this TradeDeal This)
        {

            Pair<int, int> currencyFmt = Utility.GetCurrencyFmt();
            if (debug)
            {
                //Log.Message(currencyFmt.First + "," + currencyFmt.Second);
            }
            UpdateCurrencyCount(This, currencyFmt);
            bool flag = false;
            foreach (Tradeable item in (List<Tradeable>)tradeables.GetValue(This))
            {
                if (item.ActionToDo != 0)
                {
                    flag = true;
                }
                item.ResolveTrade();
            }
            This.Reset();
            if (flag)
            {
                Utility.ResetCacheNotes();
            }
            return flag;
        }

        public static bool CanColonyAffordTrade(TradeDeal This)
        {
            int num = This.CurrencyTradeable.CountPostDealFor(Transactor.Colony);
            int notesBalanceAvaliable = Utility.GetNotesBalanceAvaliable(Transactor.Colony);
            if (num + notesBalanceAvaliable > 0)
            {
                return true;
            }
            return false;
        }

        public static void UpdateCurrencyCount(TradeDeal This, Pair<int, int> currencyfmt)
        {
            This.CurrencyTradeable.ForceTo(currencyfmt.Second);
            int num = Math.Abs(currencyfmt.First);
            Transactor transactor = (currencyfmt.First >= 0) ? Transactor.Trader : Transactor.Colony;
            for (int num2 = cacheNotes.Count - 1; num2 > -1; num2--)
            {
                int num3 = cacheNotes[num2].CountHeldBy(transactor);
                if (debug)
                {
                    Utility.DebugprintfUpdateCurrencyCount(num, num3, transactor);
                }
                if (num3 != 0)
                {
                    if (num < num3)
                    {
                        num3 = num;
                        num = 0;
                    }
                    else
                    {
                        num -= num3;
                    }
                    if (transactor == Transactor.Colony)
                    {
                        num3 = -num3;
                    }
                    if (debug)
                    {
                        Utility.DebugprintfUpdateCurrencyCount(num, num3, transactor);
                    }
                    cacheNotes[num2].ForceTo(num3);
                    if (num == 0)
                    {
                        break;
                    }
                }
            }
            if (debug)
            {
                Utility.DebugOutputTradeables((List<Tradeable>)tradeables.GetValue(This));
            }
        }

        public static void UpdateTrustFunds(int increase)
        {
            Methods.TrustFunds += increase;
        }
        public static void UpdatePawnSkillFromLegacy(Pawn pawn)
        {
            int index = 0;
            int index2 = 0;
            List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
            List<SkillRecord> skills = pawn.skills.skills;
            //Log.Message("Pawn selected: " + pawn.Name.ToString());
            int SkillXPBeforeLearn = 0;

            foreach (string item in initSkillRecord)
            {
                //Log.Message("foreach loop item = " + item);
                if (item != "Medical")
                {
                    index2 = skills.FindIndex(x => x.def.defName.ToString() == item);
                }
                else
                {
                    index2 = skills.FindIndex(x => x.def.defName.ToString() == "Medicine");
                }

                var skilltemp = skills[index];
                skills[index] = skills[index2];
                skills[index2] = skilltemp;

                //Log.Message("before if (LegacySkills[index] != 0 " + item);
                if (LegacySkills[index] != 0)
                {
                    //Log.Message("Legacy Skill " + skills[index].def.defName.ToString() + " selected at index ( " + index + ") with xp: " + LegacySkills[index]);
                    SkillXPBeforeLearn = Methods.SkillXPTotal(skills[index]);


                    skills[index].Learn(Methods.LegacySkills[index] * 0.01f);
                    //Log.Message("pawn skill selected: " + skills[index].def.defName.ToString() + " after learn with xp: " + Methods.SkillXPTotal(skills[index]) + " (+" +
                    //    ((double)Methods.SkillXPTotal(skills[index])- (double)SkillXPBeforeLearn) + ")");
                }
                index++;
            }
        }

        public static string UpdateColonyResearchFromLegacy()
        {
            int index = 0;
            string loadedResearch = "";

            List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

            List<int> initResearchRecordValue = new List<int>() { 500, 1000, 400, 300, 400, 300, 600,
                    500, 1000, 800, 800, 500,
                    700, 400,
                    400, 600, 1000, 500,
                    400, 600, 600,
                    1600, 400, 700, 700, 400, 600, 500, 600, 700, 1000, 500, 
                    600, 500, 3200, 600, 300,
                    1000, 300, 600, 500, 1200, 2000, 500, 1000, 500, 800,
                    3000, 2000, 1200, 1200, 4000, 1000, 1000, 1500,
                    2000, 1000,
                    1400, 1600, 2600,
                    4000, 2500, 4000, 4000, 2000, 6000, 6000, 3000, 2000, 3000, 3000,
                    4000, 2800, 6000, 6000, 3000, 4000};

            foreach (string item in initResearchRecord)
            {
                if (LegacyResearch[index] != 0)
                {
                    if (Find.ResearchManager.GetProgress(ResearchProjectDef.Named((item))) < LegacyResearch[index])
                    {
                        int ResearchValueGained = (int)(LegacyResearch[index] - (Find.ResearchManager.GetProgress(ResearchProjectDef.Named(item))));

                        if (LegacyResearch[index] == initResearchRecordValue[index])
                        {

                            Find.ResearchManager.FinishProject(ResearchProjectDef.Named(item), false, null, true);
                            //Log.Message("LegacyResearch[index] == initResearchRecordValue[index]");
                            //Log.Message(item + " gained " + ResearchValueGained + " research points");
                            loadedResearch += item + "(" + ResearchValueGained + ")" + ", ";
                        }
                    }
                }

                index++;
            }
            return loadedResearch;
        }

        public static string UpdateColonyResearchFromLegacySmall(Pawn pawn)
        {
            int index = 0;
            string loadedResearch = "";

            List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

            List<int> initResearchRecordValue = new List<int>() { 500, 1000, 400, 300, 400, 300, 600,
                    500, 1000, 800, 800, 500,
                    700, 400,
                    400, 600, 1000, 500,
                    400, 600, 600,
                    1600, 400, 700, 700, 400, 600, 500, 600, 700, 1000, 500,
                    600, 500, 3200, 600, 300,
                    1000, 300, 600, 500, 1200, 2000, 500, 1000, 500, 800,
                    3000, 2000, 1200, 1200, 4000, 1000, 1000, 1500,
                    2000, 1000,
                    1400, 1600, 2600,
                    4000, 2500, 4000, 4000, 2000, 6000, 6000, 3000, 2000, 3000, 3000,
                    4000, 2800, 6000, 6000, 3000, 4000};

            List<string> starterResearchRecord = new List<string>() { "TreeSowing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Pemmican",
                    "Smithing", "RecurveBow",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Hydroponics", "PackagedSurvivalMeal",
                    "GeothermalPower", "SterileMaterials",
                    "Machining", "Prosthetics", "Gunsmithing", "FlakArmor", "BlowbackOperation", "GasOperation",
                    "MicroelectronicsBasics", "HospitalBed", "MedicineProduction"
                    };

            List<int> starterResearchRecordValue = new List<int>() { 1000, 300, 400, 300, 600,
                    500, 500,
                    700, 400,
                    400, 600, 600,
                    1600, 400, 700, 700, 400, 600, 500, 700, 500,
                    3200, 600,
                    1000, 600, 500, 1200, 500, 1000,
                    3000, 1200, 1500 
                    };

            foreach (string item in initResearchRecord)
            {
                if (LegacyResearch[index] != 0)
                {
                    if (Find.ResearchManager.GetProgress(ResearchProjectDef.Named((item))) < LegacyResearch[index])
                    {
                        int ResearchValueGained = (int)(LegacyResearch[index] - (Find.ResearchManager.GetProgress(ResearchProjectDef.Named(item))));
                        //Log.Message("LegacyResearch for + " + item + " at index " + index + " equals: " + LegacyResearch[index]);
                        //Log.Message("ResearchValueGained: " + ResearchValueGained);
                        

                        if (LegacyResearch[index] == initResearchRecordValue[index])
                        {
                            if(starterResearchRecord.Contains(item))
                                { 
                                Find.ResearchManager.FinishProject(ResearchProjectDef.Named(item), false, null, true);
                                //Log.Message("LegacyResearch[index] == initResearchRecordValue[index]");
                                //Log.Message(item + " gained " + ResearchValueGained + " research points");
                                loadedResearch += item + "(" + ResearchValueGained + ")" + ", ";
                                }
                        }
                    }
                }

                index++;
            }
            return loadedResearch;
        }

        public static int CountColonyResearchFromLegacy(string loadedResearchString, char toFind)
        {
            //Log.Message("loaded ResearchString: " + loadedResearchString);
            return loadedResearchString.Count(t => t == toFind);
        }
        public static void SaveTrustFunds()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                //Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwtf";

            SafeSaver.Save(str2, "RWTF", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                //Log.Message("SaveTrustFunds");
                Scribe_Values.Look<int>(ref TrustFunds, "TrustFunds", 0);
                
            }));
        }

        public static void SaveLegacySkills()
        {
            //Log.Message("start SaveLegacySkills");

            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                //Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwls";

            SafeSaver.Save(str2, "RWLS", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                int xp = 0;
                int index = 0;

                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                foreach (string item in initSkillRecord)
                {
                    //xp = item.xpSinceLastLevel + item.XpTotalEarned;
                    xp = Methods.LegacySkills[index];
                    //Log.Message("SaveLegacySkills " + item + " at index " + index + "with xp + " + xp);
                    if (xp != 0)
                    { Scribe_Values.Look<int>(ref xp, item, 0); }
                    else
                    { Scribe_Values.Look<int>(ref xp, item, 1); }
                    index++;
                }
            }));
        }

        public static void SaveLegacyResearch()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                // Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwlr";

            SafeSaver.Save(str2, "RWLR", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                float researchValue = 0;
                int index = 0;



                List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

                foreach (string item in initResearchRecord)
                {
                    researchValue = LegacyResearch[index];
                    if (researchValue != 0)
                    { Scribe_Values.Look<float>(ref researchValue, item, 0); }
                    else
                    { Scribe_Values.Look<float>(ref researchValue, item, 1); }
                    //Log.Message("SaveLegacyResearch saving reseach " + item + " at index " + index);
                    index++;
                }
            }));
        }
        public static void SaveLegacyPower()
        {
            //Log.Message("start SaveLegacyPower");

            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                //Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwlp";

            SafeSaver.Save(str2, "RWLP", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                //Log.Message("SaveLegacyPower");
                Scribe_Values.Look<int>(ref LegacyPower, "LegacyPower", 0);
            }));
        }

        public static void LegacyResearchMessage()
        {
            //if (debug)
            //Log.Message("Legacy Research Message start");
            string msg = "";
            int index = 0;
            List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };
            //if (debug)
            //Log.Message("Legacy Research Message after list initiate before foreach loop");
            foreach (string item in initResearchRecord)
            {
                //if (debug)
                //Log.Message("Legacy Research Message foreach loop " + index);
                //if (debug)
                //Log.Message("Legacy Research #" + index + " :" + item + " with " + Methods.LegacyResearch[index]);
                if (Methods.LegacyResearch[index] != 0)
                    msg += item + ": " + Methods.LegacyResearch[index] + "\n";
                index++;
            }
            int totalResearchXP = 123700;
            msg += "\n\n" + "FloatMenuCaptionTotalLegacyResearchArchive".Translate() + " " + Methods.LegacyResearch.Sum() + " (" + Math.Round(((double)Methods.LegacyResearch.Sum() / (double)totalResearchXP) * 100, 4) + "%)";
            msg += "\n\n" + "You can access the knowledge with a Neural Implant, one-hundredth of it at a time.";
            DiaNode diaNode = new DiaNode(msg);
            DiaOption diaOption = new DiaOption("Disconnect".Translate());
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
            Find.WindowStack.Add(dialog_NodeTree);
        }

        public static int SkillXPTotal(SkillRecord skillRecord)
        {
            int SkillXpTotal = 0;
            SkillXpTotal = (int)(skillRecord.xpSinceLastLevel + skillRecord.XpTotalEarned);
            return SkillXpTotal;
        }
        public static int PawnXPTotal(Pawn pawn)
        {
            PawnXpTotal = 0;
            foreach (SkillRecord item in pawn.skills.skills)
            {
                if (item.TotallyDisabled)
                {
                    PawnXpTotal = +0;
                }
                else
                {
                    PawnXpTotal += SkillXPTotal(item);
                }
            }
            return PawnXpTotal;
        }
        public static float SkillXPTotalPercent(SkillRecord skillRecord, Pawn pawn)
        {
            float SkillXpTotalPercent = 0F;

            if (!skillRecord.TotallyDisabled)
            {
                if (!(PawnXPTotal(pawn) == 0))
                {
                    SkillXpTotalPercent = (float)SkillXPTotal(skillRecord) / PawnXPTotal(pawn);
                }
                else
                {
                    SkillXpTotalPercent = 0F;
                }
            }
            return SkillXpTotalPercent;
        }

        public static string SelectSkillToSafeNIC(Pawn pawn)
        {
            string SelectedSkillString = "";
            SkillRecord SelectedSkill;
            List<SkillRecord> skills = pawn.skills.skills;
            skills.SortByDescending(x => x.XpTotalEarned + x.xpSinceLastLevel);
            foreach (SkillRecord skill in skills)
            {
                if (!skill.TotallyDisabled)
                {
                    SelectedSkillString = skill.def.defName.ToString();
                    SelectedSkill = skill;
                    break;
                }
                else
                {
                }

            }
            return SelectedSkillString;
        }

        public static SkillRecord SelectSkillRecordToSafeNIC(Pawn pawn)
        {
            SkillRecord SelectedSkill;

            List<SkillRecord> skills = pawn.skills.skills;
            skills.SortByDescending(x => x.XpTotalEarned + x.xpSinceLastLevel);
            SelectedSkill = skills[0];

            return SelectedSkill;
        }

        public static void UpdateLegacySkills(string skill, Pawn p, int tier)
        {
            List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
            if (skill == "Medicine") { skill = "Medical"; }
            //Log.Message("UpdateLegacySkills before int index = initSkillRecord.FindIndex(x => x == skill)");
            int index = initSkillRecord.FindIndex(x => x == skill);
            const double maxSkillXP = 297000d;
            //Log.Message("UpdateLegacySkills before SkillRecord SkillRecord = p.skills.skills[0]");
            SkillRecord SkillRecord = p.skills.skills[0];
            double skillXPtotallearned = Methods.SkillXPTotal(SkillRecord);
            double xpmult = 0.075;
            if (tier != 1 && tier != 2)
            {
                tier = 1;
            }
            if (!Methods.LegacySkills.Count.Equals(0))
            {
                //Log.Message("UpdateLegacySkills before skillXPtolearn = skillXPtotallearned - LegacySkills[index]");
                //Log.Message("UpdateLegacySkills skillXPtotallearned = " + skillXPtotallearned + " and LegacySkills[index] " + LegacySkills[index]);
                double skillXPtolearn = skillXPtotallearned - LegacySkills[index];
                //Log.Message("UpdateLegacySkills skillXPtolearn " + skillXPtolearn);

                if (skillXPtolearn > 0)
                {
                    double skillSafePercentage = (Math.Pow(((double)(maxSkillXP - LegacySkills[index]) / maxSkillXP), 2));
                    double skillXPtosafe = (Math.Pow(((double)(maxSkillXP - LegacySkills[index]) / maxSkillXP), 2) * skillXPtolearn * (xpmult * tier));
                    //Log.Message("LegacySkill " + skill + " before save: " + LegacySkills[index]);
                    //Log.Message("skillXPtosafe: " + (int)skillXPtosafe + " with Percentage " + skillSafePercentage * 100 + "%");
                    LegacySkills[index] += (int)skillXPtosafe;
                    //Log.Message("LegacySkill after save: " + LegacySkills[index]);
                    MoteMaker.ThrowText(p.DrawPos, p.Map, skill + " +" + (int)skillXPtosafe + " XP", Color.green, 5f);
                }
            }
        }

        public static void UpdateLegacyResearch(Pawn p, int tier)
        {
            //Log.Message("Start call UpdateLegacyResearch");

            float researchValue = 0;
            int index, index2;
            index = index2 = 0;
            double researchmult = 0.25;
            float researchPercentageSavedBefore, researchPercentageSavedAfter = 0;
            float pawnResearchFactor = 0;
            int researchSkillindex = 11;

            if (tier != 1 && tier != 2)
            {
                tier = 1;
            }
            //Log.Message("UpdateLegacyResearch Methods.LegacySkills.Count equals " + Methods.LegacyResearch.Count);
            if (!Methods.LegacyResearch.Count.Equals(0))
            {
                List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

                List<int> initResearchRecordValue = new List<int>() { 500, 1000, 400, 300, 400, 300, 600,
                    500, 1000, 800, 800, 500,
                    700, 400,
                    400, 600, 1000, 500,
                    400, 600, 600,
                    1600, 400, 700, 700, 400, 600, 500, 600, 700, 1000, 500, 
                    600, 500, 3200, 600, 300,
                    1000, 300, 600, 500, 1200, 2000, 500, 1000, 500, 800,
                    3000, 2000, 1200, 1200, 4000, 1000, 1000, 1500,
                    2000, 1000,
                    1400, 1600, 2600,
                    4000, 2500, 4000, 4000, 2000, 6000, 6000, 3000, 2000, 3000, 3000,
                    4000, 2800, 6000, 6000, 3000, 4000};

                foreach (SkillRecord skill in p.skills.skills)
                {
                    //Log.Message("Start of foreach check for disabled Intellectual");
                    if (skill.ToString() == "Intellectual")
                    {
                        if (skill.TotallyDisabled)
                        {
                            researchSkillindex = index2;
                            //Log.Message("Pawn incapable of Research: " + p.Name);
                            //Log.Message("Research Skill for " + p.Name.ToString() + " : " + p.skills.skills[researchSkillindex].ToString());
                            //Log.Message("Pawn incapable skill : " + p.skills.skills[researchSkillindex].ToString());

                            MoteMaker.ThrowText(p.DrawPos, p.Map, p.Name + " incapable of using Legacy research upload", Color.red, 7f);
                            break;
                        }
                    }
                    index2++;
                }

                if (!p.skills.skills[researchSkillindex].TotallyDisabled)
                {
                    //Log.Message("Research Skill for " + p.Name.ToString() + " : " + p.skills.skills[researchSkillindex].ToString());
                    //Log.Message("start of foreach loop in UpdateLegacyResearch");

                    foreach (string item in initResearchRecord)
                    {
                        researchValue = Find.ResearchManager.GetProgress(ResearchProjectDef.Named(item));
                        if (!initResearchRecord.Contains(item))
                        {
                            break;
                        }
                        else
                        {
                            if (researchValue != 0)
                            {
                                //Log.Message("LegacyResearch for " + item + " at index " + index + " equals " + LegacyResearch[index]);
                                //Log.Message("researchValue equals " + researchValue);
                                if (LegacyResearch[index] < initResearchRecordValue[index] && researchValue == initResearchRecordValue[index])
                                {
                                    researchPercentageSavedBefore = (LegacyResearch[index] / initResearchRecordValue[index]) * 100;
                                    pawnResearchFactor = p.GetStatValueForPawn(StatDefOf.ResearchSpeed, p);
                                    //Log.Message("Pawn " + p.Name.ToString() + " has ResearchspeedFactor of " + pawnResearchFactor);
                                    //Log.Message("initResearchRecordValue[index] - LegacyResearch[index]: " + initResearchRecordValue[index] + " - " + LegacyResearch[index]);
                                    //Log.Message("researchmult + researchmult * tier: " + researchmult + " + " + researchmult + " * " + tier);
                                    //Log.Message("(initResearchRecordValue[index] / 500) equals " + initResearchRecordValue[index] + " / 500");
                                    //Log.Message("(initResearchRecordValue[index] / 500) equals " + initResearchRecordValue[index] / 500);
                                    if (LegacyResearch[index] == 0)
                                    {
                                        LegacyResearch[index] = 1;
                                    }
                                    float researchToSafe = (float)Math.Round((((initResearchRecordValue[index] - LegacyResearch[index]) * (researchmult + researchmult * tier)) / (initResearchRecordValue[index] / 500f)), 0);
                                    //Log.Message("foreach UpdateLegacyResearch researchToSafe equals " + researchToSafe);
                                    float researchToSafeWithSkill = (float)Math.Round((researchToSafe * pawnResearchFactor) / 1.5f, 0);
                                    //Log.Message("foreach UpdateLegacyResearch researchToSafeWithSkill equals " + researchToSafeWithSkill);
                                    //Log.Message(researchPercentageSavedBefore + "% of current research is archived before upload");

                                    //Log.Message("foreach UpdateLegacyResearch with " + item + " and " + researchValue + " researchValue and " + researchToSafeWithSkill + " researchtoSafeWithSkill");
                                    //Log.Message("hitting break with " + item);
                                    researchPercentageSavedAfter = ((LegacyResearch[index] + researchToSafeWithSkill) / initResearchRecordValue[index]) * 100;


                                    if (researchToSafeWithSkill > initResearchRecordValue[index])
                                    {
                                        //Log.Message("reseachToSafeWithSkill > initResearchRecordValue[index]");
                                        //Log.Message("researchToSafeWithSkill " + researchToSafeWithSkill);
                                        //Log.Message("initResearchRecordValue[index] " + initResearchRecordValue[index]);
                                        //Log.Message("researchToSafeWithSkill = initResearchRecordValue[index]");
                                        researchToSafeWithSkill = initResearchRecordValue[index];
                                        //Log.Message("researchToSafeWithSkill: " + researchToSafeWithSkill);
                                        researchPercentageSavedAfter = 100;
                                        //Log.Message("capped research gain at max research for " + item + " with " + initResearchRecordValue[index] + " XP");
                                    }

                                    if (researchPercentageSavedAfter >= 99.5)
                                    {
                                        researchPercentageSavedAfter = 100;
                                        //Log.Message("researchPercentageSavedAfter is >= 99.5% and set to 100");
                                    }
                                    if (researchPercentageSavedAfter == 100)
                                    {
                                        LegacyResearch[index] = initResearchRecordValue[index];
                                        //Log.Message("LegacyResearch " + LegacyResearch[index].ToString() + " set to " + initResearchRecordValue[index]);
                                        MoteMaker.ThrowText(p.DrawPos, p.Map, item + " research archive completed", Color.green, 5f);
                                    }
                                    else
                                    {
                                        LegacyResearch[index] += (int)researchToSafeWithSkill;
                                        researchPercentageSavedAfter = (LegacyResearch[index] / initResearchRecordValue[index]) * 100;
                                        //Log.Message("LegacyResearch " + LegacyResearch[index].ToString() + " increased by " + researchToSafeWithSkill);
                                        MoteMaker.ThrowText(p.DrawPos, p.Map, item + " +" + (int)researchToSafeWithSkill + " research points added, now " + (int)researchPercentageSavedAfter + "% in total archived", Color.green, 5f);
                                    }
                                    break;
                                }
                            }
                        }
                        index++;
                    }
                }
            }
        }

        public static void InitiateLegacySkillSave()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                //Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwls";

            SafeSaver.Save(str2, "RWLS", (Action)(() =>
            {
                //Log.Message("InitiateLegacySkillSave");
                ScribeMetaHeaderUtility.WriteMetaHeader();
                int xp = 0;
                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };

                foreach (string item in initSkillRecord)
                {
                    Scribe_Values.Look<int>(ref xp, item, 1);
                }
            }));
        }
        public static void InitiateLegacyResearchSave()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                //Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                //Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwlr";
            
            SafeSaver.Save(str2, "RWLR", (Action)(() =>
            {
                //Log.Message("InitiateLegacyResearchSave");
                ScribeMetaHeaderUtility.WriteMetaHeader();
                int xp = 0;
                List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };

                foreach (string item in initResearchRecord)
                {
                    Scribe_Values.Look<int>(ref xp, item, 1);
                }
            }));
        }

        public static void InitiateLegacyPowerSave()
        {
            SaveLegacyPower();
        }
        public static void NicOverdoseToPawn(Pawn p)
        {
            BodyPartRecord part = p.RaceProps.body.corePart;
            Hediff hediff = HediffMaker.MakeHediff(HediffDef_Neural.NeuralOverdose, p);
            hediff.Severity = 1;
            hediff.Part = part.parts[11].parts[0].parts[0].parts[0];
            p.health.AddHediff(hediff);

            Hediff_Injury hediff_injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, p, null);
            hediff_injury.Part = part.parts[11].parts[0];
            hediff_injury.sourceBodyPartGroup = BodyPartGroupDefOf.FullHead;
            hediff_injury.sourceHediffDef = HediffDefOf.Cut;
            hediff_injury.Severity = 15;
            hediff_injury.Part.def.bleedRate = 5;
            p.health.hediffSet.hediffs.Add(hediff_injury);
        }

        public static void NicFatigueToPawn(Pawn p)
        {
            BodyPartRecord part = p.RaceProps.body.corePart;
            Hediff hediff = HediffMaker.MakeHediff(HediffDef_Neural.NeuralFatigue, p);
            hediff.Severity = 1;
            hediff.Part = part.parts[11].parts[0].parts[0].parts[0];
            p.health.AddHediff(hediff);
        }

        public static int CalculateColonyValuables()
        {
            string colonyValue = "";
            int totalValue = 0;
            string end = "";
            if (Find.CurrentMap.IsPlayerHome)
            {
                foreach (Thing placedArt in Find.CurrentMap.listerThings.AllThings)
                {
                    if (placedArt.def.defName.StartsWith("Sculpture"))
                    {
                        CompQuality compQuality = placedArt.GetInnerIfMinified().TryGetComp<CompQuality>();
                        if (compQuality.Quality != QualityCategory.Awful && compQuality.Quality != QualityCategory.Poor && compQuality.Quality != QualityCategory.Normal)
                        {
                            colonyValue += placedArt.def.defName;
                            colonyValue += " (placed): ";
                            colonyValue += placedArt.MarketValue * placedArt.stackCount;
                            totalValue += (int)(placedArt.stackCount * placedArt.MarketValue);
                            colonyValue += "\n";
                            TrusteeCollectorThings.Add(placedArt);
                        }
                    }
                }

                foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(Find.AnyPlayerHomeMap))
                {
                    if (item.def.defName == "Silver" || item.def.defName == "Gold" || item.def.defName == "BankNote")
                    {
                        colonyValue += item.def.defName;
                        colonyValue += ": ";
                        colonyValue += item.MarketValue * item.stackCount;
                        totalValue += (int)(item.stackCount * item.MarketValue);
                        colonyValue += "\n";
                        TrusteeCollectorThings.Add(item);
                    }
                    if (item.GetInnerIfMinified().def.defName.StartsWith("Sculpture"))
                    {
                        CompQuality compQuality = item.GetInnerIfMinified().TryGetComp<CompQuality>();
                        if (compQuality.Quality != QualityCategory.Awful && compQuality.Quality != QualityCategory.Poor && compQuality.Quality != QualityCategory.Normal)
                        {
                            colonyValue += item.GetInnerIfMinified().def.defName;
                            colonyValue += ": ";
                            colonyValue += compQuality.Quality.GetLabelShort();
                            colonyValue += ": ";
                            colonyValue += item.GetInnerIfMinified().MarketValue;
                            totalValue += (int)(item.GetInnerIfMinified().stackCount * item.GetInnerIfMinified().MarketValue);
                            colonyValue += "\n";
                            TrusteeCollectorThings.Add(item);
                        }
                    }
                }
            }
            end = colonyValue + totalValue;
            if (debug)
            {
                //Log.Message(end);
            }
            int netGain = (int)(totalValue * 0.6);
            if (debug)
            {
                //Log.Message("Net gain on Trustee collection (60%): " + netGain.ToString());
            }
            return (netGain / 1000);
        }

        public static void TrusteeTakeThings(List<Thing> ThingsToDestroy)
        {
            foreach (Thing thing in ThingsToDestroy)
            {
                if (!thing.Destroyed)
                {
                    thing.Destroy();
                }
                if (!thing.Discarded)
                {
                    thing.Discard();
                }
            }
        }
        public static Boolean ColonyHasNeuralImplant(Map map)
        {
            bool ColonyHasNeuralImplant = false;
            if (map.IsPlayerHome)
            {
                foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                    var NeuralImplantOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralImplant);
                    if (NeuralImplantOnPawn != null)
                    {
                        ColonyHasNeuralImplant = true;
                        break;
                    }
                }
                return ColonyHasNeuralImplant;
            }
            return false;
        }

        public static string LegacyCacheMenu()
        {
            string text = "";

            text = "Welcome decendant to the intragalactic vault trust services™ !";
            text += "\n";
            text += "\n";
            text += "You have accumulated riches, knowledge and have proven to be a skilled companion.";
            text += "\n";
            text += "In order to progress further, you will receive previous research achievments as a mark of respect. Use it wisely.";
            text += "\n";

            return text;
        }

        public static string LegacyCacheMenuSmall()
        {
            string text = "";

            text = "Welcome decendant to the intragalactic vault trust services™ !";
            text += "\n";
            text += "\n";
            text += "You have accumulated riches, knowledge and have proven to be a skilled companion.";
            text += "\n";
            text += "With your limited technology you can access basic research to help you on your journey ahead.";
            text += "\n";

            return text;
        }
        public static string LegacyCacheMenuEmtpy()
        {
            string text = "";

            text = "Welcome traveller!";
            text += "\n";
            text += "\n";
            text += "You have tried to access the (vast) library of your ancestor's knowledge.";
            text += "\n";
            text += "An unspecified error occured during access.";
            text += "\n";
            text += "\n";
            text += "The library does not seem to contain any recorded data.";
            text += "\n";
            text += "It seems you possess a greater understanding of the universe than your ancestors.";
            text += "\n";
            text += "This might be a good time to preserve that knowledge for future generations to come.";
            text += "\n";

            return text;
        }

        public static string LegacyCacheMenuNothingGained()
        {
            string text = "";

            text = "Welcome traveller!";
            text += "\n";
            text += "\n";
            text += "You have tried to access the (vast) library of your ancestor's knowledge.";
            text += "\n";
            text += "An unspecified error occured during access.";
            text += "\n";
            text += "\n";
            text += "The library does not contain any knowledge useful to you.";
            text += "\n";
            text += "It seems you possess a greater understanding of the universe than your ancestors.";
            text += "\n";
            text += "Safe travels.";
            text += "\n";

            return text;
        }


        public static float TransferZPMPower()
        {
            float powerStored = 0f;
            int ZPMi = 1;

            if (Find.CurrentMap.IsPlayerHome)
            {
                foreach (Thing ZPM in Find.CurrentMap.listerThings.AllThings)
                {
                    if (ZPM.def.defName.StartsWith("ZeroPointModule"))
                    {
                        CompPowerBattery comp = ZPM.TryGetComp<CompPowerBattery>();
                        //Log.Message("Found ZPM no. " + ZPMi + " with power level " + comp.StoredEnergy +" at position " + ZPM.Position);
                        if (comp.StoredEnergy >= 10000f && ZPM.GetRoom().OpenRoofCount < 1)
                        {
                            powerStored += comp.StoredEnergy;
                            comp.DrawPower(comp.StoredEnergy);
                        }
                        ZPMi++;
                    }
                }
            }
            //Log.Message("returning powerStored in TransferZPMPower with value : " + powerStored);
            return powerStored;
        }
    }

        public class LoadTrustFunds
        {

            static string TrustName;
            static string LegacySkillsName;
            static string LegacyResearchName;
            static string LegacyPowerName;


        public static void LoadTrust()
            {
                //Log.Message("Loading trust");
                TrustName = "default";
                string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), TrustName + ".rwtf");
                if (!System.IO.File.Exists(file))
                {
                    //Log.Error("File Doesnt exist");
                    return;
                }

                Scribe.loader.InitLoading(file);
                Scribe_Values.Look<int>(ref Methods.TrustFunds, "TrustFunds", 0);
                Scribe.loader.FinalizeLoading();
            }
            public static void LoadLegacySkills()
            {
                //Log.Message("Loading Legacy Skills");
                LegacySkillsName = "default";
                string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), LegacySkillsName + ".rwls");
                if (!System.IO.File.Exists(file))
                {
                    //Log.Message("File Doesnt exist.. creating..");
                    Methods.InitiateLegacySkillSave();
                }

                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                //Log.Message("initiate loading LegacySkills");
                int index = 0;
                Methods.LegacySkills = new List<int> { };

                foreach (string item in initSkillRecord)
                {
                    int temploadedxp = 0;

                    Scribe.loader.InitLoading(file);
                    Scribe_Values.Look<int>(ref temploadedxp, item, 0);
                    Scribe.loader.FinalizeLoading();
                    //Log.Message("load LegacySkills at index " + index);
                    //Log.Message("load LegacySkill " + item + "from index " + index + " with loaded xp " + temploadedxp);

                    Methods.LegacySkills.Add(temploadedxp);
                    index++;
                }
            }
            public static void LoadLegacyResearch()
            {
                //Log.Message("Loading Legacy Research");
                LegacyResearchName = "default";
                string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), LegacyResearchName + ".rwlr");
                if (!System.IO.File.Exists(file))
                {
                    //Log.Message("File Doesnt exist.. creating..");
                    Methods.InitiateLegacyResearchSave();
                }

                List<string> initResearchRecord = new List<string>() { "PsychoidBrewing", "TreeSowing", "Brewing", "ComplexFurniture", "PassiveCooler", "Stonecutting", "ComplexClothing",
                    "DrugProduction", "Cocoa", "Devilstrand", "CarpetMaking", "Pemmican",
                    "Smithing", "RecurveBow",
                    "PsychiteRefining", "WakeUpProduction", "GoJuiceProduction", "PenoxycylineProduction",
                    "LongBlades", "PlateArmor", "Greatbow",
                    "Electricity", "Batteries", "BiofuelRefining", "WatermillGenerator", "NutrientPaste", "SolarPanels", "AirConditioning", "Autodoors", "Hydroponics", "TubeTelevision", "PackagedSurvivalMeal",
                    "Firefoam", "IEDs", "GeothermalPower", "SterileMaterials", "ColoredLights",
                    "Machining", "SmokepopBelt", "Prosthetics", "Gunsmithing", "FlakArmor", "Mortars", "BlowbackOperation", "GasOperation","GunTurrets", "FoamTurret",
                    "MicroelectronicsBasics", "FlatscreenTelevision", "MoisturePump", "HospitalBed", "DeepDrilling", "GroundPenetratingScanner", "TransportPod", "MedicineProduction",
                    "LongRangeMineralScanner", "ShieldBelt",
                    "PrecisionRifling", "HeavyTurrets", "MultibarrelWeapons",
                    "MultiAnalyzer", "VitalsMonitor", "Fabrication", "AdvancedFabrication", "Cryptosleep", "ReconArmor", "PoweredArmor", "ChargedShot", "Bionics", "SniperTurret", "RocketswarmLauncher",
                    "ShipBasics", "ShipCryptosleep", "ShipReactor", "ShipEngine", "ShipComputerCore", "ShipSensorCluster" };
                //Log.Message("initiate loading LegacyResearch");
                int index = 0;
                Methods.LegacyResearch = new List<float> { };

                foreach (string item in initResearchRecord)
                {
                    float temploadedresearch = 0;

                    Scribe.loader.InitLoading(file);
                    Scribe_Values.Look<float>(ref temploadedresearch, item, 0);
                    Scribe.loader.FinalizeLoading();
                    //Log.Message("load LegacyResearch at index " + index);
                    //Log.Message("load LegacyResearch " + item + "from index " + index + " with loaded xp " + temploadedresearch);

                    Methods.LegacyResearch.Add(temploadedresearch);
                    index++;
                }
            }

            public static void LoadLegacyPower()
            {
                //Log.Message("Loading legacy power");
                LegacyPowerName = "default";
                string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), LegacyPowerName + ".rwlp");
                if (!System.IO.File.Exists(file))
                {
                    //Log.Message("File " + file + " doesn't exist.. creating...");
                    Methods.InitiateLegacyPowerSave();
                    return;
                }

                Scribe.loader.InitLoading(file);
                Scribe_Values.Look<int>(ref Methods.LegacyPower, "LegacyPower", 0);
                Scribe.loader.FinalizeLoading();
            }
        }
}

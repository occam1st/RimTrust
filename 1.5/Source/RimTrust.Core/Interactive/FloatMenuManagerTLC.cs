using RimTrust.Trade.Ext;
using RimTrust.Trade;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public static class FloatMenuManagerTLC
    {
        internal static Dictionary<string, Action<Pawn>> rawItems;

        internal static Dictionary<string, bool> usesDefaultJobDriver;

        internal static Action<Pawn> currentAction;

        internal static Dictionary<string, Action> shiftKeyItems;

        static FloatMenuManagerTLC()
        {
            rawItems = new Dictionary<string, Action<Pawn>>();
            usesDefaultJobDriver = new Dictionary<string, bool>();
            currentAction = null;
            shiftKeyItems = new Dictionary<string, Action>();
            //Log.Message("New FloatMenuManagerTLC rewrite");
            
            //Access TrustFunds 
            Add("FloatMenuCaptionExchange".Translate(), delegate (Pawn p)
            {
                ExtUtil.PrepareVirtualTrade(p, new Trader_BankNoteExchange());
            }, usesDefaultJobDriver: true);
            //Access Legacy Research
            Add("FloatMenuCaptionLegacyCache".Translate(), delegate (Pawn p)
            {
                string researchGainedString = "";
                int researchGained = 0;
                string msg = Methods.LegacyCacheMenu();
                msg += "\n";
                msg += Methods.UpdateColonyResearchFromLegacy();
                researchGained = Methods.CountColonyResearchFromLegacy(msg, ',') - 2;
                //Log.Message("researchGained: " + researchGained);
                if (researchGained > 1 )
                {
                    researchGainedString = "gained " + researchGained + " research projects from legacy";
                }
               else  if (researchGained == 1)
                {
                    researchGainedString = "gained " + researchGained + " research project from legacy";
                }
                else if (researchGained == 0)
                {
                    msg = Methods.LegacyCacheMenuEmtpy();
                }
                DiaNode diaNode = new DiaNode(msg);
                DiaOption diaOption = new DiaOption("Disconnect".Translate());
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
                Find.WindowStack.Add(dialog_NodeTree);
                if (researchGained > 0)
                { 
                    MoteMaker.ThrowText(p.DrawPos, p.Map, researchGainedString, Color.green, 5f); 
                }
                //Log.Message("DiaNode done");

            });
            //Upload Legacy Power from ZPM
            Add("FloatMenuCaptionLegacyPower".Translate(), delegate (Pawn p)
            {
                int power = 0;
                power = (int)Methods.TransferZPMPower();
                Log.Message("Power level in FloatMenu return of TransferZPMPower: " + power);
                if (power >= 100)
                {
                    string item = "You transfered ";
                    item += power + " Wd to the legacy subspace-time";
                    MoteMaker.ThrowText(p.DrawPos, p.Map, item, Color.green, 5f);
                    Methods.LegacyPower += power;
                    RimTrust.Core.Patches.UpdateXML.UpdateTLCBasePowerConsumptionFromLegacyPower();
                    Methods.SaveLegacyPower();
                }   
                else
                {
                    string item = "Not enough Wd stored in ZPM";
                    MoteMaker.ThrowText(p.DrawPos, p.Map, item, Color.red, 10f);
                }
            }, usesDefaultJobDriver: true);
            //TrusteeCollector Incident Test
            if (Methods.debug)
            {
                Add("FloatMenuCaptionTrusteeCollectorTest".Translate(), delegate 
                {
                    int ValuablesGain = Methods.CalculateColonyValuables();
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
                    thing.stackCount = ValuablesGain;
                    string msg = ValuablesGain.ToString();
                    Log.Message("this is a TrusteeCollector test that fired with " + msg);
                    DiaNode diaNode = new DiaNode(msg);
                    DiaOption diaOption = new DiaOption("Disconnect".Translate());
                    diaOption.resolveTree = true;
                    diaNode.options.Add(diaOption);
                    Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
                    Find.WindowStack.Add(dialog_NodeTree);
                
                });
            }
            //Remove all mod items on shift menu
            AddShiftKeyItem("FloatMenuCaptionRemoveAll".Translate(), delegate
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DlgRemoveModContents".Translate(), delegate
                {
                    int num = 0;
                    num += ModContentRemover.RemoveAllModContentsFromWorldObjects();
                    num += ModContentRemover.RemoveAllModContentsFromPassingShips();
                    num += ModContentRemover.RemoveAllModContentsFromMaps();
                    num += ModContentRemover.RemoveAllModContentsFromAllPawns();
                    Messages.Message("MsgModContentsRemoved".Translate(num), MessageTypeDefOf.PositiveEvent);
                }, destructive: true, title: "DlgTitleRemoveModContents".Translate()));
            });
        }

        public static void Add(string str, Action<Pawn> action, bool usesDefaultJobDriver = false)
        {
            rawItems.Add(str, action);
            FloatMenuManagerTLC.usesDefaultJobDriver.Add(str, usesDefaultJobDriver);
        }

        public static void AddShiftKeyItem(string str, Action action)
        {
            shiftKeyItems.Add(str, action);
        }

        public static void Remove(string name)
        {
            rawItems.Remove(name);
            usesDefaultJobDriver.Remove(name);
            shiftKeyItems.Remove(name);
        }

        public static IEnumerable<FloatMenuOption> RequestBuild(Building target, Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (KeyValuePair<string, Action<Pawn>> pair in rawItems)
            {
                list.Add(new FloatMenuOption(pair.Key, delegate
                {
                    if (usesDefaultJobDriver[pair.Key])
                    {
                        currentAction = pair.Value;
                        Job job = new Job(CoreDefOf.UseTrustLedgerConsole, target);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                    else
                    {
                        pair.Value(pawn);
                    }
                }));
            }
             if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                foreach (KeyValuePair<string, Action> shiftKeyItem in shiftKeyItems)
                {
                    list.Add(new FloatMenuOption(shiftKeyItem.Key, shiftKeyItem.Value));
                }
                return list;
            } 
            return list;
        }
    }
}
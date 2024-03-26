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
    public static class FloatMenuManagerTLCsmall
    {
        internal static Dictionary<string, Action<Pawn>> rawItems;

        internal static Dictionary<string, bool> usesDefaultJobDriver;

        internal static Action<Pawn> currentAction;

        internal static Dictionary<string, Action> shiftKeyItems;

        static FloatMenuManagerTLCsmall()
        {
            rawItems = new Dictionary<string, Action<Pawn>>();
            usesDefaultJobDriver = new Dictionary<string, bool>();
            currentAction = null;
            shiftKeyItems = new Dictionary<string, Action>();
            //Access Legacy Research Small
            //Log.Message("TLC small before ResearchLegacySmall");
            Add("FloatMenuCaptionLegacyCacheSmall".Translate(), delegate (Pawn p)
            {
                string researchGainedString = "";
                int researchGained = 0;
                //Log.Message("TLC small before Methods.LegacyCacheMenuSmall call");
                string msg = Methods.LegacyCacheMenuSmall();
                msg += "\n";
                //Log.Message("TLC small before UpdateColonyResearchFromLegacySmall call");
                msg += Methods.UpdateColonyResearchFromLegacySmall(p);
                researchGained = Methods.CountColonyResearchFromLegacy(msg, ',') - 1;
                //Log.Message("researchGained from TCLsmall: " + researchGained);
                if (researchGained > 1)
                {
                    researchGainedString = "gained " + researchGained + " research projects from legacy";
                }
                else if (researchGained == 1)
                {
                    researchGainedString = "gained " + researchGained + " research project from legacy";
                }
                else if (researchGained == 0)
                {
                    msg = Methods.LegacyCacheMenuEmtpy();
                }
                //Log.Message("TLCSmall before display diaNodemsg with researchGained: " + researchGained);
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
                //Log.Message("DiaNode TLCsmall done");

            });
        }

        public static void Add(string str, Action<Pawn> action, bool usesDefaultJobDriver = false)
        {
            rawItems.Add(str, action);
            FloatMenuManagerTLCsmall.usesDefaultJobDriver.Add(str, usesDefaultJobDriver);
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
                        Job job = new Job(CoreDefOf.UseTLCsmall, target);
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
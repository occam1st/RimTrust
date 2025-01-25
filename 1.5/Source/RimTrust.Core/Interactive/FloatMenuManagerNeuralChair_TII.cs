using RimTrust.Trade;
using RimTrust.Trade.Ext;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    
    public static class FloatMenuManagerNeuralChair_TII
    {
        internal static Dictionary<string, Action<Pawn>> rawItems;

        internal static Dictionary<string, bool> usesDefaultJobDriver;

        internal static Action<Pawn> currentAction;

        internal static Dictionary<string, Action> shiftKeyItems;

        public static HediffDef NeuralOverdose;
        

        static FloatMenuManagerNeuralChair_TII()
        {
            rawItems = new Dictionary<string, Action<Pawn>>();
            usesDefaultJobDriver = new Dictionary<string, bool>();
            currentAction = null;
            shiftKeyItems = new Dictionary<string, Action>();
            //Legacy skill archive
            Add("FloatMenuCaptionLegacySkills".Translate(), delegate (Pawn p)
            {
                //Log.Message("LegacyArchive Message start");
                string msg = "";
                int index = 0;
                //Log.Message("LegacyArchive Message index " + index);
                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                foreach (string item in initSkillRecord)
                    {
                    //Log.Message("LegacyArchive foreach loop before msg with item + Methods.LegacySkills[index]");
                    msg += item + ": " + Methods.LegacySkills[index] + "\n";
                    //Log.Message("LegacyArchive foreach loop with index " + index);
                    index++;
                    //Log.Message("LegacyArchive foreach loop with index " + index + " after index++");
                }
                msg += "\n\n" + "FloatMenuCaptionTotalLegacySkillArchive".Translate() + " " + Methods.LegacySkills.Sum() + " (" + Math.Round(((double)Methods.LegacySkills.Sum() / (double)3564000) * 100, 4) + "%)";
                msg += "\n\n" + "You can access the knowledge with a Neural Implant, one-hundredth of it at a time.";
                DiaNode diaNode = new DiaNode(msg);
                DiaOption diaOption = new DiaOption("Disconnect".Translate());
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
                Find.WindowStack.Add(dialog_NodeTree);
            }, usesDefaultJobDriver: false);
            //Legacy skill upload
            Add("FloatMenuCaptionNeuralChair".Translate(), delegate (Pawn p)
            {
                //Log.Message("Legacy Skill upload T2");
                string selectedSkill = Methods.SelectSkillToSafeNIC(p);
                //Log.Message("Legacy Skill upload T2 selected skill = " + selectedSkill);
                SkillRecord selectedSkillRecord = Methods.SelectSkillRecordToSafeNIC(p);
                int selectedSkillXP = Methods.SkillXPTotal(selectedSkillRecord);

                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                int index = initSkillRecord.FindIndex(x => x == selectedSkill);
                //Log.Message("Legacy Skill upload T2 before call UpdateLegacySkills");
                Methods.UpdateLegacySkills(selectedSkill, p, 2);
                Methods.SaveLegacySkills();
                Methods.NicFatigueToPawn(p);
            }, usesDefaultJobDriver: true);
            //Legacy research archive
            Add("FloatMenuCaptionLegacyResearchArchive".Translate(), delegate (Pawn p)
                {
                    Methods.LegacyResearchMessage();
                }, usesDefaultJobDriver: false);
            //Legacy research upload
            Add("FloatMenuCaptionLegacyResearchUpload".Translate(), delegate (Pawn p)
                {
                    Methods.UpdateLegacyResearch(p, 2);
                    Methods.SaveLegacyResearch();
                    Methods.NicFatigueToPawn(p);
                }, usesDefaultJobDriver: true);
            //Pawn Skill disabled check
            if (RimTrust.Trade.Methods.debug)
            {
                Add("Pawn Skill check", delegate (Pawn p)
                {
                    int index = 0;
                    foreach (SkillRecord item in p.skills.skills)
                    {
                        //leave log message on, it is linked to debug flag in Methods class
                        //Log.Message("pawn skill  " + item.ToString() + " with level " + item.levelInt + ", skill disabled: " + item.TotallyDisabled);
                        index++;
                    }

                }, usesDefaultJobDriver: false);
            }
        }

        public static void Add(string str, Action<Pawn> action, bool usesDefaultJobDriver = false)
        {
            rawItems.Add(str, action);
            FloatMenuManagerNeuralChair_TII.usesDefaultJobDriver.Add(str, usesDefaultJobDriver);
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
                        Job job = new Job(CoreDefOf.UseNeuralChair_TII, target);
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
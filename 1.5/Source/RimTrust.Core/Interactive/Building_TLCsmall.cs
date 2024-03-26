using RimTrust.Trade;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public class Building_TLCsmall : Building
    {
        private CompPowerTrader powerComp;
        

        public bool CanUseTLCsmallNow
        {
            get
                {
                    return (!base.Spawned || !base.Map.gameConditionManager.ElectricityDisabled(base.Map)) && (this.powerComp == null || this.powerComp.PowerOn);
                }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {

            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            if (Methods.TrustFunds == 0)
            {
                LoadTrustFunds.LoadTrust();
            }
            if (Methods.LegacySkills.Count.Equals(0))
            {
                LoadTrustFunds.LoadLegacySkills();
            }
            if (Methods.LegacyResearch.Count.Equals(0))
            {
                LoadTrustFunds.LoadLegacyResearch();
                //Log.Message("Legacy Research loaded from TLCsmall");
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                return new List<FloatMenuOption>
            {
                item
            };
            }
            if (base.Spawned && base.Map.gameConditionManager.ElectricityDisabled(base.Map))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
                return new List<FloatMenuOption>
            {
                item2
            };
            }
            if (!powerComp.PowerOn)
            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
                return new List<FloatMenuOption>
            {
                item3
            };
            }
            if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Sight.label)), null);
                return new List<FloatMenuOption>
            {
                item4
            };
            }
            return FloatMenuManagerTLCsmall.RequestBuild(this, selPawn);
        }
    }
}
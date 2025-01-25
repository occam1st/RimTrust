using RimTrust.Trade;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public class Building_TrustLedgerConsole : Building
    {
        private CompPowerTrader powerComp;
        

        public bool CanUseTrustLedgerConsoleNow
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
            if(Methods.LegacyResearch.Count.Equals(0))
            {
                LoadTrustFunds.LoadLegacyResearch();
            }
            if (Methods.LegacyPower == 0)
            { 
                LoadTrustFunds.LoadLegacyPower();
            }
            if (Methods.LegacyPower > 0)
            {
                //TO-DO maybe load power comp or basePowerConsumption here
            }
            RimTrust.Core.Patches.Patch_LMWsDeepStorageSafe.Patch_LMWsDeepStorageSafeForBanknotes();
            RimTrust.Core.Patches.Patch_GunsGalore_RunNGunners_Banknotes.Patch_GunsGalore_RunNGunners_Accept_Banknotes();
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
            return FloatMenuManagerTLC.RequestBuild(this, selPawn);
        }
    }
}
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public class JobDriver_UseTLCsmall : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !((Building_TLCsmall)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseTLCsmallNow);
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = base.CurToil.actor;
                    if (((Building_TLCsmall)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseTLCsmallNow)
                    {
                        FloatMenuManagerTLCsmall.currentAction(actor);
                    }
                }
            };
        }
    }
}
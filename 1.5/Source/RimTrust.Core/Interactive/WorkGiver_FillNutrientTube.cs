using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
	public class WorkGiver_FillNutrientTube : WorkGiver_Scanner
	{

		private static string TemperatureTrans;
		public static string NoNutrientsTrans;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(CoreDefOf.NutrientTube);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public static void ResetStaticData()
		{
			WorkGiver_FillNutrientTube.TemperatureTrans = "BadTemperature".Translate().ToLower();
			WorkGiver_FillNutrientTube.NoNutrientsTrans = "NoNutrients".Translate();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			//Log.Message("In WorkGiver_FillNutrientTube HasJobOnThing Start");

			Building_NutrientTube building_NutrientTube = t as Building_NutrientTube;
			if (building_NutrientTube == null || building_NutrientTube.Produced || building_NutrientTube.SpaceLeftForNutrient <= 0)
			{
				//Log.Message("WG_FNT not spawned, producing or empty");
				return false;
			}
			float ambientTemperature = building_NutrientTube.AmbientTemperature;
			CompProperties_TemperatureRuinable compProperties = building_NutrientTube.def.GetCompProperties<CompProperties_TemperatureRuinable>();
			if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
			{
				JobFailReason.Is(WorkGiver_FillNutrientTube.TemperatureTrans, null);
				//Log.Message("WG_FNT bad temperature");
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				//Log.Message("WG_FNT forbidden");
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				//Log.Message("WG_FNT to be deconstructed");
				return false;
			}
			if (this.FindNutrient(pawn, building_NutrientTube) == null)
			{
				//Log.Message("WG_FNT no nutrients");
				JobFailReason.Is(WorkGiver_FillNutrientTube.NoNutrientsTrans, null);
				return false;
			}
			
			return !t.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_NutrientTube tube = (Building_NutrientTube)t;
			Thing t2 = this.FindNutrient(pawn, tube);
			return JobMaker.MakeJob(CoreDefOf.FillNutrientTube, t, t2);
		}

		public Thing FindNutrient(Pawn pawn, Building_NutrientTube tube)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);

			ThingDef Nutrients = ThingDefOf.RawPotatoes;

			if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.RawPotatoes), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false) != null)
			{
				Nutrients = ThingDefOf.RawPotatoes;
			}
			else if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(CoreDefOf.RawRice), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false) != null)
			{
				Nutrients = CoreDefOf.RawRice;
			}
			else
			{
				Nutrients = CoreDefOf.RawCorn;
			}
			//Log.Message("WG_FNT did not find any nutrients");
			//Log.Message("WG_FNT find nutrients return value: " + GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Nutrients), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false));
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Nutrients), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			
		}

		
	}
}


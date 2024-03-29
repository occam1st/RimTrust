﻿using RimWorld;
using Verse;

namespace RimTrust.Core
{
    [DefOf]
    public static class CoreDefOf
    {
        public static JobDef UseTrustLedgerConsole;
        public static ThingDef TrustLedgerConsole;
        public static JobDef UseNeuralChair;
        public static ThingDef NeuralInterfaceChair;
        public static JobDef UseNeuralChair_TII;
        public static ThingDef NeuralInterfaceChair_TII;
        public static ThingDef NutrientTube;
        public static ThingDef MealNutrientSupplementPill;
        public static JobDef FillNutrientTube;
        public static JobDef EmptyNutrientTube;
        public static ThingDef RawRice;
        public static ThingDef RawCorn;
        public static ThingDef Plant_Arborfibre;
        public static ThingDef NeuralImplant;
        public static ThingDef TLCsmall;
        public static JobDef UseTLCsmall;
        public static ThingDef ArtificialLifepod;
        public static JobDef UseArtificialLifepod;

        static CoreDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CoreDefOf));
        }
    }
}  
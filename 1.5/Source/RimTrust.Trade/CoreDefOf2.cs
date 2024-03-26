using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [RimWorld.DefOf]
    public static class CoreDefOf
    {
        public static ThingDef NeuralImplant;

        

        static CoreDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CoreDefOf));
        }
    }
}
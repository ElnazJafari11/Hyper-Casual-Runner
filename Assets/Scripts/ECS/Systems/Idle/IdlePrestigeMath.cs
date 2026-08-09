using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Shared idle prestige/phase conversion + manager-gate reset helpers.
    /// Contract: PassiveRate is raw CPS (BaseCps * OwnedCount); sim applies GlobalMultiplier.
    /// </summary>
    public static class IdlePrestigeMath
    {
        /// <summary>
        /// floor(sqrt(currency/50)); soft floor of 1 when currency &gt;= 25.
        /// Returns 0 below threshold (no prestige grant).
        /// </summary>
        public static double ConvertRunCurrency(double primaryCurrency)
        {
            double converted = System.Math.Floor(System.Math.Sqrt(System.Math.Max(0, primaryCurrency) / 50.0));
            if (converted < 1 && primaryCurrency >= 25) converted = 1;
            return converted < 1 ? 0 : converted;
        }

        public static bool ArchetypeUsesManagerGate(IdleArchetype archetype)
        {
            return archetype == IdleArchetype.AdventureCapitalist
                   || archetype == IdleArchetype.IdleMinerTycoon;
        }

        /// <summary>Raw generator CPS — do not bake GlobalMultiplier.</summary>
        public static double ComputePassiveRate(double baseCps, int ownedCount)
        {
            if (ownedCount <= 0 || baseCps <= 0) return 0;
            return baseCps * ownedCount;
        }

        public static void ResetBuyableGenerator(ref BuyableGenerator gen, IdleArchetype archetype)
        {
            gen.OwnedCount = 0;
            if (ArchetypeUsesManagerGate(archetype))
            {
                gen.RequiresManager = true;
                gen.IsAutomated = false;
            }
            else
            {
                gen.IsAutomated = !gen.RequiresManager;
            }
        }
    }
}

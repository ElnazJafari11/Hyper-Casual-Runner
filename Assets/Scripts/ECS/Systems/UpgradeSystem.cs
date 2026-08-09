using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UpgradeSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Note: In a real architecture, this might be event-driven via UI requests.
            // For now, it manages the math logic behind upgrades.
            
            foreach (var (upgradeNode, entity) in SystemAPI.Query<RefRW<UpgradeNode>>().WithEntityAccess())
            {
                // The cost of the next level is: BaseCost * (CostGrowthFactor ^ CurrentLevel)
                double nextLevelCost = upgradeNode.ValueRO.BaseCost * math.pow(upgradeNode.ValueRO.CostGrowthFactor, upgradeNode.ValueRO.CurrentLevel);
                
                // If UI triggered a buy request (simulated here via a hypothetical component or system event), 
                // we would deduct from PersistentPlayerStats and increment CurrentLevel.
            }
        }
    }
}

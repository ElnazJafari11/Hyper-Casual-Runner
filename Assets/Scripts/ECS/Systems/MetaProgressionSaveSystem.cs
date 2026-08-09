using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct MetaProgressionSaveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (saveEvent, entity) in SystemAPI.Query<RefRO<SaveProgressEventComponent>>().WithEntityAccess())
            {
                if (saveEvent.ValueRO.IsVictory)
                {
                    GameProgressData.SaveLevelCompletion(
                        saveEvent.ValueRO.CompletedLevelIndex,
                        saveEvent.ValueRO.CoinsEarnedInRun,
                        saveEvent.ValueRO.StarsEarned
                    );
                }
                else
                {
                    GameProgressData.Save();
                }

                SystemAPI.SetComponentEnabled<SaveProgressEventComponent>(entity, false);
            }
        }
    }
}

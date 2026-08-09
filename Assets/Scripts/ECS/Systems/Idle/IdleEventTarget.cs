using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Resolves which IdleSliceState an enableable/ephemeral event should mutate.
    /// Prefer explicit TargetSlice; fall back to event-on-slice or a precomputed sole-slice.
    /// Multi-slice worlds without a target refuse to apply (no cross-slice blast).
    /// Call FindSoleSlice BEFORE iterating event queries — CreateEntityQuery mid-foreach is unsafe.
    /// </summary>
    public static class IdleEventTarget
    {
        public static Entity FindSoleSlice(EntityManager em)
        {
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<IdleSliceState>());
            if (q.CalculateEntityCount() == 1)
                return q.GetSingletonEntity();
            return Entity.Null;
        }

        public static Entity Resolve(
            EntityManager em,
            Entity eventEntity,
            Entity targetFromEvent,
            Entity soleSliceFallback = default)
        {
            if (targetFromEvent != Entity.Null &&
                em.Exists(targetFromEvent) &&
                em.HasComponent<IdleSliceState>(targetFromEvent))
            {
                return targetFromEvent;
            }

            if (em.HasComponent<IdleSliceState>(eventEntity))
                return eventEntity;

            if (soleSliceFallback != Entity.Null &&
                em.Exists(soleSliceFallback) &&
                em.HasComponent<IdleSliceState>(soleSliceFallback))
            {
                return soleSliceFallback;
            }

            return Entity.Null;
        }

        public static void SyncPairedRunGold(EntityManager em, Entity sliceEntity, double gold)
        {
            if (sliceEntity == Entity.Null || !em.Exists(sliceEntity)) return;
            if (!em.HasComponent<CurrentRunStats>(sliceEntity)) return;
            var run = em.GetComponentData<CurrentRunStats>(sliceEntity);
            run.CurrentGold = gold;
            em.SetComponentData(sliceEntity, run);
        }
    }
}

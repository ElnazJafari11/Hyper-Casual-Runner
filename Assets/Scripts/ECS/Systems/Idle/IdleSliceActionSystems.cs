using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleAssignWorkerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleAssignWorkerEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleAssignWorkerEvent>>().WithEntityAccess())
            {
                int stationId = evt.ValueRO.StationId;
                int delta = evt.ValueRO.Delta == 0 ? 1 : evt.ValueRO.Delta;

                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    int next = slice.ValueRO.AssignedWorkers + delta;
                    next = System.Math.Max(0, System.Math.Min(slice.ValueRO.MaxWorkers, next));
                    slice.ValueRW.AssignedWorkers = next;
                    if (next > 0 && slice.ValueRO.ProgressionLevel < 1)
                        slice.ValueRW.ProgressionLevel = 1;
                }

                foreach (var station in SystemAPI.Query<RefRW<IdleAssignmentStation>>())
                {
                    if (stationId != 0 && station.ValueRO.StationId != stationId) continue;
                    int assigned = station.ValueRO.AssignedCount + delta;
                    assigned = System.Math.Max(0, System.Math.Min(station.ValueRO.Capacity, assigned));
                    station.ValueRW.AssignedCount = assigned;
                }

                SystemAPI.SetComponentEnabled<IdleAssignWorkerEvent>(entity, false);
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleGachaPullSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleGachaPullEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<IdleGachaPullEvent>>().WithEntityAccess())
            {
                foreach (var (gacha, slice) in SystemAPI.Query<RefRW<IdleGachaState>, RefRW<IdleSliceState>>())
                {
                    double cost = gacha.ValueRO.PullCost <= 0 ? 10.0 : gacha.ValueRO.PullCost;
                    if (slice.ValueRO.PrimaryCurrency < cost) continue;

                    slice.ValueRW.PrimaryCurrency -= cost;
                    gacha.ValueRW.PullCount += 1;

                    // Deterministic rarity ladder (no System.Random in Burst-friendly path)
                    int rarity = 1 + (gacha.ValueRO.PullCount % 5);
                    if (rarity > gacha.ValueRO.BestRarity) gacha.ValueRW.BestRarity = rarity;

                    slice.ValueRW.ClickPower += rarity * 0.5;
                    slice.ValueRW.GlobalMultiplier += rarity * 0.02f;

                    if (gacha.ValueRO.PullCount % 3 == 0)
                    {
                        gacha.ValueRW.Stage += 1;
                        slice.ValueRW.ProgressionLevel = gacha.ValueRO.Stage;
                    }

                    var sfx = ecb.CreateEntity();
                    ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                }

                // Legend of Mushroom / Idle Heroes without separate gacha component
                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>().WithNone<IdleGachaState>())
                {
                    if (slice.ValueRO.Archetype != IdleArchetype.LegendOfMushroom &&
                        slice.ValueRO.Archetype != IdleArchetype.IdleHeroes) continue;

                    double cost = 10.0 + slice.ValueRO.ProgressionLevel * 5;
                    if (slice.ValueRO.PrimaryCurrency < cost) continue;
                    slice.ValueRW.PrimaryCurrency -= cost;
                    slice.ValueRW.ClickPower += 1.0;
                    slice.ValueRW.ProgressionLevel += 1;
                    slice.ValueRW.GlobalMultiplier += 0.05f;
                }

                SystemAPI.SetComponentEnabled<IdleGachaPullEvent>(entity, false);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleNarrativeActionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleNarrativeActionEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleNarrativeActionEvent>>().WithEntityAccess())
            {
                int action = evt.ValueRO.ActionId;

                foreach (var (narr, slice) in SystemAPI.Query<RefRW<IdleNarrativeState>, RefRW<IdleSliceState>>())
                {
                    ApplyNarrative(ref narr.ValueRW, ref slice.ValueRW, action);
                }

                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>().WithNone<IdleNarrativeState>())
                {
                    ApplySliceOnlyNarrative(ref slice.ValueRW, action);
                }

                SystemAPI.SetComponentEnabled<IdleNarrativeActionEvent>(entity, false);
            }
        }

        private static void ApplyNarrative(ref IdleNarrativeState narr, ref IdleSliceState slice, int action)
        {
            switch (action)
            {
                case 0: // stoke / step / place food
                    narr.StokeCount += 1;
                    narr.Wood += 1;
                    slice.PrimaryCurrency += 1 * slice.GlobalMultiplier;
                    if (narr.StokeCount >= 5 && narr.ExploreUnlocked == 0)
                        narr.ExploreUnlocked = 1;
                    break;
                case 1: // explore / advance tile
                    if (narr.ExploreUnlocked == 0 && slice.Archetype == IdleArchetype.ADarkRoom) return;
                    narr.RoomOrStep += 1;
                    slice.ProgressionLevel = narr.RoomOrStep;
                    slice.PrimaryCurrency += 5 * slice.GlobalMultiplier;
                    narr.SoftCurrency += 2;
                    break;
                case 2: // craft / build
                    if (narr.Wood < 3) return;
                    narr.Wood -= 3;
                    slice.PassiveRate += 0.5;
                    slice.GlobalMultiplier += 0.1f;
                    break;
            }
        }

        private static void ApplySliceOnlyNarrative(ref IdleSliceState slice, int action)
        {
            switch (slice.Archetype)
            {
                case IdleArchetype.ADarkRoom:
                    if (action == 0)
                    {
                        slice.OwnedGenerators += 1; // stoke count reuse
                        slice.PrimaryCurrency += 1;
                        if (slice.OwnedGenerators >= 5) slice.ProgressionLevel = System.Math.Max(1, slice.ProgressionLevel);
                    }
                    else if (action == 1 && slice.ProgressionLevel >= 1)
                    {
                        slice.ProgressionLevel += 1;
                        slice.PrimaryCurrency += 5;
                        slice.PassiveRate += 0.25;
                    }
                    break;
                case IdleArchetype.CapybaraGo:
                    slice.ProgressionLevel += 1;
                    slice.PrimaryCurrency += 3 + slice.ProgressionLevel;
                    if (slice.ProgressionLevel % 5 == 0) slice.GlobalMultiplier += 0.1f;
                    break;
                case IdleArchetype.NekoAtsume:
                    // Place food: spend soft currency, attract cats
                    if (slice.PrimaryCurrency >= 5)
                    {
                        slice.PrimaryCurrency -= 5;
                        slice.CheckInCats += 2;
                        slice.HasOfflineClaim = true;
                    }
                    break;
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleAllocateEnergySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleAllocateEnergyEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleAllocateEnergyEvent>>().WithEntityAccess())
            {
                float amount = evt.ValueRO.Amount;
                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    if (slice.ValueRO.Archetype == IdleArchetype.RealmGrinder)
                    {
                        // Re-pick faction: amount 1 or 2
                        slice.ValueRW.FactionId = amount <= 1.5f ? 1 : 2;
                        slice.ValueRW.GlobalMultiplier += 0.15f;
                        slice.ValueRW.ProgressionLevel += 1;
                    }
                    else
                    {
                        float alloc = System.Math.Clamp(amount, 0f, slice.ValueRO.EnergyPool);
                        slice.ValueRW.EnergyAllocated = alloc;
                        if (alloc > 0 && slice.ValueRO.ProgressionLevel < 1)
                            slice.ValueRW.ProgressionLevel = 1;
                    }
                }

                SystemAPI.SetComponentEnabled<IdleAllocateEnergyEvent>(entity, false);
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleClaimOfflineSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleClaimOfflineEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, entity) in SystemAPI.Query<RefRO<IdleClaimOfflineEvent>>().WithEntityAccess())
            {
                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    if (!slice.ValueRO.HasOfflineClaim && slice.ValueRO.AfkChestSeconds < 1f &&
                        slice.ValueRO.CheckInCats <= 0)
                    {
                        // Still grant a small offline sample for demo
                        slice.ValueRW.PrimaryCurrency += 10 * slice.ValueRO.GlobalMultiplier;
                    }
                    else
                    {
                        double reward = slice.ValueRO.AfkChestSeconds * (1.0 + slice.ValueRO.ProgressionLevel) *
                                        slice.ValueRO.GlobalMultiplier;
                        reward += slice.ValueRO.CheckInCats * 5.0;
                        slice.ValueRW.PrimaryCurrency += System.Math.Max(10, reward);
                        slice.ValueRW.AfkChestSeconds = 0f;
                        slice.ValueRW.HasOfflineClaim = false;
                        // Neko: cats linger as collection count
                    }
                }

                SystemAPI.SetComponentEnabled<IdleClaimOfflineEvent>(entity, false);
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdlePhaseShiftSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdlePhaseShiftEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, entity) in SystemAPI.Query<RefRO<IdlePhaseShiftEvent>>().WithEntityAccess())
            {
                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    // Universal Paperclips / Antimatter nested layer: reset run, keep prestige mult
                    double converted = System.Math.Floor(System.Math.Sqrt(System.Math.Max(0, slice.ValueRO.PrimaryCurrency) / 50.0));
                    if (converted < 1 && slice.ValueRO.PrimaryCurrency >= 50) converted = 1;

                    if (converted >= 1 || slice.ValueRO.PrimaryCurrency >= 25)
                    {
                        slice.ValueRW.PrestigeCurrency += System.Math.Max(1, converted);
                        slice.ValueRW.PhaseIndex += 1;
                        slice.ValueRW.ProgressionLevel = slice.ValueRO.PhaseIndex;
                        slice.ValueRW.PrimaryCurrency = 0;
                        slice.ValueRW.OwnedGenerators = 0;
                        slice.ValueRW.PassiveRate = 0;
                        slice.ValueRW.ClickPower = 1 + slice.ValueRO.PrestigeCurrency * 0.5;
                        slice.ValueRW.GlobalMultiplier = 1f + (float)slice.ValueRO.PrestigeCurrency * 0.1f;
                    }
                }

                foreach (var gen in SystemAPI.Query<RefRW<BuyableGenerator>>())
                {
                    gen.ValueRW.OwnedCount = 0;
                    gen.ValueRW.IsAutomated = !gen.ValueRO.RequiresManager;
                }

                SystemAPI.SetComponentEnabled<IdlePhaseShiftEvent>(entity, false);
            }
        }
    }
}

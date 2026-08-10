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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleAssignWorkerEvent>>().WithEntityAccess())
            {
                int stationId = evt.ValueRO.StationId;
                int delta = evt.ValueRO.Delta == 0 ? 1 : evt.ValueRO.Delta;
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    int next = slice.AssignedWorkers + delta;
                    next = System.Math.Max(0, System.Math.Min(slice.MaxWorkers, next));
                    slice.AssignedWorkers = next;
                    if (next > 0 && slice.ProgressionLevel < 1)
                        slice.ProgressionLevel = 1;
                    em.SetComponentData(sliceEntity, slice);

                    if (em.HasComponent<IdleAssignmentStation>(sliceEntity))
                    {
                        var station = em.GetComponentData<IdleAssignmentStation>(sliceEntity);
                        if (stationId == 0 || station.StationId == stationId)
                        {
                            int assigned = station.AssignedCount + delta;
                            assigned = System.Math.Max(0, System.Math.Min(station.Capacity, assigned));
                            station.AssignedCount = assigned;
                            em.SetComponentData(sliceEntity, station);
                        }
                    }
                }

                SystemAPI.SetComponentEnabled<IdleAssignWorkerEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
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
            var em = state.EntityManager;
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleGachaPullEvent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);

                    if (em.HasComponent<IdleGachaState>(sliceEntity))
                    {
                        var gacha = em.GetComponentData<IdleGachaState>(sliceEntity);
                        if (TryApplyPull(ref gacha, ref slice, out int rarity))
                        {
                            if (em.HasComponent<IdleCombatState>(sliceEntity))
                            {
                                var combat = em.GetComponentData<IdleCombatState>(sliceEntity);

                                // Idle Heroes: gacha must raise auto-combat DPS, not only ClickPower/mult
                                if (slice.Archetype == IdleArchetype.IdleHeroes)
                                {
                                    double dpsGain = rarity * 0.5;
                                    slice.PassiveRate += dpsGain;
                                    combat.HeroDps += dpsGain;
                                    combat.TapDamage = slice.ClickPower;
                                }

                                // One progression axis: Stage must never smash combat Zone on ProgressionLevel.
                                slice.ProgressionLevel = System.Math.Max(slice.ProgressionLevel, combat.Zone);
                                em.SetComponentData(sliceEntity, combat);
                            }

                            em.SetComponentData(sliceEntity, gacha);
                            em.SetComponentData(sliceEntity, slice);

                            var sfx = ecb.CreateEntity();
                            ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                    }
                    else if (slice.Archetype == IdleArchetype.LegendOfMushroom ||
                             slice.Archetype == IdleArchetype.IdleHeroes)
                    {
                        double cost = 10.0 + slice.ProgressionLevel * 5;
                        if (slice.PrimaryCurrency >= cost)
                        {
                            slice.PrimaryCurrency -= cost;
                            slice.ClickPower += 1.0;
                            slice.ProgressionLevel += 1;
                            slice.GlobalMultiplier += 0.05f;
                            if (slice.Archetype == IdleArchetype.IdleHeroes)
                                slice.PassiveRate += 0.5;
                            em.SetComponentData(sliceEntity, slice);
                        }
                    }

                    IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                }

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        /// <summary>Shared pull math for manual events and LoM auto-lamp.</summary>
        public static bool TryApplyPull(ref IdleGachaState gacha, ref IdleSliceState slice, out int rarity)
        {
            rarity = 0;
            double cost = gacha.PullCost <= 0 ? 10.0 : gacha.PullCost;
            if (slice.PrimaryCurrency < cost) return false;

            // LoM stage-unlocked lamp loot: refund cost+1 so Farm click is optional after Stage>=1.
            bool lomLampLoot = slice.Archetype == IdleArchetype.LegendOfMushroom && gacha.Stage >= 1;

            slice.PrimaryCurrency -= cost;
            gacha.PullCount += 1;

            rarity = 1 + (gacha.PullCount % 5);
            if (rarity > gacha.BestRarity) gacha.BestRarity = rarity;

            slice.ClickPower += rarity * 0.5;
            slice.GlobalMultiplier += rarity * 0.02f;

            if (gacha.PullCount % 3 == 0)
            {
                gacha.Stage += 1;
                // Max — never overwrite a deeper combat Zone already mirrored into ProgressionLevel.
                slice.ProgressionLevel = System.Math.Max(slice.ProgressionLevel, gacha.Stage);
            }

            if (lomLampLoot)
                slice.PrimaryCurrency += cost + 1;

            return true;
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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleNarrativeActionEvent>>().WithEntityAccess())
            {
                int action = evt.ValueRO.ActionId;
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    if (em.HasComponent<IdleNarrativeState>(sliceEntity))
                    {
                        var narr = em.GetComponentData<IdleNarrativeState>(sliceEntity);
                        ApplyNarrative(ref narr, ref slice, action);
                        em.SetComponentData(sliceEntity, narr);
                    }
                    else
                    {
                        ApplySliceOnlyNarrative(ref slice, action);
                    }

                    em.SetComponentData(sliceEntity, slice);
                    IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                }

                SystemAPI.SetComponentEnabled<IdleNarrativeActionEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        private static void ApplyNarrative(ref IdleNarrativeState narr, ref IdleSliceState slice, int action)
        {
            switch (action)
            {
                case 0: // stoke / step / place food
                    narr.StokeCount += 1;
                    narr.Wood += 1;
                    slice.PrimaryCurrency += 1 * slice.GlobalMultiplier;
                    if (narr.ExploreUnlocked == 0)
                    {
                        // Capybara: one earned step unlocks advance. ADR: 5 stokes.
                        int need = slice.Archetype == IdleArchetype.CapybaraGo ? 1 : 5;
                        if (narr.StokeCount >= need)
                            narr.ExploreUnlocked = 1;
                    }
                    break;
                case 1: // explore / advance tile
                    TryAdvanceStep(ref narr, ref slice);
                    break;
                case 2: // craft / build
                    if (narr.Wood < 3) return;
                    narr.Wood -= 3;
                    slice.PassiveRate += 0.5;
                    slice.GlobalMultiplier += 0.1f;
                    break;
            }
        }

        /// <summary>Shared tile/step advance for manual narrative + Capybara auto-tiles.</summary>
        public static bool TryAdvanceStep(ref IdleNarrativeState narr, ref IdleSliceState slice)
        {
            if (narr.ExploreUnlocked == 0 &&
                (slice.Archetype == IdleArchetype.ADarkRoom ||
                 slice.Archetype == IdleArchetype.CapybaraGo))
                return false;

            narr.RoomOrStep += 1;
            slice.ProgressionLevel = narr.RoomOrStep;
            slice.PrimaryCurrency += 5 * slice.GlobalMultiplier;
            narr.SoftCurrency += 2;
            // Capybara live bootstrap path uses IdleNarrativeState — milestone must land here
            if (slice.Archetype == IdleArchetype.CapybaraGo && narr.RoomOrStep % 5 == 0)
                slice.GlobalMultiplier += 0.1f;
            return true;
        }

        private static void ApplySliceOnlyNarrative(ref IdleSliceState slice, int action)
        {
            switch (slice.Archetype)
            {
                case IdleArchetype.ADarkRoom:
                    if (action == 0)
                    {
                        slice.OwnedGenerators += 1;
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
                    // 0 = Place Food, 1 = Place Toys (matrix Food/Toys)
                    if (action == 0 && slice.PrimaryCurrency >= 5)
                    {
                        slice.PrimaryCurrency -= 5;
                        slice.CheckInCats += 2;
                        slice.HasOfflineClaim = true;
                    }
                    else if (action == 1 && slice.PrimaryCurrency >= 8)
                    {
                        slice.PrimaryCurrency -= 8;
                        slice.CheckInCats += 3;
                        slice.ProgressionLevel = System.Math.Max(1, slice.ProgressionLevel);
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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleAllocateEnergyEvent>>().WithEntityAccess())
            {
                float amount = evt.ValueRO.Amount;
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    // RG Align uses IdleFactionAlignEvent — energy alloc is NGU (and similar) only.
                    float alloc = System.Math.Clamp(amount, 0f, slice.EnergyPool);
                    slice.EnergyAllocated = alloc;
                    if (alloc > 0 && slice.ProgressionLevel < 1)
                        slice.ProgressionLevel = 1;

                    em.SetComponentData(sliceEntity, slice);
                }

                SystemAPI.SetComponentEnabled<IdleAllocateEnergyEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Realm Grinder Align: set FactionId only. No free Mult/Level — income path uses factionBonus in sim.
    /// First Align (neutral→1/2) is free; re-pick flip costs <see cref="IdlePrestigeMath.RealmGrinderAlignFlipCost"/>.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleFactionAlignSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleFactionAlignEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleFactionAlignEvent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);
                int faction = evt.ValueRO.FactionId;

                if (sliceEntity != Entity.Null &&
                    em.HasComponent<IdleSliceState>(sliceEntity) &&
                    (faction == 1 || faction == 2))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    if (slice.Archetype == IdleArchetype.RealmGrinder)
                    {
                        int prior = slice.FactionId;
                        if (prior == faction)
                        {
                            // Same faction — honest no-op (no charge).
                        }
                        else if (prior != 0 &&
                                 slice.PrimaryCurrency < IdlePrestigeMath.RealmGrinderAlignFlipCost)
                        {
                            // Re-pick without funds — honest no-op.
                        }
                        else
                        {
                            if (prior != 0)
                                slice.PrimaryCurrency -= IdlePrestigeMath.RealmGrinderAlignFlipCost;
                            slice.FactionId = faction;
                            em.SetComponentData(sliceEntity, slice);
                            IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                        }
                    }
                }

                SystemAPI.SetComponentEnabled<IdleFactionAlignEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdleClaimOfflineEvent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    double pending = slice.PendingClaim;
                    // IH/AfkArena: chest gate matches sim (≥10s / HasOfflineClaim). Neko cats + PendingClaim stay separate.
                    bool ihOrAfk = slice.Archetype == IdleArchetype.IdleHeroes ||
                                   slice.Archetype == IdleArchetype.AfkArena;
                    bool chestReady = ihOrAfk
                        ? (slice.HasOfflineClaim || slice.AfkChestSeconds >= 10f)
                        : (slice.AfkChestSeconds >= 1f);
                    bool hasClaim = slice.HasOfflineClaim || pending > 0 ||
                                    chestReady || slice.CheckInCats > 0;

                    if (!hasClaim)
                    {
                        // Honest no-op: no demo free grant
                        SystemAPI.SetComponentEnabled<IdleClaimOfflineEvent>(entity, false);
                        ecb.DestroyEntity(entity);
                        continue;
                    }

                    // D16: mutually exclusive — PendingClaim OR chest/cats, never both in one claim.
                    if (pending > 0)
                    {
                        slice.PrimaryCurrency += pending;
                        slice.PendingClaim = 0;
                    }
                    else
                    {
                        double reward = slice.AfkChestSeconds * (1.0 + slice.ProgressionLevel) *
                                        slice.GlobalMultiplier;
                        reward += slice.CheckInCats * 5.0;
                        if (reward > 0)
                            slice.PrimaryCurrency += reward;

                        slice.AfkChestSeconds = 0f;
                        slice.CheckInCats = 0;
                    }

                    float chestGate = ihOrAfk ? 10f : 1f;
                    slice.HasOfflineClaim = slice.PendingClaim > 0 ||
                                           slice.AfkChestSeconds >= chestGate ||
                                           slice.CheckInCats > 0;
                    em.SetComponentData(sliceEntity, slice);
                    IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                }

                SystemAPI.SetComponentEnabled<IdleClaimOfflineEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<IdlePhaseShiftEvent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);

                    // Phase shift is Paperclips / Antimatter only (hard prestige uses PrestigeSystem).
                    // AD: PhaseIndex drives named bands via IdlePrestigeMath.GetAntimatterPhaseBand (MVP honesty).
                    // TODO: [STUB] Multi-Dim buyable tiers (Dim2/Dim3) deferred — single Dim + named bands only.
                    if (slice.Archetype == IdleArchetype.UniversalPaperclips ||
                        slice.Archetype == IdleArchetype.AntimatterDimensions)
                    {
                        double converted = IdlePrestigeMath.ConvertRunCurrency(slice.PrimaryCurrency);
                        if (converted >= 1)
                        {
                            slice.PrestigeCurrency += converted;
                            slice.PhaseIndex += 1;
                            slice.ProgressionLevel = slice.PhaseIndex;
                            slice.PrimaryCurrency = 0;
                            slice.OwnedGenerators = 0;
                            slice.PassiveRate = 0;
                            slice.PendingClaim = 0;
                            slice.ManagersHired = 0;
                            slice.ClickPower = 1 + slice.PrestigeCurrency * 0.5;
                            slice.GlobalMultiplier = 1f + (float)slice.PrestigeCurrency * 0.1f;
                            em.SetComponentData(sliceEntity, slice);

                            if (em.HasComponent<PersistentPlayerStats>(sliceEntity))
                            {
                                var stats = em.GetComponentData<PersistentPlayerStats>(sliceEntity);
                                stats.PrestigeCurrency = slice.PrestigeCurrency;
                                em.SetComponentData(sliceEntity, stats);
                            }

                            if (em.HasComponent<BuyableGenerator>(sliceEntity))
                            {
                                var gen = em.GetComponentData<BuyableGenerator>(sliceEntity);
                                IdlePrestigeMath.ResetBuyableGenerator(ref gen, slice.Archetype);
                                em.SetComponentData(sliceEntity, gen);
                            }

                            if (em.HasComponent<IdleManager>(sliceEntity))
                            {
                                var manager = em.GetComponentData<IdleManager>(sliceEntity);
                                manager.IsHired = false;
                                em.SetComponentData(sliceEntity, manager);
                            }

                            IdleEventTarget.SyncPairedRunGold(em, sliceEntity, 0);
                        }
                    }
                }

                SystemAPI.SetComponentEnabled<IdlePhaseShiftEvent>(entity, false);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }
    }
}

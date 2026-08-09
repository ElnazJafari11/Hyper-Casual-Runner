using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    /// <summary>
    /// Runtime bootstrap for idle MVP slices (no SubScene required).
    /// Spawns IdleSliceState + archetype-specific helpers into the default world.
    /// </summary>
    public class IdleSliceBootstrap : MonoBehaviour
    {
        public IdleArchetype Archetype = IdleArchetype.CookieClicker;
        public string DisplayName = "Idle Slice";
        [TextArea] public string HowToPlay = "Click → buy → prestige.";

        public double StartingCurrency = 0;
        public double ClickPower = 1;
        public double GeneratorBaseCost = 15;
        public float GeneratorCostGrowth = 1.15f;
        public double GeneratorBaseCps = 1;
        public bool GeneratorRequiresManager = false;
        public double ManagerHireCost = 100;
        public int MaxWorkers = 5;
        public double PullCost = 10;

        private Entity _sliceEntity;
        private bool _spawned;

        private void Start()
        {
            TrySpawn();
        }

        private void Update()
        {
            if (!_spawned) TrySpawn();
        }

        private void TrySpawn()
        {
            if (_spawned) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            _sliceEntity = em.CreateEntity();
            em.AddComponentData(_sliceEntity, BuildInitialState());
            em.AddComponentData(_sliceEntity, new IdleSliceTag { Archetype = Archetype });
            em.AddBuffer<ResourceWallet>(_sliceEntity);

            em.AddComponentData(_sliceEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 0,
                PermanentDamageMultiplier = 1f,
                PermanentGoldMultiplier = 1f
            });
            em.AddComponentData(_sliceEntity, new CurrentRunStats
            {
                CurrentDistance = 0,
                CurrentGold = StartingCurrency,
                BaseDamage = (float)ClickPower
            });
            em.AddComponent<PrestigeEventComponent>(_sliceEntity);
            em.SetComponentEnabled<PrestigeEventComponent>(_sliceEntity, false);

            AttachArchetypeExtras(em, _sliceEntity);
            _spawned = true;
        }

        private IdleSliceState BuildInitialState()
        {
            var state = new IdleSliceState
            {
                Archetype = Archetype,
                PrimaryCurrency = StartingCurrency,
                PrestigeCurrency = 0,
                GlobalMultiplier = 1f,
                ProgressionLevel = 0,
                Timer = 0f,
                ClickPower = ClickPower,
                PassiveRate = 0,
                OwnedGenerators = 0,
                ManagersHired = 0,
                AssignedWorkers = 0,
                MaxWorkers = MaxWorkers,
                EnemyHp = 0,
                EnemyMaxHp = 0,
                AfkChestSeconds = 0,
                PhaseIndex = 0,
                FactionId = 0,
                EnergyPool = 50f,
                EnergyAllocated = 0,
                SkillXp = 0,
                CheckInCats = 0,
                HasOfflineClaim = false
            };

            switch (Archetype)
            {
                case IdleArchetype.AdventureCapitalist:
                    state.PassiveRate = 0;
                    break;
                case IdleArchetype.ClickerHeroes:
                case IdleArchetype.TapTitans2:
                case IdleArchetype.IdleHeroes:
                    state.EnemyMaxHp = 20;
                    state.EnemyHp = 20;
                    state.PassiveRate = Archetype == IdleArchetype.IdleHeroes ? 2 : 0.5;
                    break;
                case IdleArchetype.MelvorIdle:
                    state.PassiveRate = 0.5;
                    break;
                case IdleArchetype.NekoAtsume:
                    state.PrimaryCurrency = System.Math.Max(StartingCurrency, 20);
                    break;
                case IdleArchetype.AfkArena:
                    state.PassiveRate = 1;
                    break;
            }

            return state;
        }

        private void AttachArchetypeExtras(EntityManager em, Entity slice)
        {
            switch (Archetype)
            {
                case IdleArchetype.CookieClicker:
                case IdleArchetype.AdventureCapitalist:
                case IdleArchetype.AntimatterDimensions:
                case IdleArchetype.UniversalPaperclips:
                case IdleArchetype.EggInc:
                case IdleArchetype.IdleMinerTycoon:
                case IdleArchetype.RealmGrinder:
                    em.AddComponentData(slice, new BuyableGenerator
                    {
                        GeneratorId = 1,
                        OwnedCount = 0,
                        BaseCost = GeneratorBaseCost,
                        CostGrowth = GeneratorCostGrowth,
                        BaseCps = GeneratorBaseCps,
                        RequiresManager = GeneratorRequiresManager || Archetype == IdleArchetype.AdventureCapitalist,
                        IsAutomated = !(GeneratorRequiresManager || Archetype == IdleArchetype.AdventureCapitalist)
                    });
                    if (Archetype == IdleArchetype.AdventureCapitalist)
                    {
                        em.AddComponentData(slice, new IdleManager
                        {
                            TargetGeneratorId = 1,
                            HireCost = ManagerHireCost,
                            IsHired = false
                        });
                    }
                    break;

                case IdleArchetype.ClickerHeroes:
                case IdleArchetype.TapTitans2:
                    em.AddComponentData(slice, new IdleCombatState
                    {
                        TapDamage = ClickPower,
                        HeroDps = 1,
                        Zone = 1,
                        GoldPerKill = 5,
                        EnemyHp = 20,
                        EnemyMaxHp = 20
                    });
                    break;

                case IdleArchetype.IdleHeroes:
                    em.AddComponentData(slice, new IdleCombatState
                    {
                        TapDamage = ClickPower,
                        HeroDps = 3,
                        Zone = 1,
                        GoldPerKill = 5,
                        EnemyHp = 20,
                        EnemyMaxHp = 20
                    });
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = 0,
                        PullCost = PullCost,
                        BestRarity = 0,
                        Stage = 0
                    });
                    break;

                case IdleArchetype.CatsAndSoup:
                case IdleArchetype.FalloutShelter:
                    em.AddComponentData(slice, new IdleAssignmentStation
                    {
                        StationId = 1,
                        AssignedCount = 0,
                        Capacity = MaxWorkers,
                        OutputPerWorker = 1.5,
                        Interval = 1f,
                        Timer = 0f
                    });
                    break;

                case IdleArchetype.MelvorIdle:
                    em.AddComponentData(slice, new IdleSkillNode
                    {
                        SkillId = 1,
                        Level = 1,
                        Xp = 0,
                        XpToLevel = 25,
                        TickInterval = 1f,
                        Timer = 0f,
                        IsActive = true
                    });
                    break;

                case IdleArchetype.ADarkRoom:
                case IdleArchetype.CapybaraGo:
                    em.AddComponentData(slice, new IdleNarrativeState
                    {
                        RoomOrStep = 0,
                        StokeCount = 0,
                        ExploreUnlocked = Archetype == IdleArchetype.CapybaraGo ? 1 : 0,
                        Wood = 0,
                        SoftCurrency = 0
                    });
                    break;

                case IdleArchetype.LegendOfMushroom:
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = 0,
                        PullCost = PullCost,
                        BestRarity = 0,
                        Stage = 0
                    });
                    break;
            }
        }

        private void OnDestroy()
        {
            // TODO: [STUB] entity cleanup on domain reload / destroy
        }
    }
}

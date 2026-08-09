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
        public bool LoadPersistedProgress = true;

        private Entity _sliceEntity;
        private bool _spawned;
        private float _saveTimer;

        private void Start()
        {
            TrySpawn();
        }

        private void Update()
        {
            if (!_spawned) TrySpawn();
            else
            {
                _saveTimer += Time.unscaledDeltaTime;
                if (_saveTimer >= 2f)
                {
                    _saveTimer = 0f;
                    PersistNow();
                }
            }
        }

        private void TrySpawn()
        {
            if (_spawned) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            _sliceEntity = em.CreateEntity();
            var initial = BuildInitialState();
            em.AddComponentData(_sliceEntity, initial);
            em.AddComponentData(_sliceEntity, new IdleSliceTag { Archetype = Archetype });
            em.AddBuffer<ResourceWallet>(_sliceEntity);

            em.AddComponentData(_sliceEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = initial.PrestigeCurrency,
                PermanentDamageMultiplier = 1f,
                PermanentGoldMultiplier = 1f
            });
            em.AddComponentData(_sliceEntity, new CurrentRunStats
            {
                CurrentDistance = 0,
                CurrentGold = initial.PrimaryCurrency,
                BaseDamage = (float)initial.ClickPower
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

            if (LoadPersistedProgress &&
                GameProgressData.TryLoadIdleSlice(
                    (int)Archetype,
                    out var primary,
                    out var prestige,
                    out var mult,
                    out var level,
                    out var click,
                    out var passive,
                    out var gens))
            {
                state.PrimaryCurrency = primary;
                state.PrestigeCurrency = prestige;
                state.GlobalMultiplier = mult > 0f ? mult : 1f;
                state.ProgressionLevel = level;
                state.ClickPower = click > 0 ? click : state.ClickPower;
                state.PassiveRate = passive;
                state.OwnedGenerators = gens;
            }

            return state;
        }

        public void PersistNow()
        {
            if (!_spawned) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            var em = world.EntityManager;
            if (!em.Exists(_sliceEntity) || !em.HasComponent<IdleSliceState>(_sliceEntity)) return;

            var s = em.GetComponentData<IdleSliceState>(_sliceEntity);
            GameProgressData.SaveIdleSlice(
                (int)s.Archetype,
                s.PrimaryCurrency,
                s.PrestigeCurrency,
                s.GlobalMultiplier,
                s.ProgressionLevel,
                s.ClickPower,
                s.PassiveRate,
                s.OwnedGenerators);
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
                    if (Archetype == IdleArchetype.AdventureCapitalist ||
                        Archetype == IdleArchetype.IdleMinerTycoon)
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
                        HeroDps = System.Math.Max(1.0, statePassiveFromEntity(em, slice)),
                        Zone = 1,
                        GoldPerKill = 5,
                        EnemyHp = 20,
                        EnemyMaxHp = 20
                    });
                    em.AddComponentData(slice, new BuyableGenerator
                    {
                        GeneratorId = 1,
                        OwnedCount = 0,
                        BaseCost = 20,
                        CostGrowth = 1.2f,
                        BaseCps = 1,
                        RequiresManager = false,
                        IsAutomated = true
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

        private static double statePassiveFromEntity(EntityManager em, Entity slice)
        {
            if (em.HasComponent<IdleSliceState>(slice))
                return System.Math.Max(1, em.GetComponentData<IdleSliceState>(slice).PassiveRate);
            return 1;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) PersistNow();
        }

        private void OnApplicationQuit()
        {
            PersistNow();
        }

        private void OnDestroy()
        {
            PersistNow();
            // TODO: [STUB] entity cleanup on domain reload / destroy
        }
    }
}

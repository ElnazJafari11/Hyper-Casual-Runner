using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

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
        private bool _destroyed;
        private float _saveTimer;
        private int _loadedGens;
        private bool _loadedManagerHired;
        private int _loadedManagersHired;
        private int _loadedPhase;
        private int _loadedFaction;
        private float _loadedEnergy;
        private bool _loadedFromPrefs;
        private int _loadedGachaStage;
        private int _loadedGachaPullCount;
        private int _loadedGachaBestRarity;
        private int _loadedNarrRoomOrStep;
        private int _loadedNarrExploreUnlocked;
        private int _loadedNarrStokeCount;
        private double _loadedNarrSoftCurrency;
        private double _loadedNarrWood;
        private float _loadedAfkChestSeconds;

        public Entity SliceEntity => _sliceEntity;
        public bool IsSpawned => _spawned && !_destroyed;

        private void Start()
        {
            TrySpawn();
        }

        private void Update()
        {
            if (_destroyed) return;
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
            if (_spawned || _destroyed) return;
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
            em.AddComponentData(_sliceEntity, new PrestigeEventComponent { TargetSlice = _sliceEntity });
            em.SetComponentEnabled<PrestigeEventComponent>(_sliceEntity, false);

            AttachArchetypeExtras(em, _sliceEntity, initial);

            // D25: CatchUp AFTER SyncGeneratorOwnedCountFromState (inside AttachArchetypeExtras)
            // so offline grants use raw PassiveRate, not stale Mult² prefs.
            // D23: ApplyPersistedElapsed stamps LastIdleUpdateTime only when grant > 0.
            // D33: Melvor CatchUp mutates slice Level/XP; rewrite IdleSkillNode to match.
            if (_loadedFromPrefs)
            {
                var st = em.GetComponentData<IdleSliceState>(_sliceEntity);
                IdleOfflineCatchUp.ApplyPersistedElapsed(ref st);
                em.SetComponentData(_sliceEntity, st);
                if (st.Archetype == IdleArchetype.MelvorIdle && em.HasComponent<IdleSkillNode>(_sliceEntity))
                {
                    var skill = em.GetComponentData<IdleSkillNode>(_sliceEntity);
                    IdleOfflineCatchUp.SyncSkillNodeFromSlice(ref skill, in st);
                    em.SetComponentData(_sliceEntity, skill);
                }
            }

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
                HasOfflineClaim = false,
                PendingClaim = 0
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
                    // Matches IdleSkillNode: 1 currency / TickInterval (1s).
                    state.PassiveRate = 1.0;
                    break;
                case IdleArchetype.NekoAtsume:
                    state.PrimaryCurrency = System.Math.Max(StartingCurrency, 20);
                    break;
                case IdleArchetype.AfkArena:
                    state.PassiveRate = 1;
                    break;
            }

            _loadedGens = 0;
            _loadedManagerHired = false;
            _loadedManagersHired = 0;
            _loadedPhase = 0;
            _loadedFaction = 0;
            _loadedEnergy = 0f;
            _loadedFromPrefs = false;
            _loadedGachaStage = 0;
            _loadedGachaPullCount = 0;
            _loadedGachaBestRarity = 0;
            _loadedNarrRoomOrStep = 0;
            _loadedNarrExploreUnlocked = 0;
            _loadedNarrStokeCount = 0;
            _loadedNarrSoftCurrency = 0;
            _loadedNarrWood = 0;
            _loadedAfkChestSeconds = 0f;

            if (LoadPersistedProgress &&
                GameProgressData.TryLoadIdleSlice(
                    (int)Archetype,
                    out var primary,
                    out var prestige,
                    out var mult,
                    out var level,
                    out var click,
                    out var passive,
                    out var gens,
                    out var managers,
                    out var phase,
                    out var faction,
                    out var energy,
                    out var mgrHired,
                    out var energyPool,
                    out var pendingClaim,
                    out var hasOfflineClaim,
                    out var gachaStage,
                    out var gachaPulls,
                    out var gachaRarity,
                    out var narrStep,
                    out var narrExplore,
                    out var narrSoft,
                    out var afkChest))
            {
                state.PrimaryCurrency = primary;
                state.PrestigeCurrency = prestige;
                state.GlobalMultiplier = mult > 0f ? mult : 1f;
                state.ProgressionLevel = level;
                state.ClickPower = click > 0 ? click : state.ClickPower;
                state.PassiveRate = passive;
                state.OwnedGenerators = gens;
                state.ManagersHired = managers;
                state.PhaseIndex = phase;
                state.FactionId = faction;
                state.EnergyAllocated = energy;
                state.EnergyPool = energyPool > 0f ? energyPool : state.EnergyPool;
                // Melvor/Fallout claim-bank must survive PersistNow after catch-up stamps time.
                state.PendingClaim = pendingClaim;
                state.HasOfflineClaim = hasOfflineClaim || pendingClaim > 0.5;
                state.AfkChestSeconds = afkChest;
                var cozy = GameProgressData.LoadIdleCozyPersist((int)Archetype);
                state.AssignedWorkers = System.Math.Clamp(cozy.AssignedWorkers, 0, state.MaxWorkers);
                state.CheckInCats = System.Math.Max(0, cozy.CheckInCats);
                if (state.CheckInCats > 0) state.HasOfflineClaim = true;
                _loadedGens = gens;
                _loadedManagersHired = managers;
                _loadedPhase = phase;
                _loadedFaction = faction;
                _loadedEnergy = energy;
                _loadedManagerHired = mgrHired;
                _loadedGachaStage = gachaStage;
                _loadedGachaPullCount = gachaPulls;
                _loadedGachaBestRarity = gachaRarity;
                _loadedNarrRoomOrStep = narrStep;
                _loadedNarrExploreUnlocked = narrExplore;
                _loadedNarrSoftCurrency = narrSoft;
                _loadedNarrStokeCount = cozy.NarrStokeCount;
                _loadedNarrWood = cozy.NarrWood;
                _loadedAfkChestSeconds = afkChest;
                // Catch-up deferred until after AttachArchetypeExtras PassiveRate sync (D25).
                _loadedFromPrefs = true;
            }

            return state;
        }

        public void PersistNow()
        {
            if (!_spawned || _destroyed) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            var em = world.EntityManager;
            if (!em.Exists(_sliceEntity) || !em.HasComponent<IdleSliceState>(_sliceEntity)) return;

            var s = em.GetComponentData<IdleSliceState>(_sliceEntity);
            bool mgrHired = em.HasComponent<IdleManager>(_sliceEntity) &&
                            em.GetComponentData<IdleManager>(_sliceEntity).IsHired;

            // Keep dual ledger synced on save
            if (em.HasComponent<PersistentPlayerStats>(_sliceEntity))
            {
                var stats = em.GetComponentData<PersistentPlayerStats>(_sliceEntity);
                stats.PrestigeCurrency = s.PrestigeCurrency;
                em.SetComponentData(_sliceEntity, stats);
            }

            int gachaStage = 0, gachaPulls = 0, gachaRarity = 0;
            int narrStep = 0, narrExplore = 0, narrStoke = 0;
            double narrSoft = 0, narrWood = 0;
            if (em.HasComponent<IdleGachaState>(_sliceEntity))
            {
                var g = em.GetComponentData<IdleGachaState>(_sliceEntity);
                gachaStage = g.Stage;
                gachaPulls = g.PullCount;
                gachaRarity = g.BestRarity;
            }
            if (em.HasComponent<IdleNarrativeState>(_sliceEntity))
            {
                var n = em.GetComponentData<IdleNarrativeState>(_sliceEntity);
                narrStep = n.RoomOrStep;
                narrExplore = n.ExploreUnlocked;
                narrSoft = n.SoftCurrency;
                narrStoke = n.StokeCount;
                narrWood = n.Wood;
            }

            GameProgressData.SaveIdleSlice(
                (int)s.Archetype,
                s.PrimaryCurrency,
                s.PrestigeCurrency,
                s.GlobalMultiplier,
                s.ProgressionLevel,
                s.ClickPower,
                s.PassiveRate,
                s.OwnedGenerators,
                s.ManagersHired,
                s.PhaseIndex,
                s.FactionId,
                s.EnergyAllocated,
                mgrHired,
                s.EnergyPool,
                s.PendingClaim,
                s.HasOfflineClaim,
                gachaStage,
                gachaPulls,
                gachaRarity,
                narrStep,
                narrExplore,
                narrSoft,
                s.AfkChestSeconds,
                s.AssignedWorkers,
                s.CheckInCats,
                narrStoke,
                narrWood);
        }

        private void AttachArchetypeExtras(EntityManager em, Entity slice, IdleSliceState initial)
        {
            switch (Archetype)
            {
                case IdleArchetype.CookieClicker:
                case IdleArchetype.AdventureCapitalist:
                // TODO: [STUB] Antimatter multi-Dim tiers (Dim2/Dim3 buyables) deferred —
                // MVP uses one BuyableGenerator + named PhaseIndex bands (GetAntimatterPhaseBand).
                case IdleArchetype.AntimatterDimensions:
                case IdleArchetype.UniversalPaperclips:
                case IdleArchetype.EggInc:
                case IdleArchetype.IdleMinerTycoon:
                case IdleArchetype.RealmGrinder:
                {
                    bool needsManager = GeneratorRequiresManager
                        || Archetype == IdleArchetype.AdventureCapitalist
                        || Archetype == IdleArchetype.IdleMinerTycoon;
                    // Keep RequiresManager as historical gate even when previously hired;
                    // IsAutomated carries the live unlock (prestige clears it via IdlePrestigeMath).
                    bool automated = !needsManager || _loadedManagerHired;
                    var gen = new BuyableGenerator
                    {
                        GeneratorId = 1,
                        OwnedCount = 0,
                        BaseCost = GeneratorBaseCost,
                        CostGrowth = GeneratorCostGrowth,
                        BaseCps = GeneratorBaseCps,
                        RequiresManager = needsManager,
                        IsAutomated = automated
                    };
                    var st = initial;
                    IdlePrestigeMath.SyncGeneratorOwnedCountFromState(
                        ref gen, ref st, _loadedGens, automated);
                    em.AddComponentData(slice, gen);
                    em.SetComponentData(slice, st);
                    if (Archetype == IdleArchetype.AdventureCapitalist ||
                        Archetype == IdleArchetype.IdleMinerTycoon)
                    {
                        em.AddComponentData(slice, new IdleManager
                        {
                            TargetGeneratorId = 1,
                            HireCost = ManagerHireCost,
                            IsHired = _loadedManagerHired
                        });
                    }
                    break;
                }

                case IdleArchetype.ClickerHeroes:
                case IdleArchetype.TapTitans2:
                    em.AddComponentData(slice, new IdleCombatState
                    {
                        TapDamage = ClickPower,
                        HeroDps = System.Math.Max(1.0, statePassiveFromEntity(em, slice)),
                        Zone = System.Math.Max(1, initial.ProgressionLevel),
                        GoldPerKill = 5,
                        EnemyHp = 20,
                        EnemyMaxHp = 20
                    });
                    em.AddComponentData(slice, new BuyableGenerator
                    {
                        GeneratorId = 1,
                        OwnedCount = _loadedGens,
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
                        Zone = System.Math.Max(1, initial.ProgressionLevel),
                        GoldPerKill = 5,
                        EnemyHp = 20,
                        EnemyMaxHp = 20
                    });
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = _loadedGachaPullCount,
                        PullCost = PullCost,
                        BestRarity = _loadedGachaBestRarity,
                        Stage = _loadedGachaStage
                    });
                    break;

                case IdleArchetype.CatsAndSoup:
                case IdleArchetype.FalloutShelter:
                    em.AddComponentData(slice, new IdleAssignmentStation
                    {
                        StationId = 1,
                        AssignedCount = System.Math.Clamp(initial.AssignedWorkers, 0, MaxWorkers),
                        Capacity = MaxWorkers,
                        OutputPerWorker = 1.5,
                        Interval = 1f,
                        Timer = 0f
                    });
                    break;

                case IdleArchetype.MelvorIdle:
                {
                    int skillLevel = System.Math.Max(1, initial.ProgressionLevel > 0 ? initial.ProgressionLevel : 1);
                    em.AddComponentData(slice, new IdleSkillNode
                    {
                        SkillId = 1,
                        Level = skillLevel,
                        Xp = initial.SkillXp,
                        XpToLevel = IdleOfflineCatchUp.XpToLevelFor(skillLevel),
                        TickInterval = 1f,
                        Timer = 0f,
                        IsActive = true
                    });
                    break;
                }

                case IdleArchetype.ADarkRoom:
                case IdleArchetype.CapybaraGo:
                    em.AddComponentData(slice, new IdleNarrativeState
                    {
                        RoomOrStep = _loadedNarrRoomOrStep,
                        StokeCount = _loadedNarrStokeCount,
                        ExploreUnlocked = _loadedNarrExploreUnlocked,
                        Wood = _loadedNarrWood,
                        SoftCurrency = _loadedNarrSoftCurrency
                    });
                    break;

                case IdleArchetype.LegendOfMushroom:
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = _loadedGachaPullCount,
                        PullCost = PullCost,
                        BestRarity = _loadedGachaBestRarity,
                        Stage = _loadedGachaStage
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
            if (_destroyed) return;
            PersistNow();
            _destroyed = true;

            if (!_spawned) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            var em = world.EntityManager;
            if (em.Exists(_sliceEntity))
                em.DestroyEntity(_sliceEntity);
            _spawned = false;
        }
    }
}

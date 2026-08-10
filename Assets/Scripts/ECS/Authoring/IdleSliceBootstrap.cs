using UnityEngine;
using Unity.Entities;
using HyperCasualRunner;
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
        private int _loadedGachaHeroId;
        private int _loadedGachaDupeCount;
        private int _loadedGachaGearSlot;
        private int _loadedNarrRoomOrStep;
        private int _loadedNarrExploreUnlocked;
        private int _loadedNarrStokeCount;
        private double _loadedNarrSoftCurrency;
        private double _loadedNarrWood;
        private float _loadedAfkChestSeconds;
        private int _loadedNarrRunId;
        private int _loadedNarrPetId;
        private int _loadedNarrLastChoice;
        private int _loadedNarrPendingChoice;

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
            double catchUpGained = 0;
            if (_loadedFromPrefs)
            {
                var st = em.GetComponentData<IdleSliceState>(_sliceEntity);
                catchUpGained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref st);
                em.SetComponentData(_sliceEntity, st);
                if (st.Archetype == IdleArchetype.MelvorIdle && em.HasComponent<IdleSkillNode>(_sliceEntity))
                {
                    var skill = em.GetComponentData<IdleSkillNode>(_sliceEntity);
                    IdleOfflineCatchUp.SyncSkillNodeFromSlice(ref skill, in st);
                    em.SetComponentData(_sliceEntity, skill);
                }
            }

            _spawned = true;

            // R4 P1: CatchUp stamps wall-clock on grant>0 before the ≤2s autosave; flush Pending/
            // Primary immediately so force-kill between CatchUp and Persist cannot lose the bank.
            if (_loadedFromPrefs && catchUpGained > 0)
                PersistNow();
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
                    // IH floor matches attach HeroDps=3 so gacha bumps stay in lockstep with PassiveRate.
                    state.PassiveRate = Archetype == IdleArchetype.IdleHeroes ? 3 : 0.5;
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
            _loadedGachaHeroId = 0;
            _loadedGachaDupeCount = 0;
            _loadedGachaGearSlot = 0;
            _loadedNarrRoomOrStep = 0;
            _loadedNarrExploreUnlocked = 0;
            _loadedNarrStokeCount = 0;
            _loadedNarrSoftCurrency = 0;
            _loadedNarrWood = 0;
            _loadedAfkChestSeconds = 0f;
            _loadedNarrRunId = 0;
            _loadedNarrPetId = 0;
            _loadedNarrLastChoice = 0;
            _loadedNarrPendingChoice = 0;

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
                // Melvor intra-level XP (R5); attach IdleSkillNode reads initial.SkillXp.
                state.SkillXp = System.Math.Max(0, GameProgressData.LoadSkillXp((int)Archetype));
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
                var gachaId = GameProgressData.LoadGachaIdentity((int)Archetype);
                _loadedGachaHeroId = gachaId.HeroId;
                _loadedGachaDupeCount = gachaId.DupeCount;
                _loadedGachaGearSlot = gachaId.GearSlot;
                _loadedNarrRoomOrStep = narrStep;
                _loadedNarrExploreUnlocked = narrExplore;
                _loadedNarrSoftCurrency = narrSoft;
                _loadedNarrStokeCount = cozy.NarrStokeCount;
                _loadedNarrWood = cozy.NarrWood;
                _loadedAfkChestSeconds = afkChest;
                var capyDna = GameProgressData.LoadCapybaraDna((int)Archetype);
                _loadedNarrRunId = capyDna.RunId;
                _loadedNarrPetId = capyDna.PetId;
                _loadedNarrLastChoice = capyDna.LastChoice;
                _loadedNarrPendingChoice = capyDna.PendingChoice;
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
                GameProgressData.SaveGachaIdentity((int)s.Archetype, new IdleGachaIdentityPersist
                {
                    HeroId = g.HeroId,
                    DupeCount = g.DupeCount,
                    GearSlot = g.GearSlot
                });
            }
            if (em.HasComponent<IdleNarrativeState>(_sliceEntity))
            {
                var n = em.GetComponentData<IdleNarrativeState>(_sliceEntity);
                narrStep = n.RoomOrStep;
                narrExplore = n.ExploreUnlocked;
                narrSoft = n.SoftCurrency;
                narrStoke = n.StokeCount;
                narrWood = n.Wood;
                if (s.Archetype == IdleArchetype.CapybaraGo)
                {
                    GameProgressData.SaveCapybaraDna((int)s.Archetype, new IdleCapybaraDnaPersist
                    {
                        RunId = n.RunId,
                        PetId = n.PetId,
                        LastChoice = n.LastChoice,
                        PendingChoice = n.PendingChoice
                    });
                }
            }

            // Melvor: node is online XP SoT mid-bar; sync slice before prefs write.
            int skillXp = s.SkillXp;
            if (em.HasComponent<IdleSkillNode>(_sliceEntity))
            {
                var skill = em.GetComponentData<IdleSkillNode>(_sliceEntity);
                skillXp = skill.Xp;
                if (s.SkillXp != skillXp)
                {
                    s.SkillXp = skillXp;
                    em.SetComponentData(_sliceEntity, s);
                }
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
                narrWood,
                skillXp);

            // R4-F1: combat SoT is separate from Level — Stage-inflated ProgressionLevel must not
            // become Zone on cold start. Only write when IdleCombatState is present.
            if (em.HasComponent<IdleCombatState>(_sliceEntity))
            {
                var c = em.GetComponentData<IdleCombatState>(_sliceEntity);
                GameProgressData.SaveIdleCombatPersist((int)s.Archetype, new IdleCombatPersist
                {
                    Zone = c.Zone,
                    EnemyHp = c.EnemyHp,
                    EnemyMaxHp = c.EnemyMaxHp,
                    HeroDps = c.HeroDps,
                    TapDamage = c.TapDamage,
                    GoldPerKill = c.GoldPerKill
                });
            }
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
                    em.AddComponentData(slice, BuildCombatState(
                        initial, System.Math.Max(1.0, statePassiveFromEntity(em, slice))));
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
                    // Cold-start HeroDps floor 3; PassiveRate lifts when prefs have gacha DPS.
                    // Persisted combat SoT wins over Level/PassiveRate invent (R4-F1).
                    em.AddComponentData(slice, BuildCombatState(
                        initial, System.Math.Max(3.0, statePassiveFromEntity(em, slice))));
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = _loadedGachaPullCount,
                        PullCost = PullCost,
                        BestRarity = _loadedGachaBestRarity,
                        Stage = _loadedGachaStage,
                        HeroId = _loadedGachaHeroId,
                        DupeCount = _loadedGachaDupeCount,
                        GearSlot = _loadedGachaGearSlot
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
                {
                    // Capybara DNA: restore prefs when present; cold-start RunId=1 (ADR keeps 0).
                    int runId = 0;
                    int petId = 0;
                    int lastChoice = 0;
                    int pendingChoice = 0;
                    if (Archetype == IdleArchetype.CapybaraGo)
                    {
                        runId = _loadedFromPrefs && _loadedNarrRunId > 0 ? _loadedNarrRunId : 1;
                        petId = _loadedNarrPetId;
                        lastChoice = _loadedNarrLastChoice;
                        pendingChoice = _loadedNarrPendingChoice;
                    }
                    em.AddComponentData(slice, new IdleNarrativeState
                    {
                        RoomOrStep = _loadedNarrRoomOrStep,
                        StokeCount = _loadedNarrStokeCount,
                        ExploreUnlocked = _loadedNarrExploreUnlocked,
                        Wood = _loadedNarrWood,
                        SoftCurrency = _loadedNarrSoftCurrency,
                        RunId = runId,
                        PetId = petId,
                        LastChoice = lastChoice,
                        PendingChoice = pendingChoice
                    });
                    break;
                }

                case IdleArchetype.LegendOfMushroom:
                    em.AddComponentData(slice, new IdleGachaState
                    {
                        PullCount = _loadedGachaPullCount,
                        PullCost = PullCost,
                        BestRarity = _loadedGachaBestRarity,
                        Stage = _loadedGachaStage,
                        HeroId = _loadedGachaHeroId,
                        DupeCount = _loadedGachaDupeCount,
                        GearSlot = _loadedGachaGearSlot
                    });
                    break;
            }
        }

        /// <summary>
        /// Restore IdleCombatState from prefs when present; otherwise cold-start from Level.
        /// Never invent Zone from Stage-inflated ProgressionLevel when combat keys exist (R4-F1).
        /// MaxHp/Gold rebase from Zone when MaxHp≤0 (R4-D7).
        /// </summary>
        private IdleCombatState BuildCombatState(IdleSliceState initial, double defaultHeroDps)
        {
            if (_loadedFromPrefs &&
                GameProgressData.TryLoadIdleCombatPersist((int)Archetype, out var saved))
            {
                int zone = System.Math.Max(1, saved.Zone);
                float maxHp = saved.EnemyMaxHp;
                float hp = saved.EnemyHp;
                double gold = saved.GoldPerKill;
                if (maxHp <= 0f)
                {
                    maxHp = 20f + zone * 25f;
                    hp = maxHp;
                }
                if (gold <= 0)
                    gold = 5 + zone * 2;
                return new IdleCombatState
                {
                    TapDamage = saved.TapDamage > 0 ? saved.TapDamage : ClickPower,
                    HeroDps = saved.HeroDps > 0 ? saved.HeroDps : defaultHeroDps,
                    Zone = zone,
                    GoldPerKill = gold,
                    EnemyHp = hp,
                    EnemyMaxHp = maxHp
                };
            }

            return new IdleCombatState
            {
                TapDamage = ClickPower,
                HeroDps = defaultHeroDps,
                Zone = System.Math.Max(1, initial.ProgressionLevel),
                GoldPerKill = 5,
                EnemyHp = 20,
                EnemyMaxHp = 20
            };
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

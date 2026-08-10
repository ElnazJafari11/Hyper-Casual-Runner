using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode smoke for remaining matrix titles (Batch B+C).
    /// Each test targets the matrix Core Verb (not a substitute) with causal arrange + spend/sim asserts.
    /// </summary>
    [TestFixture]
    public class IdleBatchBCSmokeTests
    {
        private World _world;
        private EntityManager _em;

        private static readonly IdleArchetype[] PrefsUsed =
        {
            IdleArchetype.RealmGrinder,
            IdleArchetype.NguIdle,
            IdleArchetype.MelvorIdle,
            IdleArchetype.AfkArena,
            IdleArchetype.IdleHeroes,
            IdleArchetype.LegendOfMushroom,
            IdleArchetype.CapybaraGo,
            IdleArchetype.ADarkRoom,
            IdleArchetype.CatsAndSoup,
            IdleArchetype.NekoAtsume,
            IdleArchetype.FalloutShelter
        };

        [SetUp]
        public void SetUp()
        {
            _world = new World("IdleBatchBCSmokeWorld");
            World.DefaultGameObjectInjectionWorld = _world;
            _em = _world.EntityManager;
            foreach (var arch in PrefsUsed)
                GameProgressData.ClearIdleSlice((int)arch);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var arch in PrefsUsed)
                GameProgressData.ClearIdleSlice((int)arch);
            if (_world != null && _world.IsCreated) _world.Dispose();
        }

        private Entity CreateSlice(IdleArchetype arch, double currency)
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, new IdleSliceState
            {
                Archetype = arch,
                PrimaryCurrency = currency,
                PrestigeCurrency = 0,
                GlobalMultiplier = 1f,
                ProgressionLevel = 0,
                ClickPower = 5,
                PassiveRate = 0,
                OwnedGenerators = 0,
                EnemyHp = 20,
                EnemyMaxHp = 20,
                MaxWorkers = 5,
                EnergyPool = 50,
                AssignedWorkers = 0
            });
            _em.AddComponentData(e, new CurrentRunStats { CurrentGold = currency, BaseDamage = 5 });
            return e;
        }

        private void PumpSim(SystemHandle simSys, float totalSeconds, float dt = 0.5f)
        {
            float elapsed = 0f;
            while (elapsed < totalSeconds - 1e-4f)
            {
                elapsed += dt;
                _world.SetTime(new TimeData(elapsed, dt));
                simSys.Update(_world.Unmanaged);
            }
        }

        private void FireNarrative(SystemHandle sys, int actionId)
        {
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleNarrativeActionEvent { ActionId = actionId });
            sys.Update(_world.Unmanaged);
        }

        [Test]
        public void ADarkRoom_StokeThenExplore_AdvancesProgression()
        {
            var slice = CreateSlice(IdleArchetype.ADarkRoom, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 0, Wood = 0, SoftCurrency = 0
            });
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();

            for (int i = 0; i < 5; i++)
                FireNarrative(sys, 0);

            var narr = _em.GetComponentData<IdleNarrativeState>(slice);
            Assert.AreEqual(1, narr.ExploreUnlocked, "5 stokes unlock explore");
            Assert.AreEqual(5, narr.Wood, "Each stoke yields wood");
            Assert.AreEqual(5, narr.StokeCount);

            FireNarrative(sys, 1);

            Assert.GreaterOrEqual(_em.GetComponentData<IdleSliceState>(slice).ProgressionLevel, 1);
            Assert.Greater(_em.GetComponentData<IdleNarrativeState>(slice).RoomOrStep, 0);
        }

        [Test]
        public void RealmGrinder_AlignFaction_SetsFactionWithoutFreeMult()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 50);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 0, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = false, IsAutomated = true
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var afterBuild = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, afterBuild.OwnedGenerators, "Build verb: own a generator");
            Assert.AreEqual(35.0, afterBuild.PrimaryCurrency, 0.001, "Build cost spent");
            Assert.Greater(afterBuild.PassiveRate, 0);
            float multBefore = afterBuild.GlobalMultiplier;
            int levelBefore = afterBuild.ProgressionLevel;

            var alignSys = _world.CreateSystem<IdleFactionAlignSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleFactionAlignEvent { TargetSlice = slice, FactionId = 1 });
            alignSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.FactionId);
            Assert.AreEqual(35.0, after.PrimaryCurrency, 0.001,
                "First Align (neutral→faction) is free");
            Assert.AreEqual(multBefore, after.GlobalMultiplier, 0.001f,
                "Align must not free-stack GlobalMultiplier");
            Assert.AreEqual(levelBefore, after.ProgressionLevel,
                "Align must not free ProgressionLevel");
        }

        [Test]
        public void RealmGrinder_AlignGood_RaisesPassiveIncomeViaFactionBonus()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 50);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 0, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = false, IsAutomated = true
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var afterBuild = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterBuild.PassiveRate, 0, "Need owned gens so factionBonus multiplies PassiveRate");
            Assert.AreEqual(0, afterBuild.FactionId, "Start Neutral");
            double baseline = afterBuild.PrimaryCurrency;
            double rate = afterBuild.PassiveRate;

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 2f, dt: 1f);
            double neutralGain = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency - baseline;
            Assert.AreEqual(rate * 1.0 * 2.0, neutralGain, 0.001, "Neutral factionBonus must be 1.0");

            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PrimaryCurrency = baseline;
            _em.SetComponentData(slice, st);
            _em.SetComponentData(slice, new CurrentRunStats { CurrentGold = baseline, BaseDamage = 5 });

            var alignSys = _world.CreateSystem<IdleFactionAlignSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleFactionAlignEvent { TargetSlice = slice, FactionId = 1 });
            alignSys.Update(_world.Unmanaged);
            Assert.AreEqual(1, _em.GetComponentData<IdleSliceState>(slice).FactionId);

            PumpSim(simSys, 2f, dt: 1f);
            double goodGain = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency - baseline;
            Assert.Greater(goodGain, neutralGain, "Good Align must raise Passive income via factionBonus");
            Assert.AreEqual(rate * 1.5 * 2.0, goodGain, 0.001, "Good factionBonus must be 1.5");
        }

        [Test]
        public void RealmGrinder_AlignFlip_CostsPrimaryCurrency()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 50);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.FactionId = 1;
            _em.SetComponentData(slice, st);

            var alignSys = _world.CreateSystem<IdleFactionAlignSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleFactionAlignEvent { TargetSlice = slice, FactionId = 2 });
            alignSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(2, after.FactionId, "Re-pick Align must set new faction");
            Assert.AreEqual(50.0 - IdlePrestigeMath.RealmGrinderAlignFlipCost, after.PrimaryCurrency, 0.001,
                "Faction flip must spend AlignFlipCost");
            Assert.AreEqual(after.PrimaryCurrency, _em.GetComponentData<CurrentRunStats>(slice).CurrentGold, 0.001,
                "Run gold must stay synced after Align spend");
        }

        [Test]
        public void RealmGrinder_AlignFlip_InsufficientCurrency_IsNoOp()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 10);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.FactionId = 1;
            _em.SetComponentData(slice, st);

            var alignSys = _world.CreateSystem<IdleFactionAlignSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleFactionAlignEvent { TargetSlice = slice, FactionId = 2 });
            alignSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.FactionId, "Underfunded flip must leave FactionId");
            Assert.AreEqual(10.0, after.PrimaryCurrency, 0.001, "Underfunded flip must not spend");
        }

        [Test]
        public void RealmGrinder_Rebirth_ResetsRun_KeepsFaction()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 100);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.FactionId = 1;
            st.OwnedGenerators = 4;
            st.PassiveRate = 5;
            st.GlobalMultiplier = 2f;
            st.PhaseIndex = 0;
            _em.SetComponentData(slice, st);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1,
                OwnedCount = 4,
                BaseCost = 15,
                CostGrowth = 1.15f,
                BaseCps = 1,
                IsAutomated = true
            });

            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new PrestigeEventComponent { TargetSlice = slice });
            prestigeSys.Update(_world.Unmanaged);
            phaseSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.FactionId, "Rebirth must keep faction alignment");
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.AreEqual(0, after.OwnedGenerators);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PhaseIndex, "RG rebirth is hard prestige, not phase");
            Assert.AreEqual(0, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
        }

        [Test]
        public void NguIdle_AllocateEnergyThenTick_ProducesFromSpend()
        {
            var slice = CreateSlice(IdleArchetype.NguIdle, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.EnergyPool = 50;
            _em.SetComponentData(slice, st);

            var allocSys = _world.CreateSystem<IdleAllocateEnergySystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAllocateEnergyEvent { Amount = 10f });
            allocSys.Update(_world.Unmanaged);

            var afterAlloc = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(10f, afterAlloc.EnergyAllocated, 0.01f);

            // Snapshot AFTER alloc — free ProgressionLevel=1 must not satisfy XP/level asserts.
            double currencyBeforeTick = afterAlloc.PrimaryCurrency;
            int xpBefore = afterAlloc.SkillXp;
            int levelBefore = afterAlloc.ProgressionLevel;
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 5f);

            var afterTick = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterTick.PrimaryCurrency, currencyBeforeTick, "TickEnergy should produce currency");
            Assert.IsTrue(
                afterTick.SkillXp > xpBefore || afterTick.ProgressionLevel > levelBefore,
                "TickEnergy must raise SkillXp or ProgressionLevel beyond free alloc level");
        }

        [Test]
        public void NguIdle_Rebirth_RetainsEnergyAllocation_ResetsRunCurrency()
        {
            var slice = CreateSlice(IdleArchetype.NguIdle, 100);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.EnergyPool = 50;
            st.EnergyAllocated = 10f;
            st.SkillXp = 40;
            st.ProgressionLevel = 3;
            st.PhaseIndex = 0;
            _em.SetComponentData(slice, st);

            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new PrestigeEventComponent { TargetSlice = slice });
            prestigeSys.Update(_world.Unmanaged);
            phaseSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.AreEqual(10f, after.EnergyAllocated, 0.01f, "Rebirth retains energy allocation");
            Assert.AreEqual(50f, after.EnergyPool, 0.01f);
            Assert.AreEqual(40, after.SkillXp, "SkillXp retained on rebirth MVP");
            Assert.AreEqual(0, after.ProgressionLevel);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PhaseIndex, "Rebirth must not fire phase");
        }

        [Test]
        public void NguIdle_EnergyPool_PersistsRoundTrip()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.NguIdle);
            GameProgressData.SaveIdleSlice(
                (int)IdleArchetype.NguIdle, 10, 1, 1.1f, 2, 1, 0, 0,
                managersHired: 0, phaseIndex: 0, factionId: 0,
                energyAllocated: 12f, managerIsHired: false, energyPool: 77f);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.NguIdle,
                out _, out _, out _, out _, out _, out _, out _,
                out _, out _, out _, out var energyAlloc, out _, out var energyPool));
            Assert.AreEqual(12f, energyAlloc, 0.001f);
            Assert.AreEqual(77f, energyPool, 0.001f);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.NguIdle);
        }

        [Test]
        public void MelvorIdle_GrindSkillNode_LevelsViaSimTick()
        {
            var slice = CreateSlice(IdleArchetype.MelvorIdle, 0);
            _em.AddComponentData(slice, new IdleSkillNode
            {
                SkillId = 1,
                Level = 0,
                Xp = 0,
                XpToLevel = 20,
                TickInterval = 1f,
                Timer = 0f,
                IsActive = true
            });

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            // 4 skill ticks × 5 XP → level up at 20; also grants currency per tick.
            PumpSim(simSys, 4f, dt: 1f);

            var skill = _em.GetComponentData<IdleSkillNode>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(skill.Level, 1, "IdleSkillNode grind must level via sim");
            Assert.GreaterOrEqual(after.ProgressionLevel, 1);
            Assert.Greater(after.PrimaryCurrency, 0, "Skill ticks grant currency");
        }

        [Test]
        public void EggInc_HatchBurst_RaisesCurrencyAndPassive()
        {
            var slice = CreateSlice(IdleArchetype.EggInc, 0);
            var sys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 5f });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.PrimaryCurrency, 0);
            Assert.Greater(after.PassiveRate, 0);

            double beforeTick = after.PrimaryCurrency;
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 2f);
            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, beforeTick,
                "Habitat CPS should tick after hatch");
        }

        [Test]
        public void IdleMiner_BuyShaftThenHireManager_AutomatesShaft()
        {
            var slice = CreateSlice(IdleArchetype.IdleMinerTycoon, 200);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 0, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = true, IsAutomated = false
            });
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1, HireCost = 100, IsHired = false
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators, "Upgrade Shafts verb requires buy");
            Assert.AreEqual(185.0, afterBuy.PrimaryCurrency, 0.001);
            Assert.AreEqual(0.0, afterBuy.PassiveRate, 0.001, "Shaft not automated until manager");

            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleHireManagerEvent { TargetGeneratorId = 1 });
            hireSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.ManagersHired);
            Assert.AreEqual(85.0, after.PrimaryCurrency, 0.001, "HireCost spent");
            Assert.Greater(after.PassiveRate, 0);
            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).IsAutomated);

            // Post-hire income tick (mirror Cookie/AdvCap): rate field alone is not enough.
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            double beforeTick = after.PrimaryCurrency;
            PumpSim(simSys, 1f);
            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, beforeTick,
                "Post-hire PumpSim must accrue PrimaryCurrency from shaft automation");
        }

        [Test]
        public void TapTitans2_TapKill_AdvancesZoneAndGrantsGold()
        {
            var slice = CreateSlice(IdleArchetype.TapTitans2, 0);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 10, HeroDps = 1, Zone = 1, GoldPerKill = 5,
                EnemyHp = 3, EnemyMaxHp = 3
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ClickPower = 10;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 1f });
            sys.Update(_world.Unmanaged);

            var combat = _em.GetComponentData<IdleCombatState>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(combat.Zone, 2, "Kill advances combat Zone");
            Assert.Greater(after.PrimaryCurrency, 0, "Kill grants gold (parity with Clicker Heroes)");
            Assert.AreEqual(combat.EnemyMaxHp, combat.EnemyHp, 0.01f, "Enemy respawns after kill");
        }

        [Test]
        public void TapTitans2_CombatDps_FractionalOverOneSecond()
        {
            // Matrix "Tap / Hero DPS" — DPS half mirrors ClickerHeroes_CombatDps_FractionalOverOneSecond.
            var slice = CreateSlice(IdleArchetype.TapTitans2, 0);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 1, HeroDps = 2, Zone = 1, GoldPerKill = 5,
                EnemyHp = 100, EnemyMaxHp = 100
            });

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 1f, dt: 0.1f);

            float lost = 100f - _em.GetComponentData<IdleCombatState>(slice).EnemyHp;
            Assert.AreEqual(2.0, lost, 0.15, $"HeroDps=2 over 1s should remove ~2 HP, got {lost}");
        }

        [Test]
        public void IdleHeroes_AutoCombatDps_KillsEnemy()
        {
            // Matrix core verb is Auto-Combat via IdleCombatState — not gacha (monetization).
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 0);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 2, HeroDps = 40, Zone = 1, GoldPerKill = 5,
                EnemyHp = 10, EnemyMaxHp = 10
            });

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 1f, dt: 0.5f);

            var combat = _em.GetComponentData<IdleCombatState>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(combat.Zone, 2, "Hero DPS tick should kill and advance zone");
            Assert.Greater(after.PrimaryCurrency, 0, "Kill should grant gold");
            // Continuing DPS after respawn may leave EnemyHp < Max — only require valid pool.
            Assert.Greater(combat.EnemyMaxHp, 0f);
            Assert.LessOrEqual(combat.EnemyHp, combat.EnemyMaxHp);
            Assert.GreaterOrEqual(after.AfkChestSeconds, 1f, "IH must accrue AFK chest during combat");
        }

        [Test]
        public void IdleHeroes_ClickNoGold_GachaRaisesHeroDps_ClaimGated()
        {
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 50);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 2, HeroDps = 3, Zone = 1, GoldPerKill = 5,
                EnemyHp = 20, EnemyMaxHp = 20
            });
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 0, PullCost = 10, BestRarity = 0, Stage = 0
            });

            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var clickEvt = _em.CreateEntity();
            _em.AddComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f });
            clickSys.Update(_world.Unmanaged);
            Assert.AreEqual(50, _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0.001,
                "IH click must not mint flat gold");

            double beforeDps = _em.GetComponentData<IdleCombatState>(slice).HeroDps;
            var gachaSys = _world.CreateSystem<IdleGachaPullSystem>();
            var pullEvt = _em.CreateEntity();
            _em.AddComponentData(pullEvt, new IdleGachaPullEvent());
            gachaSys.Update(_world.Unmanaged);
            Assert.Greater(_em.GetComponentData<IdleCombatState>(slice).HeroDps, beforeDps);

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var emptyClaim = _em.CreateEntity();
            _em.AddComponentData(emptyClaim, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);
            double afterEmpty = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
            // Spent 10 on gacha; empty claim must not add demo gold.
            Assert.AreEqual(40, afterEmpty, 0.001, "Empty IH claim must pay 0");

            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.AfkChestSeconds = 12f;
            st.HasOfflineClaim = true;
            st.ProgressionLevel = 2;
            _em.SetComponentData(slice, st);

            var filledClaim = _em.CreateEntity();
            _em.AddComponentData(filledClaim, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);
            double afterFilled = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
            Assert.Greater(afterFilled, afterEmpty);

            var thirdClaim = _em.CreateEntity();
            _em.AddComponentData(thirdClaim, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);
            Assert.AreEqual(afterFilled, _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0.001);
        }

        [Test]
        public void AfkArena_SimFillsChestThenClaim_GrantsCurrency()
        {
            var slice = CreateSlice(IdleArchetype.AfkArena, 0);
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            // AfkChestSeconds += dt; claim flag at ≥10s — do not pre-seed.
            PumpSim(simSys, 10.5f, dt: 0.5f);

            var filled = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(filled.AfkChestSeconds, 10f, "Sim must accrue chest time");
            Assert.IsTrue(filled.HasOfflineClaim, "Sim must set claim flag");
            double beforeClaim = filled.PrimaryCurrency;

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.PrimaryCurrency, beforeClaim, "Claim should add chest reward");
            Assert.IsFalse(after.HasOfflineClaim);
            Assert.AreEqual(0f, after.AfkChestSeconds, 0.01f);
        }

        [Test]
        public void AfkArena_CampaignProgress_NoFlatGold()
        {
            var slice = CreateSlice(IdleArchetype.AfkArena, 5);
            var sys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 1f });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(5, after.PrimaryCurrency, 0.001, "Campaign progress must not mint flat gold");
            Assert.GreaterOrEqual(after.ProgressionLevel, 1);
            Assert.Greater(after.AfkChestSeconds, 0f);
        }

        [Test]
        public void LegendOfMushroom_RubLampThreeTimes_SpendsAndAdvancesStage()
        {
            var slice = CreateSlice(IdleArchetype.LegendOfMushroom, 100);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 0, PullCost = 10, BestRarity = 0, Stage = 0
            });

            var sys = _world.CreateSystem<IdleGachaPullSystem>();
            for (int i = 0; i < 3; i++)
            {
                double before = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
                var evt = _em.CreateEntity();
                _em.AddComponentData(evt, new IdleGachaPullEvent());
                sys.Update(_world.Unmanaged);
                double afterPull = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
                Assert.AreEqual(before - 10.0, afterPull, 0.001, $"Pull {i + 1} must spend PullCost");
            }

            var gacha = _em.GetComponentData<IdleGachaState>(slice);
            Assert.AreEqual(3, gacha.PullCount);
            Assert.GreaterOrEqual(gacha.Stage, 1, "3rd pull (PullCount % 3 == 0) advances stage");
            Assert.Greater(gacha.BestRarity, 0);
        }

        [Test]
        public void CapybaraGo_StepsThenAdvance_RaisesLevel()
        {
            // Causal gate: advance no-ops until steps earn ExploreUnlocked.
            var slice = CreateSlice(IdleArchetype.CapybaraGo, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 0, Wood = 0, SoftCurrency = 0
            });
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();

            FireNarrative(sys, 1);
            var blocked = _em.GetComponentData<IdleNarrativeState>(slice);
            Assert.AreEqual(0, blocked.RoomOrStep, "Advance without steps must be a no-op");
            Assert.AreEqual(0, blocked.ExploreUnlocked);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).ProgressionLevel);

            FireNarrative(sys, 0);
            FireNarrative(sys, 0);
            var mid = _em.GetComponentData<IdleNarrativeState>(slice);
            Assert.AreEqual(2, mid.StokeCount, "Step verb accumulates narrative steps");
            Assert.AreEqual(1, mid.ExploreUnlocked, "Earned steps unlock advance");

            FireNarrative(sys, 1);

            Assert.GreaterOrEqual(_em.GetComponentData<IdleSliceState>(slice).ProgressionLevel, 1);
            Assert.Greater(_em.GetComponentData<IdleNarrativeState>(slice).RoomOrStep, 0);
        }

        [Test]
        public void CatsAndSoup_AssignCatThenSim_ProducesOutput()
        {
            var slice = CreateSlice(IdleArchetype.CatsAndSoup, 0);
            _em.AddComponentData(slice, new IdleAssignmentStation
            {
                StationId = 1, AssignedCount = 0, Capacity = 5,
                OutputPerWorker = 1.5, Interval = 1f, Timer = 0f
            });
            var assignSys = _world.CreateSystem<IdleAssignWorkerSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAssignWorkerEvent { StationId = 1, Delta = 1 });
            assignSys.Update(_world.Unmanaged);

            Assert.AreEqual(1, _em.GetComponentData<IdleSliceState>(slice).AssignedWorkers);
            Assert.AreEqual(1, _em.GetComponentData<IdleAssignmentStation>(slice).AssignedCount);

            double before = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 2f, dt: 1f);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            // Station-only cook: 1 worker * 1.5 * 2 ticks = 3 (no continuous double-pay).
            Assert.AreEqual(before + 3.0, after.PrimaryCurrency, 0.01,
                "Cats cook via station interval only");
            Assert.AreEqual(0.0, after.PendingClaim, 0.01, "Cats live-pay — not buffered");
        }

        [Test]
        public void NekoAtsume_PlaceFood_SpendsAndAttractsCats()
        {
            var slice = CreateSlice(IdleArchetype.NekoAtsume, 20);
            var before = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleNarrativeActionEvent { ActionId = 0 });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(before - 5.0, after.PrimaryCurrency, 0.001, "Place food spends 5");
            Assert.GreaterOrEqual(after.CheckInCats, 2);
            Assert.IsTrue(after.HasOfflineClaim);
        }

        [Test]
        public void NekoAtsume_PlaceToys_SpendsAndAttractsCats()
        {
            var slice = CreateSlice(IdleArchetype.NekoAtsume, 20);
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();
            FireNarrative(sys, 1);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(12.0, after.PrimaryCurrency, 0.001, "Place toys spends 8");
            Assert.GreaterOrEqual(after.CheckInCats, 3);
            Assert.GreaterOrEqual(after.ProgressionLevel, 1);
            Assert.IsTrue(after.HasOfflineClaim);
        }

        [Test]
        public void NekoAtsume_CheckIn_ClearsCatsAndPays()
        {
            var slice = CreateSlice(IdleArchetype.NekoAtsume, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.CheckInCats = 4;
            st.HasOfflineClaim = true;
            _em.SetComponentData(slice, st);

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(0, after.CheckInCats, "Check-in clears cats (no re-farm)");
            Assert.IsFalse(after.HasOfflineClaim);
            Assert.GreaterOrEqual(after.PrimaryCurrency, 20.0); // 4 * 5
        }

        [Test]
        public void FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues()
        {
            var slice = CreateSlice(IdleArchetype.FalloutShelter, 0);
            _em.AddComponentData(slice, new IdleAssignmentStation
            {
                StationId = 1, AssignedCount = 0, Capacity = 5,
                OutputPerWorker = 1.5, Interval = 1f, Timer = 0f
            });
            var assignSys = _world.CreateSystem<IdleAssignWorkerSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAssignWorkerEvent { StationId = 1, Delta = 2 });
            assignSys.Update(_world.Unmanaged);

            Assert.AreEqual(2, _em.GetComponentData<IdleSliceState>(slice).AssignedWorkers);
            Assert.AreEqual(2, _em.GetComponentData<IdleAssignmentStation>(slice).AssignedCount,
                "Station AssignedCount must mirror dwellers");

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 2f, dt: 1f);

            var mid = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(mid.PendingClaim, 0, "Sim should accrue PendingClaim for dwellers");
            Assert.AreEqual(0.0, mid.PrimaryCurrency, 0.01, "Fallout buffers — no live PrimaryCurrency");
            Assert.IsTrue(mid.HasOfflineClaim);

            double pending = mid.PendingClaim;
            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var claimEvt = _em.CreateEntity();
            _em.AddComponentData(claimEvt, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(0.0, after.PendingClaim, 0.01, "Claim Production drains buffer");
            Assert.AreEqual(pending, after.PrimaryCurrency, 0.01);
            Assert.IsFalse(after.HasOfflineClaim);
        }

        private IdleSliceBootstrap SpawnBootstrap(IdleArchetype arch, int maxWorkers = 5)
        {
            var go = new GameObject("CozyPersist_" + arch);
            var boot = go.AddComponent<IdleSliceBootstrap>();
            boot.Archetype = arch;
            boot.MaxWorkers = maxWorkers;
            boot.LoadPersistedProgress = true;
            boot.StartingCurrency = 0;
            // EditMode: SendMessage("Start") asserts ShouldRunBehaviour — invoke TrySpawn directly.
            var trySpawn = typeof(IdleSliceBootstrap).GetMethod(
                "TrySpawn",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(trySpawn, "IdleSliceBootstrap.TrySpawn must exist for EditMode spawn");
            trySpawn.Invoke(boot, null);
            Assert.IsTrue(boot.IsSpawned, arch + " bootstrap must spawn into DefaultWorld");
            return boot;
        }

        [Test]
        public void CatsAndSoup_AssignedWorkers_SurvivePersistNowReload()
        {
            int arch = (int)IdleArchetype.CatsAndSoup;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.CatsAndSoup);
            var slice = boot.SliceEntity;
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.AssignedWorkers = 3;
            st.PrimaryCurrency = 9;
            _em.SetComponentData(slice, st);
            var station = _em.GetComponentData<IdleAssignmentStation>(slice);
            station.AssignedCount = 3;
            _em.SetComponentData(slice, station);
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.CatsAndSoup);
            try
            {
                var reloaded = _em.GetComponentData<IdleSliceState>(boot2.SliceEntity);
                Assert.AreEqual(3, reloaded.AssignedWorkers, "Cats workers must survive PersistNow reload");
                Assert.AreEqual(3, _em.GetComponentData<IdleAssignmentStation>(boot2.SliceEntity).AssignedCount,
                    "Station AssignedCount must restore via AttachArchetypeExtras");
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void FalloutShelter_PendingAndWorkers_SurvivePersistNowReload()
        {
            int arch = (int)IdleArchetype.FalloutShelter;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.FalloutShelter);
            var slice = boot.SliceEntity;
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.AssignedWorkers = 2;
            st.PendingClaim = 12.5;
            st.HasOfflineClaim = true;
            _em.SetComponentData(slice, st);
            var station = _em.GetComponentData<IdleAssignmentStation>(slice);
            station.AssignedCount = 2;
            _em.SetComponentData(slice, station);
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.FalloutShelter);
            try
            {
                var reloaded = _em.GetComponentData<IdleSliceState>(boot2.SliceEntity);
                Assert.AreEqual(2, reloaded.AssignedWorkers);
                Assert.AreEqual(12.5, reloaded.PendingClaim, 0.01, "Fallout PendingClaim must survive reload");
                Assert.IsTrue(reloaded.HasOfflineClaim);
                Assert.AreEqual(2, _em.GetComponentData<IdleAssignmentStation>(boot2.SliceEntity).AssignedCount);
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void NekoAtsume_CheckInCats_SurvivePersistNowReload()
        {
            int arch = (int)IdleArchetype.NekoAtsume;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.NekoAtsume);
            var slice = boot.SliceEntity;
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.CheckInCats = 5;
            st.HasOfflineClaim = true;
            st.PrimaryCurrency = 20;
            _em.SetComponentData(slice, st);
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.NekoAtsume);
            try
            {
                var reloaded = _em.GetComponentData<IdleSliceState>(boot2.SliceEntity);
                Assert.AreEqual(5, reloaded.CheckInCats, "Neko CheckInCats must survive PersistNow reload");
                Assert.IsTrue(reloaded.HasOfflineClaim);
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void ADarkRoom_NarrativeWoodStoke_SurvivePersistNowReload()
        {
            int arch = (int)IdleArchetype.ADarkRoom;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.ADarkRoom);
            var slice = boot.SliceEntity;
            _em.SetComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 2,
                StokeCount = 4,
                ExploreUnlocked = 1,
                Wood = 7,
                SoftCurrency = 3
            });
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.ADarkRoom);
            try
            {
                var narr = _em.GetComponentData<IdleNarrativeState>(boot2.SliceEntity);
                Assert.AreEqual(2, narr.RoomOrStep);
                Assert.AreEqual(4, narr.StokeCount, "ADR StokeCount must survive reload");
                Assert.AreEqual(1, narr.ExploreUnlocked);
                Assert.AreEqual(7.0, narr.Wood, 0.01, "ADR Wood must survive reload");
                Assert.AreEqual(3.0, narr.SoftCurrency, 0.01);
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void IdleHeroes_AfkChest_FillsOverSimTime()
        {
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PassiveRate = 2;
            st.EnemyHp = 100;
            st.EnemyMaxHp = 100;
            _em.SetComponentData(slice, st);

            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 10.5f, dt: 0.5f);

            var filled = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(filled.AfkChestSeconds, 10f, "Idle Heroes AFK chest must accrue in simulation");
            Assert.IsTrue(filled.HasOfflineClaim);
            double beforeClaim = filled.PrimaryCurrency;

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);

            var afterClaim = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterClaim.PrimaryCurrency, beforeClaim);
            Assert.AreEqual(0f, afterClaim.AfkChestSeconds, 0.01f);
            Assert.IsFalse(afterClaim.HasOfflineClaim);
        }

        [Test]
        public void IdleHeroes_GachaPull_RaisesHeroDps()
        {
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 100);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 0, PullCost = 10, BestRarity = 0, Stage = 0
            });
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 2, HeroDps = 3, Zone = 1, GoldPerKill = 5, EnemyHp = 20, EnemyMaxHp = 20
            });
            double beforeDps = _em.GetComponentData<IdleCombatState>(slice).HeroDps;
            double beforePassive = _em.GetComponentData<IdleSliceState>(slice).PassiveRate;

            var sys = _world.CreateSystem<IdleGachaPullSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleGachaPullEvent());
            sys.Update(_world.Unmanaged);

            var combat = _em.GetComponentData<IdleCombatState>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(combat.HeroDps, beforeDps, "Gacha must raise IdleCombatState.HeroDps");
            Assert.Greater(after.PassiveRate, beforePassive, "Gacha must raise PassiveRate");
        }

        [Test]
        public void IdleHeroes_GachaStageBump_DoesNotDropProgressionBelowZone()
        {
            // Zone deep; Stage low — stage-bump pull must not smash ProgressionLevel below Zone.
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 100);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 2, PullCost = 10, BestRarity = 0, Stage = 0
            });
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 2, HeroDps = 3, Zone = 5, GoldPerKill = 5, EnemyHp = 20, EnemyMaxHp = 20
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ProgressionLevel = 5;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleGachaPullSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleGachaPullEvent());
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            var gacha = _em.GetComponentData<IdleGachaState>(slice);
            var combat = _em.GetComponentData<IdleCombatState>(slice);
            Assert.AreEqual(1, gacha.Stage, "3rd pull bumps Stage");
            Assert.AreEqual(5, combat.Zone, "Combat Zone unchanged by gacha");
            Assert.GreaterOrEqual(after.ProgressionLevel, combat.Zone,
                "ProgressionLevel must never drop below Zone after Stage write");
        }

        [Test]
        public void IdleHeroes_Kill_DoesNotDropStageInflatedProgressionLevel()
        {
            // R3-F1: Stage-inflated Level above Zone must survive combat kill Max (not clobber to Zone).
            // Level=8 > Zone+1 so unconditional Level=Zone would regress to 6.
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 0);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 1, HeroDps = 50, Zone = 5, GoldPerKill = 5,
                EnemyHp = 10f, EnemyMaxHp = 10f
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ProgressionLevel = 8;
            _em.SetComponentData(slice, st);

            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 0.5f, dt: 0.5f);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            var combat = _em.GetComponentData<IdleCombatState>(slice);
            Assert.AreEqual(6, combat.Zone, "One kill advances Zone 5→6");
            Assert.GreaterOrEqual(after.ProgressionLevel, 8,
                "Kill must not drop Stage-inflated ProgressionLevel");
            Assert.GreaterOrEqual(after.ProgressionLevel, combat.Zone);
        }

        [Test]
        public void ClickerHeroes_TapKill_DoesNotDropStageInflatedProgressionLevel()
        {
            // R3-F1 tap-kill writer (CH/TT2); IH clicks are auto-combat-only no-ops.
            var slice = CreateSlice(IdleArchetype.ClickerHeroes, 0);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 20, HeroDps = 0, Zone = 5, GoldPerKill = 5,
                EnemyHp = 10f, EnemyMaxHp = 10f
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ProgressionLevel = 6; // acceptance: Stage-inflated Level=6, Zone=5
            st.ClickPower = 20;
            _em.SetComponentData(slice, st);

            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 1f });
            clickSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            var combat = _em.GetComponentData<IdleCombatState>(slice);
            Assert.AreEqual(6, combat.Zone);
            Assert.GreaterOrEqual(after.ProgressionLevel, 6,
                "Tap-kill must keep ProgressionLevel ≥ prior Stage-inflated Level (never 5)");
        }

        [Test]
        public void IdleHeroes_ClaimBelowTenSeconds_NoPayout()
        {
            // R3-F2: seconds∈[1,10) + HasOfflineClaim=false → no payout for IH chest.
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 40);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.AfkChestSeconds = 5f;
            st.HasOfflineClaim = false;
            st.ProgressionLevel = 2;
            _em.SetComponentData(slice, st);

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent());
            claimSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(40, after.PrimaryCurrency, 0.001, "Sub-10s IH claim must pay 0");
            Assert.AreEqual(5f, after.AfkChestSeconds, 0.01f, "Chest seconds must remain until gate met");
            Assert.IsFalse(after.HasOfflineClaim);
        }

        [Test]
        public void LegendOfMushroom_AutoLamp_PullsAfterStageUnlock()
        {
            // Stage>=1 + lamp loot: start with one pull's worth; auto-lamp must sustain further pulls without Farm click.
            var slice = CreateSlice(IdleArchetype.LegendOfMushroom, 15);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 3, PullCost = 10, BestRarity = 1, Stage = 1, AutoTimer = 0f
            });

            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 5f, dt: 0.5f);

            var gacha = _em.GetComponentData<IdleGachaState>(slice);
            Assert.GreaterOrEqual(gacha.PullCount, 5, "Auto-lamp + lamp loot should sustain ≥2 pulls from Stage>=1 without farm clicks");
        }

        [Test]
        public void CapybaraGo_FiveSteps_RaisesGlobalMultiplier()
        {
            var slice = CreateSlice(IdleArchetype.CapybaraGo, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 0, Wood = 0, SoftCurrency = 0
            });
            float before = _em.GetComponentData<IdleSliceState>(slice).GlobalMultiplier;
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();

            FireNarrative(sys, 0); // earn ExploreUnlocked — no seed
            for (int i = 0; i < 5; i++)
                FireNarrative(sys, 1);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(5, _em.GetComponentData<IdleNarrativeState>(slice).RoomOrStep);
            Assert.Greater(after.GlobalMultiplier, before, "Milestone mult must fire on live IdleNarrativeState path");
        }

        [Test]
        public void CapybaraGo_AutoTiles_AdvanceWithoutEvents()
        {
            var slice = CreateSlice(IdleArchetype.CapybaraGo, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 1, Wood = 0, SoftCurrency = 0, AutoTimer = 0f
            });
            float before = _em.GetComponentData<IdleSliceState>(slice).GlobalMultiplier;

            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 5.5f, dt: 0.5f);

            var narr = _em.GetComponentData<IdleNarrativeState>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(narr.RoomOrStep, 5, "Auto-tiles must raise RoomOrStep without narrative events");
            Assert.Greater(after.GlobalMultiplier, before, "Auto steps must fire milestone mult every 5 tiles");
        }

        [Test]
        public void CapybaraGo_ColdStart_AutoTilesRequireTakeStep()
        {
            // Mirrors bootstrap ExploreUnlocked=0: sim alone must not advance; one Take Step unlocks auto-tiles.
            var slice = CreateSlice(IdleArchetype.CapybaraGo, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 0, Wood = 0, SoftCurrency = 0, AutoTimer = 0f
            });

            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 3f, dt: 0.5f);
            Assert.AreEqual(0, _em.GetComponentData<IdleNarrativeState>(slice).RoomOrStep,
                "Cold-start sim must not auto-advance while ExploreUnlocked=0");

            var narrSys = _world.CreateSystem<IdleNarrativeActionSystem>();
            FireNarrative(narrSys, 0); // Take Step → unlock
            Assert.AreEqual(1, _em.GetComponentData<IdleNarrativeState>(slice).ExploreUnlocked);

            PumpSim(sim, 2.5f, dt: 0.5f);
            Assert.GreaterOrEqual(_em.GetComponentData<IdleNarrativeState>(slice).RoomOrStep, 2,
                "After Take Step unlock, auto-tiles must advance RoomOrStep");
        }

        [Test]
        public void LegendOfMushroom_Stage_PersistsRoundTrip_AutoLampStillFires()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.LegendOfMushroom);
            GameProgressData.SaveIdleSlice(
                (int)IdleArchetype.LegendOfMushroom, 15, 0, 1f, 1, 1, 0, 0,
                gachaStage: 1, gachaPullCount: 3, gachaBestRarity: 2);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.LegendOfMushroom,
                out _, out _, out _, out _, out _, out _, out _,
                out _, out _, out _, out _, out _, out _, out _, out _,
                out var stage, out var pulls, out var rarity,
                out _, out _, out _, out _));
            Assert.AreEqual(1, stage, "LoM Stage must survive reload");
            Assert.AreEqual(3, pulls);
            Assert.AreEqual(2, rarity);

            var slice = CreateSlice(IdleArchetype.LegendOfMushroom, 15);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = pulls, PullCost = 10, BestRarity = rarity, Stage = stage, AutoTimer = 0f
            });
            var sim = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(sim, 5f, dt: 0.5f);
            Assert.GreaterOrEqual(_em.GetComponentData<IdleGachaState>(slice).PullCount, 5,
                "Restored Stage≥1 must still sustain auto-lamp pulls");
            GameProgressData.ClearIdleSlice((int)IdleArchetype.LegendOfMushroom);
        }

        [Test]
        public void CapybaraGo_ExploreUnlocked_PersistsRoundTrip()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CapybaraGo);
            GameProgressData.SaveIdleSlice(
                (int)IdleArchetype.CapybaraGo, 0, 0, 1.5f, 2, 1, 0, 0,
                narrRoomOrStep: 7, narrExploreUnlocked: 1, narrSoftCurrency: 12.5);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.CapybaraGo,
                out _, out _, out var mult, out var level, out _, out _, out _,
                out _, out _, out _, out _, out _, out _, out _, out _,
                out _, out _, out _,
                out var step, out var explore, out var soft, out _));
            Assert.AreEqual(1, explore, "ExploreUnlocked must survive reload");
            Assert.AreEqual(7, step);
            Assert.AreEqual(12.5, soft, 0.001);
            Assert.AreEqual(1.5f, mult, 0.001f);
            Assert.AreEqual(2, level);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CapybaraGo);
        }

        [Test]
        public void IdleHeroes_AfkChestSeconds_PersistsRoundTrip()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.IdleHeroes);
            GameProgressData.SaveIdleSlice(
                (int)IdleArchetype.IdleHeroes, 40, 0, 1f, 3, 2, 2, 0,
                gachaStage: 2, gachaPullCount: 6, gachaBestRarity: 4,
                afkChestSeconds: 8.5f);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.IdleHeroes,
                out _, out _, out _, out _, out _, out _, out _,
                out _, out _, out _, out _, out _, out _, out _, out _,
                out var stage, out var pulls, out var rarity,
                out _, out _, out _, out var chest));
            Assert.AreEqual(8.5f, chest, 0.01f, "IH AfkChestSeconds must survive reload");
            Assert.AreEqual(2, stage);
            Assert.AreEqual(6, pulls);
            Assert.AreEqual(4, rarity);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.IdleHeroes);
        }

        [Test]
        public void IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach()
        {
            int arch = (int)IdleArchetype.IdleHeroes;
            GameProgressData.ClearIdleSlice(arch);

            // Legacy path: PassiveRate prefs only (no CombatHeroDps key) — attach must lift HeroDps.
            GameProgressData.SaveIdleSlice(
                arch, 40, 0, 1f, 3, 2, 7.5, 0,
                gachaStage: 2, gachaPullCount: 4, gachaBestRarity: 3,
                afkChestSeconds: 1f);

            var boot = SpawnBootstrap(IdleArchetype.IdleHeroes);
            try
            {
                var reloaded = _em.GetComponentData<IdleSliceState>(boot.SliceEntity);
                var reCombat = _em.GetComponentData<IdleCombatState>(boot.SliceEntity);
                Assert.AreEqual(7.5, reloaded.PassiveRate, 0.01, "PassiveRate must survive prefs round-trip");
                Assert.GreaterOrEqual(reCombat.HeroDps, 7.5 - 0.01,
                    "Attach must restore HeroDps from PassiveRate, not hardcode bare 3");
                Assert.Greater(reCombat.HeroDps, 3.0 + 0.01,
                    "Reloaded HeroDps must exceed cold-start floor when PassiveRate reflects gacha");
            }
            finally
            {
                Object.DestroyImmediate(boot.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void IdleHeroes_HeroDps_SurvivePersistNowReload()
        {
            int arch = (int)IdleArchetype.IdleHeroes;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.IdleHeroes);
            var slice = boot.SliceEntity;
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PassiveRate = 8.0;
            st.PrimaryCurrency = 40;
            _em.SetComponentData(slice, st);
            var combat = _em.GetComponentData<IdleCombatState>(slice);
            combat.HeroDps = 8.0;
            _em.SetComponentData(slice, combat);
            double preSaveDps = combat.HeroDps;
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.IdleHeroes);
            try
            {
                var reCombat = _em.GetComponentData<IdleCombatState>(boot2.SliceEntity);
                Assert.GreaterOrEqual(reCombat.HeroDps, preSaveDps - 0.01,
                    "PersistNow reload must keep HeroDps ≥ pre-save (combat prefs or PassiveRate)");
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void IdleHeroes_CombatState_PersistsRoundTrip_ZoneNotRebuiltFromStageLevel()
        {
            // R4-F1: Stage-inflated Level=8 with live Zone=5 must reload Zone=5 (not 8) + mid-fight HP/DPS.
            int arch = (int)IdleArchetype.IdleHeroes;
            GameProgressData.ClearIdleSlice(arch);

            var boot = SpawnBootstrap(IdleArchetype.IdleHeroes);
            var slice = boot.SliceEntity;
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ProgressionLevel = 8;
            st.PrimaryCurrency = 40;
            _em.SetComponentData(slice, st);
            _em.SetComponentData(slice, new IdleCombatState
            {
                TapDamage = 2,
                HeroDps = 12,
                Zone = 5,
                GoldPerKill = 15,
                EnemyHp = 40,
                EnemyMaxHp = 145
            });
            boot.PersistNow();
            Object.DestroyImmediate(boot.gameObject);

            var boot2 = SpawnBootstrap(IdleArchetype.IdleHeroes);
            try
            {
                var reloaded = _em.GetComponentData<IdleSliceState>(boot2.SliceEntity);
                Assert.AreEqual(8, reloaded.ProgressionLevel, "Stage-inflated Level must still load");
                var combat = _em.GetComponentData<IdleCombatState>(boot2.SliceEntity);
                Assert.AreEqual(5, combat.Zone, "Zone must not teleport to Stage-inflated Level=8");
                Assert.AreEqual(40f, combat.EnemyHp, 0.01f, "Mid-fight EnemyHp must survive reload");
                Assert.AreEqual(145f, combat.EnemyMaxHp, 0.01f, "EnemyMaxHp must match Zone scale");
                Assert.AreEqual(12.0, combat.HeroDps, 0.01, "HeroDps must survive reload");
                Assert.AreEqual(2.0, combat.TapDamage, 0.01);
                Assert.AreEqual(15.0, combat.GoldPerKill, 0.01);
            }
            finally
            {
                Object.DestroyImmediate(boot2.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void ADarkRoom_Craft_SpendsWoodForPassive()
        {
            var slice = CreateSlice(IdleArchetype.ADarkRoom, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 0, Wood = 0, SoftCurrency = 0
            });
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();
            for (int i = 0; i < 5; i++)
                FireNarrative(sys, 0);

            Assert.AreEqual(5, _em.GetComponentData<IdleNarrativeState>(slice).Wood);
            FireNarrative(sys, 2); // craft spends 3 wood
            FireNarrative(sys, 2); // second craft — only 2 wood left, no-op

            var narr = _em.GetComponentData<IdleNarrativeState>(slice);
            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(2, narr.Wood);
            Assert.AreEqual(0.5, after.PassiveRate, 0.01, "One craft bumps PassiveRate");
        }

        [Test]
        public void IdleMiner_Prestige_ReLocksManager_BuyAloneDoesNotAutomate()
        {
            var slice = CreateSlice(IdleArchetype.IdleMinerTycoon, 200);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 0, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = true, IsAutomated = false
            });
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1, HireCost = 100, IsHired = false
            });
            _em.AddComponent<PrestigeEventComponent>(slice);
            _em.SetComponentEnabled<PrestigeEventComponent>(slice, false);

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();
            var hireEvt = _em.CreateEntity();
            _em.AddComponentData(hireEvt, new IdleHireManagerEvent { TargetGeneratorId = 1 });
            hireSys.Update(_world.Unmanaged);

            var genHired = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.IsTrue(genHired.IsAutomated);
            Assert.IsTrue(genHired.RequiresManager, "Hire must keep RequiresManager for prestige re-lock");

            var funded = _em.GetComponentData<IdleSliceState>(slice);
            funded.PrimaryCurrency = 100;
            _em.SetComponentData(slice, funded);
            _em.SetComponentEnabled<PrestigeEventComponent>(slice, true);
            _world.CreateSystem<PrestigeSystem>().Update(_world.Unmanaged);

            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.IsTrue(gen.RequiresManager);
            Assert.IsFalse(gen.IsAutomated);
            Assert.IsFalse(_em.GetComponentData<IdleManager>(slice).IsHired);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).PassiveRate);

            var post = _em.GetComponentData<IdleSliceState>(slice);
            post.PrimaryCurrency = 50;
            _em.SetComponentData(slice, post);
            var buy2 = _em.CreateEntity();
            _em.AddComponentData(buy2, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).PassiveRate,
                "Buy shaft alone after prestige must not raise PassiveRate");

            post = _em.GetComponentData<IdleSliceState>(slice);
            post.PrimaryCurrency = 200;
            _em.SetComponentData(slice, post);
            var hire2 = _em.CreateEntity();
            _em.AddComponentData(hire2, new IdleHireManagerEvent { TargetGeneratorId = 1 });
            hireSys.Update(_world.Unmanaged);
            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PassiveRate, 0);
        }

        [Test]
        public void Melvor_OfflineCatchUp_BanksPendingClaimCapped()
        {
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 1.0,
                ProgressionLevel = 0,
                SkillXp = 0,
                PendingClaim = 0,
                HasOfflineClaim = false
            };

            double gained = IdleOfflineCatchUp.Apply(ref state, 60);
            Assert.AreEqual(60.0, gained, 0.001);
            Assert.AreEqual(60.0, state.PendingClaim, 0.001);
            Assert.AreEqual(0.0, state.PrimaryCurrency, 0.001);
            Assert.IsTrue(state.HasOfflineClaim);

            state.PendingClaim = 0;
            state.HasOfflineClaim = false;
            double capped = IdleOfflineCatchUp.Apply(ref state, IdleOfflineCatchUp.CapSeconds + 3600);
            Assert.AreEqual(IdleOfflineCatchUp.CapSeconds, capped, 0.001);
        }

        [Test]
        public void Melvor_ClaimOffline_NoDemoWithoutPending_PaysCatchUp()
        {
            var empty = CreateSlice(IdleArchetype.MelvorIdle, 0);
            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var noOp = _em.CreateEntity();
            _em.AddComponentData(noOp, new IdleClaimOfflineEvent { TargetSlice = empty });
            claimSys.Update(_world.Unmanaged);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(empty).PrimaryCurrency,
                "Melvor claim without catch-up must not demo-grant");

            var st = _em.GetComponentData<IdleSliceState>(empty);
            st.PendingClaim = 40;
            st.HasOfflineClaim = true;
            _em.SetComponentData(empty, st);
            var pay = _em.CreateEntity();
            _em.AddComponentData(pay, new IdleClaimOfflineEvent { TargetSlice = empty });
            claimSys.Update(_world.Unmanaged);
            var after = _em.GetComponentData<IdleSliceState>(empty);
            Assert.AreEqual(40.0, after.PrimaryCurrency, 0.001);
            Assert.AreEqual(0.0, after.PendingClaim, 0.001);
            Assert.IsFalse(after.HasOfflineClaim);
        }

        [Test]
        public void EggInc_OfflineCatchUp_AddsPrimaryCurrency()
        {
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.EggInc,
                PrimaryCurrency = 10,
                GlobalMultiplier = 2f,
                PassiveRate = 5
            };
            double gained = IdleOfflineCatchUp.Apply(ref state, 10);
            Assert.AreEqual(100.0, gained, 0.001);
            Assert.AreEqual(110.0, state.PrimaryCurrency, 0.001);
            Assert.AreEqual(0.0, state.PendingClaim, 0.001);
            Assert.IsFalse(state.HasOfflineClaim);
        }

        [Test]
        public void Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds()
        {
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 1.0,
                AfkChestSeconds = 0f,
                PendingClaim = 0,
                HasOfflineClaim = false
            };
            IdleOfflineCatchUp.Apply(ref state, 120);
            Assert.Greater(state.PendingClaim, 0);
            Assert.AreEqual(0f, state.AfkChestSeconds, 0.001f);
        }

        [Test]
        public void PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp()
        {
            // Acceptance: OS Init with no slices must not wipe T−1h; Melvor CatchUp still banks PendingClaim.
            string stamped = System.DateTime.UtcNow.AddHours(-1)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamped;

            var offline = _world.CreateSystem<OfflineSimulationSystem>();
            offline.Update(_world.Unmanaged);
            Assert.AreEqual(stamped, GameProgressData.LastIdleUpdateTime,
                "OfflineSimulation must not wipe LastIdleUpdateTime before bootstrap catch-up");

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 1.0,
                ProgressionLevel = 1,
                SkillXp = 0,
                PendingClaim = 0,
                HasOfflineClaim = false,
                AfkChestSeconds = 0f
            };
            double elapsed = (System.DateTime.UtcNow -
                System.DateTime.Parse(stamped, null, System.Globalization.DateTimeStyles.RoundtripKind)).TotalSeconds;
            double gained = IdleOfflineCatchUp.Apply(ref state, elapsed);
            Assert.Greater(gained, 0);
            Assert.Greater(state.PendingClaim, 0);
            Assert.AreEqual(0f, state.AfkChestSeconds, 0.001f);
        }

        [Test]
        public void IdleMiner_OfflineCatchUp_AddsPrimaryCurrency_NoPendingClaim()
        {
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.IdleMinerTycoon,
                PrimaryCurrency = 5,
                GlobalMultiplier = 1f,
                PassiveRate = 3
            };
            double gained = IdleOfflineCatchUp.Apply(ref state, 20);
            Assert.AreEqual(60.0, gained, 0.001);
            Assert.AreEqual(65.0, state.PrimaryCurrency, 0.001);
            Assert.AreEqual(0.0, state.PendingClaim, 0.001);
            Assert.IsFalse(state.HasOfflineClaim);
        }

        [Test]
        public void CatsAndSoup_OfflineCatchUp_StationWorkers_AddsPrimary()
        {
            // 2 workers × 1.5 OutputPerWorker × 10s / 1s Interval = 30 Primary (not Pending).
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.CatsAndSoup,
                PrimaryCurrency = 1,
                GlobalMultiplier = 1f,
                PassiveRate = 0,
                AssignedWorkers = 2,
                PendingClaim = 0,
                HasOfflineClaim = false
            };
            double gained = IdleOfflineCatchUp.Apply(ref state, 10);
            Assert.AreEqual(30.0, gained, 0.001);
            Assert.AreEqual(31.0, state.PrimaryCurrency, 0.001);
            Assert.AreEqual(0.0, state.PendingClaim, 0.001);
            Assert.IsFalse(state.HasOfflineClaim);

            state.AssignedWorkers = 0;
            state.PrimaryCurrency = 0;
            Assert.AreEqual(0.0, IdleOfflineCatchUp.Apply(ref state, 60), 0.001,
                "Zero workers must grant 0 (D23 stamp preserved on ApplyPersistedElapsed)");
        }

        [Test]
        public void FalloutShelter_OfflineCatchUp_StationWorkers_BanksPendingClaim()
        {
            // 1 worker × 1.5 × GlobalMultiplier 2 × 8s = 24 PendingClaim.
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.FalloutShelter,
                PrimaryCurrency = 0,
                GlobalMultiplier = 2f,
                PassiveRate = 0,
                AssignedWorkers = 1,
                PendingClaim = 0,
                HasOfflineClaim = false
            };
            double gained = IdleOfflineCatchUp.Apply(ref state, 8);
            Assert.AreEqual(24.0, gained, 0.001);
            Assert.AreEqual(24.0, state.PendingClaim, 0.001);
            Assert.AreEqual(0.0, state.PrimaryCurrency, 0.001);
            Assert.IsTrue(state.HasOfflineClaim);
        }

        [Test]
        public void NekoAtsume_OfflineCatchUp_AccruesCheckInCats()
        {
            // 50s / 5s interval = 10 cats; Primary untouched; claim flag set (D26).
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.NekoAtsume,
                PrimaryCurrency = 20,
                GlobalMultiplier = 1f,
                PassiveRate = 0,
                CheckInCats = 0,
                HasOfflineClaim = false
            };
            double gained = IdleOfflineCatchUp.Apply(ref state, 50);
            Assert.AreEqual(10.0, gained, 0.001);
            Assert.AreEqual(10, state.CheckInCats);
            Assert.AreEqual(20.0, state.PrimaryCurrency, 0.001, "Neko CatchUp must not double-bank Primary");
            Assert.IsTrue(state.HasOfflineClaim);

            // Cap 20: 18 + floor(60/5)=12 → only +2.
            state.CheckInCats = 18;
            state.HasOfflineClaim = false;
            Assert.AreEqual(2.0, IdleOfflineCatchUp.Apply(ref state, 60), 0.001);
            Assert.AreEqual(20, state.CheckInCats);
            Assert.IsTrue(state.HasOfflineClaim);

            // Already full / sub-interval → 0 (D23 stamp preserved on ApplyPersistedElapsed).
            state.HasOfflineClaim = false;
            Assert.AreEqual(0.0, IdleOfflineCatchUp.Apply(ref state, 120), 0.001,
                "Full buffer must grant 0");
            Assert.AreEqual(0.0, IdleOfflineCatchUp.Apply(ref state, 4), 0.001,
                "Sub-interval must grant 0");
        }

        [Test]
        public void Melvor_PendingClaim_SurvivesPersistAndColdReload()
        {
            // R3 P0: catch-up banks PendingClaim then PersistNow stamps time — claim must survive reload.
            int arch = (int)IdleArchetype.MelvorIdle;
            GameProgressData.ClearIdleSlice(arch);

            GameProgressData.SaveIdleSlice(
                arch, 0, 0, 1f, 1, 1, 1.0, 0,
                pendingClaim: 0, hasOfflineClaim: false);
            string stamped = System.DateTime.UtcNow.AddHours(-1)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamped;

            // Mirror BuildInitialState load + post-attach ApplyPersistedElapsed.
            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                arch,
                out var primary, out _, out var mult, out var level, out _, out var passive,
                out _, out _, out _, out _, out _, out _, out _, out var pending, out var hasClaim));
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = primary,
                GlobalMultiplier = mult > 0f ? mult : 1f,
                ProgressionLevel = level,
                PassiveRate = passive,
                PendingClaim = pending,
                HasOfflineClaim = hasClaim,
                SkillXp = 0
            };
            double gained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref state);
            Assert.Greater(gained, 3000, "1h Melvor catch-up must bank PendingClaim");
            Assert.Greater(state.PendingClaim, 3000);
            Assert.IsTrue(state.HasOfflineClaim);
            Assert.AreNotEqual(stamped, GameProgressData.LastIdleUpdateTime,
                "Positive grant must stamp LastIdleUpdateTime");
            double banked = state.PendingClaim;
            int levelAfterCatchUp = state.ProgressionLevel;

            // PersistNow / quit mirror — stamps now again and must write PendingClaim.
            GameProgressData.SaveIdleSlice(
                arch,
                state.PrimaryCurrency,
                state.PrestigeCurrency,
                state.GlobalMultiplier,
                state.ProgressionLevel,
                1,
                state.PassiveRate,
                0,
                pendingClaim: state.PendingClaim,
                hasOfflineClaim: state.HasOfflineClaim);

            // Cold reload with no new AFK window: restore PendingClaim; ApplyPersistedElapsed ~0.
            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                arch,
                out primary, out _, out mult, out level, out _, out passive,
                out _, out _, out _, out _, out _, out _, out _, out pending, out hasClaim));
            var reloaded = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = primary,
                GlobalMultiplier = mult > 0f ? mult : 1f,
                ProgressionLevel = level,
                PassiveRate = passive,
                PendingClaim = pending,
                HasOfflineClaim = hasClaim
            };
            Assert.AreEqual(banked, reloaded.PendingClaim, 0.01,
                "PendingClaim must restore from SaveIdleSlice without a second AFK window");
            Assert.IsTrue(reloaded.HasOfflineClaim);
            Assert.AreEqual(levelAfterCatchUp, reloaded.ProgressionLevel);

            double secondGain = IdleOfflineCatchUp.ApplyPersistedElapsed(ref reloaded);
            Assert.Less(secondGain, 5.0, "Just-stamped reload must not re-accrue a full AFK window");
            Assert.AreEqual(banked, reloaded.PendingClaim, 5.0,
                "Cold reload must keep catch-up PendingClaim durable");
        }

        [Test]
        public void Melvor_BootstrapLoadPath_CatchUpBanksPendingAndSkillLevel()
        {
            // R3 required: timestamp + TryLoad + ApplyPersistedElapsed (not bare Apply alone).
            int arch = (int)IdleArchetype.MelvorIdle;
            GameProgressData.ClearIdleSlice(arch);
            GameProgressData.SaveIdleSlice(
                arch, 0, 0, 1f, 1, 1, 1.0, 0,
                pendingClaim: 0, hasOfflineClaim: false);
            GameProgressData.LastIdleUpdateTime = System.DateTime.UtcNow.AddSeconds(-120)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                arch,
                out var primary, out _, out var mult, out var level, out _, out var passive,
                out _, out _, out _, out _, out _, out _, out _, out var pending, out var hasClaim));

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = primary,
                GlobalMultiplier = mult > 0f ? mult : 1f,
                ProgressionLevel = level > 0 ? level : 1,
                PassiveRate = passive,
                PendingClaim = pending,
                HasOfflineClaim = hasClaim,
                SkillXp = 0
            };
            IdleOfflineCatchUp.EnsureMelvorPassiveDefault(ref state);
            int levelBefore = state.ProgressionLevel;
            double gained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref state);

            Assert.Greater(gained, 100);
            Assert.Greater(state.PendingClaim, 100);
            Assert.AreEqual(0.0, state.PrimaryCurrency, 0.001, "Melvor banks PendingClaim, not Primary");
            Assert.IsTrue(state.HasOfflineClaim);
            Assert.GreaterOrEqual(state.ProgressionLevel, levelBefore,
                "Skill ticks from catch-up must be applied on bootstrap load path");
        }

        [Test]
        public void Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending()
        {
            // R5 P1: live IdleSliceBootstrap.TrySpawn (not hand-mirrored Attach→CatchUp→Sync→Save).
            // Prefs + T−300s → TrySpawn must CatchUp, D33 sync node, R4 PersistNow Pending (+ R5 SkillXp).
            int arch = (int)IdleArchetype.MelvorIdle;
            GameProgressData.ClearIdleSlice(arch);
            GameProgressData.SaveIdleSlice(
                arch, 0, 0, 1f, 1, 1, 1.0, 0,
                pendingClaim: 0, hasOfflineClaim: false, skillXp: 0);
            GameProgressData.LastIdleUpdateTime = System.DateTime.UtcNow.AddSeconds(-300)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);

            var boot = SpawnBootstrap(IdleArchetype.MelvorIdle);
            try
            {
                var slice = boot.SliceEntity;
                Assert.IsTrue(_em.HasComponent<IdleSkillNode>(slice), "Melvor TrySpawn must attach IdleSkillNode");
                var state = _em.GetComponentData<IdleSliceState>(slice);
                var skill = _em.GetComponentData<IdleSkillNode>(slice);

                Assert.AreEqual(state.ProgressionLevel, skill.Level,
                    "Live TrySpawn D33: IdleSkillNode.Level must match ProgressionLevel");
                Assert.AreEqual(state.SkillXp, skill.Xp,
                    "Live TrySpawn D33: IdleSkillNode.Xp must match slice SkillXp");
                Assert.Greater(state.PendingClaim, 0,
                    "300s Melvor CatchUp via TrySpawn must bank PendingClaim");
                Assert.Greater(state.ProgressionLevel, 1,
                    "300s skill ticks via TrySpawn must raise ProgressionLevel");

                double banked = state.PendingClaim;
                int xpAfter = state.SkillXp;
                int levelAfter = state.ProgressionLevel;

                // R4 PersistNow already ran inside TrySpawn when catchUpGained > 0 — no 2s wait.
                Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                    arch,
                    out _, out _, out _, out var loadedLevel, out _, out _,
                    out _, out _, out _, out _, out _, out _, out _, out var pending, out var hasClaim));
                Assert.AreEqual(banked, pending, 0.01,
                    "Cold TryLoad must restore Pending flushed by live TrySpawn PersistNow");
                Assert.IsTrue(hasClaim);
                Assert.AreEqual(levelAfter, loadedLevel);
                Assert.AreEqual(xpAfter, GameProgressData.LoadSkillXp(arch),
                    "R5: SkillXp must survive TrySpawn PersistNow cold load");
            }
            finally
            {
                Object.DestroyImmediate(boot.gameObject);
                GameProgressData.ClearIdleSlice(arch);
            }
        }

        [Test]
        public void Melvor_AttachThenCatchUp_SyncsSkillNodeAndPersistsPending()
        {
            // R4 P0/P1 / D33: Attach IdleSkillNode → CatchUp → SyncSkillNode → immediate Save
            // asserts node Level/Xp match slice + Pending durable without waiting for 2s autosave.
            int arch = (int)IdleArchetype.MelvorIdle;
            GameProgressData.ClearIdleSlice(arch);
            GameProgressData.SaveIdleSlice(
                arch, 0, 0, 1f, 1, 1, 1.0, 0,
                pendingClaim: 0, hasOfflineClaim: false);
            GameProgressData.LastIdleUpdateTime = System.DateTime.UtcNow.AddSeconds(-300)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);

            var slice = CreateSlice(IdleArchetype.MelvorIdle, 0);
            var state = _em.GetComponentData<IdleSliceState>(slice);
            state.ProgressionLevel = 1;
            state.SkillXp = 0;
            state.PassiveRate = 1.0;
            state.GlobalMultiplier = 1f;
            state.PendingClaim = 0;
            state.HasOfflineClaim = false;
            _em.SetComponentData(slice, state);

            // Mirror AttachArchetypeExtras Melvor arm (pre-CatchUp level).
            _em.AddComponentData(slice, new IdleSkillNode
            {
                SkillId = 1,
                Level = 1,
                Xp = 0,
                XpToLevel = 25,
                TickInterval = 1f,
                Timer = 0f,
                IsActive = true
            });

            state = _em.GetComponentData<IdleSliceState>(slice);
            double gained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref state);
            _em.SetComponentData(slice, state);

            Assert.Greater(gained, 200, "300s Melvor CatchUp must bank PendingClaim");
            Assert.Greater(state.PendingClaim, 200);
            Assert.Greater(state.ProgressionLevel, 1, "300s skill ticks must raise ProgressionLevel");

            var stale = _em.GetComponentData<IdleSkillNode>(slice);
            Assert.AreEqual(1, stale.Level, "Pre-sync node must still be attach-time level (D33 orphan)");

            IdleOfflineCatchUp.SyncSkillNodeFromSlice(ref stale, in state);
            _em.SetComponentData(slice, stale);

            var skill = _em.GetComponentData<IdleSkillNode>(slice);
            Assert.AreEqual(state.ProgressionLevel, skill.Level,
                "SyncSkillNodeFromSlice must match post-CatchUp ProgressionLevel");
            Assert.AreEqual(state.SkillXp, skill.Xp);
            Assert.AreEqual(IdleOfflineCatchUp.XpToLevelFor(state.ProgressionLevel), skill.XpToLevel);

            // P1: Persist immediately after stamp (bootstrap PersistNow mirror — no 2s wait).
            double banked = state.PendingClaim;
            int levelAfterCatchUp = state.ProgressionLevel;
            GameProgressData.SaveIdleSlice(
                arch,
                state.PrimaryCurrency,
                state.PrestigeCurrency,
                state.GlobalMultiplier,
                state.ProgressionLevel,
                1,
                state.PassiveRate,
                0,
                pendingClaim: state.PendingClaim,
                hasOfflineClaim: state.HasOfflineClaim);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                arch,
                out _, out _, out _, out var loadedLevel, out _, out _,
                out _, out _, out _, out _, out _, out _, out _, out var pending, out var hasClaim));
            Assert.AreEqual(banked, pending, 0.01,
                "PendingClaim must restore from immediate post-CatchUp SaveIdleSlice");
            Assert.IsTrue(hasClaim);
            Assert.AreEqual(levelAfterCatchUp, loadedLevel);

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 1f, dt: 1f);
            var afterSim = _em.GetComponentData<IdleSliceState>(slice);
            var skillAfter = _em.GetComponentData<IdleSkillNode>(slice);
            Assert.GreaterOrEqual(afterSim.ProgressionLevel, levelAfterCatchUp,
                "One sim tick must not drop ProgressionLevel below CatchUp level");
            Assert.GreaterOrEqual(skillAfter.Level, levelAfterCatchUp);
        }
    }
}

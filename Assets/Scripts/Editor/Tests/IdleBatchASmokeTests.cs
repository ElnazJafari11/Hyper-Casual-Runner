using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode smoke for Batch A idle MVPs (Cookie, AdvCap, Clicker Heroes, Paperclips, Antimatter).
    /// Exercises matrix core verbs with spend asserts and at least one sim beat where CPS matters.
    /// </summary>
    [TestFixture]
    public class IdleBatchASmokeTests
    {
        private World _world;
        private EntityManager _em;

        [SetUp]
        public void SetUp()
        {
            _world = new World("IdleBatchASmokeWorld");
            World.DefaultGameObjectInjectionWorld = _world;
            _em = _world.EntityManager;
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.AntimatterDimensions);
        }

        [TearDown]
        public void TearDown()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.AntimatterDimensions);
            if (_world != null && _world.IsCreated) _world.Dispose();
        }

        private Entity CreateSlice(IdleArchetype arch, double currency, bool withGen = true, bool requiresManager = false,
            double baseCost = 15)
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
                EnergyPool = 50
            });
            _em.AddComponentData(e, new CurrentRunStats { CurrentGold = currency, BaseDamage = 5 });
            if (withGen)
            {
                _em.AddComponentData(e, new BuyableGenerator
                {
                    GeneratorId = 1,
                    OwnedCount = 0,
                    BaseCost = baseCost,
                    CostGrowth = 1.15f,
                    BaseCps = 1,
                    RequiresManager = requiresManager,
                    IsAutomated = !requiresManager
                });
            }
            return e;
        }

        private void FireClick(SystemHandle clickSys, Entity targetSlice = default)
        {
            var clickEvt = _em.CreateEntity();
            _em.AddComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f, TargetSlice = targetSlice });
            clickSys.Update(_world.Unmanaged);
        }

        private void FireBuy(SystemHandle buySys, Entity targetSlice, int generatorId = 1, int amount = 1)
        {
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent
            {
                GeneratorId = generatorId,
                Amount = amount,
                TargetSlice = targetSlice
            });
            buySys.Update(_world.Unmanaged);
        }

        private void FireHire(SystemHandle hireSys, Entity targetSlice, int targetGeneratorId = 1)
        {
            var hireEvt = _em.CreateEntity();
            _em.AddComponentData(hireEvt, new IdleHireManagerEvent
            {
                TargetGeneratorId = targetGeneratorId,
                TargetSlice = targetSlice
            });
            hireSys.Update(_world.Unmanaged);
        }

        private void FirePhase(SystemHandle phaseSys, Entity targetSlice)
        {
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdlePhaseShiftEvent { TargetSlice = targetSlice });
            phaseSys.Update(_world.Unmanaged);
        }

        private void FirePrestigeEvt(SystemHandle prestigeSys, Entity targetSlice)
        {
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new PrestigeEventComponent { TargetSlice = targetSlice });
            prestigeSys.Update(_world.Unmanaged);
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

        [Test]
        public void CookieClicker_ClickThenBuy_RaisesCurrencyAndOwnedGens()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 0);
            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();

            // ClickPower=5, BaseCost=15 → exactly 3 clicks fund the first buy (no injected currency).
            for (int i = 0; i < 3; i++)
                FireClick(clickSys, slice);

            var afterClick = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(15.0, afterClick.PrimaryCurrency, 0.001, "Three clicks should fund BaseCost=15");

            FireBuy(buySys, slice);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators, "Buy should own 1 generator");
            Assert.AreEqual(1, gen.OwnedCount, "BuyableGenerator.OwnedCount should mirror");
            Assert.AreEqual(0.0, afterBuy.PrimaryCurrency, 0.001, "Buy should spend full earned balance");
            Assert.Greater(afterBuy.PassiveRate, 0, "Automated generator should grant CPS");

            double beforeTick = afterBuy.PrimaryCurrency;
            PumpSim(simSys, 2f);
            var afterTick = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterTick.PrimaryCurrency, beforeTick, "IdleSliceSimulationSystem should accrue CPS");
        }

        [Test]
        public void AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate()
        {
            // Fund buy (15) + hire (100) without pre-owning the business.
            var slice = CreateSlice(IdleArchetype.AdventureCapitalist, 200, true, true);
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1,
                HireCost = 100,
                IsHired = false
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();

            FireBuy(buySys, slice);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators, "Must buy business before manager");
            Assert.AreEqual(1, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
            Assert.AreEqual(185.0, afterBuy.PrimaryCurrency, 0.001, "Business BaseCost=15 spent");
            Assert.AreEqual(0.0, afterBuy.PassiveRate, 0.001, "RequiresManager → no CPS until hire");

            FireHire(hireSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.ManagersHired);
            Assert.AreEqual(85.0, after.PrimaryCurrency, 0.001, "HireCost=100 deducted");
            Assert.Greater(after.PassiveRate, 0, "Manager should unlock automation CPS");
            Assert.IsTrue(_em.GetComponentData<IdleManager>(slice).IsHired);
            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).IsAutomated);
        }

        [Test]
        public void ClickerHeroes_TapKillThenBuyHero_AdvancesZoneAndDps()
        {
            // Prefab path: IdleCombatState is SoT. BaseCost=5 matches GoldPerKill so buy is kill-funded.
            var slice = CreateSlice(IdleArchetype.ClickerHeroes, 0, true, false, baseCost: 5);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 10,
                HeroDps = 1,
                Zone = 1,
                GoldPerKill = 5,
                EnemyHp = 5,
                EnemyMaxHp = 5
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ClickPower = 10;
            st.ProgressionLevel = 1;
            _em.SetComponentData(slice, st);

            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            FireClick(clickSys, slice);

            var combatAfterKill = _em.GetComponentData<IdleCombatState>(slice);
            var afterKill = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(combatAfterKill.Zone, 2, "Kill should advance combat Zone");
            Assert.Greater(afterKill.PrimaryCurrency, 0, "Kill should grant gold");
            Assert.AreEqual(combatAfterKill.Zone, afterKill.ProgressionLevel);
            Assert.AreEqual(combatAfterKill.EnemyMaxHp, combatAfterKill.EnemyHp, 0.01f, "Enemy respawns");

            double beforeBuy = afterKill.PrimaryCurrency;
            double beforeDps = combatAfterKill.HeroDps;
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            FireBuy(buySys, slice);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            var combatAfterBuy = _em.GetComponentData<IdleCombatState>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators, "Buy Heroes verb");
            Assert.AreEqual(1, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
            Assert.AreEqual(beforeBuy - 5.0, afterBuy.PrimaryCurrency, 0.001, "Hero cost spent");
            Assert.Greater(afterBuy.PassiveRate, 0, "Hero should raise passive DPS");
            Assert.GreaterOrEqual(combatAfterBuy.HeroDps, afterBuy.PassiveRate,
                "Buy must sync IdleCombatState.HeroDps to at least PassiveRate");
            Assert.GreaterOrEqual(combatAfterBuy.HeroDps, beforeDps, "HeroDps must not drop on buy");
        }

        [Test]
        public void ClickerHeroes_CombatDps_FractionalOverOneSecond()
        {
            var slice = CreateSlice(IdleArchetype.ClickerHeroes, 0, false);
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
        public void ClickerHeroes_PrestigeAtZeroGold_IsNoOp_AfterGoldResetsCombat()
        {
            var slice = CreateSlice(IdleArchetype.ClickerHeroes, 0, false);
            _em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 5, HeroDps = 8, Zone = 7, GoldPerKill = 19,
                EnemyHp = 40, EnemyMaxHp = 40
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.ProgressionLevel = 7;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<PrestigeSystem>();
            FirePrestigeEvt(sys, slice);

            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).PrestigeCurrency);
            Assert.AreEqual(7, _em.GetComponentData<IdleCombatState>(slice).Zone);

            st = _em.GetComponentData<IdleSliceState>(slice);
            st.PrimaryCurrency = 100;
            _em.SetComponentData(slice, st);

            FirePrestigeEvt(sys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            var combat = _em.GetComponentData<IdleCombatState>(slice);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.AreEqual(1, combat.Zone);
            Assert.AreEqual(20f, combat.EnemyHp, 0.01f);
            Assert.Greater(after.GlobalMultiplier, 1f);
        }

        [Test]
        public void Paperclips_ManufactureThenPhaseShift_ConvertsToPrestige()
        {
            var slice = CreateSlice(IdleArchetype.UniversalPaperclips, 0, false);
            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();

            // Manufacture via clicks until phase threshold (≥25).
            for (int i = 0; i < 6; i++)
                FireClick(clickSys, slice);

            var manufactured = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(manufactured.PrimaryCurrency, 25, "Manufacture must fund phase");

            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            FirePhase(phaseSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.Greater(after.GlobalMultiplier, 1f);
            Assert.GreaterOrEqual(after.PhaseIndex, 1);
        }

        [Test]
        public void Antimatter_BuyDimension_RaisesOwnedAndCps()
        {
            var slice = CreateSlice(IdleArchetype.AntimatterDimensions, 50);
            var before = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency;
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            FireBuy(buySys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.AreEqual(1, after.OwnedGenerators);
            Assert.AreEqual(1, gen.OwnedCount);
            Assert.AreEqual(before - 15.0, after.PrimaryCurrency, 0.001, "Dimension BaseCost spent");
            Assert.Greater(after.PassiveRate, 0);
        }

        [Test]
        public void GameProgressData_IdleSlice_RoundTripTwoArchetypes_NoKeyCollision()
        {
            GameProgressData.SaveIdleSlice((int)IdleArchetype.CookieClicker, 123.5, 4, 1.4f, 3, 2.5, 7.25, 2);
            GameProgressData.SaveIdleSlice((int)IdleArchetype.AntimatterDimensions, 999, 11, 2.0f, 8, 3.0, 4.5, 6);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.CookieClicker,
                out var primary, out var prestige, out var mult, out var level,
                out var click, out var passive, out var gens));
            Assert.AreEqual(123.5, primary, 0.001);
            Assert.AreEqual(4, prestige, 0.001);
            Assert.AreEqual(1.4f, mult, 0.001f);
            Assert.AreEqual(3, level);
            Assert.AreEqual(2.5, click, 0.001);
            Assert.AreEqual(7.25, passive, 0.001);
            Assert.AreEqual(2, gens);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.AntimatterDimensions,
                out var p2, out var pr2, out var m2, out var l2,
                out var c2, out var pa2, out var g2));
            Assert.AreEqual(999, p2, 0.001);
            Assert.AreEqual(11, pr2, 0.001);
            Assert.AreEqual(2.0f, m2, 0.001f);
            Assert.AreEqual(8, l2);
            Assert.AreEqual(3.0, c2, 0.001);
            Assert.AreEqual(4.5, pa2, 0.001);
            Assert.AreEqual(6, g2);

            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            Assert.IsFalse(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.CookieClicker,
                out _, out _, out _, out _, out _, out _, out _),
                "Clear must remove Cookie keys");
            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.AntimatterDimensions,
                out var still, out _, out _, out _, out _, out _, out _),
                "Antimatter keys must survive Cookie clear");
            Assert.AreEqual(999, still, 0.001);
        }

        [Test]
        public void A1_Cookie_SimCps_AppliesGlobalMultiplierOnce()
        {
            // Buy → sim path (not injected PassiveRate): Mult=2, BaseCps=1, buy 1 → 1s → +2 not ~4
            var slice = CreateSlice(IdleArchetype.CookieClicker, 15);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.GlobalMultiplier = 2f;
            _em.SetComponentData(slice, st);

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            FireBuy(buySys, slice);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators);
            Assert.AreEqual(1, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
            Assert.AreEqual(1.0, afterBuy.PassiveRate, 0.001, "Buy must set raw PassiveRate = BaseCps * Owned");
            Assert.AreEqual(0.0, afterBuy.PrimaryCurrency, 0.001, "First buy spends full 15");

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            PumpSim(simSys, 1f, 0.25f);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            // Expected: PassiveRate * Mult * 1s = 1 * 2 * 1 = 2 (not mult² = 4)
            Assert.AreEqual(2.0, after.PrimaryCurrency, 0.05, "CPS must apply GlobalMultiplier once");
            Assert.AreEqual(1.0, after.PassiveRate, 0.001, "PassiveRate stays raw");
        }

        [Test]
        public void A2_Cookie_Prestige_SingleConversion_NoForcedExtra()
        {
            const double currency = 200; // floor(sqrt(200/50)) = floor(2) = 2
            var slice = CreateSlice(IdleArchetype.CookieClicker, currency);
            double expected = IdlePrestigeMath.ConvertRunCurrency(currency);
            Assert.AreEqual(2.0, expected, 0.001);

            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            FirePrestigeEvt(prestigeSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(expected, after.PrestigeCurrency, 0.001, "Exactly one conversion, not conversion+1");
            Assert.AreEqual(0.0, after.PrimaryCurrency, 0.001);
        }

        [Test]
        public void PrestigeSystem_ZeroCurrency_DoesNotAward()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 0);
            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            FirePrestigeEvt(prestigeSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(0.0, after.PrestigeCurrency, 0.001, "Below threshold must not grant prestige");
        }

        [Test]
        public void A3_AdvCap_AfterPrestige_RequiresManager_BuyLeavesPassiveZero()
        {
            var slice = CreateSlice(IdleArchetype.AdventureCapitalist, 200, true, true);
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1,
                HireCost = 100,
                IsHired = false
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();
            var prestigeSys = _world.CreateSystem<PrestigeSystem>();

            FireBuy(buySys, slice);
            FireHire(hireSys, slice);

            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).IsAutomated);
            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PassiveRate, 0);

            // Fund angel threshold then prestige
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PrimaryCurrency = 100;
            _em.SetComponentData(slice, st);

            FirePrestigeEvt(prestigeSys, slice);

            var genAfter = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.IsTrue(genAfter.RequiresManager, "Angel reset must restore RequiresManager");
            Assert.IsFalse(genAfter.IsAutomated, "Automation cleared after reset");
            Assert.IsFalse(_em.GetComponentData<IdleManager>(slice).IsHired);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).ManagersHired);

            // Re-fund and buy without hire
            st = _em.GetComponentData<IdleSliceState>(slice);
            st.PrimaryCurrency = 50;
            _em.SetComponentData(slice, st);
            FireBuy(buySys, slice);

            Assert.AreEqual(0.0, _em.GetComponentData<IdleSliceState>(slice).PassiveRate, 0.001,
                "Buy without re-hire must leave PassiveRate at 0");
        }

        [Test]
        public void A4_AdvCap_HireWithZeroOwned_LeavesPassiveZero()
        {
            var slice = CreateSlice(IdleArchetype.AdventureCapitalist, 200, true, true);
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1,
                HireCost = 100,
                IsHired = false
            });

            Assert.AreEqual(0, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);

            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();
            FireHire(hireSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.ManagersHired);
            Assert.AreEqual(0.0, after.PassiveRate, 0.001, "Hire with OwnedCount==0 must not grant phantom CPS");
            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).IsAutomated);
            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).RequiresManager,
                "Hire must not clear RequiresManager");
        }

        [Test]
        public void A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost()
        {
            // Real reload: SaveIdleSlice → clear world → TryLoad → bootstrap SyncGeneratorOwnedCountFromState
            const int archId = (int)IdleArchetype.CookieClicker;
            GameProgressData.ClearIdleSlice(archId);
            // Stale Mult² Passive in save (9) must be rebuilt to raw 3 on load when automated.
            GameProgressData.SaveIdleSlice(
                archId,
                primaryCurrency: 100,
                prestigeCurrency: 0,
                globalMultiplier: 1f,
                progressionLevel: 0,
                clickPower: 1,
                passiveRate: 9,
                ownedGenerators: 3);

            using (var q = _em.CreateEntityQuery(ComponentType.ReadOnly<IdleSliceState>()))
                _em.DestroyEntity(q);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                archId,
                out var primary,
                out _,
                out var mult,
                out _,
                out var click,
                out var savedPassive,
                out var gens),
                "Reload must read PlayerPrefs written by SaveIdleSlice");
            Assert.AreEqual(3, gens);
            Assert.AreEqual(9.0, savedPassive, 0.001, "precondition: stale Mult² passive still on disk");

            var slice = CreateSlice(IdleArchetype.CookieClicker, primary);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.GlobalMultiplier = mult > 0f ? mult : 1f;
            st.ClickPower = click > 0 ? click : st.ClickPower;
            st.PassiveRate = savedPassive; // intentional stale until sync
            st.OwnedGenerators = gens;
            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            Assert.AreEqual(0, gen.OwnedCount, "precondition: component not yet synced");

            IdlePrestigeMath.SyncGeneratorOwnedCountFromState(ref gen, ref st, gens, automated: true);
            _em.SetComponentData(slice, gen);
            _em.SetComponentData(slice, st);

            Assert.AreEqual(3, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
            Assert.AreEqual(3.0, _em.GetComponentData<IdleSliceState>(slice).PassiveRate, 0.001,
                "Load sync must rebuild raw PassiveRate from Owned×BaseCps");

            double expectedCost = 15 * System.Math.Pow(1.15, 3);
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            FireBuy(buySys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(100 - expectedCost, after.PrimaryCurrency, 0.05,
                "Next buy cost must use BaseCost * growth^OwnedCount");
            Assert.AreEqual(4, _em.GetComponentData<BuyableGenerator>(slice).OwnedCount);
        }

        [Test]
        public void FirePrestige_DoesNotAlsoCreatePhaseEvent_PaperclipsPhaseOnly()
        {
            // Paperclips uses Phase only — PrestigeSystem path must not be required for phase.
            var slice = CreateSlice(IdleArchetype.UniversalPaperclips, 100);
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            var prestigeSys = _world.CreateSystem<PrestigeSystem>();

            FirePhase(phaseSys, slice);

            var afterPhase = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterPhase.PrestigeCurrency, 0);
            double prestigeAfterPhase = afterPhase.PrestigeCurrency;

            // Prestige with 0 currency must not add more
            FirePrestigeEvt(prestigeSys, slice);

            var afterPrestige = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(prestigeAfterPhase, afterPrestige.PrestigeCurrency, 0.001,
                "Phase then empty Prestige must not double-award");
        }

        [Test]
        public void AdventureCapitalist_AngelReset_ConvertsAndResetsManagers()
        {
            var slice = CreateSlice(IdleArchetype.AdventureCapitalist, 100, true, true);
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1,
                HireCost = 100,
                IsHired = true
            });
            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            gen.OwnedCount = 3;
            gen.IsAutomated = true;
            _em.SetComponentData(slice, gen);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.OwnedGenerators = 3;
            st.ManagersHired = 1;
            st.PassiveRate = 3;
            st.PhaseIndex = 0;
            _em.SetComponentData(slice, st);

            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            FirePrestigeEvt(prestigeSys, slice);
            // Phase with no event should be a no-op (Angel Reset is Prestige-only).
            phaseSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.AreEqual(0, after.OwnedGenerators);
            Assert.AreEqual(0, after.ManagersHired);
            Assert.AreEqual(0, after.PhaseIndex, "Angel Reset must not advance PhaseIndex");
            Assert.IsFalse(_em.GetComponentData<IdleManager>(slice).IsHired);
            Assert.IsTrue(_em.GetComponentData<BuyableGenerator>(slice).RequiresManager);
        }

        [Test]
        public void Antimatter_PhaseShift_IncrementsPhaseIndex()
        {
            var slice = CreateSlice(IdleArchetype.AntimatterDimensions, 100);
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            var prestigeSys = _world.CreateSystem<PrestigeSystem>();
            FirePhase(phaseSys, slice);
            // Empty prestige after phase (currency already 0) must not double-award.
            FirePrestigeEvt(prestigeSys, slice);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.PhaseIndex);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PrimaryCurrency);
        }
    }
}

using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode smoke for Batch A idle MVPs (Cookie, AdvCap, Clicker Heroes, Paperclips, Antimatter).
    /// Proves core verb + one progression beat without Play Mode.
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
        }

        [TearDown]
        public void TearDown()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            if (_world != null && _world.IsCreated) _world.Dispose();
        }

        private Entity CreateSlice(IdleArchetype arch, double currency, bool withGen = true, bool requiresManager = false)
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
                    BaseCost = 15,
                    CostGrowth = 1.15f,
                    BaseCps = 1,
                    RequiresManager = requiresManager,
                    IsAutomated = !requiresManager
                });
            }
            return e;
        }

        [Test]
        public void CookieClicker_ClickThenBuy_RaisesCurrencyAndOwnedGens()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 0);
            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();

            var clickEvt = _em.CreateEntity();
            _em.AddComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f });
            // grant enough for a buy via many clicks
            for (int i = 0; i < 20; i++)
            {
                if (!_em.HasComponent<IdleClickEvent>(clickEvt))
                {
                    clickEvt = _em.CreateEntity();
                    _em.AddComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f });
                }
                else
                {
                    _em.SetComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f });
                    _em.SetComponentEnabled<IdleClickEvent>(clickEvt, true);
                }
                clickSys.Update(_world.Unmanaged);
            }

            var afterClick = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(afterClick.PrimaryCurrency, 0, "Click should grant cookies");

            // Ensure affordable
            afterClick.PrimaryCurrency = 100;
            _em.SetComponentData(slice, afterClick);

            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, afterBuy.OwnedGenerators, "Buy should own 1 generator");
            Assert.Greater(afterBuy.PassiveRate, 0, "Automated generator should grant CPS");
        }

        [Test]
        public void AdventureCapitalist_HireManager_AutomatesPassiveRate()
        {
            var slice = CreateSlice(IdleArchetype.AdventureCapitalist, 200, true, true);
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1,
                HireCost = 100,
                IsHired = false
            });

            // Own one business first
            var gen = _em.GetComponentData<BuyableGenerator>(slice);
            gen.OwnedCount = 1;
            _em.SetComponentData(slice, gen);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.OwnedGenerators = 1;
            st.PassiveRate = 0;
            _em.SetComponentData(slice, st);

            var hireSys = _world.CreateSystem<IdleManagerHireSystem>();
            var hireEvt = _em.CreateEntity();
            _em.AddComponentData(hireEvt, new IdleHireManagerEvent { TargetGeneratorId = 1 });
            hireSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.ManagersHired);
            Assert.Greater(after.PassiveRate, 0, "Manager should unlock automation CPS");
            Assert.IsTrue(_em.GetComponentData<IdleManager>(slice).IsHired);
        }

        [Test]
        public void ClickerHeroes_TapKill_AdvancesZone()
        {
            var slice = CreateSlice(IdleArchetype.ClickerHeroes, 0, false);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.EnemyHp = 5;
            st.EnemyMaxHp = 5;
            st.ClickPower = 10;
            st.ProgressionLevel = 0;
            _em.SetComponentData(slice, st);

            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var clickEvt = _em.CreateEntity();
            _em.AddComponentData(clickEvt, new IdleClickEvent { Multiplier = 1f });
            clickSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(after.ProgressionLevel, 1, "Kill should advance zone/level");
            Assert.Greater(after.PrimaryCurrency, 0, "Kill should grant gold");
        }

        [Test]
        public void Paperclips_PhaseShift_ConvertsToPrestige()
        {
            var slice = CreateSlice(IdleArchetype.UniversalPaperclips, 100);
            var phaseSys = _world.CreateSystem<IdlePhaseShiftSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdlePhaseShiftEvent());
            phaseSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.PrestigeCurrency, 0);
            Assert.AreEqual(0, after.PrimaryCurrency);
            Assert.Greater(after.GlobalMultiplier, 1f);
        }

        [Test]
        public void Antimatter_BuyDimension_RaisesOwnedAndCps()
        {
            var slice = CreateSlice(IdleArchetype.AntimatterDimensions, 50);
            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleBuyGeneratorEvent { GeneratorId = 1, Amount = 1 });
            buySys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.OwnedGenerators);
            Assert.Greater(after.PassiveRate, 0);
        }

        [Test]
        public void GameProgressData_IdleSlice_RoundTrip()
        {
            GameProgressData.SaveIdleSlice((int)IdleArchetype.CookieClicker, 123.5, 4, 1.4f, 3, 2.5, 7.25, 2);
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
        }
    }
}

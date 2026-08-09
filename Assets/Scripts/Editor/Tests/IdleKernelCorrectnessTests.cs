using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// Kernel P0/P1 correctness gates from review_01_kernel (multi-slice, mult, offline, claim).
    /// </summary>
    [TestFixture]
    public class IdleKernelCorrectnessTests
    {
        private World _world;
        private EntityManager _em;

        [SetUp]
        public void SetUp()
        {
            _world = new World("IdleKernelCorrectnessWorld");
            World.DefaultGameObjectInjectionWorld = _world;
            _em = _world.EntityManager;
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.EggInc);
        }

        [TearDown]
        public void TearDown()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.EggInc);
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
                ClickPower = 5,
                PassiveRate = 0,
                OwnedGenerators = 0,
                MaxWorkers = 5,
                EnergyPool = 50
            });
            _em.AddComponentData(e, new CurrentRunStats { CurrentGold = currency, BaseDamage = 5 });
            return e;
        }

        [Test]
        public void Prestige_ZeroCurrency_IsNoOp()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 0);
            var sys = _world.CreateSystem<PrestigeSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new PrestigeEventComponent { TargetSlice = slice });
            sys.Update(_world.Unmanaged);

            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(slice).PrestigeCurrency);
        }

        [Test]
        public void BuyWithMult2_PassiveRateIsRaw_SimAppliesOnce()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 100);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.GlobalMultiplier = 2f;
            _em.SetComponentData(slice, st);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 0, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = false, IsAutomated = true
            });

            var buySys = _world.CreateSystem<IdleBuyGeneratorSystem>();
            var buyEvt = _em.CreateEntity();
            _em.AddComponentData(buyEvt, new IdleBuyGeneratorEvent
            {
                TargetSlice = slice, GeneratorId = 1, Amount = 1
            });
            buySys.Update(_world.Unmanaged);

            var afterBuy = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1.0, afterBuy.PassiveRate, 0.001, "PassiveRate must be BaseCps*Owned (not ×Mult)");

            var simSys = _world.CreateSystem<IdleSliceSimulationSystem>();
            float elapsed = 0f;
            while (elapsed < 1f - 1e-4f)
            {
                elapsed += 0.25f;
                _world.SetTime(new TimeData(elapsed, 0.25f));
                simSys.Update(_world.Unmanaged);
            }

            // Start after buy spent 15 → 85; + PassiveRate*Mult*1s = 1*2*1 = 2 → 87
            Assert.AreEqual(87.0, _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0.05);
        }

        [Test]
        public void TwoSlices_OneClick_OnlyMutatesTarget()
        {
            var a = CreateSlice(IdleArchetype.CookieClicker, 0);
            var b = CreateSlice(IdleArchetype.EggInc, 0);
            var clickSys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { TargetSlice = a, Multiplier = 1f });
            clickSys.Update(_world.Unmanaged);

            Assert.Greater(_em.GetComponentData<IdleSliceState>(a).PrimaryCurrency, 0);
            Assert.AreEqual(0, _em.GetComponentData<IdleSliceState>(b).PrimaryCurrency, 0.001);
        }

        [Test]
        public void SaveLoad_RestoresOwnedGeneratorsField()
        {
            GameProgressData.SaveIdleSlice(
                (int)IdleArchetype.CookieClicker, 50, 2, 1.2f, 1, 3, 4, 7,
                managersHired: 1, phaseIndex: 2, factionId: 1, energyAllocated: 5f, managerIsHired: true);

            Assert.IsTrue(GameProgressData.TryLoadIdleSlice(
                (int)IdleArchetype.CookieClicker,
                out _, out _, out _, out _, out _, out _, out var gens,
                out var managers, out var phase, out var faction, out var energy, out var hired));
            Assert.AreEqual(7, gens);
            Assert.AreEqual(1, managers);
            Assert.AreEqual(2, phase);
            Assert.AreEqual(1, faction);
            Assert.AreEqual(5f, energy, 0.001f);
            Assert.IsTrue(hired);
        }

        [Test]
        public void OfflineCatchup_IdleSlice_WithoutProducer_SetsPendingClaim()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PassiveRate = 2;
            st.GlobalMultiplier = 1f;
            _em.SetComponentData(slice, st);

            // Stamp last update 60s ago
            GameProgressData.LastIdleUpdateTime =
                System.DateTime.UtcNow.AddSeconds(-60).ToString("O", System.Globalization.CultureInfo.InvariantCulture);

            var offline = _world.CreateSystem<OfflineSimulationSystem>();
            offline.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.IsTrue(after.HasOfflineClaim);
            Assert.Greater(after.PendingClaim, 0, "Offline catchup must credit PendingClaim without ProducerComponent");
            // ~2 CPS * 60s = 120 (cap path still applies)
            Assert.AreEqual(120.0, after.PendingClaim, 5.0);
        }

        [Test]
        public void Claim_WithoutEvidence_IsNoOp()
        {
            var slice = CreateSlice(IdleArchetype.CookieClicker, 10);
            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent { TargetSlice = slice });
            claimSys.Update(_world.Unmanaged);

            Assert.AreEqual(10, _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0.001);
        }
    }
}

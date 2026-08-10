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
    /// Round 02: D14 stamp race, D15 Kernel B = IdleOfflineCatchUp, D16 claim conservation.
    /// Round 03: D23 zero-grant stamp, D25 CatchUp-after-Sync PassiveRate.
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
            GameProgressData.ClearIdleSlice((int)IdleArchetype.MelvorIdle);
            GameProgressData.LastIdleUpdateTime = "";
        }

        [TearDown]
        public void TearDown()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.CookieClicker);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.EggInc);
            GameProgressData.ClearIdleSlice((int)IdleArchetype.MelvorIdle);
            GameProgressData.LastIdleUpdateTime = "";
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
        public void OfflineCatchup_IdleSlice_WithoutProducer_UsesKernelBCatchUp()
        {
            // Kernel B authority is IdleOfflineCatchUp (not OfflineSimulationSystem).
            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 2,
                PendingClaim = 0,
                AfkChestSeconds = 0,
                HasOfflineClaim = false
            };

            double gained = IdleOfflineCatchUp.Apply(ref state, 60);
            Assert.AreEqual(120.0, gained, 0.001);
            Assert.AreEqual(120.0, state.PendingClaim, 0.001);
            Assert.AreEqual(0.0, state.PrimaryCurrency, 0.001);
            Assert.AreEqual(0f, state.AfkChestSeconds, 0.01f, "Melvor catch-up must not bump AfkChestSeconds");
            Assert.IsTrue(state.HasOfflineClaim);
        }

        [Test]
        public void OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime()
        {
            string stamp = System.DateTime.UtcNow.AddSeconds(-60)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamp;

            var offline = _world.CreateSystem<OfflineSimulationSystem>();
            offline.Update(_world.Unmanaged);

            Assert.AreEqual(stamp, GameProgressData.LastIdleUpdateTime,
                "D14: OS must not wipe AFK window when no producers were processed");
        }

        [Test]
        public void OfflineSimulation_ThenBootstrapCatchUp_StillAccruesPendingClaim()
        {
            // Play-order mirror: Init OS (empty) then Kernel B catch-up after slice exists.
            string stamp = System.DateTime.UtcNow.AddSeconds(-60)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamp;

            var offline = _world.CreateSystem<OfflineSimulationSystem>();
            offline.Update(_world.Unmanaged);
            Assert.AreEqual(stamp, GameProgressData.LastIdleUpdateTime);

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.MelvorIdle,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 1,
                PendingClaim = 0,
                HasOfflineClaim = false
            };

            if (!System.DateTime.TryParse(
                    GameProgressData.LastIdleUpdateTime,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out System.DateTime lastTime))
                Assert.Fail("LastIdleUpdateTime must remain parseable after empty OS pass");

            double elapsed = (System.DateTime.UtcNow - lastTime).TotalSeconds;
            Assert.Greater(elapsed, 50, "AFK window must survive empty OfflineSimulation");
            double gained = IdleOfflineCatchUp.Apply(ref state, elapsed);
            Assert.Greater(gained, 50);
            Assert.Greater(state.PendingClaim, 50);
            Assert.IsTrue(state.HasOfflineClaim);
        }

        [Test]
        public void Claim_PendingClaim_DoesNotAlsoPayAfkChest()
        {
            var slice = CreateSlice(IdleArchetype.MelvorIdle, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.PendingClaim = 40;
            st.AfkChestSeconds = 12f;
            st.HasOfflineClaim = true;
            st.ProgressionLevel = 2;
            st.GlobalMultiplier = 1f;
            _em.SetComponentData(slice, st);

            var claimSys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent { TargetSlice = slice });
            claimSys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(40.0, after.PrimaryCurrency, 0.001,
                "D16: claim with PendingClaim must pay pending only (not + chest formula)");
            Assert.AreEqual(0.0, after.PendingClaim, 0.001);
            Assert.AreEqual(12f, after.AfkChestSeconds, 0.01f,
                "Chest left for a later claim when pending was preferred");
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

        [Test]
        public void ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime()
        {
            // D23: PassiveRate=0 non-Melvor → Apply returns 0 → AFK stamp must survive.
            string stamp = System.DateTime.UtcNow.AddSeconds(-60)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamp;

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.NekoAtsume,
                PrimaryCurrency = 20,
                GlobalMultiplier = 1f,
                PassiveRate = 0,
                PendingClaim = 0
            };

            double gained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref state);
            Assert.AreEqual(0.0, gained, 0.001);
            Assert.AreEqual(stamp, GameProgressData.LastIdleUpdateTime,
                "D23: zero-grant CatchUp must not wipe LastIdleUpdateTime");
            Assert.AreEqual(20.0, state.PrimaryCurrency, 0.001);
        }

        [Test]
        public void BootstrapLoadPath_SyncBeforeCatchUp_UsesRawPassiveNotMultSquared()
        {
            // D25: stale Mult² PassiveRate in prefs must be Sync'd before CatchUp.
            // Mult=2, Owned=3, BaseCps=1 → raw Passive=3; 60s → grant 3*2*60=360 (not 9*2*60=1080).
            const double baseCps = 1.0;
            const int owned = 3;
            const float mult = 2f;
            const double staleMultSquaredPassive = 9.0; // Mult² * Owned * BaseCps
            const double elapsed = 60.0;

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.CookieClicker,
                PrimaryCurrency = 100,
                GlobalMultiplier = mult,
                PassiveRate = staleMultSquaredPassive,
                OwnedGenerators = owned
            };
            var gen = new BuyableGenerator
            {
                GeneratorId = 1,
                OwnedCount = 0,
                BaseCost = 15,
                CostGrowth = 1.15f,
                BaseCps = baseCps,
                RequiresManager = false,
                IsAutomated = true
            };

            // Mirror bootstrap: AttachArchetypeExtras Sync, then Apply (CatchUp).
            IdlePrestigeMath.SyncGeneratorOwnedCountFromState(ref gen, ref state, owned, automated: true);
            Assert.AreEqual(3.0, state.PassiveRate, 0.001,
                "Sync must rebuild raw PassiveRate = BaseCps*Owned before CatchUp");

            double gained = IdleOfflineCatchUp.Apply(ref state, elapsed);
            double expected = IdlePrestigeMath.ComputePassiveRate(baseCps, owned) * mult * elapsed;
            Assert.AreEqual(expected, gained, 0.001, "CatchUp grant must be raw×Mult×t (not Mult²)");
            Assert.AreEqual(100.0 + expected, state.PrimaryCurrency, 0.001);
            // AreNotEqual has no double-delta overload (CS1503 if passed 0.001 as message).
            Assert.That(System.Math.Abs(gained - (staleMultSquaredPassive * mult * elapsed)), Is.GreaterThan(0.001),
                "CatchUp must not apply Mult twice (stale Mult² PassiveRate path)");
        }

        [Test]
        public void ApplyPersistedElapsed_PositiveGrant_StampsLastIdleUpdateTime()
        {
            string stamp = System.DateTime.UtcNow.AddSeconds(-60)
                .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            GameProgressData.LastIdleUpdateTime = stamp;

            var state = new IdleSliceState
            {
                Archetype = IdleArchetype.EggInc,
                PrimaryCurrency = 0,
                GlobalMultiplier = 1f,
                PassiveRate = 2
            };

            double gained = IdleOfflineCatchUp.ApplyPersistedElapsed(ref state);
            Assert.Greater(gained, 50);
            Assert.AreNotEqual(stamp, GameProgressData.LastIdleUpdateTime,
                "Positive grant must consume AFK stamp");
        }
    }
}

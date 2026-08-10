using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// Prestige P2: Realm Grinder Evil factionBonus 1.25 (mirrors Good 1.5 in Batch BC).
    /// Dedicated fixture so concurrent IdleBatchBCSmokeTests edits cannot wipe the assert.
    /// </summary>
    [TestFixture]
    public class IdlePrestigeFactionBonusTests
    {
        private World _world;
        private EntityManager _em;

        [SetUp]
        public void SetUp()
        {
            _world = new World("IdlePrestigeFactionBonusTestWorld");
            World.DefaultGameObjectInjectionWorld = _world;
            _em = _world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
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

        [Test]
        public void RealmGrinder_AlignEvil_RaisesPassiveIncomeViaFactionBonus()
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
            _em.AddComponentData(evt, new IdleFactionAlignEvent { TargetSlice = slice, FactionId = 2 });
            alignSys.Update(_world.Unmanaged);
            Assert.AreEqual(2, _em.GetComponentData<IdleSliceState>(slice).FactionId);

            PumpSim(simSys, 2f, dt: 1f);
            double evilGain = _em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency - baseline;
            Assert.Greater(evilGain, neutralGain, "Evil Align must raise Passive income via factionBonus");
            Assert.AreEqual(rate * 1.25 * 2.0, evilGain, 0.001, "Evil factionBonus must be 1.25");
        }
    }
}

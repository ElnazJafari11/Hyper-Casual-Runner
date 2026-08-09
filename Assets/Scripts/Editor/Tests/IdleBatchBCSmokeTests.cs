using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode smoke for remaining matrix titles (Batch B+C).
    /// One core verb + progression beat each.
    /// </summary>
    [TestFixture]
    public class IdleBatchBCSmokeTests
    {
        private World _world;
        private EntityManager _em;

        [SetUp]
        public void SetUp()
        {
            _world = new World("IdleBatchBCSmokeWorld");
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
            return e;
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
            {
                var evt = _em.CreateEntity();
                _em.AddComponentData(evt, new IdleNarrativeActionEvent { ActionId = 0 });
                sys.Update(_world.Unmanaged);
            }

            var narr = _em.GetComponentData<IdleNarrativeState>(slice);
            Assert.AreEqual(1, narr.ExploreUnlocked, "5 stokes unlock explore");

            var explore = _em.CreateEntity();
            _em.AddComponentData(explore, new IdleNarrativeActionEvent { ActionId = 1 });
            sys.Update(_world.Unmanaged);

            Assert.GreaterOrEqual(_em.GetComponentData<IdleSliceState>(slice).ProgressionLevel, 1);
        }

        [Test]
        public void RealmGrinder_AlignFaction_RaisesMultAndLevel()
        {
            var slice = CreateSlice(IdleArchetype.RealmGrinder, 20);
            var sys = _world.CreateSystem<IdleAllocateEnergySystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAllocateEnergyEvent { Amount = 1f });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.FactionId);
            Assert.Greater(after.GlobalMultiplier, 1f);
            Assert.GreaterOrEqual(after.ProgressionLevel, 1);
        }

        [Test]
        public void NguIdle_AllocateEnergy_SetsAllocation()
        {
            var slice = CreateSlice(IdleArchetype.NguIdle, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.EnergyPool = 50;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleAllocateEnergySystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAllocateEnergyEvent { Amount = 10f });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(10f, after.EnergyAllocated, 0.01f);
            Assert.GreaterOrEqual(after.ProgressionLevel, 1);
        }

        [Test]
        public void MelvorIdle_TrainSkillClick_GainsCurrency()
        {
            // EditMode World DeltaTime can be 0; use click path for a deterministic beat.
            var slice = CreateSlice(IdleArchetype.MelvorIdle, 0);
            var sys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 1f });
            sys.Update(_world.Unmanaged);

            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0);
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
        }

        [Test]
        public void IdleMiner_HireManager_AutomatesShaft()
        {
            var slice = CreateSlice(IdleArchetype.IdleMinerTycoon, 200);
            _em.AddComponentData(slice, new BuyableGenerator
            {
                GeneratorId = 1, OwnedCount = 1, BaseCost = 15, CostGrowth = 1.15f,
                BaseCps = 1, RequiresManager = true, IsAutomated = false
            });
            _em.AddComponentData(slice, new IdleManager
            {
                TargetGeneratorId = 1, HireCost = 100, IsHired = false
            });
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.OwnedGenerators = 1;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleManagerHireSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleHireManagerEvent { TargetGeneratorId = 1 });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.AreEqual(1, after.ManagersHired);
            Assert.Greater(after.PassiveRate, 0);
        }

        [Test]
        public void TapTitans2_TapKill_AdvancesZone()
        {
            var slice = CreateSlice(IdleArchetype.TapTitans2, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.EnemyHp = 3; st.EnemyMaxHp = 3; st.ClickPower = 10;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleClickProduceSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClickEvent { Multiplier = 1f });
            sys.Update(_world.Unmanaged);

            Assert.GreaterOrEqual(_em.GetComponentData<IdleSliceState>(slice).ProgressionLevel, 1);
        }

        [Test]
        public void IdleHeroes_GachaPull_RaisesStageOrPower()
        {
            var slice = CreateSlice(IdleArchetype.IdleHeroes, 100);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 0, PullCost = 10, BestRarity = 0, Stage = 0
            });
            var before = _em.GetComponentData<IdleSliceState>(slice).ClickPower;

            var sys = _world.CreateSystem<IdleGachaPullSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleGachaPullEvent());
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.Greater(after.ClickPower, before);
            Assert.AreEqual(1, _em.GetComponentData<IdleGachaState>(slice).PullCount);
        }

        [Test]
        public void AfkArena_ClaimChest_GrantsCurrency()
        {
            var slice = CreateSlice(IdleArchetype.AfkArena, 0);
            var st = _em.GetComponentData<IdleSliceState>(slice);
            st.AfkChestSeconds = 12f;
            st.HasOfflineClaim = true;
            _em.SetComponentData(slice, st);

            var sys = _world.CreateSystem<IdleClaimOfflineSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleClaimOfflineEvent());
            sys.Update(_world.Unmanaged);

            Assert.Greater(_em.GetComponentData<IdleSliceState>(slice).PrimaryCurrency, 0);
        }

        [Test]
        public void LegendOfMushroom_RubLamp_AdvancesStage()
        {
            var slice = CreateSlice(IdleArchetype.LegendOfMushroom, 100);
            _em.AddComponentData(slice, new IdleGachaState
            {
                PullCount = 2, PullCost = 10, BestRarity = 1, Stage = 0
            });

            var sys = _world.CreateSystem<IdleGachaPullSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleGachaPullEvent());
            sys.Update(_world.Unmanaged);

            // 3rd pull % 3 == 0 → stage++
            Assert.GreaterOrEqual(_em.GetComponentData<IdleGachaState>(slice).Stage, 1);
        }

        [Test]
        public void CapybaraGo_NextStep_AdvancesLevel()
        {
            var slice = CreateSlice(IdleArchetype.CapybaraGo, 0);
            _em.AddComponentData(slice, new IdleNarrativeState
            {
                RoomOrStep = 0, StokeCount = 0, ExploreUnlocked = 1, Wood = 0, SoftCurrency = 0
            });
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleNarrativeActionEvent { ActionId = 1 });
            sys.Update(_world.Unmanaged);

            Assert.GreaterOrEqual(_em.GetComponentData<IdleSliceState>(slice).ProgressionLevel, 1);
        }

        [Test]
        public void CatsAndSoup_AssignCat_IncreasesWorkers()
        {
            var slice = CreateSlice(IdleArchetype.CatsAndSoup, 0);
            _em.AddComponentData(slice, new IdleAssignmentStation
            {
                StationId = 1, AssignedCount = 0, Capacity = 5,
                OutputPerWorker = 1.5, Interval = 1f, Timer = 0f
            });
            var sys = _world.CreateSystem<IdleAssignWorkerSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAssignWorkerEvent { StationId = 1, Delta = 1 });
            sys.Update(_world.Unmanaged);

            Assert.AreEqual(1, _em.GetComponentData<IdleSliceState>(slice).AssignedWorkers);
            Assert.AreEqual(1, _em.GetComponentData<IdleAssignmentStation>(slice).AssignedCount);
        }

        [Test]
        public void NekoAtsume_PlaceFood_AttractsCats()
        {
            var slice = CreateSlice(IdleArchetype.NekoAtsume, 20);
            var sys = _world.CreateSystem<IdleNarrativeActionSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleNarrativeActionEvent { ActionId = 0 });
            sys.Update(_world.Unmanaged);

            var after = _em.GetComponentData<IdleSliceState>(slice);
            Assert.GreaterOrEqual(after.CheckInCats, 2);
        }

        [Test]
        public void FalloutShelter_AssignDweller_FillsStation()
        {
            var slice = CreateSlice(IdleArchetype.FalloutShelter, 0);
            _em.AddComponentData(slice, new IdleAssignmentStation
            {
                StationId = 1, AssignedCount = 0, Capacity = 5,
                OutputPerWorker = 1.5, Interval = 1f, Timer = 0f
            });
            var sys = _world.CreateSystem<IdleAssignWorkerSystem>();
            var evt = _em.CreateEntity();
            _em.AddComponentData(evt, new IdleAssignWorkerEvent { StationId = 1, Delta = 2 });
            sys.Update(_world.Unmanaged);

            Assert.AreEqual(2, _em.GetComponentData<IdleSliceState>(slice).AssignedWorkers);
        }
    }
}

using NUnit.Framework;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class AdvancedObstaclesTests
    {
        private World testWorld;
        private EntityManager entityManager;

        [SetUp]
        public void SetUp()
        {
            testWorld = new World("AdvancedObstaclesTestWorld");
            World.DefaultGameObjectInjectionWorld = testWorld;
            entityManager = testWorld.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (testWorld != null && testWorld.IsCreated)
            {
                testWorld.Dispose();
            }
        }

        [Test]
        public void ObstacleComponents_StructLayoutsAndDefaults_AreValid()
        {
            var wallComp = new MovingWallComponent
            {
                MovementAxis = new float3(1, 0, 0),
                MoveDistance = 4.0f,
                Speed = 2.0f,
                InitialPosition = new float3(0, 1, 10),
                CollisionRadius = 1.5f,
                DamageAmount = 25.0f
            };

            Assert.AreEqual(1.5f, wallComp.CollisionRadius, "MovingWallComponent CollisionRadius mismatch");
            Assert.AreEqual(25.0f, wallComp.DamageAmount, "MovingWallComponent DamageAmount mismatch");

            var pendulumComp = new PendulumSwingComponent
            {
                MaxAngleDegrees = 45.0f,
                Speed = 3.0f,
                PhaseOffset = 0.5f,
                SwingAxis = new float3(0, 0, 1),
                InitialRotation = quaternion.identity,
                CollisionRadius = 2.0f,
                DamageAmount = 30.0f
            };

            Assert.AreEqual(45.0f, pendulumComp.MaxAngleDegrees, "PendulumSwingComponent MaxAngleDegrees mismatch");
            Assert.AreEqual(3.0f, pendulumComp.Speed, "PendulumSwingComponent Speed mismatch");

            var splitComp = new SplittingHazardComponent
            {
                SplitCount = 3,
                ChildPrefab = Entity.Null,
                ImpulseForce = 2.5f,
                TriggerDistance = 6.0f,
                HasSplit = false,
                CollisionRadius = 1.2f,
                DamageAmount = 15.0f
            };

            Assert.AreEqual(3, splitComp.SplitCount, "SplittingHazardComponent SplitCount mismatch");
            Assert.IsFalse(splitComp.HasSplit, "SplittingHazardComponent initial HasSplit should be false");
        }

        [Test]
        public void MovingWallSystem_TranslatesPositionViaSineWave()
        {
            var systemHandle = testWorld.CreateSystem<MovingWallSystem>();

            Entity wallEntity = entityManager.CreateEntity();
            float3 startPos = new float3(0, 1, 10);
            float3 moveAxis = new float3(1, 0, 0); // X-axis movement
            float moveDist = 5.0f;
            float speed = 2.0f;

            entityManager.AddComponentData(wallEntity, new MovingWallComponent
            {
                MovementAxis = moveAxis,
                MoveDistance = moveDist,
                Speed = speed,
                InitialPosition = startPos,
                CollisionRadius = 1.5f,
                DamageAmount = 20.0f
            });
            entityManager.AddComponentData(wallEntity, LocalTransform.FromPosition(startPos));

            // Set time t = 0.785398s (PI/4) -> sin(2 * PI/4) = sin(PI/2) = 1.0
            float t = math.PI / 4.0f; // speed * t = 2 * (PI/4) = PI/2 -> sin(PI/2) = 1.0
            testWorld.SetTime(new Unity.Core.TimeData(t, 0.016f));

            systemHandle.Update(testWorld.Unmanaged);

            var transform = entityManager.GetComponentData<LocalTransform>(wallEntity);
            float expectedX = startPos.x + 1.0f * moveDist; // 0 + 5 = 5
            Assert.AreEqual(expectedX, transform.Position.x, 0.001f, "Moving wall position along X axis should match sine wave maximum");
            Assert.AreEqual(startPos.y, transform.Position.y, 0.001f, "Y position should remain unchanged");
            Assert.AreEqual(startPos.z, transform.Position.z, 0.001f, "Z position should remain unchanged");
        }

        [Test]
        public void PendulumSwingSystem_RotatesAroundSwingAxis()
        {
            var systemHandle = testWorld.CreateSystem<PendulumSwingSystem>();

            Entity pendulumEntity = entityManager.CreateEntity();
            float maxAngle = 90.0f;
            float speed = 1.0f;
            float3 swingAxis = new float3(0, 0, 1); // Z-axis rotation

            entityManager.AddComponentData(pendulumEntity, new PendulumSwingComponent
            {
                MaxAngleDegrees = maxAngle,
                Speed = speed,
                PhaseOffset = 0.0f,
                SwingAxis = swingAxis,
                InitialRotation = quaternion.identity,
                CollisionRadius = 1.0f,
                DamageAmount = 15.0f
            });
            entityManager.AddComponentData(pendulumEntity, LocalTransform.FromRotation(quaternion.identity));

            // Set time t = PI/2 -> sin(1 * PI/2) = 1.0 -> angle = 90 deg = PI/2 rad
            float t = math.PI / 2.0f;
            testWorld.SetTime(new Unity.Core.TimeData(t, 0.016f));

            systemHandle.Update(testWorld.Unmanaged);

            var transform = entityManager.GetComponentData<LocalTransform>(pendulumEntity);
            quaternion expectedRot = quaternion.AxisAngle(swingAxis, math.radians(90.0f));

            // Compare quaternion dot product to verify identical rotation (or anti-aligned representation)
            float dot = math.abs(math.dot(transform.Rotation.value, expectedRot.value));
            Assert.AreEqual(1.0f, dot, 0.001f, "Pendulum rotation should match 90 degree Z-axis rotation at sine peak");
        }

        [Test]
        public void SplittingHazardSystem_TriggersSplit_WhenPlayerInDistance()
        {
            var systemHandle = testWorld.CreateSystem<SplittingHazardSystem>();

            // Create Player Entity at position (0, 0, 4)
            Entity playerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(playerEntity, new PlayerComponent());
            entityManager.AddComponentData(playerEntity, LocalTransform.FromPosition(new float3(0, 0, 4)));

            // Create Child Hazard Prefab Entity
            Entity childPrefab = entityManager.CreateEntity();
            entityManager.AddComponentData(childPrefab, LocalTransform.Identity);
            entityManager.AddComponentData(childPrefab, new ObstacleComponent { CollisionRadius = 0.5f, DamageAmount = 5.0f });

            // Create Parent Splitting Hazard Entity at position (0, 0, 5) - distance 1.0 <= TriggerDistance 5.0
            Entity parentHazard = entityManager.CreateEntity();
            entityManager.AddComponentData(parentHazard, new SplittingHazardComponent
            {
                SplitCount = 2,
                ChildPrefab = childPrefab,
                ImpulseForce = 3.0f,
                TriggerDistance = 5.0f,
                HasSplit = false,
                CollisionRadius = 1.5f,
                DamageAmount = 20.0f
            });
            entityManager.AddComponentData(parentHazard, LocalTransform.FromPosition(new float3(0, 0, 5)));

            // Run system
            systemHandle.Update(testWorld.Unmanaged);

            // Verify parent entity was destroyed
            Assert.IsFalse(entityManager.Exists(parentHazard), "Parent hazard entity should be destroyed upon splitting");

            // Verify audio event entity was created
            var audioQuery = entityManager.CreateEntityQuery(typeof(PlaySoundEventComponent));
            Assert.AreEqual(1, audioQuery.CalculateEntityCount(), "An audio event entity should be spawned for split explosion SFX");

            // Verify child hazard entities were instantiated (prefab + 2 children = 3 total or query for ObstacleComponent)
            var obstacleQuery = entityManager.CreateEntityQuery(typeof(ObstacleComponent));
            Assert.GreaterOrEqual(obstacleQuery.CalculateEntityCount(), 2, "Child hazard entities should be instantiated");
        }

        [Test]
        public void SplittingHazardSystem_DoesNotTrigger_WhenPlayerOutsideDistance()
        {
            var systemHandle = testWorld.CreateSystem<SplittingHazardSystem>();

            // Create Player Entity at position (0, 0, 0)
            Entity playerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(playerEntity, new PlayerComponent());
            entityManager.AddComponentData(playerEntity, LocalTransform.FromPosition(new float3(0, 0, 0)));

            // Create Parent Splitting Hazard Entity at position (0, 0, 20) - distance 20.0 > TriggerDistance 5.0
            Entity parentHazard = entityManager.CreateEntity();
            entityManager.AddComponentData(parentHazard, new SplittingHazardComponent
            {
                SplitCount = 2,
                ChildPrefab = Entity.Null,
                ImpulseForce = 3.0f,
                TriggerDistance = 5.0f,
                HasSplit = false,
                CollisionRadius = 1.5f,
                DamageAmount = 20.0f
            });
            entityManager.AddComponentData(parentHazard, LocalTransform.FromPosition(new float3(0, 0, 20)));

            systemHandle.Update(testWorld.Unmanaged);

            // Verify parent hazard still exists and has not split
            Assert.IsTrue(entityManager.Exists(parentHazard), "Parent hazard entity should remain active");
            var hazardComp = entityManager.GetComponentData<SplittingHazardComponent>(parentHazard);
            Assert.IsFalse(hazardComp.HasSplit, "HasSplit should remain false when player is out of range");
        }
    }
}

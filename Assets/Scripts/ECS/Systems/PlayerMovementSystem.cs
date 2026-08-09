using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PlayerMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Halt movement if not playing
            bool isPlaying = true;
            if (SystemAPI.HasSingleton<LevelStateComponent>())
            {
                if (SystemAPI.GetSingleton<LevelStateComponent>().CurrentState != GameState.Playing)
                {
                    isPlaying = false;
                }
            }

            if (!isPlaying) return;

            float dt = SystemAPI.Time.DeltaTime;
            
            // Try to get the input singleton. If it doesn't exist yet, we just move forward with 0 swerve.
            float swerveInput = 0f;
            if (SystemAPI.HasSingleton<InputComponent>())
            {
                swerveInput = SystemAPI.GetSingleton<InputComponent>().SwerveDelta;
            }

            // Sensitivity multiplier for swerve
            float swerveSensitivity = 0.05f;

            foreach (var (transform, player) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerComponent>>())
            {
                // Auto-run forward
                float3 newPos = transform.ValueRO.Position;
                newPos.z += player.ValueRO.ForwardSpeed * dt;

                // Swerve horizontally
                newPos.x += swerveInput * swerveSensitivity;
                
                // Clamp X to track bounds (assuming standard hyper-casual track width)
                newPos.x = math.clamp(newPos.x, -4f, 4f);

                transform.ValueRW.Position = newPos;
            }
        }
    }
}

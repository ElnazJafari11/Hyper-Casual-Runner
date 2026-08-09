using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct MathTweenSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float time = (float)SystemAPI.Time.ElapsedTime;

            foreach (var (transform, tween) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MathTweenComponent>>())
            {
                float sineWave = math.sin(time * tween.ValueRO.Speed);
                
                if (tween.ValueRO.Property == TweenProperty.PositionY)
                {
                    float3 pos = transform.ValueRO.Position;
                    pos.y = tween.ValueRO.BaseValue + (sineWave * tween.ValueRO.Amplitude);
                    transform.ValueRW.Position = pos;
                }
                else if (tween.ValueRO.Property == TweenProperty.ScaleUniform)
                {
                    // For bouncy scale, use absolute sine so it only scales up
                    float bounce = math.abs(sineWave);
                    transform.ValueRW.Scale = tween.ValueRO.BaseValue + (bounce * tween.ValueRO.Amplitude);
                }
                else if (tween.ValueRO.Property == TweenProperty.RotationY)
                {
                    // Continuous rotation using time, not sin wave
                    transform.ValueRW.Rotation = quaternion.RotateY(time * tween.ValueRO.Speed);
                }
            }
        }
    }
}

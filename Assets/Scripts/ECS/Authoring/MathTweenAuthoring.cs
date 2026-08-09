using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class MathTweenAuthoring : MonoBehaviour
    {
        public TweenProperty Property = TweenProperty.PositionY;
        public float Amplitude = 0.5f;
        public float Speed = 2f;
        public float BaseValue = 1f;

        class Baker : Baker<MathTweenAuthoring>
        {
            public override void Bake(MathTweenAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MathTweenComponent
                {
                    Property = authoring.Property,
                    Amplitude = authoring.Amplitude,
                    Speed = authoring.Speed,
                    BaseValue = authoring.BaseValue
                });
            }
        }
    }
}

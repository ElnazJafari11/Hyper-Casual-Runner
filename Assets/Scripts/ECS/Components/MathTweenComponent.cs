using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum TweenProperty
    {
        PositionY,
        ScaleUniform,
        RotationY
    }

    public struct MathTweenComponent : IComponentData
    {
        public TweenProperty Property;
        public float Amplitude;
        public float Speed;
        public float BaseValue;
    }
}

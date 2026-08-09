using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct PlayerComponent : IComponentData
    {
        public float ForwardSpeed;
        public float SwerveSpeed;
        public float MaxSwerveDistance;
    }
}

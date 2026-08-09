using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct StiltsComponent : IComponentData
    {
        public float HeelHeight;
        public int CurrentHeels;
    }

    public struct HurdleWallComponent : IComponentData
    {
        public float RequiredHeight;
        public float CollisionRadius;
        public bool Cleared;
    }
}

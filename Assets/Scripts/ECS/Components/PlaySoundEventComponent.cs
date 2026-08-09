using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum SoundType
    {
        Pickup,
        Explosion,
        Victory
    }

    public struct PlaySoundEventComponent : IComponentData
    {
        public SoundType SoundToPlay;
    }
}

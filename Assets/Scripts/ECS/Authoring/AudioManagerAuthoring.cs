using UnityEngine;
using Unity.Entities;

namespace HyperCasualRunner.ECS.Authoring
{
    public class AudioManagerAuthoring : MonoBehaviour
    {
        public AudioClip PickupSFX;
        public AudioClip ExplosionSFX;
        public AudioClip VictorySFX;

        class Baker : Baker<AudioManagerAuthoring>
        {
            public override void Bake(AudioManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Add a managed component to hold the Unity AudioClips and an AudioSource
                AddComponentObject(entity, new AudioManagerComponent 
                { 
                    PickupSFX = authoring.PickupSFX,
                    ExplosionSFX = authoring.ExplosionSFX,
                    VictorySFX = authoring.VictorySFX
                });
            }
        }
    }

    public class AudioManagerComponent : IComponentData
    {
        public AudioClip PickupSFX;
        public AudioClip ExplosionSFX;
        public AudioClip VictorySFX;
        public AudioSource Source; // Cached at runtime
    }
}

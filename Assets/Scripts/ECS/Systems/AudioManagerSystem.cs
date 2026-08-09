using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class AudioManagerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            
            if (!SystemAPI.ManagedAPI.HasSingleton<AudioManagerComponent>()) return;
            
            var audioManager = SystemAPI.ManagedAPI.GetSingleton<AudioManagerComponent>();
            
            // Lazy initialization of the AudioSource so we don't have to inject it manually
            if (audioManager.Source == null)
            {
                GameObject audioObj = new GameObject("GlobalAudioSource");
                audioManager.Source = audioObj.AddComponent<AudioSource>();
                audioManager.Source.playOnAwake = false;
            }

            foreach (var (soundEvent, entity) in SystemAPI.Query<RefRO<PlaySoundEventComponent>>().WithEntityAccess())
            {
                AudioClip clipToPlay = null;
                switch (soundEvent.ValueRO.SoundToPlay)
                {
                    case SoundType.Pickup: clipToPlay = audioManager.PickupSFX; break;
                    case SoundType.Explosion: clipToPlay = audioManager.ExplosionSFX; break;
                    case SoundType.Victory: clipToPlay = audioManager.VictorySFX; break;
                }

                if (clipToPlay != null)
                {
                    audioManager.Source.PlayOneShot(clipToPlay);
                }

                // Consume the event
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}

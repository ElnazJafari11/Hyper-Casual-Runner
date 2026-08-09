using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SwerveInputSystem : SystemBase
    {
        private float _lastMouseX;

        protected override void OnCreate()
        {
            // Ensure the singleton exists
            EntityManager.CreateEntity(typeof(InputComponent));
        }

        protected override void OnUpdate()
        {
            float delta = 0f;
            bool interacting = false;

            if (Input.GetMouseButtonDown(0))
            {
                _lastMouseX = Input.mousePosition.x;
                interacting = true;
            }
            else if (Input.GetMouseButton(0))
            {
                float currentMouseX = Input.mousePosition.x;
                delta = currentMouseX - _lastMouseX;
                _lastMouseX = currentMouseX;
                interacting = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                delta = 0f;
            }

            // Write to the singleton InputComponent so all player entities can read it
            SystemAPI.SetSingleton(new InputComponent 
            { 
                SwerveDelta = delta,
                IsInteracting = interacting
            });
        }
    }
}

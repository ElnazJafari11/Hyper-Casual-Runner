using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    // Tag component indicating this entity should be destroyed this frame, triggering VFX
    public struct DestroyEventComponent : IComponentData, IEnableableComponent
    {
    }
}

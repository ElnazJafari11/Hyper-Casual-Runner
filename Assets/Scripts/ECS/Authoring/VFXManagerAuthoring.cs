using UnityEngine;
using Unity.Entities;

namespace HyperCasualRunner.ECS.Authoring
{
    public class VFXManagerAuthoring : MonoBehaviour
    {
        public GameObject HitVFXPrefab;

        class Baker : Baker<VFXManagerAuthoring>
        {
            public override void Bake(VFXManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                // We use a Managed component here because it holds a reference to a Unity Object (GameObject)
                AddComponentObject(entity, new VFXManagerComponent { HitVFXPrefab = authoring.HitVFXPrefab });
            }
        }
    }

    public class VFXManagerComponent : IComponentData
    {
        public GameObject HitVFXPrefab;
    }
}

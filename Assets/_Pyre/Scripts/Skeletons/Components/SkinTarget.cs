using Unity.Entities;

namespace Pyre.Skeletons.Components
{
    /// <summary>
    /// Where the computed skin matrices go, and the space they have to be in.
    ///
    /// Entities Graphics' own SkinnedMeshRendererBaker puts DynamicBuffer&lt;SkinMatrix&gt; on the entity of
    /// the SkinnedMeshRenderer's GameObject, and parents the render entities to SkinnedMeshRenderer.rootBone
    /// (falling back to the renderer's own transform) with an identity LocalTransform. So the skin matrices
    /// are consumed in <see cref="SkinSpaceBone"/> space, not world space and not the deformed entity's space.
    ///
    /// A baker may only add components to its own primary and additional entities, which is why this is a
    /// reference held by the skeleton root rather than a component on the deformed entity itself.
    /// </summary>
    public struct SkinTarget : IComponentData
    {
        /// <summary>Entity carrying DynamicBuffer&lt;SkinMatrix&gt;.</summary>
        public Entity DeformedEntity;

        /// <summary>Bone whose LocalToWorld defines the skinning space.</summary>
        public Entity SkinSpaceBone;
    }
}

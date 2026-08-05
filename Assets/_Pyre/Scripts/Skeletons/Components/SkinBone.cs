using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Skeletons.Components
{
    /// <summary>
    /// SkinnedMeshRenderer.bones[i] paired with sharedMesh.bindposes[i], in the renderer's own order — the
    /// order the DynamicBuffer&lt;SkinMatrix&gt; on <see cref="SkinTarget.DeformedEntity"/> is indexed by.
    /// </summary>
    public struct SkinBone : IBufferElementData
    {
        public Entity Bone;
        public float4x4 BindPose;
    }
}

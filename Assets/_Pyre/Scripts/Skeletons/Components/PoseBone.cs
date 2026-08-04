using Unity.Entities;

namespace Pyre.Skeletons.Components
{
    /// <summary>
    /// Every transform under the model root that the clips animate, in hierarchy order, matching the bake
    /// order of <see cref="Settings.SkeletonClipSet.bonePaths"/> exactly. SkeletonPoseSystem writes
    /// LocalTransform to these.
    ///
    /// Deliberately a different set from <see cref="SkinBone"/>: SkinnedMeshRenderer.bones omits parents
    /// that carry animation but influence no vertices, and leaving those frozen animates the character wrong.
    ///
    /// Named PoseBone rather than SkeletonBone because UnityEngine.SkeletonBone exists — the collision forces
    /// a using alias on every file that imports both namespaces.
    /// </summary>
    public struct PoseBone : IBufferElementData
    {
        public Entity Bone;
    }
}

using Pyre.Skeletons.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Deformations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Pyre.Skeletons.Systems
{
    /// <summary>
    /// Fills the DynamicBuffer&lt;SkinMatrix&gt; that Entities Graphics created on each SkinnedMeshRenderer's
    /// entity, from the bone LocalToWorlds that TransformSystemGroup rebuilt earlier in the same frame.
    ///
    /// This is the half Unity does not ship. Entities Graphics renders a deformed mesh from these matrices,
    /// but nothing has written them since com.unity.animation was discontinued.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(DeformationsInPresentation))]
    public partial struct SkinMatrixSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> _localToWorlds;
        private BufferLookup<SkinMatrix> _skinMatrices;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SkinTarget>();

            _localToWorlds = state.GetComponentLookup<LocalToWorld>(true);
            _skinMatrices = state.GetBufferLookup<SkinMatrix>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _localToWorlds.Update(ref state);
            _skinMatrices.Update(ref state);

            state.Dependency = new SkinMatrixJob
            {
                LocalToWorlds = _localToWorlds,
                SkinMatrices = _skinMatrices,
            }.Schedule(state.Dependency);
        }
    }

    [BurstCompile]
    internal partial struct SkinMatrixJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorlds;

        /// <summary>
        /// Written single-threaded via .Schedule(), for the same reason as SkeletonPoseSystem: this writes
        /// into a buffer on an entity outside the query, which parallel scheduling cannot prove is disjoint.
        /// </summary>
        public BufferLookup<SkinMatrix> SkinMatrices;

        private void Execute(in SkinTarget target, in DynamicBuffer<SkinBone> bones)
        {
            if (!SkinMatrices.HasBuffer(target.DeformedEntity))
                return;

            if (!LocalToWorlds.HasComponent(target.SkinSpaceBone))
                return;

            // Entities Graphics parents the render entities to SkinSpaceBone with an identity LocalTransform,
            // so the matrices it consumes are in that bone's space — not world, and not the deformed
            // entity's space when the two transforms differ.
            var worldToSkinSpace = math.inverse(LocalToWorlds[target.SkinSpaceBone].Value);

            var skinMatrices = SkinMatrices[target.DeformedEntity];
            var count = math.min(bones.Length, skinMatrices.Length);

            for (var i = 0; i < count; i++)
            {
                var bone = bones[i].Bone;

                // A null or unbaked bone keeps whatever is already in the slot, which is the bind-pose
                // matrix Entities Graphics wrote at bake time.
                if (!LocalToWorlds.HasComponent(bone))
                    continue;

                var matrix = math.mul(
                    worldToSkinSpace,
                    math.mul(LocalToWorlds[bone].Value, bones[i].BindPose));

                skinMatrices[i] = new SkinMatrix
                {
                    Value = new float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz),
                };
            }
        }
    }
}

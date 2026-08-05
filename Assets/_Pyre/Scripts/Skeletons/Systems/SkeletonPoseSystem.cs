using Pyre.Skeletons.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Skeletons.Systems
{
    /// <summary>
    /// Advances every <see cref="SkeletonPose"/> and writes the resulting pose onto its bones.
    ///
    /// Runs before TransformSystemGroup so the bone LocalToWorlds are rebuilt from the new pose in the same
    /// frame — SkinMatrixSystem reads those in PresentationSystemGroup, later in the very same frame.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct SkeletonPoseSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _localTransforms;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SkeletonPose>();

            _localTransforms = state.GetComponentLookup<LocalTransform>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _localTransforms.Update(ref state);

            state.Dependency = new SkeletonPoseJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                LocalTransforms = _localTransforms,
            }.Schedule(state.Dependency);
        }
    }

    [BurstCompile]
    internal partial struct SkeletonPoseJob : IJobEntity
    {
        public float DeltaTime;

        /// <summary>
        /// Written single-threaded via .Schedule(). Parallel writes here would need
        /// [NativeDisableParallelForRestriction], which is only sound if no two skeletons share a bone — an
        /// assumption not worth taking on for one character.
        /// </summary>
        public ComponentLookup<LocalTransform> LocalTransforms;

        private void Execute(ref SkeletonPose pose, in DynamicBuffer<PoseBone> bones)
        {
            if (!pose.Library.IsCreated)
                return;

            ref var library = ref pose.Library.Value;

            if (library.Clips.Length == 0)
                return;

            ref var clipA = ref library.Clips[math.clamp(pose.ClipA, 0, library.Clips.Length - 1)];
            ref var clipB = ref library.Clips[math.clamp(pose.ClipB, 0, library.Clips.Length - 1)];

            var step = DeltaTime * pose.Speed;

            pose.TimeA = Advance(pose.TimeA, step, ref clipA);
            pose.TimeB = Advance(pose.TimeB, step, ref clipB);

            ResolveFrames(pose.TimeA, ref clipA, out var a0, out var a1, out var aFraction);
            ResolveFrames(pose.TimeB, ref clipB, out var b0, out var b1, out var bFraction);

            var blend = math.saturate(pose.Blend);
            var boneCount = math.min(bones.Length, math.min(clipA.BoneCount, clipB.BoneCount));

            for (var i = 0; i < boneCount; i++)
            {
                var bone = bones[i].Bone;

                if (!LocalTransforms.HasComponent(bone))
                    continue;

                var key = Sample(ref clipA, a0, a1, aFraction, i);

                if (blend > 0f)
                {
                    var target = Sample(ref clipB, b0, b1, bFraction, i);

                    key.Translation = math.lerp(key.Translation, target.Translation, blend);
                    key.Rotation = math.slerp(key.Rotation, target.Rotation, blend);
                    key.Scale = math.lerp(key.Scale, target.Scale, blend);
                }

                LocalTransforms[bone] = new LocalTransform
                {
                    Position = key.Translation,
                    Rotation = key.Rotation,
                    Scale = key.Scale,
                };
            }
        }

        private static float Advance(float time, float step, ref SkeletonClipBlob clip)
        {
            if (clip.Length <= 0f)
                return 0f;

            time += step;

            if (!clip.Looping)
                return math.clamp(time, 0f, clip.Length);

            time = math.fmod(time, clip.Length);

            // fmod keeps the sign of the dividend, so a negative Speed needs the wrap back up.
            return time < 0f ? time + clip.Length : time;
        }

        private static void ResolveFrames(float time, ref SkeletonClipBlob clip,
            out int frame, out int nextFrame, out float fraction)
        {
            var last = math.max(0, clip.FrameCount - 1);
            var position = time * clip.FrameRate;

            frame = math.clamp((int)math.floor(position), 0, last);

            // The bake includes the clip's end frame, so for a looping clip the last frame already equals
            // the first — clamping here interpolates across the loop seam instead of holding on it.
            nextFrame = math.min(frame + 1, last);
            fraction = math.saturate(position - frame);
        }

        private static BoneKey Sample(ref SkeletonClipBlob clip, int frame, int nextFrame, float fraction, int bone)
        {
            ref var from = ref clip.Keys[frame * clip.BoneCount + bone];
            ref var to = ref clip.Keys[nextFrame * clip.BoneCount + bone];

            return new BoneKey
            {
                Translation = math.lerp(from.Translation, to.Translation, fraction),
                Rotation = math.slerp(from.Rotation, to.Rotation, fraction),
                Scale = math.lerp(from.Scale, to.Scale, fraction),
            };
        }
    }
}

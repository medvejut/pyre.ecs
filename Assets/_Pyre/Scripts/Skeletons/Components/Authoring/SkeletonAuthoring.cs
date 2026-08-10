using Pyre.Skeletons.Settings;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Skeletons.Components
{
    /// <summary>
    /// Sits on the model root — the transform that owns the bone hierarchy and the SkinnedMeshRenderer as
    /// descendants. Bakes the clips of a <see cref="SkeletonClipSet"/> into a blob asset and records the two
    /// bone lists the skeleton systems need.
    /// </summary>
    public class SkeletonAuthoring : MonoBehaviour
    {
        public SkeletonClipSet ClipSet;

        [Tooltip("Clip the skeleton starts on. Empty falls back to the first baked clip.")]
        public string DefaultClip;

        public class SkeletonBaker : Baker<SkeletonAuthoring>
        {
            public override void Bake(SkeletonAuthoring authoring)
            {
                DependsOn(authoring.ClipSet);

                var set = authoring.ClipSet;

                if (set == null)
                    return;

                if (!set.HasBakedData)
                {
                    Debug.LogError(
                        $"[{authoring.name}] {set.name} has no baked data. Press Bake Clips on the asset.", authoring);
                    return;
                }

                var bones = SkeletonClipSet.BonesWithoutRoot(GetComponentsInChildren<Transform>());

                if (!BonesMatchBakedOrder(authoring, set, bones))
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Dynamic, not Renderable: SkeletonPoseSystem writes LocalTransform to every one of these.
                var skeleton = AddBuffer<PoseBone>(entity);
                skeleton.ResizeUninitialized(bones.Length);

                for (var i = 0; i < bones.Length; i++)
                    skeleton[i] = new PoseBone { Bone = GetEntity(bones[i], TransformUsageFlags.Dynamic) };

                var library = BuildLibrary(set);
                AddBlobAsset(ref library, out _);

                var defaultClip = set.IndexOf(authoring.DefaultClip);

                if (defaultClip < 0)
                {
                    if (!string.IsNullOrEmpty(authoring.DefaultClip))
                    {
                        Debug.LogWarning(
                            $"[{authoring.name}] {set.name} has no clip named '{authoring.DefaultClip}'. " +
                            "Falling back to the first baked clip.", authoring);
                    }

                    defaultClip = 0;
                }

                AddComponent(entity, new SkeletonPose
                {
                    Library = library,
                    ClipA = defaultClip,
                    ClipB = defaultClip,
                    TimeA = 0f,
                    TimeB = 0f,
                    Blend = 0f,
                    Speed = 1f,
                });

                BakeSkin(authoring, entity);
            }

            /// <summary>
            /// The blob is indexed by the bone order recorded at bake time. If the hierarchy under this
            /// authoring differs from the one the clips were sampled against, every pose is silently applied
            /// to the wrong bone — cheap to detect here, very confusing to debug later.
            /// </summary>
            private static bool BonesMatchBakedOrder(SkeletonAuthoring authoring, SkeletonClipSet set,
                Transform[] bones)
            {
                if (bones.Length != set.boneCount)
                {
                    Debug.LogError(
                        $"[{authoring.name}] has {bones.Length} bones but {set.name} was baked against " +
                        $"{set.boneCount}. Re-bake the set against this model.", authoring);
                    return false;
                }

                for (var i = 0; i < bones.Length; i++)
                {
                    var path = SkeletonClipSet.GetBonePath(authoring.transform, bones[i]);

                    if (path == set.bonePaths[i])
                        continue;

                    Debug.LogError(
                        $"[{authoring.name}] bone {i} is '{path}' but {set.name} was baked with " +
                        $"'{set.bonePaths[i]}'. Re-bake the set against this model.", authoring);
                    return false;
                }

                return true;
            }

            private void BakeSkin(SkeletonAuthoring authoring, Entity entity)
            {
                var skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();

                if (skinnedMesh == null || skinnedMesh.sharedMesh == null)
                {
                    Debug.LogWarning(
                        $"[{authoring.name}] has no SkinnedMeshRenderer, so nothing will be deformed.", authoring);
                    return;
                }

                // The renderer itself is tracked by GetComponentInChildren, but the bind poses live in the mesh
                // asset — without this a re-import of the model leaves stale BindPose values behind.
                DependsOn(skinnedMesh.sharedMesh);

                var skinBones = skinnedMesh.bones;
                var bindPoses = skinnedMesh.sharedMesh.bindposes;

                if (skinBones.Length != bindPoses.Length)
                {
                    Debug.LogError(
                        $"[{authoring.name}] has {skinBones.Length} skin bones but {bindPoses.Length} bind poses.",
                        authoring);
                    return;
                }

                // Entities Graphics puts DynamicBuffer<SkinMatrix> on the renderer's own entity and expects the
                // matrices in rootBone space — see SkinTarget.
                var skinSpace = skinnedMesh.rootBone != null ? skinnedMesh.rootBone : skinnedMesh.transform;

                AddComponent(entity, new SkinTarget
                {
                    DeformedEntity = GetEntity(skinnedMesh.gameObject, TransformUsageFlags.Dynamic),
                    SkinSpaceBone = GetEntity(skinSpace, TransformUsageFlags.Dynamic),
                });

                var skin = AddBuffer<SkinBone>(entity);
                skin.ResizeUninitialized(skinBones.Length);

                for (var i = 0; i < skinBones.Length; i++)
                {
                    skin[i] = new SkinBone
                    {
                        Bone = skinBones[i] != null
                            ? GetEntity(skinBones[i], TransformUsageFlags.Dynamic)
                            : Entity.Null,
                        BindPose = (float4x4)bindPoses[i],
                    };
                }
            }

            private static BlobAssetReference<SkeletonClipLibrary> BuildLibrary(SkeletonClipSet set)
            {
                using var builder = new BlobBuilder(Allocator.Temp);

                ref var library = ref builder.ConstructRoot<SkeletonClipLibrary>();
                var clips = builder.Allocate(ref library.Clips, set.bakedClips.Length);

                for (var c = 0; c < set.bakedClips.Length; c++)
                {
                    var source = set.bakedClips[c];

                    clips[c].Length = source.length;
                    clips[c].FrameRate = set.sampleRate;
                    clips[c].FrameCount = source.frameCount;
                    clips[c].BoneCount = set.boneCount;
                    clips[c].Looping = source.looping;

                    var keys = builder.Allocate(ref clips[c].Keys, source.translations.Length);

                    for (var k = 0; k < keys.Length; k++)
                    {
                        keys[k] = new BoneKey
                        {
                            Translation = source.translations[k],
                            Rotation = source.rotations[k],
                            Scale = source.scales[k],
                        };
                    }
                }

                return builder.CreateBlobAssetReference<SkeletonClipLibrary>(Allocator.Persistent);
            }
        }
    }
}

# Player skeletal animation — pure ECS (no third-party packages)

> **Status:** Stages 0–2 done. Stage 3 (`SkeletonPoseSystem`) is next. Code lives in
> `Assets/_Pyre/Scripts/Skeletons/`, namespace `Pyre.Skeletons.*`. The Context section below describes the
> starting point and is kept as written; Stage 1's checkpoint records what was resolved since.

## Context

The Hero currently has no animation at all. `Assets/_Pyre/Prefabs/Hero.prefab` renders its body via a
child `body-mesh` with a plain `MeshFilter` + `MeshRenderer` pointing at the *static* mesh inside
`Assets/_Pyre/Models/character-oopi.fbx` (localScale 3). There is not a single `Animator` or
`SkinnedMeshRenderer` anywhere in the project.

Two things were tried and both failed for the same underlying reason:

1. **`Animator` stops working once the model is in the subscene.** Baking converts GameObjects to
   entities and drops every `MonoBehaviour` that has no baker. Entities/Entities.Graphics ship
   companion-GameObject bakers for `Light`, `VisualEffect`, `AudioSource`, `ParticleSystem`,
   `ReflectionProbe` — which is exactly why `LightBlinkAnimationSystem` and `BurningViewSystem` can
   reach those through `SystemAPI.ManagedAPI`. **`Animator` is not on that list**, so it is silently
   discarded during conversion.

2. **The "default HDRP material + skinned mesh" warning.** Entities Graphics *does* bake a
   `SkinnedMeshRenderer` — it produces a `DeformedEntity` reference plus a `DynamicBuffer<SkinMatrix>`
   and expects the material's shader to contain Shader Graph's **Linear Blend Skinning** (or **Compute
   Deformation**) node. `HDRP/Lit` (shader guid `6e4ae4064600d784cac1e41a9e6f2e59`, what every material
   in `Assets/_Pyre/Materials/` uses) has neither node, so Entities Graphics warns that the mesh will
   render undeformed.

So Entities Graphics 6.5.0 can *render* a deformed mesh, but nothing *drives* the deformation: Unity
discontinued `com.unity.animation`, and there is no replacement in the box. **That missing half is
what we are writing.** We fill `DynamicBuffer<SkinMatrix>` ourselves from animation data we bake into
blob assets, so the character stays a normal entity in the subscene with no GameObject view and no new
packages.

**Scope:** player (Hero / `character-oopi.fbx`) only. Enemy stays a static mesh. Interaction animation
is driven by the existing ignite/burn gameplay state (`IgnitionProgress`, `Burning`) — no new input
action.

### Honest complexity assessment

This is the largest of the options and amounts to writing a miniature animation runtime: roughly
**450–600 lines of new C#** across ~10 files, plus a Shader Graph and an editor baking tool. Budget a
few focused sessions. The parts that reliably eat time are not the code volume but:

* getting bind-pose / root-space maths right (a wrong space gives an exploded or inside-out mesh),
* discovering exactly which entity Entities Graphics puts the `SkinMatrix` buffer on,
* the fact that Unity documents the render side and the compute side but not the join between them.

The payoff is that it stays 100% ECS and it teaches blob assets, `ComponentLookup` writes across
entities, transform-system ordering, and system groups — which is what you're after.

The staging below is deliberate: **each stage ends at something you can see on screen**, so a failure
is localized instead of being a black screen with five suspects.

---

## How the pieces fit

All of this lives under `Assets/_Pyre/Scripts/Skeletons/`, namespace `Pyre.Skeletons.*` — deliberately
*not* `Pyre.Animations`, which is property/tween animation (blink, pulse, light blink) and shares nothing
with skeletal deformation but the word.

```
AnimationClip (FBX)
   │  editor bake (sample TRS per frame per bone)
   ▼
SkeletonClipSet (ScriptableObject, serialized float arrays)
   │  Baker → BlobAssetReference<SkeletonClipLibrary>
   ▼
SkeletonPose (IComponentData)     ──┐
SkeletonBone   (IBufferElementData) │  SkeletonPoseSystem  (before TransformSystemGroup)
                                    ▼  writes LocalTransform of every bone entity
                          TransformSystemGroup → LocalToWorld per bone
                                    │
SkinBone (IBufferElementData) ──────┤  SkinMatrixSystem  (PresentationSystemGroup)
SkinTarget (IComponentData)         ▼  writes DynamicBuffer<SkinMatrix>
                    Entities Graphics deformation systems → GPU
                                    ▼
                Shader Graph material w/ Linear Blend Skinning node
```

Two separate bone lists, and this distinction matters:

* **`SkeletonBone`** — *every* transform under the model root that the clips animate, in hierarchy
  order. This is what `SkeletonPoseSystem` writes `LocalTransform` to.
* **`SkinBone`** — only `SkinnedMeshRenderer.bones[i]` plus `sharedMesh.bindposes[i]`, in the
  renderer's own order. This is what the skin-matrix system reads.

They are usually *not* the same set. `smr.bones` omits intermediate/parent transforms that carry
animation but influence no vertices; if you drive only `smr.bones`, those parents stay frozen at bind
pose and the character animates wrong in a way that is very confusing to debug.

---

## Stage 0 — Prerequisites

* **`git lfs pull`.** Every `.fbx` in this checkout is a 131-byte LFS pointer. Nothing below works
  until the real files are present.
* **Importer settings on `Assets/_Pyre/Models/character-oopi.fbx`** (currently `animationType: 2`
  Generic, `avatarSetup: 0` No Avatar, `clipAnimations: []`):
  * Keep **Animation Type = Generic**.
  * Set **Avatar Definition = Create From This Model** (`avatarSetup: 1`). Not strictly required by
    this approach — generic clips bind by transform path, so `SampleAnimation` works without an
    Avatar — but it makes the rig inspectable and costs nothing.
  * Keep **`optimizeGameObjects: 0`**. If this is ever turned on the bone hierarchy is stripped and
    there is nothing left to animate.
  * Under **Model**, enable **Read/Write** on the mesh. Entities Graphics' deformation path needs CPU
    access to the mesh data.
  * Confirm the clips actually import as sub-assets. The FBX carries 24 takes:
    `idle`, `walk`, `sprint`, `crouch`, `jump`, `fall`, `die`, `sit`, `drive`, `static`, `pick-up`,
    `interact-left`, `interact-right`, `emote-yes`, `emote-no`, `attack-kick-left`, `attack-kick-right`,
    `attack-melee-left`, `attack-melee-right`, `holding-left`, `holding-right`, `holding-both`,
    `holding-left-shoot`, `holding-right-shoot`, `holding-both-shoot`.
* Confirm in the Project window that the imported model prefab has a real `SkinnedMeshRenderer` and a
  bone hierarchy. If the FBX has animation but no skinning, this whole approach does not apply and the
  model needs to be re-exported.

---

## Stage 1 — Get a skinned mesh rendering, undeformed

Goal: the warning disappears and the character still draws. No animation yet.

1. **New Shader Graph** `Assets/_Pyre/Shaders/Hero_Skinned.shadergraph`, HDRP **Lit** target, to
   match the look of the existing HDRP/Lit materials.
   * Add a **Linear Blend Skinning** node; wire its `Position` / `Normal` / `Tangent` outputs into the
     Vertex block. This node only appears when Entities Graphics is installed — if it is missing,
     stop and resolve that first.
   * Base Color: `Sample Texture 2D` of the character colormap (the existing materials bind
     `_BaseColorMap`; the character's atlas lives in `Assets/_Pyre/Textures/`) by UV0. Mirror
     `Assets/_Pyre/Shaders/Bomb Shader Graph.shadergraph` for the target/settings conventions already
     used here.
   * The **Compute Deformation** node and the `ENABLE_COMPUTE_DEFORMATIONS` scripting define are only
     needed for blend shapes. Skip both — linear blend skinning is enough and is one less moving part.
2. **New material** `Assets/_Pyre/Materials/Hero_Skinned.mat` using that graph.
3. **Edit `Assets/_Pyre/Prefabs/Hero.prefab`:** delete the `body-mesh` child and replace it with an
   instance of the `character-oopi` model prefab (which brings the `SkinnedMeshRenderer` *and* the bone
   hierarchy). Set its localScale to **3** to preserve the current size, and assign `Hero_Skinned.mat` in
   the `SkinnedMeshRenderer`'s material slot — this overrides the FBX-embedded material without having
   to extract it.
   * Leave the other children (`indicator-square-c`, `Icon_Fire`, `Icon_Ignition`, `Fire`) untouched;
     they are parented to the Hero root, which still moves normally.
   * `LocalTransform` only supports uniform scale, so 3 is fine.
4. Re-open/re-bake the subscene `Assets/_Pyre/Scenes/Prototype/Entity Sub Scene.unity`.

**Checkpoint:** Play. The character renders in bind pose, no HDRP-material warning.

**Resolved** (from `SkinnedMeshRendererBaker` in Entities Graphics 6.5.0, not assumed):

* `DynamicBuffer<SkinMatrix>` is created by Entities Graphics itself, on the entity of the
  **`SkinnedMeshRenderer`'s own GameObject** (`GetEntity(TransformUsageFlags.Dynamic)` in its baker),
  sized to `smr.bones.Length` and pre-filled with bind-pose matrices. **Our baker must not add it.**
* `DeformedEntity` and `DeformedMeshIndex` are `internal` to `Unity.Rendering` — unusable from
  `Assembly-CSharp`, and unnecessary. `Unity.Deformations.SkinMatrix` is public.
* The renderer entities are additional entities parented to `smr.rootBone` (falling back to the
  renderer's transform) with an identity `LocalTransform`. **So skin matrices are consumed in root-bone
  space** — not world space, and not the deformed entity's space when the two transforms differ.
* A baker may only add components to its own primary and additional entities
  (`Baker.CheckValidAdditionalEntity`), so a baker on the model root cannot write to the deformed
  entity. Hence `SkinTarget` below.

---

## Stage 2 — Bake clips and skeleton into blob assets

Follow the existing conventions: `Feature/Components/*.cs`, `Feature/Components/Authoring/*Authoring.cs`,
`Feature/Systems/*System.cs`, `Feature/Settings/*.cs`, namespace `Pyre.Skeletons.*`. There are no asmdefs,
so runtime code lands in `Assembly-CSharp` and editor-only code must live in an `Editor/` folder
(`Assembly-CSharp-Editor`, which references `Assembly-CSharp`, so the one-way dependency is fine).

**`Assets/_Pyre/Scripts/Skeletons/Settings/SkeletonClipSet.cs`** — a `ScriptableObject`
(`[CreateAssetMenu(menuName = "Pyre/Skeletons/Skeleton Clip Set")]`, matching the existing
`BlinkAnimationConfig` / `PulseAnimationConfig` pattern). Holds the model prefab, the list of
`AnimationClip`s, a sample rate (30 is plenty), and the *baked output*: for each clip, a flat
`Vector3[] translations`, `Quaternion[] rotations`, `float[] scales` laid out `[frame * boneCount + bone]`,
plus `boneCount`, `frameCount`, `length`, `looping`, and the bone paths the order was recorded against.

**`Assets/_Pyre/Scripts/Skeletons/Editor/SkeletonClipSetEditor.cs`** — a `[CustomEditor]` with a
**Bake Clips** button. Doing the sampling here rather than inside a Baker matters: `SampleAnimation`
*mutates the hierarchy it samples*, and bakers must not have side effects. The button:

```csharp
var instance = (GameObject)PrefabUtility.InstantiatePrefab(set.ModelPrefab);
var bones = instance.GetComponentsInChildren<Transform>(true); // hierarchy order, index 0 = root
foreach (var clip in set.Clips)
    for (var f = 0; f < frameCount; f++)
    {
        clip.SampleAnimation(instance, f / (float)set.SampleRate);
        for (var b = 0; b < bones.Length; b++)
        {
            translations[f * bones.Length + b] = bones[b].localPosition;
            rotations   [f * bones.Length + b] = bones[b].localRotation;
            scales      [f * bones.Length + b] = bones[b].localScale.x;
        }
    }
Object.DestroyImmediate(instance);
EditorUtility.SetDirty(set);
```

Sampling into the live hierarchy and reading transforms back is what makes clip-curve-path ↔ bone-index
mapping a non-issue: Unity resolves the paths, we just read the result in a fixed order.

**Root motion:** if the clips translate the root bone, the character will slide and fight
`PlayerMovementSystem` (which writes `PhysicsVelocity.Linear` directly). Behind a `stripRootMotion`
toggle, hold the bone named by `rootMotionBone` (default `root`, matching the importer's
`rootMotionBoneName`) at its **bind-pose translation** for every frame. Not zero — zeroing drops the hip
height offset and sinks the character through the floor.

**`Assets/_Pyre/Scripts/Skeletons/Components/SkeletonClipLibrary.cs`**

```csharp
public struct BoneKey { public float3 Translation; public quaternion Rotation; public float Scale; }

public struct SkeletonClipBlob
{
    public float Length, FrameRate;
    public int FrameCount, BoneCount;
    public bool Looping;
    public BlobArray<BoneKey> Keys;   // [frame * BoneCount + bone]
}

public struct SkeletonClipLibrary { public BlobArray<SkeletonClipBlob> Clips; }
```

**`Assets/_Pyre/Scripts/Skeletons/Components/SkeletonBone.cs`** — `IBufferElementData { Entity Bone; }`,
hierarchy order, matching the bake order exactly.

**`Assets/_Pyre/Scripts/Skeletons/Components/SkinBone.cs`** — `IBufferElementData { Entity Bone; float4x4 BindPose; }`,
in `smr.bones` order.

**`Assets/_Pyre/Scripts/Skeletons/Components/SkinTarget.cs`** —
`IComponentData { Entity DeformedEntity; Entity SkinSpaceBone; }`. Not in the original plan; forced by the
two Stage 1 findings above. `SkinBone` lives on the skeleton root next to `SkeletonBone`, and Stage 4
writes through a `BufferLookup<SkinMatrix>` into `DeformedEntity` rather than iterating the buffer directly.

**`Assets/_Pyre/Scripts/Skeletons/Components/Authoring/SkeletonAuthoring.cs`** — sits on the model root,
which owns both the bone hierarchy and the `SkinnedMeshRenderer` as descendants. Its baker:

* `DependsOn(authoring.ClipSet)` — same rebake-dependency idiom as `BlinkAnimationSourceAuthoring`.
* Builds the `SkeletonClipLibrary` with a `BlobBuilder` and `AddBlobAsset` (so it is deduplicated and
  serialized into the subscene).
* Fills `SkeletonBone` from `GetComponentsInChildren<Transform>()` — the Baker overload, which defaults to
  `includeInactive: true` and so matches the bake tool's ordering — via
  `GetEntity(t, TransformUsageFlags.Dynamic)`. **`Dynamic` is required**, the bones must have a writable
  `LocalTransform`.
* Fills `SkinBone` from `smr.bones[i]` + `smr.sharedMesh.bindposes[i]`, and adds `SkinTarget`.
* Adds `SkeletonPose` (Stage 3). It does **not** add the `SkinMatrix` buffer — Entities Graphics does.
* Verifies its own bone list against the paths recorded at bake time. The blob is indexed by that order;
  a mismatch would apply every pose to the wrong bone silently.

**Asset:** `Assets/_Pyre/Settings/Hero Skeleton Clips.asset`, created from the menu, pointed at
`character-oopi` with the clips you want, then **Bake Clips**.

---

## Stage 3 — Sample the pose onto the bones

**`Assets/_Pyre/Scripts/Skeletons/Components/SkeletonPose.cs`** — designed for a manual two-clip
blend from the start, which is what makes idle↔walk not pop:

```csharp
public struct SkeletonPose : IComponentData
{
    public BlobAssetReference<SkeletonClipLibrary> Library;
    public int   ClipA, ClipB;
    public float TimeA, TimeB;
    public float Blend;   // 0 = pure A, 1 = pure B
    public float Speed;
}
```

**`Assets/_Pyre/Scripts/Skeletons/Systems/SkeletonPoseSystem.cs`** — Burst `ISystem`,
`[UpdateInGroup(typeof(SimulationSystemGroup))] [UpdateBefore(typeof(TransformSystemGroup))]`, so the
bone `LocalToWorld`s are rebuilt from the new pose in the same frame.

An `IJobEntity` over `(ref SkeletonPose, in DynamicBuffer<SkeletonBone>)` advances both clip times
(wrapping on `Length` when `Looping`), samples each clip at its two bracketing frames with
`math.lerp` / `math.slerp`, blends A→B by `Blend`, and writes the result through a
`ComponentLookup<LocalTransform>`.

Write via `.Schedule()` (single-threaded) rather than `.ScheduleParallel()`. Parallel writes through a
`ComponentLookup` need `[NativeDisableParallelForRestriction]`, which is only safe because bone sets
are disjoint — an assumption not worth taking on for one character.

**Checkpoint:** set `SkeletonAuthoring.DefaultClip` to `walk` and press Play. The bone entities should visibly animate
in the Entities Hierarchy inspector *even though the mesh is still in bind pose* — nothing is feeding
`SkinMatrix` yet. Seeing the transforms move here proves Stages 2–3 independently of the GPU side.

---

## Stage 4 — Compute skin matrices

**`Assets/_Pyre/Scripts/Skeletons/Systems/SkinMatrixSystem.cs`** — Burst `ISystem`,
`[UpdateInGroup(typeof(PresentationSystemGroup))]` and
`[UpdateBefore(typeof(Unity.Rendering.DeformationsInPresentation))]`. That group **is** public in Entities
Graphics 6.5.0, so no fallback is needed. `PushSkinMatrixSystem` runs inside it and reads the buffer.

For each skeleton root holding `(SkinTarget, DynamicBuffer<SkinBone>)`, with `rootLtw` = the
`LocalToWorld` of **`SkinTarget.SkinSpaceBone`** — `smr.rootBone`, which is the space the render entities
are parented to, and therefore the space Linear Blend Skinning expects:

```csharp
var worldToRoot = math.inverse(ltwLookup[skinTarget.SkinSpaceBone].Value);
var skinMatrices = skinMatrixLookup[skinTarget.DeformedEntity];
for (var i = 0; i < skinBones.Length; i++)
{
    var m = math.mul(worldToRoot, math.mul(ltwLookup[skinBones[i].Bone].Value, skinBones[i].BindPose));
    skinMatrices[i] = new SkinMatrix { Value = new float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz) };
}
```

This is exactly what `SkinnedMeshRendererBaker` computes at bake time (`rootMatrixInv * boneLtw * bindPose`),
so the identity case is checkable: with `SkeletonPoseSystem` disabled the runtime result must equal the
baked bind-pose buffer.

**Checkpoint:** the mesh deforms. If it explodes or turns inside out, the cause is almost always this
formula's spaces — confirm `SkinSpaceBone` really is `smr.rootBone` and check `bindposes` orientation
before touching anything else.

---

## Stage 5 — Drive clip selection from gameplay

**`Assets/_Pyre/Scripts/Skeletons/Components/CharacterSkeletonState.cs`**

```csharp
public struct CharacterSkeletonState : IComponentData
{
    public float NormalizedSpeed;  // 0..1
    public bool  IsBurning;
    public float IgnitionRatio;    // 0..1, "being heated" reaction
}
```

**`Assets/_Pyre/Scripts/Skeletons/Systems/CharacterSkeletonStateSystem.cs`** — Burst `ISystem` in
`SimulationSystemGroup`, `[UpdateBefore(typeof(SkeletonPoseSystem))]`. Nothing currently
stores a speed anywhere (`PlayerMovementSystem` writes `PhysicsVelocity.Linear` and moves on), so
derive it:

* `NormalizedSpeed = saturate(length(velocity.Linear.xz) / movement.MoveSpeed)` from `PhysicsVelocity`
  + `PlayerMovement` (`Assets/_Pyre/Scripts/Player/Components/PlayerMovement.cs`).
* `IsBurning = HasComponent<Burning>(entity)` — added by `FirePropagationSystem`
  (`Assets/_Pyre/Scripts/Gameplay/Systems/FirePropagationSystem.cs:100`), removed by
  `FireExtinguishingSystem`.
* `IgnitionRatio = IgnitionProgress.Elapsed / Ignitable.IgnitionTime` — the Hero is `Ignitable`, so
  this rises while it stands in heat and decays via `CoolingRate`. This is the "interaction" signal:
  a flinch/react pose that blends in as the player starts catching fire, and a burning loop once
  `Burning` lands.

**`Assets/_Pyre/Scripts/Skeletons/Systems/CharacterClipSelectionSystem.cs`** — maps that state to
`SkeletonPose`:

* `ClipA = Idle`, `ClipB = Walk`, `Blend = NormalizedSpeed` — a hand-rolled 1D blend tree, and the
  reason `SkeletonPose` carries two slots.
* Scale `Speed` with `NormalizedSpeed` so footfalls roughly track ground speed.
* When `IsBurning`, swap `ClipB` to the burn/panic clip and drive `Blend` toward 1 over a short fade
  rather than snapping.
* Keep `TimeA`/`TimeB` phase-synced while blending locomotion clips, otherwise idle and walk fight
  each other.

Clip indices come from named fields on the `SkeletonClipSet` asset, resolved via `SkeletonClipSet.IndexOf`
at bake time — do not hardcode integers in systems. Candidate roles from the imported takes: `idle`,
`walk`, `sprint`, `interact-right` or `emote-no` for the ignition flinch, `die` or `fall` for burning.

---

## Files

Everything skeletal lives in its own feature folder, `Assets/_Pyre/Scripts/Skeletons/`. `Animations/` keeps
only the tween/property animation it already had.

**Done — Stage 1**
```
Assets/_Pyre/Shaders/Hero_Skinned.shadergraph
Assets/_Pyre/Materials/Hero_Skinned.mat
```

**Done — Stage 2**
```
Assets/_Pyre/Scripts/Skeletons/Settings/SkeletonClipSet.cs
Assets/_Pyre/Scripts/Skeletons/Editor/SkeletonClipSetEditor.cs
Assets/_Pyre/Scripts/Skeletons/Components/SkeletonClipLibrary.cs
Assets/_Pyre/Scripts/Skeletons/Components/SkeletonPose.cs
Assets/_Pyre/Scripts/Skeletons/Components/SkeletonBone.cs
Assets/_Pyre/Scripts/Skeletons/Components/SkinBone.cs
Assets/_Pyre/Scripts/Skeletons/Components/SkinTarget.cs
Assets/_Pyre/Scripts/Skeletons/Components/Authoring/SkeletonAuthoring.cs
```

**Remaining**
```
Assets/_Pyre/Settings/Hero Skeleton Clips.asset                        (Stage 2, authored in-editor)
Assets/_Pyre/Scripts/Skeletons/Systems/SkeletonPoseSystem.cs           (Stage 3)
Assets/_Pyre/Scripts/Skeletons/Systems/SkinMatrixSystem.cs             (Stage 4)
Assets/_Pyre/Scripts/Skeletons/Components/CharacterSkeletonState.cs    (Stage 5)
Assets/_Pyre/Scripts/Skeletons/Systems/CharacterSkeletonStateSystem.cs (Stage 5)
Assets/_Pyre/Scripts/Skeletons/Systems/CharacterClipSelectionSystem.cs (Stage 5)
```

**Modified**
```
Assets/_Pyre/Models/character-oopi.fbx.meta          (avatarSetup, mesh Read/Write — done)
Assets/_Pyre/Prefabs/Hero.prefab                     (body-mesh → skinned model instance — done;
                                                      still needs SkeletonAuthoring on the model root)
Assets/_Pyre/Scenes/Prototype/Entity Sub Scene.unity (re-bake)
```

Nothing in `Assets/_Pyre/Scripts/Player/` changes — movement stays exactly as it is and the skeleton only
reads from it.

---

## Verification

Each stage has its own checkpoint above; run them in order rather than wiring everything and playing
once. End to end:

1. Open `Assets/_Pyre/Scenes/Prototype.unity`, enter Play mode.
2. Console is clean — in particular no Entities Graphics material/deformation warning.
3. WASD: the character blends idle → walk, speed of the cycle tracks movement speed, and it turns with
   the existing `RotationSpeed` slerp without foot sliding beyond what the clips imply.
4. Walk into the campfire: as `IgnitionProgress.Elapsed` climbs the reaction pose blends in; when
   `Burning` is added the burn state takes over; `FireExtinguishingSystem` returns it to locomotion.
5. *Window → Entities → Hierarchy* on the Hero: bone `LocalTransform`s change per frame and the
   `SkinMatrix` buffer is non-identity.
6. Trigger an explosion (`DebugActionsSystem` has debug keys) and confirm knockback still reads
   correctly with the animated mesh.
7. Profile briefly — `SkeletonPoseSystem` is `.Schedule()`d single-threaded, which is
   fine for one character but is the first thing to revisit if the Enemy is animated later.

---

## Risks

* ~~**`git lfs pull` is a hard prerequisite**~~ — done. Clip names are listed in Stage 0. Bone count is
  still unknown until the set is baked; the baker asserts it against the hierarchy.
* ~~**Which entity owns `SkinMatrix`**~~ — resolved at the Stage 1 checkpoint, see there.
* **Bind-pose/root-space maths** is the most likely source of a broken-looking mesh; Stage 3's
  checkpoint isolates it from the sampling code.
* **Root motion** in the clips will fight `PlayerMovementSystem` — handled by the `stripRootMotion`
  bake toggle.
* **`optimizeGameObjects` and `optimizeBones`** in the importer both reshape the bone set. Bake
  `SkinBone` from `smr.bones` at bake time (which we do) rather than assuming a hierarchy order.
* **Blend shapes are out of scope.** Adding them later means the Compute Deformation node plus the
  `ENABLE_COMPUTE_DEFORMATIONS` scripting define.
* **Fallback:** if the deformation path proves unworkable in HDRP 17.5 / Entities Graphics 6.5.0, the
  escape hatch is a hybrid presentation layer — entity stays sim-only, a plain GameObject with an
  `Animator` is spawned at runtime and synced from `LocalToWorld`, following the existing
  `AudioBridge` / `CameraBridge` idiom. Stages 0, 1 and 5 carry over; Stages 2–4 are discarded.

using System.Collections.Generic;
using Pyre.Gameplay.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Pyre.UI
{
    /// <summary>
    /// Spawns a world-space progress icon for every entity that carries <see cref="Ignitable"/> or
    /// <see cref="Explosive"/>, and destroys it once the entity stops matching. Entities are read-only here:
    /// nothing is written back into the world, so there are no structural changes and no ECS-side view state.
    /// </summary>
    public class ComponentIcons : MonoBehaviour
    {
        [Header("Ignition Progress")]
        [SerializeField] private bool showIgnitionProgress = true;
        [SerializeField] private ProgressIconView ignitionIconPrefab;
        [SerializeField] private Vector3 ignitionOffset = new(0f, 2.25f, 0f);

        [Header("Explode Timer")]
        [SerializeField] private bool showExplodeTimer = true;
        [SerializeField] private ProgressIconView explodeIconPrefab;
        [SerializeField] private Vector3 explodeOffset = new(0f, 3f, 0f);

        private readonly Dictionary<Entity, ProgressIconView> _ignitionIcons = new();
        private readonly Dictionary<Entity, ProgressIconView> _explodeIcons = new();
        private readonly HashSet<Entity> _liveEntities = new();
        private readonly List<Entity> _staleEntities = new();

        private Camera _camera;
        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _ignitableQuery;
        private EntityQuery _explosiveQuery;

        private void LateUpdate()
        {
            if (!TryResolveDependencies())
                return;

            var cameraRotation = _camera.transform.rotation;

            UpdateIgnitionIcons(cameraRotation);
            UpdateExplodeIcons(cameraRotation);
        }

        private void OnDestroy()
        {
            ClearIcons(_ignitionIcons);
            ClearIcons(_explodeIcons);
        }

        /// <summary>
        /// Resolved lazily rather than in Start: the default world is null during domain reload and on
        /// play mode exit, and subscene entities do not exist for the first few frames while it streams in.
        /// </summary>
        private bool TryResolveDependencies()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world is not { IsCreated: true })
            {
                _world = null;
                return false;
            }

            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return false;

            if (_world == world)
                return true;

            _world = world;
            _entityManager = world.EntityManager;

            _ignitableQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Ignitable>(),
                ComponentType.ReadOnly<IgnitionProgress>(),
                ComponentType.ReadOnly<LocalToWorld>());

            _explosiveQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Explosive>(),
                ComponentType.ReadOnly<LocalToWorld>());

            return true;
        }

        private void UpdateIgnitionIcons(Quaternion cameraRotation)
        {
            if (!showIgnitionProgress || ignitionIconPrefab == null)
            {
                ClearIcons(_ignitionIcons);
                return;
            }

            using var entities = _ignitableQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var ignitable = _entityManager.GetComponentData<Ignitable>(entity);
                var progress = _entityManager.GetComponentData<IgnitionProgress>(entity);
                var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);

                var icon = GetOrCreateIcon(_ignitionIcons, entity, ignitionIconPrefab);

                icon.SetVisible(progress.Elapsed > 0f && !_entityManager.HasComponent<Burning>(entity));
                icon.SetProgress(ignitable.IgnitionTime > 0f ? progress.Elapsed / ignitable.IgnitionTime : 0f);
                icon.Place((Vector3)localToWorld.Position + ignitionOffset, cameraRotation);
            }

            RemoveStaleIcons(_ignitionIcons, entities);
        }

        private void UpdateExplodeIcons(Quaternion cameraRotation)
        {
            if (!showExplodeTimer || explodeIconPrefab == null)
            {
                ClearIcons(_explodeIcons);
                return;
            }

            using var entities = _explosiveQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var explosive = _entityManager.GetComponentData<Explosive>(entity);
                var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);

                // ExplodeTimer is added by ExplodeSystem only once the fuse is lit.
                var isCountingDown = _entityManager.HasComponent<ExplodeTimer>(entity);
                var icon = GetOrCreateIcon(_explodeIcons, entity, explodeIconPrefab);

                icon.SetVisible(isCountingDown);

                if (isCountingDown && explosive.Delay > 0f)
                {
                    var timer = _entityManager.GetComponentData<ExplodeTimer>(entity);
                    icon.SetProgress(timer.TimeRemaining / explosive.Delay);
                }

                icon.Place((Vector3)localToWorld.Position + explodeOffset, cameraRotation);
            }

            RemoveStaleIcons(_explodeIcons, entities);
        }

        private ProgressIconView GetOrCreateIcon(
            Dictionary<Entity, ProgressIconView> icons, Entity entity, ProgressIconView prefab)
        {
            if (icons.TryGetValue(entity, out var icon) && icon != null)
                return icon;

            icon = Instantiate(prefab, transform);
            icons[entity] = icon;

            return icon;
        }

        private void RemoveStaleIcons(Dictionary<Entity, ProgressIconView> icons, NativeArray<Entity> liveEntities)
        {
            _liveEntities.Clear();
            foreach (var entity in liveEntities)
            {
                _liveEntities.Add(entity);
            }

            _staleEntities.Clear();
            foreach (var pair in icons)
            {
                if (!_liveEntities.Contains(pair.Key))
                    _staleEntities.Add(pair.Key);
            }

            foreach (var entity in _staleEntities)
            {
                DestroyIcon(icons[entity]);
                icons.Remove(entity);
            }
        }

        private void ClearIcons(Dictionary<Entity, ProgressIconView> icons)
        {
            if (icons.Count == 0)
                return;

            foreach (var icon in icons.Values)
            {
                DestroyIcon(icon);
            }

            icons.Clear();
        }

        private static void DestroyIcon(ProgressIconView icon)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
    }
}

using System.Collections.Generic;
using Pyre.Gameplay.Components;
using Pyre.UI.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Pyre.UI
{
    public class DebugIconsUI : MonoBehaviour
    {
        [Header("Ignition Progress")]
        [SerializeField] private DebugIconView ignitionIconPrefab;

        [Header("Explode Timer")]
        [SerializeField] private DebugIconView explodeIconPrefab;

        [Header("Placement")]
        [SerializeField] private Vector3 fallbackOffset = new(0f, 2.25f, 0f);

        private readonly Dictionary<Entity, DebugIconView> _ignitionIcons = new();
        private readonly Dictionary<Entity, DebugIconView> _explodeIcons = new();
        private readonly List<Entity> _staleEntities = new();

        private Camera _camera;
        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _ignitableQuery;
        private EntityQuery _explosiveQuery;

        private void Start()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            _entityManager = _world.EntityManager;
            _camera = Camera.main;

            _ignitableQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Ignitable>(),
                ComponentType.ReadOnly<IgnitionProgress>(),
                ComponentType.ReadOnly<LocalToWorld>());

            _explosiveQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Explosive>(),
                ComponentType.ReadOnly<LocalToWorld>());
        }

        private void LateUpdate()
        {
            var cameraRotation = _camera.transform.rotation;

            UpdateIgnitionIcons(cameraRotation);
            UpdateExplodeIcons(cameraRotation);
        }

        private void UpdateIgnitionIcons(Quaternion cameraRotation)
        {
            if (ignitionIconPrefab == null)
                return;

            using var entities = _ignitableQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var ignitable = _entityManager.GetComponentData<Ignitable>(entity);
                var progress = _entityManager.GetComponentData<IgnitionProgress>(entity);
                var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);

                var (isEnabled, offset) = GetIconSettings(entity);

                var icon = GetOrCreateIcon(_ignitionIcons, entity, ignitionIconPrefab);

                icon.SetVisible(isEnabled && progress.Elapsed > 0f && !_entityManager.HasComponent<Burning>(entity));
                icon.SetProgress(ignitable.IgnitionTime > 0f ? progress.Elapsed / ignitable.IgnitionTime : 0f);
                icon.Place((Vector3)localToWorld.Position + offset, cameraRotation);
            }

            RemoveStaleIcons(_ignitionIcons, entities);
        }

        private void UpdateExplodeIcons(Quaternion cameraRotation)
        {
            if (explodeIconPrefab == null)
                return;

            using var entities = _explosiveQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var explosive = _entityManager.GetComponentData<Explosive>(entity);
                var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);

                var isCountingDown = _entityManager.HasComponent<ExplodeTimer>(entity);
                var (isEnabled, offset) = GetIconSettings(entity);

                var icon = GetOrCreateIcon(_explodeIcons, entity, explodeIconPrefab);

                icon.SetVisible(isEnabled && isCountingDown);

                if (isCountingDown && explosive.Delay > 0f)
                {
                    var timer = _entityManager.GetComponentData<ExplodeTimer>(entity);
                    icon.SetProgress(timer.TimeRemaining / explosive.Delay);
                }

                icon.Place((Vector3)localToWorld.Position + offset, cameraRotation);
            }

            RemoveStaleIcons(_explodeIcons, entities);
        }

        private (bool isEnabled, Vector3 offset) GetIconSettings(Entity entity)
        {
            if (_entityManager.HasComponent<DebugIconSettings>(entity))
            {
                var settings = _entityManager.GetComponentData<DebugIconSettings>(entity);
                return (settings.Enabled, settings.Offset);
            }

            return (isEnabled: true, fallbackOffset);
        }

        private DebugIconView GetOrCreateIcon(Dictionary<Entity, DebugIconView> icons, Entity entity, DebugIconView prefab)
        {
            if (icons.TryGetValue(entity, out var icon))
            {
                return icon;
            }

            icon = Instantiate(prefab, transform);
            icons[entity] = icon;

            return icon;
        }

        private void RemoveStaleIcons(Dictionary<Entity, DebugIconView> icons, NativeArray<Entity> liveEntities)
        {
            _staleEntities.Clear();
            foreach (var pair in icons)
            {
                if (!liveEntities.Contains(pair.Key))
                {
                    _staleEntities.Add(pair.Key);
                }
            }

            foreach (var entity in _staleEntities)
            {
                var icon = icons[entity];
                Destroy(icon.gameObject);
                icons.Remove(entity);
            }
        }
    }
}
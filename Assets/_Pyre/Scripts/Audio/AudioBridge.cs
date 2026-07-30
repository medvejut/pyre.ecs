using Pyre.Audio.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio
{
    public class AudioBridge : MonoBehaviour
    {
        [SerializeField] private float volume = 1f;

        private EntityQuery _entityQuery;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityQuery = entityManager.CreateEntityQuery(typeof(SoundEvent));
        }

        private void LateUpdate()
        {
            var buffer = _entityQuery.GetSingletonBuffer<SoundEvent>();

            foreach (var soundEvent in buffer)
            {
                if (soundEvent.Clip)
                {
                    PlayClip(soundEvent.Clip.Value, soundEvent.Position, volume, soundEvent.SpatialBlend);
                }
            }

            buffer.Clear();
        }

        private static void PlayClip(AudioClip clip, Vector3 position, float volume, float spatialBlend)
        {
            var gameObject = new GameObject("One shot audio");
            gameObject.transform.position = position;
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
            audioSource.Play();
            Destroy(gameObject, clip.length * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
        }
    }
}
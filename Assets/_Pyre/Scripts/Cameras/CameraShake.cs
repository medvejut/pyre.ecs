using UnityEngine;
using Random = UnityEngine.Random;

namespace Pyre.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        [Header("Default")]
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private float defaultAmplitude = 1f;

        private float _duration;
        private float _amplitude;

        public void Shake(float duration, float amplitude)
        {
            _duration = duration;
            _amplitude = amplitude;
        }

        public void Shake()
        {
            Shake(defaultDuration, defaultAmplitude);
        }

        private void LateUpdate()
        {
            if (_duration <= 0)
            {
                transform.localPosition = Vector3.zero;
                return;
            }

            _duration -= Time.deltaTime;

            var t = _duration;
            var strength = _amplitude * t;

            transform.localPosition = Random.insideUnitSphere * strength;
        }
    }
}
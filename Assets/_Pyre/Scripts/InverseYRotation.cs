using UnityEngine;

namespace Pyre
{
    [ExecuteAlways]
    public class InverseYRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void LateUpdate()
        {
            var euler = target.localEulerAngles;
            transform.localRotation = Quaternion.Euler(euler.x, -euler.y, euler.z);
        }
    }
}
using UnityEngine;

namespace Pyre
{
    public class InverseYRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void LateUpdate()
        {
            if (target)
            {
                var euler = target.localEulerAngles;
                transform.localRotation = Quaternion.Euler(euler.x, -euler.y, euler.z);
            }
        }
    }
}
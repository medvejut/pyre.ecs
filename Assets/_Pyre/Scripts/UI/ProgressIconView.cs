using UnityEngine;
using UnityEngine.UI;

namespace Pyre.UI
{
    public class ProgressIconView : MonoBehaviour
    {
        [SerializeField] private Image fill;

        public void SetVisible(bool visible)
        {
            fill.enabled = visible;
        }

        public void SetProgress(float value)
        {
            fill.fillAmount = Mathf.Clamp01(value);
        }

        public void Place(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}

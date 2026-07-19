using UnityEngine;

namespace Pyre.Input
{
    public class HeroInput : MonoBehaviour
    {
        private PlayerInputActions _inputActions;

        public Vector2 GetMove() => _inputActions != null ? _inputActions.Gameplay.Move.ReadValue<Vector2>() : Vector2.zero;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }
    }
}
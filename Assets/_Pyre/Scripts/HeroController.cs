using UnityEngine;
using UnityEngine.InputSystem;

namespace Pyre
{
    [RequireComponent(typeof(CharacterController))]
    public class HeroController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [Space]
        [SerializeField] private Vector3 isometricDirectionMultiplier = new(0, 45f, 0);

        private CharacterController _characterController;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var inputDirection = GetInputDirection();
            var isometricDirection = Quaternion.Euler(isometricDirectionMultiplier) * inputDirection;

            var moveDirection = isometricDirection * moveSpeed;
            moveDirection.y = 0f;

            _characterController.Move(moveDirection * Time.deltaTime);

            if (isometricDirection != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(isometricDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private static Vector3 GetInputDirection()
        {
            var keyboardInput = Vector2.zero;
            var keyboard = Keyboard.current;

            if (keyboard.wKey.isPressed)
            {
                keyboardInput.y += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                keyboardInput.y -= 1f;
            }

            if (keyboard.aKey.isPressed)
            {
                keyboardInput.x -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                keyboardInput.x += 1f;
            }

            return new Vector3(keyboardInput.x, 0f, keyboardInput.y).normalized;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using MoonsAndStars.Assets.Code.Scripts.Player.Input;

namespace MoonsAndStars.Assets.Code.Scripts.Player.Input
{
    public class ShipController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _boostSpeed = 10f;
        [SerializeField] private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnBoost(InputValue input)
        {
            throw new System.NotImplementedException();
        }

        public void OnFire(InputValue input)
        {
            throw new System.NotImplementedException();
        }

        public void OnLook(InputValue input)
        {
            throw new System.NotImplementedException();
        }

        public void OnMove(InputValue input)
        {
            float movement = input.Get<float>();
            _rb.AddForce(Vector3.forward * movement * _speed);
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
        
        }

        public void OnStrafe(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpDown(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
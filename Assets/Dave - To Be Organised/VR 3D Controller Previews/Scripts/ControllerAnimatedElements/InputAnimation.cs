using UnityEngine;
using UnityEngine.InputSystem;

namespace VR3DControllerPreviews
{
    public abstract class InputAnimation : MonoBehaviour
    {
        [Header("Reference")]
        public InputActionReference inputAction;

        private void OnEnable()
        {
            inputAction.action.started += Started;
            inputAction.action.performed += Performed;
            inputAction.action.canceled += Canceled;
        }

        private void OnDisable()
        {
            inputAction.action.started -= Started;
            inputAction.action.performed -= Performed;
            inputAction.action.canceled -= Canceled;
        }

        private void Canceled(InputAction.CallbackContext context) => InputCanceled(context);
        private void Performed(InputAction.CallbackContext context) => InputPerformed(context);
        private void Started(InputAction.CallbackContext context) => InputStarted(context);

        protected abstract void SetStartPosition();
        protected abstract void InputCanceled(InputAction.CallbackContext context);
        protected abstract void InputPerformed(InputAction.CallbackContext context);
        protected abstract void InputStarted(InputAction.CallbackContext context);

    }
}
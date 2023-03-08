using UnityEngine;
using UnityEngine.InputSystem;

namespace VR3DControllerPreviews
{
    /// <summary>
    /// Takes an InputActionReference and animates a GameObject
    /// Used on preview VR controller models
    /// </summary>
    public class InputFloatAnimation : InputAnimation
    {
        [Header("Settings")]
        public Vector3 startEuler = Vector3.zero;
        public Vector3 endEuler = Vector3.one;

#if UNITY_EDITOR
        [Space(10)]
        [Range(0f, 1f), SerializeField] public float _preview;
#endif

        protected override void SetStartPosition() => Animate(0);

        protected override void InputCanceled(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());
        protected override void InputPerformed(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());
        protected override void InputStarted(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());

        private void Animate(float lerp)
        {
            transform.eulerAngles = Vector3.Lerp(startEuler, endEuler, lerp);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Animate(_preview);
        }
#endif
    }
}

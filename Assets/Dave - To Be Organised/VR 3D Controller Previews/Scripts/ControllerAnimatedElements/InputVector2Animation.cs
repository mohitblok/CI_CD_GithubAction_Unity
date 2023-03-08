using UnityEngine;
using UnityEngine.InputSystem;

namespace VR3DControllerPreviews
{
    /// <summary>
    /// Takes an InputActionReference and animates a GameObject
    /// Used on preview VR controller models
    /// </summary>
    public class InputVector2Animation : InputAnimation
    {
        [Header("Settings")]
        public float maxRange = 20;
        [SerializeField] private bool _invertX = false;
        [SerializeField] private bool _invertY = false;
        public InspectorButton btn_SavePositions = new InspectorButton("Save Start Position");
        [SerializeField] private Vector3 _startEuler = Vector3.zero;

#if UNITY_EDITOR
        [Space(10)]
        [Range(-1f, 1f), SerializeField] public float _previewX = 0;
        [Range(-1f, 1f), SerializeField] public float _previewY = 0;
#endif

        private void Awake() => CalculateStartPoint();

        private void CalculateStartPoint()
        {
            _startEuler = transform.localEulerAngles;
        }

        protected override void SetStartPosition() => Animate(Vector2.zero);

        protected override void InputCanceled(InputAction.CallbackContext context) => Animate(context.ReadValue<Vector2>());
        protected override void InputPerformed(InputAction.CallbackContext context) => Animate(context.ReadValue<Vector2>());
        protected override void InputStarted(InputAction.CallbackContext context) => Animate(context.ReadValue<Vector2>());

        private void Animate(Vector2 lerp)
        {
            Vector3 newEuler = _startEuler;
            newEuler += Vector3.forward * (maxRange * lerp.x * (_invertX ? -1 : 1));
            newEuler += Vector3.right * (maxRange * lerp.y * (_invertY ? -1 : 1));
            transform.localEulerAngles = newEuler;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            btn_SavePositions.Assign(CalculateStartPoint);
            Animate(new Vector2(_previewX, _previewY));
        }
#endif
    }
}

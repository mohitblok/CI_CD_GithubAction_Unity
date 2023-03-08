using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace VR3DControllerPreviews
{
    /// <summary>
    /// Takes an InputActionReference and animates a GameObject
    /// Used on preview VR controller models
    /// </summary>
    public class InputButtonAnimation : InputAnimation
    {
        [Header("Settings")]
        public float depressionDistanceInMilimeters = 1f;
        [SerializeField] private Direction _direction = Direction.Down;
#if UNITY_EDITOR
        [Space(10)]
        [Range(0f, 1f), SerializeField] private float _preview;
        public InspectorButton btn_SavePositions = new InspectorButton("Save Positions");
#endif
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _endPosition;
        private float DepressionDistanceInMilimeters => depressionDistanceInMilimeters * 0.001f;

        private enum Direction
        {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }

        private void Awake() => CalculateStartAndEndPoints();

        private void CalculateStartAndEndPoints()
        {
            _startPosition = transform.localPosition;
            _endPosition = _startPosition + (GetLocalAxis(_direction) * DepressionDistanceInMilimeters);
        }

        protected override void SetStartPosition() => Animate(0);

        protected override void InputCanceled(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());
        protected override void InputPerformed(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());
        protected override void InputStarted(InputAction.CallbackContext context) => Animate(context.ReadValue<float>());

        private void Animate(float lerp)
        {
            transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, lerp);
        }

        private Vector3 GetLocalAxis(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward: return transform.forward;
                case Direction.Backward: return -transform.forward;
                case Direction.Left: return -transform.right;
                case Direction.Right: return transform.right;
                case Direction.Up: return transform.up;
                case Direction.Down: return -transform.up;
                default: return Vector3.zero;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            btn_SavePositions.Assign(CalculateStartAndEndPoints);
            Animate(_preview);
        }
#endif
    }
}
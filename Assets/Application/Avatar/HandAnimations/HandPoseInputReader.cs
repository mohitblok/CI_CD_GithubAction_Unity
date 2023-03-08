using UnityEngine;
using UnityEngine.InputSystem;

namespace Bloktopia.Player.Hands
{
    public class HandPoseInputReader : MonoBehaviour
    {
        [System.Serializable]
        public class HandInputActions
        {
            [field: SerializeField] public InputActionReference thumbDown;
            [field: SerializeField] public InputActionReference indexFinger;
            [field: SerializeField] public InputActionReference otherFingers;

            public void Init()
            {
                thumbDown?.action.Enable();
                indexFinger?.action.Enable();
                otherFingers?.action.Enable();
            }

            public bool ThumbDown => false; // thumbDown.action.ReadValue<bool>();
            public float IndexFinger => indexFinger.action.ReadValue<float>();
            public float OtherFingers => otherFingers.action.ReadValue<float>();
        }

        [field: SerializeField] public HandInputActions leftHand { get; private set; }
        [field: SerializeField] public HandInputActions rightHand { get; private set; }

        private void Start()
        {
            leftHand.Init();
            rightHand.Init();
        }
    }
}

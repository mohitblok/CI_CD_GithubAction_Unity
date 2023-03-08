using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Bloktopia.Fusion.Rig
{
    /// <summary>
    /// provides the logic for applying the networked input from the local XROrigin to the individual
    /// NetworkedRigComponents
    /// </summary>
    [RequireComponent(typeof(NetworkTransform))]
    // ordered after the network transform & rigidbody to enable overriding interpolation behaviour 
    [OrderAfter(typeof(NetworkTransform), typeof(NetworkRigidbody))]
    public class XRNetworkedRig : NetworkBehaviour
    {
        private Transform _transform;

        [field: SerializeField]
        public NetworkTransform Head { get; private set; }

        [field: SerializeField]
        public NetworkTransform LeftHand { get; private set; }

        [field: SerializeField]
        public NetworkTransform RightHand { get; private set; }

        [field: SerializeField]
        public List<InputToRotationSetTweener> LeftHandPoses { get; private set; }

        [field: SerializeField]
        public List<InputToRotationSetTweener> RightHandPoses { get; private set; }

        public NetworkTransform networkTransform { get; private set; }

        public bool IsLocalNetworkRig => Object.HasInputAuthority;

        [Networked(OnChanged = nameof(OnHandGestureChanged)), Capacity(6)]
        private NetworkArray<byte> handGesture => default;

        private const int LEFT_THUMB_INDEX = 0;
        private const int LEFT_INDEX_INDEX = 1;
        private const int LEFT_OTHER_INDEX = 2;
        private const int RIGHT_THUMB_INDEX = 3;
        private const int RIGHT_INDEX_INDEX = 4;
        private const int RIGHT_OTHER_INDEX = 5;

        private static void OnHandGestureChanged(Changed<XRNetworkedRig> changed)
        {
            changed.Behaviour.UpdateHandGestureChanged();
        }

        public void UpdateHandGestureChanged()
        {
            // Hand posing
            LeftHandPoses.ForEach(leftHand =>
            {
                leftHand.MapThumb(handGesture[LEFT_THUMB_INDEX] == 255);
                leftHand.MapIndexFinger((float)handGesture[LEFT_INDEX_INDEX] / 255);
                leftHand.MapOtherFingers((float)handGesture[LEFT_OTHER_INDEX] / 255);
            });
            RightHandPoses.ForEach(rightHand =>
            {
                rightHand.MapThumb(handGesture[RIGHT_THUMB_INDEX] == 255);
                rightHand.MapIndexFinger((float)handGesture[RIGHT_INDEX_INDEX] / 255);
                rightHand.MapOtherFingers((float)handGesture[RIGHT_OTHER_INDEX] / 255);
            });
        }

        private void Awake()
        {
            if (Head == null || LeftHand == null || RightHand == null)
            {
                Debug.LogError($"NetworkedRigComponents not set on {gameObject.name} as required");
            }

            networkTransform = GetComponent<NetworkTransform>();
            _transform = transform;
        }

        public override void Spawned()
        {
            base.Spawned();
            name = Object.InputAuthority.ToString();

            if (!IsLocalNetworkRig)
            {
                return;
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (GetInput<RigInput>(out var input))
            {
                _transform.position = input.playAreaPosition;
                _transform.rotation = input.playAreaRotation;
                Head.Transform.position = input.headPosition;
                Head.Transform.rotation = input.headRotation;
                LeftHand.Transform.position = input.leftHandPosition;
                LeftHand.Transform.rotation = input.leftHandRotation;
                RightHand.Transform.position = input.rightHandPosition;
                RightHand.Transform.rotation = input.rightHandRotation;

                /*// Hand posing
                LeftHandPoses.ForEach(leftHand =>
                {
                    leftHand.MapThumb(input.leftHand.thumb);
                    leftHand.MapIndexFinger(input.leftHand.indexFinger);
                    leftHand.MapOtherFingers(input.leftHand.otherFingers);
                });
                RightHandPoses.ForEach(rightHand =>
                {
                    rightHand.MapThumb(input.rightHand.thumb);
                    rightHand.MapIndexFinger(input.rightHand.indexFinger);
                    rightHand.MapOtherFingers(input.rightHand.otherFingers);
                });

                if (!Object.HasStateAuthority)
                {
                    return;
                }

                handGesture.Set(LEFT_THUMB_INDEX, input.leftHand.thumb ? (byte)255 : (byte)0);
                handGesture.Set(LEFT_INDEX_INDEX, (byte)(255 * input.leftHand.indexFinger));
                handGesture.Set(LEFT_OTHER_INDEX, (byte)(255 * input.leftHand.otherFingers));
                
                handGesture.Set(RIGHT_THUMB_INDEX, input.rightHand.thumb ? (byte)255 : (byte)0);
                handGesture.Set(RIGHT_INDEX_INDEX, (byte)(255 * input.rightHand.indexFinger));
                handGesture.Set(RIGHT_OTHER_INDEX, (byte)(255 * input.rightHand.otherFingers));*/
            }
        }

        public override void Render()
        {
            base.Render();

            if (!IsLocalNetworkRig)
            {
                return;
            }

            // Extrapolate for local user:
            // we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hardware positions
            // To update the visual object, and not the actual networked position, we move the interpolation targets
            /*networkTransform.InterpolationTarget.position = XRRig.Instance.transform.position;
            networkTransform.InterpolationTarget.rotation = XRRig.Instance.transform.rotation;
            Head.InterpolationTarget.position = XRRig.Instance.head.position;
            Head.InterpolationTarget.rotation = XRRig.Instance.head.rotation;
            LeftHand.InterpolationTarget.position = XRRig.Instance.leftHand.position;
            LeftHand.InterpolationTarget.rotation = XRRig.Instance.leftHand.rotation;
            RightHand.InterpolationTarget.position = XRRig.Instance.rightHand.position;
            RightHand.InterpolationTarget.rotation = XRRig.Instance.rightHand.rotation;*/
        }

        private static XRRig FindXRRigInScene()
        {
            var localRig = FindObjectOfType<XRRig>();

            if (localRig == null)
            {
                Debug.LogError("Missing XRRig in the scene");
            }

            return localRig;
        }
    }
}
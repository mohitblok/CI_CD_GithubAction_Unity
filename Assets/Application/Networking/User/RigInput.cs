using Fusion;
using UnityEngine;

namespace Bloktopia.Fusion.Rig
{
    /// <summary>
    /// A basic networked input struct to be provided to the <see cref="XRNetworkedRig"/> by <see cref="XRRig"/>
    /// and allow individual <see cref="NetworkedRigComponent"/>s to be networked as expected.
    /// </summary>
    [System.Serializable]
    public struct RigInput : INetworkInput
    {
        public Vector3 playAreaPosition;
        public Quaternion playAreaRotation;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;

        // Hand posing
        public HandPoseInput leftHand;
        public HandPoseInput rightHand;
    }

    [System.Serializable]
    public struct HandPoseInput : INetworkInput
    {
        public bool thumb;
        public float indexFinger;
        public float otherFingers;
    }
}
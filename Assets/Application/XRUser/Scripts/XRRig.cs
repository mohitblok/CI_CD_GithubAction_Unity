using Bloktopia.Player.Hands;
using Fusion;
using UnityEngine;

namespace Bloktopia.Fusion.Rig
{
    public class XRRig : MonoSingleton<XRRig>
    {
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        [field: SerializeField] public HandPoseInputReader handPoseInput { get; private set; }
    }
}
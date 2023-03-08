using Bloktopia.Fusion.Rig;
using Fusion;
using UnityEngine;

public class UserNetworkInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RunnerCallbacks.Instance.onInputCallback += OnInput;
    }

    private void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if(XRRig.Instance == null)
            return;
        
        var rigInput = new RigInput()
        {
            playAreaPosition = XRRig.Instance.transform.position,
            playAreaRotation = XRRig.Instance.transform.rotation,
            headPosition = XRRig.Instance.head.position,
            headRotation = XRRig.Instance.head.rotation,
            leftHandPosition = XRRig.Instance.leftHand.position,
            leftHandRotation = XRRig.Instance.leftHand.rotation,
            rightHandPosition = XRRig.Instance.rightHand.position,
            rightHandRotation = XRRig.Instance.rightHand.rotation,

            //Hand Posing
            /*leftHand = new HandPoseInput()
            {
                thumb = XRRig.Instance.handPoseInput.leftHand.ThumbDown,
                indexFinger = XRRig.Instance.handPoseInput.leftHand.IndexFinger,
                otherFingers = XRRig.Instance.handPoseInput.leftHand.OtherFingers,
            },
            rightHand = new HandPoseInput()
            {
                thumb = XRRig.Instance.handPoseInput.rightHand.ThumbDown,
                indexFinger = XRRig.Instance.handPoseInput.rightHand.IndexFinger,
                otherFingers = XRRig.Instance.handPoseInput.rightHand.OtherFingers,
            }*/
        };

        input.Set(rigInput);
    }

    private void OnDestroy()
    {
        RunnerCallbacks.Instance.onInputCallback -= OnInput;
    }
}
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bloktopia
{
    public class RotationSetTweener : MonoBehaviour
    {
        public List<Transform> transforms = new();
        public List<Quaternion> startRot = new();
        public List<Quaternion> endRot = new();

        private float _lastLerp = 0;

        public void Interpolate(float lerp)
        {
            if(lerp == _lastLerp)
            {
                return;
            }

            for (int i = 0; i < transforms.Count; i++)
            {
                transforms[i].localRotation = Quaternion.Slerp(startRot[i], endRot[i], lerp);
            }

            _lastLerp = lerp;
        }
    }
}

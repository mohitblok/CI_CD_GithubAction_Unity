using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Bloktopia
{
    public class InputToRotationSetTweener : MonoBehaviour
    {
        public List<InputToTweenData> tweens = new();

        private bool cachedInputThumb;
        private float cachedInputIndexFinger;
        private float cachedInputOtherFingers;

        [System.Serializable]
        public class InputToTweenData
        {
            public string description;
            [Range(0f, 1f)]
            public float input = 0;

            public List<RotationSetTweener> tweeners = new();

            public void Tween(float lerp)
            {
                tweeners.ForEach(tweener => tweener.Interpolate(lerp));
            }

#if UNITY_EDITOR
            public void OnValidate()
            {
                Tween(input);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            tweens.ForEach(tween => tween.OnValidate());
        }
#endif

        private void Update() => ApplyCachedInput();

        private void ApplyCachedInput()
        {
            tweens[0].Tween(cachedInputThumb == true ? 1 : 0);
            tweens[1].Tween(Mathf.Clamp01(cachedInputIndexFinger));
            tweens[2].Tween(Mathf.Clamp01(cachedInputOtherFingers));
        }

        public void MapThumb(bool state) => cachedInputThumb = state;
        public void MapIndexFinger(float value) => cachedInputIndexFinger = value;
        public void MapOtherFingers(float value) => cachedInputOtherFingers = value;
    }
}

using System;
using UnityEngine;

namespace Utilities
{
    public class RenderChecker : MonoBehaviour
    {
        public Action WillRenderObject;
        public Action BecameVisible;
        public Action BecameInvisible;

        private void Start()
        {
            gameObject.ForceComponent<MeshRenderer>();
        }

        private void OnWillRenderObject()
        {
            WillRenderObject?.Invoke();
        }

        private void OnBecameVisible()
        {
            BecameVisible?.Invoke();
        }

        private void OnBecameInvisible()
        {
            BecameInvisible?.Invoke();
        }
    }
}
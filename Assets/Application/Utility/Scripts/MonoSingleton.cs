using UnityEngine;

namespace Bloktopia
{
    /// <summary>
    /// Allows you to create a singleton and takes care of itself to ensure only 1 is in place
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
            }
        }
        
        public static void CreateInstance()
        {
            Instance = (T) FindObjectOfType(typeof(T), true);

            if (Instance == null)
            {
                var root = GetSingletonRoot();
                var go = new GameObject(typeof(T).Name);
                go.transform.SetParent(root.transform, false);
                Instance = go.AddComponent<T>();
            }
        }
        
        public static void DestroyInstance()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
        
        protected static GameObject GetSingletonRoot()
        {
            const string RootName = "Singletons";
            var root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
            }

            return root;
        }
    }
}

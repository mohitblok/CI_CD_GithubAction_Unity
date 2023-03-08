using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Controls the updating of the UI content within the admin panel on the Interaction System tab.
    /// </summary>
    public class InteractionSystemAdminUIManager : MonoBehaviour
    {
        private static InteractionSystemAdminUIManager _instance;
        private static InteractionSystemAdminUIManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<InteractionSystemAdminUIManager>();
                }
                return _instance;
            }
        }

        [Header("Reference")]
        [SerializeField] private InputActionAssetTabManager _inputActionAssetTabManager;

        private List<InputActionUIDisplay> _inputActionUIDisplays = new();
        private WaitForEndOfFrame _waitForEndOfFrame;

        private void Start()
        {
            _inputActionAssetTabManager.Init(InteractionSystem.GetInputActionAssets());

            StartCoroutine(UpdateAllDisplays());
        }

        public static void RegisterInputActionUIDisplay(InputActionUIDisplay inputActionUIDisplay)
        {
            Instance._inputActionUIDisplays.Insert(0, inputActionUIDisplay);
        }

        public static void UnregisterAllInputActionUIDisplays()
        {
            Instance._inputActionUIDisplays.Clear();
        }

        public static void InstantDisplayUpdate()
        {
            for (int i = Instance._inputActionUIDisplays.Count; i-- > 0;)
            {
                Instance._inputActionUIDisplays[i].DisplayEnabledState();
            }
        }

        IEnumerator UpdateAllDisplays()
        {
            while (true)
            {
                for (int i = _inputActionUIDisplays.Count; i-- > 0;)
                {
                    // If tab changed mid refresh, stop
                    if (i > _inputActionUIDisplays.Count)
                    {
                        break;
                    }

                    _inputActionUIDisplays[i].DisplayEnabledState();

                    yield return _waitForEndOfFrame;
                }

                yield return _waitForEndOfFrame;
            }
        }
    }
}
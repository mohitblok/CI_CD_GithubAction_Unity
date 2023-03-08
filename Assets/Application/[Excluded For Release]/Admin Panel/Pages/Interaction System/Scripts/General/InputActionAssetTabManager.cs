using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Builds the tabs displaying the used InputActionAssets in the Admin Panel > InteractionSystem
    /// </summary>
    public class InputActionAssetTabManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private InputActionMapUITab _tabPrefab;
        [SerializeField] private Transform _tabContainer;
        [SerializeField] private InputActionMapUIListBuilder inputActionMapUIListBuilder;

        private List<InputActionMapUITab> _tabScripts = new();

        public void Init(List<InputActionAsset> inputActionAssets)
        {
            inputActionAssets.ForEach(inputActionAssets => BuildTab(inputActionAssets));

            SwitchTab(inputActionAssets[0]);
        }

        private void BuildTab(InputActionAsset inputActionAsset)
        {
            InputActionMapUITab tab = Instantiate(_tabPrefab, _tabContainer);
            tab.transform.SetSiblingIndex(tab.transform.parent.childCount - 2);
            tab.Init(inputActionAsset, this);

            _tabScripts.Add(tab);
        }

        public void SwitchTab(InputActionAsset inputActionAsset)
        {
            _tabScripts.ForEach(tab =>tab.SetState(tab.inputActionAsset == inputActionAsset));

            InteractionSystemAdminUIManager.UnregisterAllInputActionUIDisplays();

            inputActionMapUIListBuilder.Init(inputActionAsset);

            InteractionSystemAdminUIManager.InstantDisplayUpdate();
        }
    }
}
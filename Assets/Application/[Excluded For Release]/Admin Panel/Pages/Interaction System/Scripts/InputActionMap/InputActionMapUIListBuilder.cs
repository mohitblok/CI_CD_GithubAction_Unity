using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Builds the UI elements for a defined InputActionAsset in the Admin Panel > Interaction Systems
    /// </summary>
    public class InputActionMapUIListBuilder : MonoBehaviour
    {
        [SerializeField] private InputActionMapUIDisplay _listEntryPrefab;
        [SerializeField] private Transform _listContainer;
        private List<InputActionMapUIDisplay> _inputActionMapUIDisplays = new();

        public void Init(InputActionAsset inputActionAsset)
        {
            ClearExistingDisplays();

            inputActionAsset.actionMaps.ToList().ForEach(actionMap =>
            {
                CreateInputActionMapListEntry(actionMap);
            });
        }

        private void CreateInputActionMapListEntry(InputActionMap actionMap)
        {
            InputActionMapUIDisplay listEntry = Instantiate(_listEntryPrefab, _listContainer);
            listEntry.Init(actionMap);
            
            _inputActionMapUIDisplays.Add(listEntry);
        }

        private void ClearExistingDisplays()
        {
            _inputActionMapUIDisplays.ForEach(display =>
            {
                Destroy(display.gameObject);
            });

            _inputActionMapUIDisplays.Clear();
        }
    }
}
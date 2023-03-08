using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Builds a list to display the InputActions inside the Admin Panel InteractionSystem
    /// </summary>
    public class InputActionUIListBuilder : MonoBehaviour
    {
        [SerializeField] private InputActionUIDisplay _listEntryPrefab;
        [SerializeField] private Transform _listContainer;

        public void Init(InputActionMap inputActionMap)
        {
            inputActionMap.actions.ToList().ForEach(action =>
            {
                CreateInputActionListEntry(action);
            });
        }

        private void CreateInputActionListEntry(InputAction action)
        {
            InputActionUIDisplay listEntry = Instantiate(_listEntryPrefab, _listContainer);
            listEntry.Init(action);
        }
    }
}
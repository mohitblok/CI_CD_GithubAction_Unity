using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Updates the name in the UI display and passes a call to build a list
    /// </summary>
    public class InputActionMapUIDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _nameReadout;
        [SerializeField] private InputActionUIListBuilder _inputActionUIListBuilder;

        public void Init(InputActionMap actionMap)
        {
            _nameReadout.text = actionMap.name;
            _inputActionUIListBuilder.Init(actionMap);
        }
    }
}
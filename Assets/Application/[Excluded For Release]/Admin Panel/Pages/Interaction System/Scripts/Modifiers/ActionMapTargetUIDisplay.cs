using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AdminPanel.InteractionSystemDebugging
{
    public class ActionMapTargetUIDisplay : MonoBehaviour
    {
        [SerializeField] private Image _colourChangingPanel;
        [SerializeField] private TextMeshProUGUI _nameReadout;
        private InputActionMap _inputActionMap;

        public void Init(InputActionMap inputActionMap, bool enabling)
        {
            _inputActionMap = inputActionMap;

            _nameReadout.text = inputActionMap.name;

            SetColour(enabling);
        }

        private void SetColour(bool enabling)
        {
            _colourChangingPanel.color =
                enabling ? InteractionSystemUI.ENABLED_COLOUR : InteractionSystemUI.DISABLED_COLOUR; ;
        }
    }
}

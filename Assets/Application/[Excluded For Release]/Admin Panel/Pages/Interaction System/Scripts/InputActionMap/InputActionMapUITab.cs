using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AdminPanel.InteractionSystemDebugging
{
    public class InputActionMapUITab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _nameReadout;
        [SerializeField] private Image _colourChangingPanel;

        public InputActionAsset inputActionAsset { get; private set; }
        private InputActionAssetTabManager _tabManager;

        private readonly Color32 ACTIVE_COLOUR = new Color32(255, 156, 255, 255);
        private readonly Color32 INACTIVE_COLOUR = Color.white;

        public void Init(InputActionAsset inputActionAsset, InputActionAssetTabManager tabManager)
        {
            this.inputActionAsset = inputActionAsset;
            _tabManager = tabManager;

            UpdateTabNameReadout();
        }

        private void UpdateTabNameReadout()
        {
            _nameReadout.text = inputActionAsset.name;
        }

        public void SetState(bool state)
        {
            _colourChangingPanel.color = state ? ACTIVE_COLOUR : INACTIVE_COLOUR;
        }

        public void ActivateTab()
        {
            _tabManager.SwitchTab(inputActionAsset);
        }
    }
}
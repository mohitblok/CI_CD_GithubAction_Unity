using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debugging.Admin
{
    /// <summary>
    /// Tab used by the Admin Panel. Clickable
    /// </summary>
    public class Tab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _tabText;
        [SerializeField] private Image _tabGraphic;

        private string _tabName;
        private AdminPanel _adminPanelManager;
        private Color32 _activeTabColour;
        private Color32 _inactiveTabColour;
        private Color32 _noContentYetTabColour;

        public void SetTabData(
            string tabName,
            Color32 activeTabColour,
            Color32 inactiveTabColour,
            Color32 noContentYetTabColour,
            AdminPanel adminPanelManager)
        {
            _tabName = tabName;
            _activeTabColour = activeTabColour;
            _inactiveTabColour = inactiveTabColour;
            _adminPanelManager = adminPanelManager;
            _noContentYetTabColour = noContentYetTabColour;

            UpdataTabDisplay();
        }

        private void UpdataTabDisplay()
        {
            if (!_tabText)
            {
                return;
            }

            _tabText.text = _tabName;
        }

        public void SetActive(TabState tabState)
        {
            switch (tabState)
            {
                case TabState.Active: _tabGraphic.color = _activeTabColour; break;
                case TabState.Inactive: _tabGraphic.color = _inactiveTabColour; break;
                case TabState.NoContent: _tabGraphic.color = _noContentYetTabColour; break;
            }
        }

        public void OnClick() => _adminPanelManager.SetActiveTab(_tabName);
    }
}
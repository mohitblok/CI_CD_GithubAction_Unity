using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Debugging.Admin
{
    /// <summary>
    /// Creates the admin page (Tabs and Content) and manages the switching between them.
    /// </summary>
    public class AdminPanel : MonoBehaviour
    {
        private static AdminPanel _instance;

        [Header("References")]
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private Transform _tabsContainer;
        private Transform _activeContent;

        [Header("Prefabs")]
        [SerializeField] private Transform _tabPrefab;

        [Header("Settings")]
        [SerializeField] private Color32 _activeTabColour = new Color32(255, 154, 0, 255);
        [SerializeField] private Color32 _inactiveTabColour = new Color32(255, 255, 255, 255);
        [SerializeField] private Color32 _noContentYetTabColour = new Color32(180, 180, 180, 255);

        [Header("Data")]
        [SerializeField] private List<AdminPanelTabData> _content = new();

        /// <summary>
        /// True if the Admin Panel is currently open. False if not.
        /// </summary>
        public static bool IsOpen
        {
            get => _instance != null && _instance.gameObject != null;
        }

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            CreateAdminPage();
        }

        private void CreateAdminPage()
        {
            // Create Tabs
            for (int i = 0; i < _content.Count; i++)
            {
                Transform newTab = Instantiate(_tabPrefab, _tabsContainer);

                _content[i].tabScript = newTab.GetComponent<Tab>();
                _content[i].tabScript.SetTabData
                (
                    _content[i].tabName,
                    _activeTabColour,
                    _inactiveTabColour,
                    _noContentYetTabColour,
                    this
                );
            }

            // Update the tab bar
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tabsContainer.GetComponent<RectTransform>());

            // Set the active page to the first tab in the 'content' list
            SetActiveTab(_content[0].tabName);
        }

        /// <summary>
        /// Called by the UI Buttons on tabs in the admin panel.
        /// <para>If a page with the specified name exists, it opens it.</para>
        /// </summary>
        /// <param name="tabName">Name of tab to open</param>
        public void SetActiveTab(string tabName)
        {
            // Get reference to active tab content
            AdminPanelTabData activeTab = GetActiveTab();

            // Dont reload the current tab
            if (activeTab != null && tabName == activeTab.tabName)
            {
                return;
            }

            // Destroy the old tab content
            if (_activeContent)
            {
                Destroy(_activeContent.gameObject);
            }

            // Set the new tab as active
            for (int i = 0; i < _content.Count; i++)
            {
                if (_content[i].tabName == tabName)
                {
                    _content[i].isActiveTab = true;
                    _content[i].tabScript.SetActive(TabState.Active);

                    if (_content[i].contentPrefab)
                    {
                        _activeContent = Instantiate(_content[i].contentPrefab, _contentContainer);
                    }
                }
                else
                {
                    _content[i].isActiveTab = false;
                    _content[i].tabScript.SetActive(_content[i].contentPrefab ? TabState.Inactive : TabState.NoContent);
                }
            }
        }

        private AdminPanelTabData GetActiveTab() => _content.FirstOrDefault(content => content.isActiveTab);

        public void Close()
        {
            Destroy(gameObject);
        }
        public static void ClosePanel()
        {
            if (IsOpen)
            {
                Destroy(_instance.gameObject);
            }
        }
    }

    public enum TabState
    {
        Active,
        Inactive,
        NoContent
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace Debugging.Admin
{
    /// <summary>
    /// Opens the AdminPanel if keybind is pressed
    /// </summary>
    public class AdminPanelController : MonoSingleton<AdminPanelController>
    {
        private GameObject _adminPanelPrefab;
        private readonly string PREFAB_RESOURCE_PATH = "Admin Panel";

        private void Start()
        {
#if !UNITY_EDITOR
            if(!Debug.isDebugBuild)
            {
                Destroy(gameObject);
            }
#endif
            GetAdminPanelPrefab();
            Subscribe();
        }

        private void ToggleAdminPanel(InputAction.CallbackContext obj)
        {
            if (AdminPanel.IsOpen)
            {
                AdminPanel.ClosePanel();
            }
            else
            {
                Instantiate(_adminPanelPrefab, transform);
            }
        }

        private void GetAdminPanelPrefab()
        {
            _adminPanelPrefab = Resources.Load(PREFAB_RESOURCE_PATH) as GameObject;
        }

        private void Subscribe()
        {
            InteractionSystem.DebugInput.Debugging.ToggleAdminPanel.performed += ToggleAdminPanel;
        }
        private void Unsubscribe()
        {
            InteractionSystem.DebugInput.Debugging.ToggleAdminPanel.performed -= ToggleAdminPanel;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
    }
}
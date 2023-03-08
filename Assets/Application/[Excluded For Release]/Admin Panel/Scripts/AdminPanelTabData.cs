using UnityEngine;

namespace Debugging.Admin
{
    [System.Serializable]
    public class AdminPanelTabData
    {
        public string tabName;
        public Transform contentPrefab;
        [HideInInspector] public bool isActiveTab;
        [HideInInspector] public Tab tabScript;
    }
}
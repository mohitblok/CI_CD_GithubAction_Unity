using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AdminPanel.InteractionSystemDebugging
{
    public class ModifierUIDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _modifierNameReadout;
        [SerializeField] private TextMeshProUGUI _ownerButtonReadout;
        [SerializeField] private ActionMapTargetUIDisplay _actionMapTargetPrefab;
        [SerializeField] private Transform _enabledActionMapListContainer;
        [SerializeField] private Transform _disabledActionMapListContainer;
        private ActionMapModifier _modifier;

        public void Init(ActionMapModifier modifier)
        {
            _modifier = modifier;

            _modifierNameReadout.text = modifier.name;
            _ownerButtonReadout.text = modifier.owner.name;

            CreateActionMapTargets(modifier.MapsToEnable, _enabledActionMapListContainer, true);
            CreateActionMapTargets(modifier.MapsToDisable, _disabledActionMapListContainer, false);
        }

        private void CreateActionMapTargets(List<InputActionMapReference> inputActionMaps, Transform container, bool enabling)
        {
            if (inputActionMaps == null)
            {
                return;
            }

            foreach (var inputActionMap in inputActionMaps)
            {
                if (inputActionMap == null)
                {
                    continue;
                }

                ActionMapTargetUIDisplay entry = Instantiate(_actionMapTargetPrefab, container);
                entry.Init(inputActionMap.Map, enabling);
            }
        }

#if UNITY_EDITOR
        // Called by button component
        public void SelectOwner_InEditor()
        {
            Selection.activeObject = _modifier.owner;
        }
#endif
    }
}
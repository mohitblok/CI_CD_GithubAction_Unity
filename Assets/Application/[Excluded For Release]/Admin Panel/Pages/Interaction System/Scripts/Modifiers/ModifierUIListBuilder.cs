using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdminPanel.InteractionSystemDebugging
{
    public class ModifierUIListBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ModifierUIDisplay _modifierPrefab;
        [SerializeField] private Transform _container;

        private List<ModifierUIDisplay> _modifierDisplays = new();
        public List<ActionMapModifier> _modifiers = new();
        private WaitForEndOfFrame _waitForEndOfFrame;

        private void Start()
        {
            InvokeRepeating(nameof(CheckList), 0, 0.1f);
        }

        private void DrawModifiers()
        {
            RemoveExistingDisplays();

            foreach (var modifier in _modifiers)
            {
                if (modifier == null)
                {
                    continue;
                }

                ModifierUIDisplay entry = Instantiate(_modifierPrefab, _container);
                entry.Init(modifier);

                _modifierDisplays.Add(entry);
            }
        }

        private void RemoveExistingDisplays()
        {
            for (int i = _modifierDisplays.Count; i-- > 0;)
            {
                Destroy(_modifierDisplays[i].gameObject);
                _modifierDisplays.RemoveAt(i);
            }
        }

        private void CheckList()
        {
            if (!ListsMatch(_modifiers, InteractionSystem.modifiers))
            {
                _modifiers = InteractionSystem.modifiers.ToList();
                DrawModifiers();
            }
        }

        private bool ListsMatch<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] == null && b[i] == null)
                {
                    continue;
                }

                if(a[i] == null || b[i] == null)
                {
                    return false;
                }

                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
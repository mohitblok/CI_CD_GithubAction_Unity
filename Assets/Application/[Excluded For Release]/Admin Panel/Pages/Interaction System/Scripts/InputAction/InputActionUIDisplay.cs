using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AdminPanel.InteractionSystemDebugging
{
    /// <summary>
    /// Hooks into a UI view to display the state of an InputAction.
    /// <para>Call <see cref="Init"/> to begin.</para>
    /// </summary>
    public class InputActionUIDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _nameReadout;
        [SerializeField] private TextMeshProUGUI _valueReadout;
        [SerializeField] private Image _colourChangingPanel;
        [SerializeField] private AlphaPulse _startedPulse;
        [SerializeField] private AlphaPulse _performedPulse;
        [SerializeField] private AlphaPulse _cancelledPulse;
        private InputAction _inputAction;

        public void Init(InputAction inputAction)
        {
            _inputAction = inputAction;

            DisplayName();
            SubscribeToEvents();

            InteractionSystemAdminUIManager.RegisterInputActionUIDisplay(this);
        }

        public void DisplayEnabledState()
        {
            if (_colourChangingPanel)
            {
                _colourChangingPanel.color =
                    _inputAction.enabled ? InteractionSystemUI.ENABLED_COLOUR : InteractionSystemUI.DISABLED_COLOUR;
            }
        }

        private void DisplayName()
        {
            _nameReadout.text = _inputAction.name;
        }
        private void DisplayValue(InputAction.CallbackContext context)
        {
            if (context.action.type == InputActionType.Button)
            {
                _valueReadout.text = context.ReadValueAsButton() == true ? "true" : "false";
                return;
            }
            if (context.valueType == typeof(float))
            {
                _valueReadout.text = context.ReadValue<float>().ToString("N2");
                return;
            }

            if (context.valueType == typeof(Vector2))
            {
                Vector2 val = context.ReadValue<Vector2>();
                _valueReadout.text =
                    $"{val.x.ToString("N2")}, " +
                    $"{val.y.ToString("N2")}";
                return;
            }

            if (context.valueType == typeof(Vector3))
            {
                Vector3 val = context.ReadValue<Vector3>();
                _valueReadout.text = 
                    $"{val.x.ToString("N2")}, " +
                    $"{val.y.ToString("N2")}, " +
                    $"{val.z.ToString("N2")}";
                return;
            }

            if (context.valueType == typeof(Quaternion))
            {
                Quaternion val = context.ReadValue<Quaternion>();
                _valueReadout.text = 
                    $"{val.x.ToString("N2")}, " +
                    $"{val.y.ToString("N2")}, " +
                    $"{val.z.ToString("N2")}, " +
                    $"{val.w.ToString("N2")}";
                return;
            }
        }

        private void SubscribeToEvents()
        {
            UnsubscribeFromEvents();

            if (_inputAction == null)
            {
                return;
            }

            _inputAction.started += Started;
            _inputAction.performed += Performed;
            _inputAction.canceled += Cancelled;
        }
        private void UnsubscribeFromEvents()
        {
            if (_inputAction == null)
            {
                return;
            }

            _inputAction.started -= Started;
            _inputAction.performed -= Performed;
            _inputAction.canceled -= Cancelled;
        }

        private void Started(InputAction.CallbackContext context)
        {
            _startedPulse.Pulse();
            DisplayValue(context);
        }
        private void Performed(InputAction.CallbackContext context)
        {
            _performedPulse.Pulse();
            DisplayValue(context);
        }
        private void Cancelled(InputAction.CallbackContext context)
        {
            _cancelledPulse.Pulse();
            DisplayValue(context);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}
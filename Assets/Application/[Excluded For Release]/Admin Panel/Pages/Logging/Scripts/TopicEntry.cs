using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bloktopia.AdminPanel.Logging
{
    public class TopicEntry : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _nameReadout;
        [SerializeField] private Image _viewStateDisplay;

        [Header("Settings")]
        [SerializeField] private Sprite _viewStateOn;
        [SerializeField] private Sprite _viewStateOff;
        private bool _viewState = true;

        public void ToggleViewState()
        {
            SetViewState(!_viewState);
        }

        public void SetViewState(bool state)
        {
            _viewState = state;
            _viewStateDisplay.sprite = state ? _viewStateOn : _viewStateOff;
        }
    }
}
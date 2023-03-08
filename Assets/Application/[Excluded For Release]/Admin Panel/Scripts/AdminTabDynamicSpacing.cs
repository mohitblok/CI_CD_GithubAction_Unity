using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Debugging.Admin
{
    /// <summary>
    /// This class sets the size of the divider in the Admin screens Tabs. Vehicle related tabs should be on the left, Application tabs are on the right.
    /// </summary>
    public class AdminTabDynamicSpacing : MonoBehaviour
    {
        [Header("References")]
        public LayoutElement divider;
        public HorizontalLayoutGroup horizontalLayoutGroup;
        public RectTransform container;

        [Header("Settings")]
        public float minimumDividerWidth = 10;
        private List<RectTransform> tabRects = new List<RectTransform>();
        private float totalTabWidths;
        private float result;

        private void OnEnable()
        {
            UpdateSpacing();
        }

        void UpdateSpacing()
        {
            // Get references to tab RectTransforms
            if (tabRects.Count == 0)
            {
                totalTabWidths = 0;

                foreach (Tab tab in GetComponentsInChildren<Tab>())
                {
                    RectTransform rectTransform = tab.GetComponent<RectTransform>();
                    tabRects.Add(rectTransform);
                    totalTabWidths += rectTransform.sizeDelta.x;
                }
            }

            // Get width of container
            float containerWidth = GetContainerWidth();

            // Calculate size of the divider needed to make the tabs anchor to left and right of the screen
            result = containerWidth - (totalTabWidths + (tabRects.Count * horizontalLayoutGroup.spacing) + horizontalLayoutGroup.padding.left + horizontalLayoutGroup.padding.right);

            // Apply the width with a minimum value
            divider.minWidth = Mathf.Max(minimumDividerWidth, result);
        }

        private float GetContainerWidth()
        {
            return container.sizeDelta.x == 0 ? container.rect.width : container.sizeDelta.x;
        }
    }
}
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.UI;

namespace TraderSearch
{
    public class TraderSearchController : SearchFieldController
    {
        public static TraderSearchController Current { get; private set; }

        private static FilterPanel _activeTraderFilterPanel;

        private TradingGridView _traderGridView;
        private Transform _updateButton;
        private Transform _loyaltyPanel;

        public static bool IsSearchFilterActive(FilterPanel panel)
        {
            TraderSearchController controller = Current;
            return controller != null && controller.Query.Length > 0 && panel != null && ReferenceEquals(panel, _activeTraderFilterPanel);
        }

        public static bool Matches(Item item)
        {
            TraderSearchController controller = Current;
            return controller != null && controller.MatchesQuery(item);
        }

        public void OnTraderScreenShown(TradingGridView traderGridView, DefaultUIButton updateButton, FilterTab allItemsTab)
        {
            Current = this;
            _traderGridView = traderGridView;
            _activeTraderFilterPanel = traderGridView != null ? traderGridView.FilterPanel : null;
            _updateButton = updateButton != null ? updateButton.transform : null;
            _loyaltyPanel = allItemsTab != null ? allItemsTab.transform.parent : null;

            EnsureSearchField(_updateButton != null ? _updateButton.parent : null, "TraderSearchField");
            ResetQueryAfterShow();
        }

        public void OnScreenFullClose()
        {
            ClearQuerySilently();
            _activeTraderFilterPanel = null;
        }

        protected override void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
            if (ReferenceEquals(_activeTraderFilterPanel, _traderGridView != null ? _traderGridView.FilterPanel : null))
            {
                _activeTraderFilterPanel = null;
            }
            base.OnDestroy();
        }

        protected override void RefreshFilter()
        {
            if (_traderGridView == null || !_traderGridView.gameObject.activeSelf)
            {
                return;
            }
            FilterPanel panel = _activeTraderFilterPanel;
            if (panel != null)
            {
                panel.FilterChanged();
            }
        }

        protected override void ApplyPlacement()
        {
            if (SearchField == null || _updateButton == null)
            {
                return;
            }

            RectTransform field = (RectTransform)SearchField.transform;
            Transform parent = _updateButton.parent;
            if (field.parent != parent)
            {
                field.SetParent(parent, false);
            }

            bool sharedParent = _loyaltyPanel != null && _loyaltyPanel.parent == parent;
            int siblingIndex = sharedParent
                ? Mathf.Min(_updateButton.GetSiblingIndex(), _loyaltyPanel.GetSiblingIndex()) + 1
                : _updateButton.GetSiblingIndex() + 1;
            field.SetSiblingIndex(siblingIndex);

            if (parent.GetComponent<HorizontalLayoutGroup>() != null || parent.GetComponent<VerticalLayoutGroup>() != null)
            {
                RectTransform buttonRect = _updateButton as RectTransform;
                float height = buttonRect != null && buttonRect.rect.height > 4f ? buttonRect.rect.height : 35f;
                LayoutElement layoutElement = field.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = field.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.ignoreLayout = false;
                layoutElement.minWidth = 158f;
                layoutElement.preferredWidth = 296f;
                layoutElement.preferredHeight = height;
                layoutElement.flexibleWidth = 0f;
            }
            else
            {
                PositionBetweenNeighbors(field, (RectTransform)parent);
            }
        }

        private void PositionBetweenNeighbors(RectTransform field, RectTransform parent)
        {
            RectTransform buttonRect = _updateButton as RectTransform;
            if (buttonRect == null)
            {
                return;
            }

            Rect a = LocalRect(buttonRect, parent);
            field.anchorMin = new Vector2(0.5f, 0.5f);
            field.anchorMax = new Vector2(0.5f, 0.5f);
            field.pivot = new Vector2(0.5f, 0.5f);
            float height = Mathf.Clamp(a.height > 4f ? a.height : 35f, 30f, 45f);

            RectTransform loyaltyRect = _loyaltyPanel as RectTransform;
            if (loyaltyRect == null)
            {
                field.sizeDelta = new Vector2(296f, height);
                Vector2 pivotPos = new Vector2(a.xMax + 10f + 158f, a.center.y);
                field.anchoredPosition = pivotPos - parent.rect.center;
                return;
            }

            Rect b = LocalRect(loyaltyRect, parent);
            float gapLeft;
            float gapRight;
            if (a.center.x <= b.center.x)
            {
                gapLeft = a.xMax;
                gapRight = b.xMin;
            }
            else
            {
                gapLeft = b.xMax;
                gapRight = a.xMin;
            }

            const float padding = 8f;
            float width = Mathf.Clamp(gapRight - gapLeft - 2f * padding, 135f, 365f);
            Vector2 center = new Vector2((gapLeft + gapRight) * 0.5f, a.center.y);
            field.sizeDelta = new Vector2(width, height);
            field.anchoredPosition = center - parent.rect.center;
        }
    }
}

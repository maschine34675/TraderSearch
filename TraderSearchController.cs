using System;
using System.Collections;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraderSearch
{
    public class TraderSearchController : MonoBehaviour
    {
        private static readonly FieldInfo StashSearchWindowField = AccessTools.Field(typeof(ItemUiContext), "_stashSearchWindow");
        private static readonly FieldInfo SearchFieldField = AccessTools.Field(typeof(StashSearchWindow), "_searchField");

        public static TraderSearchController Current { get; private set; }

        private static string _query = string.Empty;
        private static FilterPanel _activeTraderFilterPanel;

        private TMP_InputField _searchField;
        private TradingGridView _traderGridView;
        private Transform _updateButton;
        private Transform _loyaltyPanel;
        private int _lastFocusedFrame = -100;
        private bool _templateMissingLogged;

        public bool IsInputFocused => _searchField != null && _searchField.isFocused;
        public bool WasFocusedRecently => Time.frameCount - _lastFocusedFrame <= 1;

        public static bool IsSearchFilterActive(FilterPanel panel)
        {
            return _query.Length > 0 && panel != null && ReferenceEquals(panel, _activeTraderFilterPanel);
        }

        public static bool Matches(Item item)
        {
            if (item == null)
            {
                return false;
            }
            string name = item.LocalizedName();
            if (!string.IsNullOrEmpty(name) && name.IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            string shortName = item.LocalizedShortName();
            return !string.IsNullOrEmpty(shortName) && shortName.IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void OnTraderScreenShown(TradingGridView traderGridView, DefaultUIButton updateButton, FilterTab allItemsTab)
        {
            Current = this;
            _traderGridView = traderGridView;
            _activeTraderFilterPanel = traderGridView != null ? traderGridView.FilterPanel : null;
            _updateButton = updateButton != null ? updateButton.transform : null;
            _loyaltyPanel = allItemsTab != null ? allItemsTab.transform.parent : null;

            EnsureSearchField();
            bool hadQuery = _query.Length > 0;
            _query = string.Empty;
            if (_searchField != null)
            {
                _searchField.SetTextWithoutNotify(string.Empty);
            }
            if (hadQuery)
            {
                RefreshGrid();
            }
        }

        public void OnScreenFullClose()
        {
            _query = string.Empty;
            _activeTraderFilterPanel = null;
            if (_searchField != null)
            {
                _searchField.SetTextWithoutNotify(string.Empty);
            }
        }

        public void ClearSearchAndDefocus()
        {
            if (_searchField == null)
            {
                return;
            }
            _searchField.SetTextWithoutNotify(string.Empty);
            if (_query.Length > 0)
            {
                _query = string.Empty;
                RefreshGrid();
            }
            _searchField.DeactivateInputField();
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == _searchField.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void Update()
        {
            if (_searchField != null && _searchField.isFocused)
            {
                _lastFocusedFrame = Time.frameCount;
            }
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
            if (ReferenceEquals(_activeTraderFilterPanel, _traderGridView != null ? _traderGridView.FilterPanel : null))
            {
                _activeTraderFilterPanel = null;
            }
            _query = string.Empty;
        }

        private void OnQueryChanged(string text)
        {
            string newQuery = (text ?? string.Empty).Trim();
            if (newQuery == _query)
            {
                return;
            }
            _query = newQuery;
            RefreshGrid();
        }

        private void RefreshGrid()
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

        private void EnsureSearchField()
        {
            if (_searchField != null)
            {
                ApplyPlacement();
                StartCoroutine(ReapplyPlacementAtEndOfFrame());
                return;
            }
            if (_updateButton == null)
            {
                return;
            }

            TMP_InputField template = FindSearchFieldTemplate();
            if (template == null)
            {
                if (!_templateMissingLogged)
                {
                    _templateMissingLogged = true;
                    Plugin.Log.LogWarning("TraderSearch: could not resolve the stash search input field to clone; no search field will be shown.");
                }
                return;
            }

            GameObject go = Instantiate(template.gameObject, _updateButton.parent, false);
            go.name = "TraderSearchField";
            go.SetActive(false);

            _searchField = go.GetComponent<TMP_InputField>();
            if (_searchField == null)
            {
                Plugin.Log.LogWarning("TraderSearch: cloned search field has no TMP_InputField component; aborting.");
                Destroy(go);
                return;
            }
            _searchField.onValueChanged = new TMP_InputField.OnChangeEvent();
            _searchField.onEndEdit = new TMP_InputField.SubmitEvent();
            _searchField.onSubmit = new TMP_InputField.SubmitEvent();
            _searchField.onSelect = new TMP_InputField.SelectionEvent();
            _searchField.onDeselect = new TMP_InputField.SelectionEvent();
            _searchField.onValueChanged.AddListener(OnQueryChanged);
            _searchField.SetTextWithoutNotify(string.Empty);
            _searchField.interactable = true;

            foreach (Button button in go.GetComponentsInChildren<Button>(true))
            {
                button.onClick = new Button.ButtonClickedEvent();
            }

            if (go.GetComponent<Graphic>() == null)
            {
                Image background = go.AddComponent<Image>();
                background.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
                background.raycastTarget = true;
            }

            ApplyPlacement();
            go.SetActive(true);
            StartCoroutine(ReapplyPlacementAtEndOfFrame());
        }

        private static TMP_InputField FindSearchFieldTemplate()
        {
            ItemUiContext context = ItemUiContext.Instance;
            if (context == null)
            {
                return null;
            }
            StashSearchWindow window = StashSearchWindowField?.GetValue(context) as StashSearchWindow;
            if (window == null)
            {
                return null;
            }
            return SearchFieldField?.GetValue(window) as TMP_InputField;
        }

        private IEnumerator ReapplyPlacementAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            if (_searchField != null)
            {
                ApplyPlacement();
            }
        }

        private void ApplyPlacement()
        {
            if (_searchField == null || _updateButton == null)
            {
                return;
            }

            RectTransform field = (RectTransform)_searchField.transform;
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

        private static Rect LocalRect(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 min = relativeTo.InverseTransformPoint(corners[0]);
            Vector3 max = relativeTo.InverseTransformPoint(corners[2]);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}

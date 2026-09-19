using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Ragfair;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TraderSearch
{
    public class AddOfferSearchController : SearchFieldController
    {
        private const float MaxWidth = 296f;
        private const float MinWidth = 120f;
        private const float Padding = 8f;
        private const float RebuildDelay = 0.15f;
        private const int ResultRows = 10;

        private static readonly FieldInfo FilterRulesField = AccessTools.Field(typeof(FilterPanel), "_filterRules");

        public static AddOfferSearchController Current { get; private set; }
        public static bool SuppressDeselect { get; private set; }

        private AddOfferWindow _window;
        private GridView _gridView;
        private RectTransform _autoSelectSimilar;
        private Grid _stashGrid;
        private NewOfferItemContext _itemContext;
        private InventoryController _inventoryController;
        private ItemUiContext _itemUiContext;
        private InventoryController _subscribedController;

        private Stash _resultsStash;
        private ReferenceGrid _resultsGrid;
        private bool _showingResults;
        private bool _rebuildPending;
        private float _rebuildAt;
        private float _savedStashScroll = 1f;
        private FilterPanel.FilterRule _wantedCategory;
        private bool _wantedCategoryShown = true;

        public void OnAddOfferWindowShown(AddOfferWindow window, NewOfferItemContext itemContext, InventoryController inventoryController, ItemUiContext itemUiContext)
        {
            Current = this;
            _window = window;
            _gridView = window._gridView;
            _autoSelectSimilar = window._autoSelectSimilar != null ? window._autoSelectSimilar.transform as RectTransform : null;
            _itemContext = itemContext;
            _inventoryController = inventoryController;
            _itemUiContext = itemUiContext;
            _stashGrid = _gridView != null ? _gridView.Grid : null;
            _showingResults = false;
            _rebuildPending = false;
            _wantedCategory = null;
            _wantedCategoryShown = true;
            ReleaseResultsGrid();
            Subscribe(inventoryController);

            EnsureSearchField(_autoSelectSimilar != null ? _autoSelectSimilar.parent : null, "TraderSearchOfferField");
            ResetQueryAfterShow();
        }

        protected override void Update()
        {
            base.Update();
            if (_rebuildPending && Time.unscaledTime >= _rebuildAt)
            {
                _rebuildPending = false;
                ApplyQuery();
            }
        }

        protected override void OnDestroy()
        {
            Unsubscribe();
            ReleaseResultsGrid();
            if (Current == this)
            {
                Current = null;
            }
            base.OnDestroy();
        }

        protected override void RefreshFilter()
        {
            ScheduleRebuild();
        }

        private void ScheduleRebuild()
        {
            _rebuildPending = true;
            _rebuildAt = Time.unscaledTime + RebuildDelay;
        }

        private void ApplyQuery()
        {
            if (_gridView == null || _stashGrid == null || _itemContext == null || _window == null
                || !_window.gameObject.activeInHierarchy || !_gridView.gameObject.activeInHierarchy)
            {
                return;
            }
            try
            {
                if (Query.Length > 0)
                {
                    ShowResults();
                }
                else if (_showingResults)
                {
                    ShowStash();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("TraderSearch: failed to update the Add Offer search results: " + ex);
            }
        }

        private void ShowResults()
        {
            List<Item> matches = _stashGrid.ContainedItems
                .Where(pair => MatchesQuery(pair.Key))
                .OrderBy(pair => pair.Value.y)
                .ThenBy(pair => pair.Value.x)
                .Select(pair => pair.Key)
                .ToList();

            if (!_showingResults)
            {
                _savedStashScroll = CurrentScroll();
            }

            RememberCategory();
            CloseGridView();
            ReleaseResultsGrid();

            EnsureResultsStash();
            ReferenceGrid grid = new ReferenceGrid("hideout", _stashGrid.GridWidth, ResultRows, true, false, Array.Empty<ItemFilter>(), _resultsStash);
            _resultsStash.Grids[0] = grid;
            foreach (Item item in matches)
            {
                grid.AddAnywhere(item, EErrorHandlingType.Ignore);
            }
            _resultsGrid = grid;

            _gridView.Show(grid, _itemContext, _inventoryController, _itemUiContext);
            _showingResults = true;
            RestoreCategory();
            StartCoroutine(ScrollAtEndOfFrame(1f));
        }

        private void ShowStash()
        {
            RememberCategory();
            CloseGridView();
            ReleaseResultsGrid();
            _gridView.Show(_stashGrid, _itemContext, _inventoryController, _itemUiContext);
            _showingResults = false;
            RestoreCategory();
            StartCoroutine(ScrollAtEndOfFrame(_savedStashScroll));
        }

        private void CloseGridView()
        {
            SuppressDeselect = true;
            try
            {
                _gridView.Close();
            }
            finally
            {
                SuppressDeselect = false;
            }
        }

        private void EnsureResultsStash()
        {
            if (_resultsStash != null)
            {
                return;
            }
            _resultsStash = Singleton<ItemFactory>.Instance.CreateFakeStash();
            new ItemController(_resultsStash, _inventoryController.ID, _inventoryController.Name, true, EOwnerType.Mail);
        }

        private void ReleaseResultsGrid()
        {
            if (_resultsGrid != null)
            {
                _resultsGrid.RemoveAll();
                _resultsGrid = null;
            }
        }
        private void RememberCategory()
        {
            FilterPanel.FilterRule current = _gridView.FilterPanel != null ? _gridView.FilterPanel.CurrentFilter : null;
            if (current != null && current.Type == FilterPanel.EFilterItemType.All)
            {
                current = null;
            }
            if (_wantedCategoryShown || current != null)
            {
                _wantedCategory = current;
            }
        }

        private void RestoreCategory()
        {
            _wantedCategoryShown = _wantedCategory == null;
            FilterPanel panel = _gridView.FilterPanel;
            if (_wantedCategory == null || panel == null || panel._filterTabs == null)
            {
                return;
            }
            int index = FilterRulesField?.GetValue(null) is FilterPanel.FilterRule[] rules ? Array.IndexOf(rules, _wantedCategory) : -1;
            if (index < 0 || index >= panel._filterTabs.Length)
            {
                return;
            }
            FilterTab tab = panel._filterTabs[index];
            if (tab != null && tab.CanHandlePointerClick)
            {
                tab.HandlePointerClick(false);
                _wantedCategoryShown = true;
            }
        }

        private float CurrentScroll()
        {
            ScrollRect scrollRect = _gridView.GetComponentInParent<ScrollRect>();
            return scrollRect != null ? scrollRect.verticalNormalizedPosition : 1f;
        }

        private IEnumerator ScrollAtEndOfFrame(float position)
        {
            yield return new WaitForEndOfFrame();
            ScrollRect scrollRect = _gridView != null ? _gridView.GetComponentInParent<ScrollRect>() : null;
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(position);
            }
        }

        private void Subscribe(InventoryController controller)
        {
            if (controller == _subscribedController)
            {
                return;
            }
            Unsubscribe();
            if (controller == null)
            {
                return;
            }
            controller.AddItemEvent += OnItemAdded;
            controller.RemoveItemEvent += OnItemRemoved;
            controller.RefreshItemEvent += OnItemRefreshed;
            _subscribedController = controller;
        }

        private void Unsubscribe()
        {
            if (_subscribedController == null)
            {
                return;
            }
            _subscribedController.AddItemEvent -= OnItemAdded;
            _subscribedController.RemoveItemEvent -= OnItemRemoved;
            _subscribedController.RefreshItemEvent -= OnItemRefreshed;
            _subscribedController = null;
        }

        private void OnItemAdded(AddItemEventArgs args)
        {
            if (_showingResults && args.Status == CommandStatus.Succeed && args.To != null && args.To.Container == _stashGrid)
            {
                ScheduleRebuild();
            }
        }

        private void OnItemRemoved(RemoveItemEventArgs args)
        {
            if (!_showingResults || args.Status != CommandStatus.Succeed || _resultsGrid == null || !_resultsGrid.Contains(args.Item))
            {
                return;
            }
            _itemContext?.OfferContext?.DeselectItem(args.Item);
            ScheduleRebuild();
        }

        private void OnItemRefreshed(RefreshItemEventArgs args)
        {
            if (_showingResults && _resultsGrid != null && args.Item != null && _resultsGrid.Contains(args.Item))
            {
                ScheduleRebuild();
            }
        }

        protected override void ApplyPlacement()
        {
            if (SearchField == null || _autoSelectSimilar == null)
            {
                return;
            }
            RectTransform reference = _autoSelectSimilar.parent as RectTransform;
            if (reference == null)
            {
                return;
            }
            Rect toggle = LocalRect(_autoSelectSimilar, reference);
            float left = GridAreaLeft(reference, toggle);
            float right = RowContentLeft(reference, toggle, left) - Padding;
            float width = Mathf.Clamp(Mathf.Min(MaxWidth, right - left), MinWidth, MaxWidth);
            float height = Mathf.Clamp(toggle.height > 4f ? toggle.height + 6f : 26f, 24f, 28f);
            Vector2 centerInReference = new Vector2(left + width * 0.5f, toggle.center.y);
            Vector3 worldCenter = reference.TransformPoint(centerInReference);
            RectTransform host = reference;
            while (!ContainsSpan(host, reference, left, left + width)
                   && host.GetComponent<Canvas>() == null
                   && host.parent is RectTransform next)
            {
                host = next;
            }

            RectTransform field = (RectTransform)SearchField.transform;
            if (field.parent != host)
            {
                field.SetParent(host, false);
            }
            field.SetAsLastSibling();

            LayoutElement layoutElement = field.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = field.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.ignoreLayout = true;

            field.anchorMin = new Vector2(0.5f, 0.5f);
            field.anchorMax = new Vector2(0.5f, 0.5f);
            field.pivot = new Vector2(0.5f, 0.5f);
            field.sizeDelta = new Vector2(width, height);
            Vector2 centerInHost = host.InverseTransformPoint(worldCenter);
            field.anchoredPosition = centerInHost - host.rect.center;
        }
        private float GridAreaLeft(RectTransform reference, Rect toggle)
        {
            float left = float.MaxValue;
            if (_gridView != null)
            {
                ConsiderLeftEdge(_gridView.transform as RectTransform, reference, ref left);
                if (_gridView.FilterPanel != null)
                {
                    ConsiderLeftEdge(_gridView.FilterPanel.transform as RectTransform, reference, ref left);
                }
            }
            if (left == float.MaxValue || left >= toggle.xMin)
            {
                left = toggle.xMin - Padding - MaxWidth;
            }
            return left;
        }
        private float RowContentLeft(RectTransform reference, Rect toggle, float gridLeft)
        {
            Transform scrollArea = null;
            if (_gridView != null)
            {
                ScrollRect scrollRect = _gridView.GetComponentInParent<ScrollRect>();
                scrollArea = scrollRect != null ? scrollRect.transform : _gridView.transform;
            }
            Transform filterStrip = _gridView != null && _gridView.FilterPanel != null ? _gridView.FilterPanel.transform : null;

            float result = toggle.xMin;
            foreach (Graphic graphic in reference.GetComponentsInChildren<Graphic>(false))
            {
                Transform t = graphic.transform;
                if ((SearchField != null && t.IsChildOf(SearchField.transform))
                    || (scrollArea != null && t.IsChildOf(scrollArea))
                    || (filterStrip != null && t.IsChildOf(filterStrip)))
                {
                    continue;
                }
                RectTransform rt = graphic.rectTransform;
                if (rt.rect.width <= 1f || rt.rect.height <= 1f)
                {
                    continue;
                }
                Rect r = LocalRect(rt, reference);
                bool sameRow = r.center.y >= toggle.yMin && r.center.y <= toggle.yMax;
                if (sameRow && r.xMin > gridLeft + 40f && r.xMin < result)
                {
                    result = r.xMin;
                }
            }
            return result;
        }

        private static void ConsiderLeftEdge(RectTransform target, RectTransform reference, ref float left)
        {
            if (target == null || target.rect.width <= 4f)
            {
                return;
            }
            float x = LocalRect(target, reference).xMin;
            if (x < left)
            {
                left = x;
            }
        }

        private static bool ContainsSpan(RectTransform host, RectTransform reference, float xMin, float xMax)
        {
            Rect hostInReference = LocalRect(host, reference);
            return hostInReference.xMin <= xMin + 0.5f && hostInReference.xMax >= xMax - 0.5f;
        }
    }
}

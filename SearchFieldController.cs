using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraderSearch
{
    public abstract class SearchFieldController : MonoBehaviour
    {
        private static readonly FieldInfo StashSearchWindowField = AccessTools.Field(typeof(ItemUiContext), "_stashSearchWindow");
        private static readonly FieldInfo SearchFieldField = AccessTools.Field(typeof(StashSearchWindow), "_searchField");
        private static readonly List<SearchFieldController> LiveControllers = new List<SearchFieldController>();
        private static bool _templateMissingLogged;

        private int _lastFocusedFrame = -100;

        protected TMP_InputField SearchField { get; private set; }

        protected string Query { get; private set; } = string.Empty;

        public bool IsInputFocused => SearchField != null && SearchField.isFocused;
        public bool WasFocusedRecently => Time.frameCount - _lastFocusedFrame <= 1;
        public static bool AnyInputFocused
        {
            get
            {
                foreach (SearchFieldController controller in LiveControllers)
                {
                    if (controller != null && controller.IsInputFocused)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool MatchesQuery(Item item)
        {
            if (item == null)
            {
                return false;
            }
            string name = item.LocalizedName();
            if (!string.IsNullOrEmpty(name) && name.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            string shortName = item.LocalizedShortName();
            return !string.IsNullOrEmpty(shortName) && shortName.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void ClearSearchAndDefocus()
        {
            if (SearchField == null)
            {
                return;
            }
            SearchField.SetTextWithoutNotify(string.Empty);
            if (Query.Length > 0)
            {
                Query = string.Empty;
                RefreshFilter();
            }
            SearchField.DeactivateInputField();
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == SearchField.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        protected abstract void RefreshFilter();
        protected abstract void ApplyPlacement();

        protected virtual void Awake()
        {
            LiveControllers.Add(this);
        }

        protected virtual void OnDestroy()
        {
            LiveControllers.Remove(this);
            Query = string.Empty;
        }

        protected virtual void Update()
        {
            if (SearchField != null && SearchField.isFocused)
            {
                _lastFocusedFrame = Time.frameCount;
            }
        }
        protected void ResetQueryAfterShow()
        {
            bool hadQuery = Query.Length > 0;
            Query = string.Empty;
            if (SearchField != null)
            {
                SearchField.SetTextWithoutNotify(string.Empty);
            }
            if (hadQuery)
            {
                RefreshFilter();
            }
        }
        protected void ClearQuerySilently()
        {
            Query = string.Empty;
            if (SearchField != null)
            {
                SearchField.SetTextWithoutNotify(string.Empty);
            }
        }

        protected void EnsureSearchField(Transform initialParent, string objectName)
        {
            if (SearchField != null)
            {
                ApplyPlacement();
                StartCoroutine(ReapplyPlacementAtEndOfFrame());
                return;
            }
            if (initialParent == null)
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

            GameObject go = Instantiate(template.gameObject, initialParent, false);
            go.name = objectName;
            go.SetActive(false);

            TMP_InputField field = go.GetComponent<TMP_InputField>();
            if (field == null)
            {
                Plugin.Log.LogWarning("TraderSearch: cloned search field has no TMP_InputField component; aborting.");
                Destroy(go);
                return;
            }
            SearchField = field;
            field.onValueChanged = new TMP_InputField.OnChangeEvent();
            field.onEndEdit = new TMP_InputField.SubmitEvent();
            field.onSubmit = new TMP_InputField.SubmitEvent();
            field.onSelect = new TMP_InputField.SelectionEvent();
            field.onDeselect = new TMP_InputField.SelectionEvent();
            field.onValueChanged.AddListener(OnQueryChanged);
            field.SetTextWithoutNotify(string.Empty);
            field.interactable = true;

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

        protected static Rect LocalRect(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 min = relativeTo.InverseTransformPoint(corners[0]);
            Vector3 max = relativeTo.InverseTransformPoint(corners[2]);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private void OnQueryChanged(string text)
        {
            string newQuery = (text ?? string.Empty).Trim();
            if (newQuery == Query)
            {
                return;
            }
            Query = newQuery;
            RefreshFilter();
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
            if (SearchField != null)
            {
                ApplyPlacement();
            }
        }
    }
}

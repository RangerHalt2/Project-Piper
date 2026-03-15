// Created by: Kyle Woo
// Centralized button color manager that controls text/underline colors for many buttons.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIButtonColorManager : MonoBehaviour
{
    private enum VisualState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    [Serializable]
    private class StateColors
    {
        public Color normalColor = Color.white;
        public Color highlightedColor = Color.white;
        public Color pressedColor = Color.white;
        public Color selectedColor = Color.white;
        public Color disabledColor = Color.gray;

        public Color Get(VisualState state)
        {
            switch (state)
            {
                case VisualState.Highlighted:
                    return highlightedColor;
                case VisualState.Pressed:
                    return pressedColor;
                case VisualState.Selected:
                    return selectedColor;
                case VisualState.Disabled:
                    return disabledColor;
                default:
                    return normalColor;
            }
        }
    }

    [Serializable]
    private class ManagedButtonEntry
    {
        [Tooltip("Inspector label only.")]
        public string name = "Button Entry";

        [Tooltip("The actual Button that drives this entry's states.")]
        public Button button = null;

        [Tooltip("Text Graphics for this specific button.")]
        public List<Graphic> textTargets = new List<Graphic>();

        [Tooltip("Underline Image Graphics for this specific button.")]
        public List<Graphic> underlineTargets = new List<Graphic>();

        [Tooltip("Optional additional Graphics that should follow this button's state.")]
        public List<Graphic> extraTargets = new List<Graphic>();

        [NonSerialized] public bool isPointerInside;
        [NonSerialized] public bool isPointerDown;
        [NonSerialized] public bool isSelected;
        [NonSerialized] public bool lastInteractable;
        [NonSerialized] public Coroutine transitionRoutine;
    }

    private struct TransitionEntry
    {
        public Graphic graphic;
        public Color from;
        public Color to;
    }

    [Header("Managed Buttons")]
    [Tooltip("Each entry maps one Button to its own text/underline targets.")]
    [SerializeField] private List<ManagedButtonEntry> buttons = new List<ManagedButtonEntry>();

    [Header("State Colors - Text")]
    [SerializeField] private StateColors textColors = new StateColors();

    [Header("State Colors - Underline")]
    [SerializeField] private StateColors underlineColors = new StateColors();

    [Header("State Colors - Extra")]
    [SerializeField] private StateColors extraColors = new StateColors();

    [Header("Transition")]
    [Tooltip("If true, force all managed buttons to use Transition.None.")]
    [SerializeField] private bool disableBuiltInButtonTransition = true;

    [Tooltip("Color fade duration between visual states.")]
    [SerializeField] private float fadeDuration = 0.1f;

    [Tooltip("Use unscaled time so transitions are not affected by pause/timeScale.")]
    [SerializeField] private bool useUnscaledTime = true;

    private void Awake()
    {
        SetupRelays();
    }

    private void OnEnable()
    {
        SetupRelays();

        for (int i = 0; i < buttons.Count; i++)
        {
            ManagedButtonEntry entry = buttons[i];
            if (entry == null || entry.button == null)
            {
                continue;
            }

            entry.isPointerInside = false;
            entry.isPointerDown = false;
            entry.isSelected = false;
            entry.lastInteractable = entry.button.interactable;
            ApplyState(i, EvaluateState(entry), true);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ManagedButtonEntry entry = buttons[i];
            if (entry != null && entry.transitionRoutine != null)
            {
                StopCoroutine(entry.transitionRoutine);
                entry.transitionRoutine = null;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ManagedButtonEntry entry = buttons[i];
            if (entry == null || entry.button == null)
            {
                continue;
            }

            if (entry.lastInteractable != entry.button.interactable)
            {
                entry.lastInteractable = entry.button.interactable;
                ApplyState(i, EvaluateState(entry), false);
            }
        }
    }

    public void RefreshAllNow()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ManagedButtonEntry entry = buttons[i];
            if (entry == null || entry.button == null)
            {
                continue;
            }

            ApplyState(i, EvaluateState(entry), true);
        }
    }

    private void SetupRelays()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ManagedButtonEntry entry = buttons[i];
            if (entry == null || entry.button == null)
            {
                continue;
            }

            if (disableBuiltInButtonTransition)
            {
                entry.button.transition = Selectable.Transition.None;
            }

            UIButtonColorManagerRelay relay = entry.button.GetComponent<UIButtonColorManagerRelay>();
            if (relay == null)
            {
                relay = entry.button.gameObject.AddComponent<UIButtonColorManagerRelay>();
            }

            relay.Configure(this, i);
        }
    }

    private VisualState EvaluateState(ManagedButtonEntry entry)
    {
        if (entry.button == null || !entry.button.interactable)
        {
            return VisualState.Disabled;
        }

        if (entry.isPointerDown)
        {
            return VisualState.Pressed;
        }

        if (entry.isPointerInside)
        {
            return VisualState.Highlighted;
        }

        if (entry.isSelected)
        {
            return VisualState.Selected;
        }

        return VisualState.Normal;
    }

    private void ApplyState(int index, VisualState state, bool instant)
    {
        if (index < 0 || index >= buttons.Count)
        {
            return;
        }

        ManagedButtonEntry entry = buttons[index];
        if (entry == null)
        {
            return;
        }

        if (entry.transitionRoutine != null)
        {
            StopCoroutine(entry.transitionRoutine);
            entry.transitionRoutine = null;
        }

        if (instant || fadeDuration <= 0f)
        {
            ApplyStateImmediate(entry, state);
            return;
        }

        entry.transitionRoutine = StartCoroutine(ApplyStateRoutine(index, state));
    }

    private void ApplyStateImmediate(ManagedButtonEntry entry, VisualState state)
    {
        ApplyColorToTargets(entry.textTargets, textColors.Get(state));
        ApplyColorToTargets(entry.underlineTargets, underlineColors.Get(state));
        ApplyColorToTargets(entry.extraTargets, extraColors.Get(state));
    }

    private IEnumerator ApplyStateRoutine(int index, VisualState state)
    {
        if (index < 0 || index >= buttons.Count)
        {
            yield break;
        }

        ManagedButtonEntry entry = buttons[index];
        if (entry == null)
        {
            yield break;
        }

        List<TransitionEntry> transitionEntries = BuildTransitionEntries(entry, state);
        if (transitionEntries.Count == 0)
        {
            entry.transitionRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.001f, fadeDuration);

        while (elapsed < safeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);

            for (int i = 0; i < transitionEntries.Count; i++)
            {
                TransitionEntry item = transitionEntries[i];
                if (item.graphic != null)
                {
                    item.graphic.color = Color.Lerp(item.from, item.to, t);
                }
            }

            yield return null;
        }

        for (int i = 0; i < transitionEntries.Count; i++)
        {
            TransitionEntry item = transitionEntries[i];
            if (item.graphic != null)
            {
                item.graphic.color = item.to;
            }
        }

        entry.transitionRoutine = null;
    }

    private List<TransitionEntry> BuildTransitionEntries(ManagedButtonEntry entry, VisualState state)
    {
        List<TransitionEntry> items = new List<TransitionEntry>();
        AddTransitionEntries(items, entry.textTargets, textColors.Get(state));
        AddTransitionEntries(items, entry.underlineTargets, underlineColors.Get(state));
        AddTransitionEntries(items, entry.extraTargets, extraColors.Get(state));
        return items;
    }

    private static void ApplyColorToTargets(List<Graphic> targets, Color color)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Graphic graphic = targets[i];
            if (graphic != null)
            {
                graphic.color = color;
            }
        }
    }

    private static void AddTransitionEntries(List<TransitionEntry> items, List<Graphic> targets, Color toColor)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Graphic graphic = targets[i];
            if (graphic == null)
            {
                continue;
            }

            TransitionEntry item = new TransitionEntry
            {
                graphic = graphic,
                from = graphic.color,
                to = toColor
            };
            items.Add(item);
        }
    }

    internal void NotifyPointerEnter(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isPointerInside = true;
        ApplyState(index, EvaluateState(entry), false);
    }

    internal void NotifyPointerExit(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isPointerInside = false;
        entry.isPointerDown = false;
        ApplyState(index, EvaluateState(entry), false);
    }

    internal void NotifyPointerDown(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isPointerDown = true;
        ApplyState(index, EvaluateState(entry), false);
    }

    internal void NotifyPointerUp(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isPointerDown = false;
        ApplyState(index, EvaluateState(entry), false);
    }

    internal void NotifySelect(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isSelected = true;
        ApplyState(index, EvaluateState(entry), false);
    }

    internal void NotifyDeselect(int index)
    {
        if (!TryGetEntry(index, out ManagedButtonEntry entry))
        {
            return;
        }

        entry.isSelected = false;
        ApplyState(index, EvaluateState(entry), false);
    }

    private bool TryGetEntry(int index, out ManagedButtonEntry entry)
    {
        if (index >= 0 && index < buttons.Count)
        {
            entry = buttons[index];
            return entry != null;
        }

        entry = null;
        return false;
    }
}

[DisallowMultipleComponent]
public class UIButtonColorManagerRelay : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [SerializeField] private UIButtonColorManager manager;
    [SerializeField] private int index = -1;

    public void Configure(UIButtonColorManager owner, int managedIndex)
    {
        manager = owner;
        index = managedIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifyPointerEnter(index);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifyPointerExit(index);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifyPointerDown(index);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifyPointerUp(index);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifySelect(index);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (manager != null)
        {
            manager.NotifyDeselect(index);
        }
    }
}

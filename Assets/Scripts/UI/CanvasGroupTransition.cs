// Created by: Kyle Woo
// Reusable CanvasGroup fade + optional scale transition helper.
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupTransition : MonoBehaviour
{
    // Components this transition drives.
    [Header("References")]
    [Tooltip("CanvasGroup to animate. If null, this GameObject's CanvasGroup is used.")]
    [SerializeField] private CanvasGroup targetCanvasGroup;

    [Tooltip("Optional RectTransform to scale during transitions.")]
    [SerializeField] private RectTransform scaleTarget;

    [Tooltip("Optional next transition used by TransitionToConfiguredNext().")]
    [SerializeField] private CanvasGroupTransition configuredNextTransition;

    [Tooltip("Optional object to activate from a button event before playing transitions.")]
    [SerializeField] private GameObject configuredActivationTarget;

    // Timing and easing for the tween.
    [Header("Timing")]
    [Tooltip("Delay before a transition begins, measured in real-time seconds (unscaled).")]
    [SerializeField] private float preTransitionDelay = 0f;
    
    [Tooltip("Animation duration in seconds.")]
    [SerializeField] private float duration = 0.3f;

    [Tooltip("Use unscaled time so transitions still play while Time.timeScale is 0.")]
    [SerializeField] private bool useUnscaledTime = true;

    [Tooltip("Curve controlling transition easing.")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Controls whether hidden panels should be deactivated for cleaner hierarchy/input handling.
    [Header("Visibility")]
    [Tooltip("Disable this object after hide transition completes.")]
    [SerializeField] private bool disableObjectOnHide = true;

    // Optional scaling values used alongside alpha fade to add extra motion.
    [Header("Scale")]
    [Tooltip("Scale used at start of show transition.")]
    [SerializeField] private Vector3 showFromScale = new Vector3(1.05f, 1.05f, 1f);

    [Tooltip("Scale used at end of show transition.")]
    [SerializeField] private Vector3 showToScale = Vector3.one;

    [Tooltip("Scale used at end of hide transition.")]
    [SerializeField] private Vector3 hideToScale = new Vector3(1.08f, 1.08f, 1f);

    private Coroutine runningTransition;

    // Auto-wire references when the component is first added/reset in the editor.
    private void Reset()
    {
        targetCanvasGroup = GetComponent<CanvasGroup>();
        if (targetCanvasGroup == null)
        {
            targetCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (transform is RectTransform rect)
        {
            scaleTarget = rect;
        }
    }

    // Ensure runtime safety if references were not manually set.
    private void Awake()
    {
        if (targetCanvasGroup == null)
        {
            targetCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (targetCanvasGroup == null)
        {
            targetCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Fade/scale into view.
    public void Show()
    {
        StartTransition(0f, 1f, showFromScale, showToScale, true, true);
    }

    // Fade/scale out of view.
    public void Hide()
    {
        StartTransition(1f, 0f, showToScale, hideToScale, false, disableObjectOnHide);
    }

    // Hide this panel, then show the target panel. Useful for menu-to-menu flow.
    public void TransitionTo(CanvasGroupTransition next)
    {
        if (next == null)
        {
            Hide();
            return;
        }

        if (runningTransition != null)
        {
            StopCoroutine(runningTransition);
        }

        runningTransition = StartCoroutine(HideThenShowRoutine(next));
    }

    // Hide this panel and then show the inspector-configured next panel.
    // This exists so Unity Button onClick can call a no-argument function.
    public void TransitionToConfiguredNext()
    {
        TransitionTo(configuredNextTransition);
    }

    // Activates the configured target object.
    // Useful as a direct button event when enabling a hidden menu container.
    public void ActivateConfiguredTarget()
    {
        if (configuredActivationTarget != null)
        {
            configuredActivationTarget.SetActive(true);
        }
    }

    // Deactivates the configured target object.
    public void DeactivateConfiguredTarget()
    {
        if (configuredActivationTarget != null)
        {
            configuredActivationTarget.SetActive(false);
        }
    }

    // Activates configuredNextTransition's GameObject and fades it in.
    // This is handy when the target starts inactive and should appear after button press.
    public void ActivateAndShowConfiguredNext()
    {
        if (configuredNextTransition == null)
        {
            return;
        }

        configuredNextTransition.gameObject.SetActive(true);
        configuredNextTransition.Show();
    }

    // Immediate-only helper for button events that should just fade/zoom out this panel.
    public void HideSelfFromButton()
    {
        Hide();
    }

    // Immediately show without animation.
    public void InstantShow()
    {
        if (runningTransition != null)
        {
            StopCoroutine(runningTransition);
            runningTransition = null;
        }

        gameObject.SetActive(true);
        targetCanvasGroup.alpha = 1f;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;

        if (scaleTarget != null)
        {
            scaleTarget.localScale = showToScale;
        }
    }

    // Immediately hide without animation.
    public void InstantHide()
    {
        if (runningTransition != null)
        {
            StopCoroutine(runningTransition);
            runningTransition = null;
        }

        targetCanvasGroup.alpha = 0f;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        if (scaleTarget != null)
        {
            scaleTarget.localScale = hideToScale;
        }

        if (disableObjectOnHide)
        {
            gameObject.SetActive(false);
        }
    }

    // Starts a transition and cancels any currently running transition to avoid overlap.
    private void StartTransition(
        float fromAlpha,
        float toAlpha,
        Vector3 fromScale,
        Vector3 toScale,
        bool activateAtStart,
        bool disableAtEnd)
    {
        if (runningTransition != null)
        {
            StopCoroutine(runningTransition);
        }

        runningTransition = StartCoroutine(
            TransitionRoutine(fromAlpha, toAlpha, fromScale, toScale, activateAtStart, disableAtEnd));
    }

    // Runs a hide on this panel and then triggers show on the next panel.
    private IEnumerator HideThenShowRoutine(CanvasGroupTransition next)
    {
        yield return TransitionRoutine(
            1f,
            0f,
            showToScale,
            hideToScale,
            false,
            disableObjectOnHide);
        next.Show();
        runningTransition = null;
    }

    // Core tween routine for alpha + optional scale.
    private IEnumerator TransitionRoutine(
        float fromAlpha,
        float toAlpha,
        Vector3 fromScale,
        Vector3 toScale,
        bool activateAtStart,
        bool disableAtEnd)
    {
        if (preTransitionDelay > 0f)
        {
            float delayElapsed = 0f;
            while (delayElapsed < preTransitionDelay)
            {
                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (activateAtStart)
        {
            gameObject.SetActive(true);
        }

        // Disable interaction while animating to prevent accidental clicks.
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;
        targetCanvasGroup.alpha = fromAlpha;

        if (scaleTarget != null)
        {
            scaleTarget.localScale = fromScale;
        }

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.001f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float eased = ease.Evaluate(t);

            targetCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            if (scaleTarget != null)
            {
                scaleTarget.localScale = Vector3.LerpUnclamped(fromScale, toScale, eased);
            }

            yield return null;
        }

        targetCanvasGroup.alpha = toAlpha;

        if (scaleTarget != null)
        {
            scaleTarget.localScale = toScale;
        }

        bool visible = toAlpha > 0.99f;
        targetCanvasGroup.interactable = visible;
        targetCanvasGroup.blocksRaycasts = visible;

        if (!visible && disableAtEnd)
        {
            gameObject.SetActive(false);
        }

        runningTransition = null;
    }
}

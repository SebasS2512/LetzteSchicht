using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintUI : MonoBehaviour
{
    public static HintUI Instance;

    [Header("UI")]
    [SerializeField] private CanvasGroup hintGroup;
    [SerializeField] private TMP_Text hintText;

    [Header("Verhalten")]
    [SerializeField] private float fadeDuration = 0.4f;

    [SerializeField] private int maxSticky = 6;
    private readonly List<string> stickyHints = new List<string>();

    private string nearbyText;
    private string temporaryText;
    private float temporaryTimer;

    private string lastBuiltText;

    private void Awake()
    {
        Instance = this;

        if (hintGroup != null)
        {
            hintGroup.alpha = 0f;

            hintGroup.blocksRaycasts = false;
            hintGroup.interactable = false;
        }

        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        ClearAll();
    }

    private void Update()
    {
        if (temporaryTimer > 0f)
        {
            temporaryTimer -= Time.deltaTime;
            if (temporaryTimer <= 0f) temporaryText = null;
        }

        bool uiBlocked = PauseMenu.IsPaused
                         || (UIController.Instance != null && UIController.Instance.IsNoteOpen());

        string wanted = BuildText();
        bool shouldShow = !uiBlocked && !string.IsNullOrEmpty(wanted);

        if (shouldShow && hintText != null && wanted != lastBuiltText)
        {
            hintText.text = wanted;
            lastBuiltText = wanted;
        }

        UpdateFade(shouldShow);
    }

    private string BuildText()
    {
        List<string> lines = new List<string>(stickyHints);

        if (!string.IsNullOrEmpty(nearbyText) && !lines.Contains(nearbyText))
        {
            lines.Add(nearbyText);
        }

        if (!string.IsNullOrEmpty(temporaryText) && !lines.Contains(temporaryText))
        {
            lines.Add(temporaryText);
        }

        return string.Join("\n", lines);
    }

    private void UpdateFade(bool visible)
    {
        if (hintGroup == null) return;

        float target;
        if (visible)
        {
            target = 1f;
        }
        else
        {
            target = 0f;
        }

        float step;
        if (fadeDuration > 0f)
        {
            step = Time.unscaledDeltaTime / fadeDuration;
        }
        else
        {
            step = 1f;
        }

        hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, target, step);
    }

    public void AddSticky(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (stickyHints.Contains(text)) return;

        stickyHints.Add(text);

        while (maxSticky > 0 && stickyHints.Count > maxSticky)
        {
            stickyHints.RemoveAt(0);
        }
    }

    public void ShowNearby(string text)
    {
        nearbyText = text;
    }

    public void HideNearby(string textToClear = null)
    {
        if (textToClear != null && nearbyText != textToClear) return;

        nearbyText = null;
    }

    public void ShowTemporary(string text, float duration = 8f)
    {
        temporaryText = text;
        temporaryTimer = duration;
    }

    public void ClearAll()
    {
        stickyHints.Clear();
        nearbyText = null;
        temporaryText = null;
        temporaryTimer = 0f;
        lastBuiltText = null;
    }
}
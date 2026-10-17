using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    private string actionText;
    private string commandText;
    private bool promptActive;

    private bool isNoteOpen = false;

    [Header("Interaktions-Hinweis")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text actionTextComponent;
    [SerializeField] private TMP_Text commandTextComponent;

    [Header("Notiz-Fenster")]
    [SerializeField] private GameObject noteInspectPanel;
    [SerializeField] private TMP_Text noteTextDisplay;

    [Header("Notiz-Sound")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip noteOpenClip;
    [SerializeField] private AudioClip noteCloseClip;

    [Header("Aufgabenbrett")]
    [SerializeField] private TMP_Text objectiveBoardText;

    public void SetObjectiveBoard(string text)
    {
        if (objectiveBoardText != null) objectiveBoardText.text = text;
    }

    public void ShowPrompt(string action, string command)
    {
        actionText = action;
        commandText = command;
        promptActive = true;
    }

    public void HidePrompt()
    {
        promptActive = false;
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isNoteOpen) return;

        if (PauseMenu.IsPaused)
        {
            if (promptRoot != null && promptRoot.activeSelf) promptRoot.SetActive(false);
            return;
        }

        UpdatePromptVisibility();

        if (promptActive)
        {
            UpdatePromptTexts();
        }
    }

    private void UpdatePromptVisibility()
    {
        if (promptRoot != null && promptRoot.activeSelf != promptActive)
        {
            promptRoot.SetActive(promptActive);
        }
    }

    private void UpdatePromptTexts()
    {
        if (actionTextComponent != null && actionTextComponent.text != actionText)
        {
            actionTextComponent.text = actionText;
        }

        string command = "[E] " + commandText;
        if (commandTextComponent != null && commandTextComponent.text != command)
        {
            commandTextComponent.text = command;
        }
    }

    public void ShowNote(string textContent)
    {
        if (promptRoot != null) promptRoot.SetActive(false);

        SoundUtils.Play(uiAudioSource, noteOpenClip, 0.05f);

        noteInspectPanel.SetActive(true);
        noteTextDisplay.text = textContent;
        isNoteOpen = true;

        SetGamePaused(true);
    }

    public void HideNote()
    {
        if (noteTextDisplay != null) noteTextDisplay.text = "";

        noteInspectPanel.SetActive(false);
        isNoteOpen = false;

        SetGamePaused(false);

        SoundUtils.Play(uiAudioSource, noteCloseClip, 0.05f);
    }

    private void SetGamePaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    public bool IsNoteOpen()
    {
        return isNoteOpen;
    }
}
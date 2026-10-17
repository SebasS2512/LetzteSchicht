using System;
using UnityEngine;

[System.Serializable]
public class Objective
{
    public string id;

    public string description;

    [Tooltip("Wie oft muss gemeldet werden?")]
    public int targetCount = 1;

    [Tooltip("Wird angezeigt, wenn der Spieler lange nicht weiterkommt. " +
             "Konkreter formulieren als die Beschreibung, z.B. " +
             "'die Türen im Ostflügel abschließen - [R] bei geschlossener Tür'.")]
    [TextArea(2, 3)]
    public string idleHint;

    [HideInInspector] public int currentCount;

    public bool IsDone => currentCount >= targetCount;

    public string CurrentHint
    {
        get
        {
            if (!string.IsNullOrEmpty(idleHint)) return idleHint;

            return description;
        }
    }
}

[System.Serializable]
public class NightObjectives
{
    public int night;
    public Objective[] objectives;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public static event Action<string, int, int> OnObjectiveProgress;

    [Header("Aufgaben pro Nacht (im Inspector befüllen)")]
    [SerializeField] private NightObjectives[] nightConfigs;

    [Header("Sound")]
    [Tooltip("2D-Quelle (Spatial Blend 0). Aufgaben-Sounds sind Rückmeldung " +
             "an den Spieler, nicht Teil der Welt.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Eine ganze Aufgabe abgehakt. Zwischenschritte bleiben still - " +
             "den Stand liest der Spieler am Brett ab.")]
    [SerializeField] private AudioClip objectiveDoneClip;

    [Tooltip("ALLE Aufgaben der Nacht erledigt. Sollte deutlich anders " +
             "klingen als das Abhaken - er sagt: du darfst zum Nacht-Ende.")]
    [SerializeField] private AudioClip allDoneClip;

    [Tooltip("Verzögerung für den 'Alles fertig'-Sound, damit er nicht " +
             "gleichzeitig mit dem letzten Abhaken kommt.")]
    [SerializeField] private float allDoneDelay = 0.6f;

    [Header("Wenn alles erledigt ist")]
    [Tooltip("Wird eingeblendet, sobald alle Aufgaben der Nacht fertig sind, " +
             "und danach vom IdleHintSystem wiederholt. Als Satzteil schreiben, " +
             "das hinter dessen Vorspann passt - z.B. 'zurück ins Bett gehen'.")]
    [TextArea(2, 3)]
    [SerializeField] private string allDoneHint = "zurück ins Bett gehen und die Nacht beenden";

    [Tooltip("Steht dauerhaft unter der Liste, sobald alles erledigt ist. " +
             "Anders als der Tipp oben verschwindet diese Zeile nicht wieder. " +
             "Hier ein vollständiger Satz, kein Satzteil.")]
    [SerializeField] private string allDoneBoardLine = "Alles erledigt - ab ins Bett.";

    private Objective[] currentObjectives = new Objective[0];

    private bool allDoneReported = false;

    private void Awake()
    {
        Instance = this;
        NightManager.OnNightChanged += LoadObjectivesForNight;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= LoadObjectivesForNight;
    }

    private void LoadObjectivesForNight(int night)
    {
        currentObjectives = new Objective[0];

        foreach (NightObjectives config in nightConfigs)
        {
            if (config.night == night)
            {
                currentObjectives = config.objectives;
                break;
            }
        }

        foreach (Objective objective in currentObjectives)
        {
            objective.currentCount = 0;
        }

        allDoneReported = false;

        UpdateBoard();
    }

    public void ReportProgress(string objectiveId)
    {
        foreach (Objective objective in currentObjectives)
        {
            if (objective.id == objectiveId && !objective.IsDone)
            {
                objective.currentCount++;
                UpdateBoard();

                IdleHintSystem.NotifyActivity();

                if (OnObjectiveProgress != null)
                {
                    OnObjectiveProgress.Invoke(
                        objective.id, objective.currentCount, objective.targetCount);
                }

                if (objective.IsDone)
                {
                    SoundUtils.Play(audioSource, objectiveDoneClip, 0f);
                }

                CheckAllDone();
                return;
            }
        }
    }

    public void ReportRegress(string objectiveId)
    {
        foreach (Objective objective in currentObjectives)
        {
            if (objective.id == objectiveId && objective.currentCount > 0)
            {
                objective.currentCount--;

                allDoneReported = false;

                UpdateBoard();
                return;
            }
        }
    }

    public bool IsObjectiveDone(string objectiveId)
    {
        if (string.IsNullOrEmpty(objectiveId)) return false;

        foreach (Objective objective in currentObjectives)
        {
            if (objective.id == objectiveId) return objective.IsDone;
        }

        return true;
    }

    public string GetCurrentHint()
    {
        foreach (Objective objective in currentObjectives)
        {
            if (!objective.IsDone) return objective.CurrentHint;
        }

        if (currentObjectives.Length > 0) return allDoneHint;

        return null;
    }

    private void CheckAllDone()
    {
        if (allDoneReported) return;
        if (!AllObjectivesDone()) return;

        allDoneReported = true;

        StartCoroutine(PlayAllDoneDelayed());
    }

    private System.Collections.IEnumerator PlayAllDoneDelayed()
    {
        yield return new WaitForSeconds(allDoneDelay);
        SoundUtils.Play(audioSource, allDoneClip, 0f);

        IdleHintSystem.ShowHintNow();
    }

    public bool AllObjectivesDone()
    {
        if (currentObjectives.Length == 0) return false;

        foreach (Objective objective in currentObjectives)
        {
            if (!objective.IsDone) return false;
        }

        return true;
    }

    private string BuildHeading()
    {
        if (NightManager.Instance == null) return "";

        return "Nacht " + NightManager.Instance.CurrentNight
               + " von " + NightManager.Instance.LastNight + "\n\n";
    }

    public bool AllObjectivesDoneExcept(string exceptId)
    {
        if (currentObjectives.Length == 0) return false;

        bool hadOthers = false;
        foreach (Objective objective in currentObjectives)
        {
            if (objective.id == exceptId) continue;

            hadOthers = true;
            if (!objective.IsDone) return false;
        }

        return hadOthers;
    }

    private void UpdateBoard()
    {
        string text = BuildHeading() + "Aufgaben:\n";

        foreach (Objective objective in currentObjectives)
        {
            string checkbox;
            if (objective.IsDone)
            {
                checkbox = "[x] ";
            }
            else
            {
                checkbox = "[  ] ";
            }

            string progress;
            if (objective.targetCount > 1)
            {
                progress = " (" + objective.currentCount + "/" + objective.targetCount + ")";
            }
            else
            {
                progress = "";
            }

            text += checkbox + objective.description + progress + "\n";
        }

        if (AllObjectivesDone() && !string.IsNullOrEmpty(allDoneBoardLine))
        {
            text += "\n" + allDoneBoardLine + "\n";
        }

        UIController.Instance.SetObjectiveBoard(text);
    }
}
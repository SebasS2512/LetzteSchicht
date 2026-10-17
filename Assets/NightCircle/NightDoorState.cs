using UnityEngine;

[System.Serializable]
public class NightDoorSetting
{
    public int night = 1;

    [Tooltip("Unlocked = geht auf. NeedsKey = braucht den Schlüssel unten. " +
             "Sealed = geht diese Nacht gar nicht auf.")]
    public Door.DoorMode mode = Door.DoorMode.Sealed;

    [Tooltip("Nur bei NeedsKey und beim Abschließen wichtig.")]
    public string requiredKeyId = "hausmeister";

    [Tooltip("Zählt die Tür in dieser Nacht zur Abschließ-Aufgabe? " +
             "Denk daran, den targetCount der Aufgabe passend zu setzen.")]
    public bool canBeLocked = false;

    [Tooltip("Tür steht beim Nachtbeginn schon offen. Für Türen, die " +
             "offen sind, obwohl sie es nicht sein sollten.")]
    public bool startsOpen = false;
}

[RequireComponent(typeof(Door))]
public class NightDoorState : MonoBehaviour
{
    [Tooltip("Eine Zeile pro Nacht. Nächte ohne Eintrag behalten den " +
             "Zustand der letzten passenden Nacht.")]
    [SerializeField] private NightDoorSetting[] settings;

    private Door door;

    private void Awake()
    {
        door = GetComponent<Door>();
        NightManager.OnNightChanged += Apply;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= Apply;
    }

    private void Apply(int night)
    {
        NightDoorSetting setting = FindSetting(night);
        if (setting == null) return;

        door.SetMode(setting.mode);
        door.SetRequiredKeyId(setting.requiredKeyId);
        door.SetCanBeLocked(setting.canBeLocked);
        door.SetStartsOpen(setting.startsOpen);

        door.ResetToStartState();
    }

    private NightDoorSetting FindSetting(int night)
    {
        NightDoorSetting best = null;

        foreach (NightDoorSetting setting in settings)
        {
            if (setting.night == night) return setting;

            if (setting.night < night && (best == null || setting.night > best.night))
            {
                best = setting;
            }
        }

        return best;
    }
}
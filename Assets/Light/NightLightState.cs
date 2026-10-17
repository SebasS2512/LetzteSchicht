using UnityEngine;

[System.Serializable]
public class NightLightSetting
{
    public int night = 1;

    [Tooltip("Birne durchgebrannt: kein Licht, kein Summen. Alle anderen " +
             "Werte werden dann ignoriert.")]
    public bool broken = false;

    [Tooltip("Steady = ruhig. Subtle = leichtes Schwanken. " +
             "Unstable = kurze Aussetzer. Dying = meistens dunkel.")]
    public FlickeringLight.FlickerMode mode = FlickeringLight.FlickerMode.Steady;

    [Tooltip("Faktor auf die Grundhelligkeit. 1 = voll, 0.6 = spürbar " +
             "schwächer. Damit wird das Gebäude von Nacht zu Nacht müder.")]
    [Range(0f, 1.5f)] public float intensityScale = 1f;

    [Tooltip("Nur bei Unstable/Dying: Wie lange sie am Stück leuchtet.")]
    public Vector2 onDuration = new Vector2(1.5f, 6f);

    [Tooltip("Nur bei Unstable/Dying: Wie lang ein Aussetzer dauert. " +
             "Kürzere Werte = nervöseres Blinken.")]
    public Vector2 offDuration = new Vector2(0.04f, 0.25f);
}

[RequireComponent(typeof(FlickeringLight))]
public class NightLightState : MonoBehaviour
{
    [Tooltip("Eine Zeile pro Nacht. Nächte ohne Eintrag behalten den " +
             "Zustand der letzten passenden Nacht.")]
    [SerializeField] private NightLightSetting[] settings;

    private FlickeringLight flickeringLight;

    private void Awake()
    {
        flickeringLight = GetComponent<FlickeringLight>();
        NightManager.OnNightChanged += Apply;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= Apply;
    }

    private void Apply(int night)
    {
        NightLightSetting setting = FindSetting(night);
        if (setting == null) return;

        if (setting.broken)
        {
            flickeringLight.SetBroken(true);
            return;
        }

        flickeringLight.SetBroken(false);
        flickeringLight.SetDurations(setting.onDuration, setting.offDuration);
        flickeringLight.SetIntensityScale(setting.intensityScale);
        flickeringLight.SetMode(setting.mode);
    }

    private NightLightSetting FindSetting(int night)
    {
        NightLightSetting best = null;

        foreach (NightLightSetting setting in settings)
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
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public enum FlickerMode
    {
        Steady,
        Subtle,
        Unstable,
        Dying
    }

    [Header("Verhalten")]
    [Tooltip("Steady = ruhig. Subtle = leichtes Schwanken. " +
             "Unstable = kurze Aussetzer. Dying = meistens dunkel, zuckt nur auf.")]
    [SerializeField] private FlickerMode mode = FlickerMode.Unstable;

    [Header("Bauteile")]
    [Tooltip("Leer lassen = das erste Light unter diesem Objekt wird genommen.")]
    [SerializeField] private Light targetLight;

    [Tooltip("Lampen-Modell, dessen Material mitleuchten soll. " +
             "Leer = nur das Light ändert sich, das Modell bleibt hell.")]
    [SerializeField] private Renderer glowRenderer;

    [SerializeField] private Color glowColor = new Color(0.85f, 0.95f, 1f);

    [Header("Schwanken (alle Modi außer Steady)")]
    [Tooltip("Wie stark die Helligkeit schwankt. 1 = gar nicht. Gilt für ALLE " +
             "Nächte - für dauerhaft ruhiges Licht den Modus Steady nehmen.")]
    [SerializeField, Range(0.3f, 1f)] private float minBrightness = 0.85f;

    [Tooltip("Tempo des Schwankens. Höher = nervöser.")]
    [SerializeField] private float noiseSpeed = 6f;

    [Header("Aussetzer (Unstable / Dying)")]
    [Tooltip("Sekunden am Stück hell (Min/Max). Bei Dying umgekehrt: so lange " +
             "dunkel. Nicht auf 0 lassen - das gibt ein Stroboskop.")]
    [SerializeField] private Vector2 onDuration = new Vector2(1.5f, 6f);

    [Tooltip("Sekunden pro Aussetzer (Min/Max). Bei Dying umgekehrt: " +
             "so lange blitzt sie auf.")]
    [SerializeField] private Vector2 offDuration = new Vector2(0.04f, 0.25f);

    [Header("Sound")]
    [Tooltip("Dauerbrummen. Loop AN, Play On Awake AUS.")]
    [SerializeField] private AudioSource buzzSource;

    [Tooltip("Grundlautstärke des Brummens. Der Effekt-Regler kommt oben drauf.")]
    [SerializeField, Range(0f, 1f)] private float buzzVolume = 0.3f;

    [Tooltip("Eigene Quelle für das Knacken. Loop AUS.")]
    [SerializeField] private AudioSource zapSource;

    [Tooltip("Kurzes Knacken, wenn die Lampe anspringt.")]
    [SerializeField] private AudioClip zapClip;

    private float baseIntensity;
    private float noiseOffset;

    private float intensityScale = 1f;

    private bool isLit = true;
    private bool isBroken;
    private float stateTimer;

    private float burstTimer;
    private float burstStepTimer;
    private bool burstLit;

    private Material glowMaterial;

    private float lastAppliedFinal = -1f;

    private void Awake()
    {
        if (targetLight == null) targetLight = GetComponentInChildren<Light>();
        if (targetLight != null) baseIntensity = targetLight.intensity;

        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");
        }

        noiseOffset = Random.value * 100f;

        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;

        if (glowMaterial != null) Destroy(glowMaterial);
    }

    private void HandleNightChanged(int night)
    {
        burstTimer = 0f;
    }

    private void OnEnable()
    {
        isLit = ShouldStartLit(mode);
        stateTimer = PickDuration();

        if (!isBroken && buzzSource != null) SoundUtils.PlayLoop(buzzSource, buzzVolume);
    }

    private void OnDisable()
    {
        if (buzzSource != null) buzzSource.Stop();
    }

    private bool ShouldStartLit(FlickerMode checkedMode)
    {
        if (checkedMode == FlickerMode.Dying)
        {
            return false;
        }

        return true;
    }

    public void FlickerBurst(float duration)
    {
        if (isBroken) return;

        burstTimer = duration;
        burstStepTimer = 0f;
        burstLit = false;
    }

    public void SetMode(FlickerMode newMode)
    {
        mode = newMode;
        isBroken = false;

        isLit = ShouldStartLit(newMode);
        stateTimer = PickDuration();

        if (buzzSource != null) SoundUtils.PlayLoop(buzzSource, buzzVolume);
    }

    public void SetIntensityScale(float scale)
    {
        intensityScale = Mathf.Max(0f, scale);
    }

    public void SetDurations(Vector2 newOnDuration, Vector2 newOffDuration)
    {
        onDuration = SanitizeRange(newOnDuration, 1.5f, 6f);
        offDuration = SanitizeRange(newOffDuration, 0.04f, 0.25f);

        stateTimer = PickDuration();
    }

    public void SetBroken(bool broken)
    {
        isBroken = broken;

        if (broken)
        {
            burstTimer = 0f;
            Apply(0f);
            if (buzzSource != null) buzzSource.Stop();
        }
        else if (buzzSource != null)
        {
            SoundUtils.PlayLoop(buzzSource, buzzVolume);
        }
    }

    private void Update()
    {
        if (burstTimer > 0f)
        {
            UpdateBurst();
            return;
        }

        if (isBroken)
        {
            Apply(0f);
            return;
        }

        if (mode == FlickerMode.Steady)
        {
            Apply(1f);
            UpdateBuzz();
            return;
        }

        if (mode != FlickerMode.Subtle)
        {
            UpdateOnOffState();
        }

        float noise = Mathf.PerlinNoise(noiseOffset, Time.time * noiseSpeed);

        float brightness;
        if (isLit)
        {
            brightness = Mathf.Lerp(minBrightness, 1f, noise);
        }
        else
        {
            brightness = 0f;
        }

        Apply(brightness);
        UpdateBuzz();
    }

    private void UpdateBurst()
    {
        burstTimer -= Time.unscaledDeltaTime;
        burstStepTimer -= Time.unscaledDeltaTime;

        if (burstStepTimer <= 0f)
        {
            burstLit = !burstLit;
            burstStepTimer = Random.Range(0.03f, 0.12f);

            if (burstLit && zapClip != null) SoundUtils.Play(zapSource, zapClip, 0.15f);
        }

        float brightness;
        if (burstLit)
        {
            brightness = Random.Range(0.55f, 1f);
        }
        else
        {
            brightness = 0f;
        }

        Apply(brightness);
    }

    private void UpdateBuzz()
    {
        if (buzzSource == null) return;

        float litFactor;
        if (isLit)
        {
            litFactor = 1f;
        }
        else
        {
            litFactor = 0.2f;
        }

        buzzSource.volume = buzzVolume * GameSettings.SfxVolume * litFactor;
    }

    private void UpdateOnOffState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        isLit = !isLit;
        stateTimer = PickDuration();

        if (isLit && zapClip != null)
        {
            SoundUtils.Play(zapSource, zapClip, 0.1f);
        }
    }

    private float PickDuration()
    {
        bool useShortSpan;
        if (mode == FlickerMode.Dying)
        {
            useShortSpan = isLit;
        }
        else
        {
            useShortSpan = !isLit;
        }

        Vector2 range;
        if (useShortSpan)
        {
            range = offDuration;
        }
        else
        {
            range = onDuration;
        }

        return Random.Range(range.x, range.y);
    }

    private Vector2 SanitizeRange(Vector2 range, float fallbackMin, float fallbackMax)
    {
        if (range.x <= 0f && range.y <= 0f) return new Vector2(fallbackMin, fallbackMax);
        if (range.y < range.x) return new Vector2(range.x, range.x);

        return range;
    }

    private void Apply(float brightness)
    {
        float final = brightness * intensityScale;

        if (Mathf.Approximately(final, lastAppliedFinal)) return;

        lastAppliedFinal = final;

        if (targetLight != null)
        {
            targetLight.intensity = baseIntensity * final;
        }

        if (glowMaterial != null)
        {
            glowMaterial.SetColor(EmissionColorId, glowColor * final * 2f);
        }
    }
}
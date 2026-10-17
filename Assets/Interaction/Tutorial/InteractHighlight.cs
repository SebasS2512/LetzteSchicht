using UnityEngine;

public class InteractHighlight : MonoBehaviour
{
    public enum HideMode
    {
        OnInteract,
        OnLock,
        OnObjectiveDone,
        Manual
    }

    [Header("Wann verschwindet sie?")]
    [SerializeField] private HideMode hideMode = HideMode.OnInteract;

    [Tooltip("Nur bei 'OnObjectiveDone' relevant.")]
    [SerializeField] private string objectiveId = "";

    [Header("Wann ist sie überhaupt sichtbar?")]
    [Tooltip("Bis zu welcher Nacht die Markierung erscheint. 1 = nur Tutorial.")]
    [SerializeField] private int showUntilNight = 1;

    [Header("Aussehen")]
    [SerializeField] private Color lineColor = Color.white;

    [Tooltip("Dicke der Linien in Metern. 0.01 ist dezent, 0.03 auffällig.")]
    [SerializeField] private float lineWidth = 0.012f;

    [Tooltip("Abstand der Box zum Objekt, damit die Linien nicht im " +
             "Mesh verschwinden.")]
    [SerializeField] private float padding = 0.04f;

    [Tooltip("Sanftes Pulsieren. Hilft beim Finden, ohne aufdringlich zu sein.")]
    [SerializeField] private bool pulse = true;
    [SerializeField] private float pulseSpeed = 1.8f;
    [SerializeField, Range(0f, 1f)] private float pulseMinAlpha = 0.35f;

    [Tooltip("Eigenes Material für die Linien. Leer lassen - dann baut " +
             "das Skript selbst eins.")]
    [SerializeField] private Material lineMaterial;

    private LineRenderer lineRenderer;
    private bool isHidden;

    private static readonly Vector3[] CornerSigns =
    {
        new Vector3(-1, -1, -1), new Vector3( 1, -1, -1),
        new Vector3( 1, -1,  1), new Vector3(-1, -1,  1),
        new Vector3(-1,  1, -1), new Vector3( 1,  1, -1),
        new Vector3( 1,  1,  1), new Vector3(-1,  1,  1)
    };

    private static readonly int[] EdgePath =
    {
        0, 1, 2, 3, 0, 4, 5, 6, 7, 4, 5, 1, 2, 6, 7, 3
    };

    private void Start()
    {
        BuildBox();
        NightManager.OnNightChanged += HandleNightChanged;

        if (NightManager.Instance != null)
        {
            HandleNightChanged(NightManager.Instance.CurrentNight);
        }
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        isHidden = night > showUntilNight;
        if (lineRenderer != null) lineRenderer.enabled = !isHidden;
    }

    private void Update()
    {
        if (isHidden || lineRenderer == null) return;

        if (hideMode == HideMode.OnObjectiveDone
            && ObjectiveManager.Instance != null
            && ObjectiveManager.Instance.IsObjectiveDone(objectiveId))
        {
            Hide();
            return;
        }

        if (pulse) UpdatePulse();
    }

    private void UpdatePulse()
    {
        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(pulseMinAlpha, 1f, wave);

        Color c = lineColor;
        c.a = alpha;

        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    public void NotifyInteracted(bool wasLockAction)
    {
        if (hideMode == HideMode.OnInteract && !wasLockAction) Hide();
        else if (hideMode == HideMode.OnLock && wasLockAction) Hide();
    }

    public void Hide()
    {
        isHidden = true;
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    public void Show()
    {
        if (NightManager.Instance != null
            && NightManager.Instance.CurrentNight > showUntilNight) return;

        isHidden = false;
        if (lineRenderer != null) lineRenderer.enabled = true;
    }

    private void BuildBox()
    {
        Bounds bounds = CalculateLocalBounds();

        GameObject holder = new GameObject("HighlightBox");
        holder.transform.SetParent(transform, false);

        lineRenderer = holder.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = false;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.material = lineMaterial != null ? lineMaterial : CreateDefaultMaterial();
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

        Vector3 extents = bounds.extents + Vector3.one * padding;

        lineRenderer.positionCount = EdgePath.Length;
        for (int i = 0; i < EdgePath.Length; i++)
        {
            Vector3 corner = bounds.center + Vector3.Scale(extents, CornerSigns[EdgePath[i]]);
            lineRenderer.SetPosition(i, corner);
        }
    }

    private Bounds CalculateLocalBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one * 0.4f);
        }

        Bounds result = new Bounds();
        bool initialized = false;

        foreach (Renderer r in renderers)
        {
            Bounds rb = r.localBounds;

            foreach (Vector3 sign in CornerSigns)
            {
                Vector3 cornerLocal = rb.center + Vector3.Scale(rb.extents, sign);
                Vector3 world = r.transform.TransformPoint(cornerLocal);
                Vector3 ourLocal = transform.InverseTransformPoint(world);

                if (!initialized)
                {
                    result = new Bounds(ourLocal, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    result.Encapsulate(ourLocal);
                }
            }
        }

        return result;
    }

    private Material CreateDefaultMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        return new Material(shader);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightModeChange
{
    [Tooltip("Die Lampe, die dauerhaft umgestellt wird.")]
    public FlickeringLight light;

    public FlickeringLight.FlickerMode newMode = FlickeringLight.FlickerMode.Dying;
}

[System.Serializable]
public class LightFlickerBurst
{
    [Tooltip("Die Lampe, die kurz zucken soll.")]
    public FlickeringLight light;

    [Tooltip("Wie lange gezuckt wird. Bei einem Nachtübergang etwas " +
             "kürzer wählen als die Wartezeit davor.")]
    public float duration = 1.2f;
}

[System.Serializable]
public class ObjectMove
{
    public Transform target;

    [Tooltip("Wohin. Am einfachsten ein leeres GameObject, das du an die " +
             "Zielstelle stellst und passend drehst.")]
    public Transform destination;

    [Tooltip("Auch die Drehung übernehmen. Ein Stuhl, der plötzlich zur " +
             "Wand zeigt, wirkt stärker als einer, der nur woanders steht.")]
    public bool matchRotation = true;

    [Tooltip("0 = sofort umsetzen (richtig für 'war eben noch anders'). " +
             "Größer 0 = sichtbar hingleiten (für Poltergeist-Momente).")]
    public float moveDuration = 0f;
}

[System.Serializable]
public class ObjectSpawn
{
    public GameObject prefab;

    [Tooltip("Wo es erscheint. Drehung wird mit übernommen.")]
    public Transform spawnPoint;

    [Tooltip("Als Kind des Spawnpunkts anhängen - dann wandert es mit, " +
             "falls sich der Punkt bewegt.")]
    public bool parentToSpawnPoint = false;
}

public class ScareEvent : MonoBehaviour
{
    public enum MusicBehaviour
    {
        Nothing,
        Duck,
        Silence,
        Replace
    }

    [Header("Auslöser")]
    [Tooltip("Bei Trigger-Zone: Braucht einen Collider mit 'Is Trigger'. " +
             "Sonst von Hand über Play() aus einem anderen Skript.")]
    [SerializeField] private bool triggerOnPlayerEnter = true;

    [Tooltip("Nur in dieser Nacht auslösen. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 0;

    [Tooltip("Nur einmal pro Nacht auslösen.")]
    [SerializeField] private bool oncePerNight = true;

    [Header("Nur außer Sichtweite")]
    [Tooltip("Wartet, bis der Spieler NICHT hinschaut. Nur sinnvoll für " +
             "Veränderungen, die er nicht sehen soll - bei Effekten, die " +
             "er MITBEKOMMEN soll, unbedingt ausschalten.")]
    [SerializeField] private bool requireOutOfSight = false;

    [Tooltip("Welcher Punkt darf nicht sichtbar sein? Leer = dieses Objekt.")]
    [SerializeField] private Transform sightCheckPoint;

    [Tooltip("Nach so vielen Sekunden wird trotzdem ausgelöst, damit das " +
             "Ereignis nicht ewig hängt, falls der Spieler stehen bleibt.")]
    [SerializeField] private float sightTimeout = 30f;

    [Header("Musik während der Sequenz")]
    [Tooltip("Nothing = läuft weiter. Duck = leiser. Silence = ganz aus. " +
             "Replace = eigener Clip statt der normalen Musik.")]
    [SerializeField] private MusicBehaviour musicBehaviour = MusicBehaviour.Nothing;

    [Tooltip("Nur bei 'Replace': Diese Musik läuft während der Sequenz.")]
    [SerializeField] private AudioClip eventMusicClip;

    [Tooltip("Wartezeit, bevor der Sound kommt. Gibt der Musik Zeit " +
             "auszublenden - die Stille davor ist die halbe Wirkung.")]
    [SerializeField] private float leadIn = 0f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scareClip;

    [Tooltip("Wie lange der Zustand nach dem Sound anhält, bevor die " +
             "Musik zurückkommt.")]
    [SerializeField] private float holdAfterSound = 2f;

    [Header("Hinweis kurz nach dem Ereignis")]
    [Tooltip("Ein Gedanke des Hausmeisters, der kurz eingeblendet wird. " +
             "Leer lassen = kein Hinweis. Kurz halten und in Ich-Form " +
             "schreiben, dann wirkt es nicht wie eine Systemmeldung.")]
    [TextArea(2, 3)]
    [SerializeField] private string playerThought = "";

    [Tooltip("Wartezeit nach dem Sound. Der Gedanke soll nicht in den " +
             "Krach hineinfallen, sondern danach kommen.")]
    [SerializeField] private float thoughtDelay = 2.5f;

    [SerializeField] private float thoughtDuration = 8f;

    [Header("Licht - kurzer Zuck-Ausbruch")]
    [Tooltip("Für Momente und Übergänge: Die Lampe zuckt kurz heftig und " +
             "läuft danach genau so weiter wie vorher. Kollidiert NICHT " +
             "mit NightLightState.")]
    [SerializeField] private LightFlickerBurst[] flickerBursts;

    [Header("Licht - dauerhaft umstellen")]
    [Tooltip("Für bleibende Veränderungen: Die Lampe ist ab jetzt kaputt. " +
             "Wird beim nächsten Nachtwechsel von NightLightState überschrieben.")]
    [SerializeField] private LightModeChange[] lightModeChanges;

    [Header("Licht - einfache Lights ohne Skript")]
    [Tooltip("Nur für Lampen OHNE FlickeringLight-Skript.")]
    [SerializeField] private Light[] plainFlickerLights;

    [SerializeField] private float plainFlickerDuration = 0.6f;

    [Header("Objekte verschieben")]
    [SerializeField] private ObjectMove[] objectMoves;

    [Header("Objekte erscheinen lassen")]
    [SerializeField] private ObjectSpawn[] objectSpawns;

    [Tooltip("Beim Nachtwechsel alles zurücksetzen: verschobene Objekte " +
             "wandern zurück, erschaffene verschwinden.")]
    [SerializeField] private bool restoreOnNightChange = false;

    [Header("Objekte ein-/ausblenden")]
    [SerializeField] private GameObject[] activateObjects;
    [SerializeField] private GameObject[] deactivateObjects;

    private bool isRunning;
    private bool hasFiredThisNight;

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private Camera playerCamera;

    private void Awake()
    {
        StoreOriginalTransforms();
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
        ReleaseMusic();
    }

    private void StoreOriginalTransforms()
    {
        originalPositions = new Vector3[objectMoves.Length];
        originalRotations = new Quaternion[objectMoves.Length];

        for (int i = 0; i < objectMoves.Length; i++)
        {
            ObjectMove move = objectMoves[i];

            if (move == null) continue;
            if (move.target == null) continue;

            originalPositions[i] = move.target.position;
            originalRotations[i] = move.target.rotation;
        }
    }

    private void HandleNightChanged(int night)
    {
        hasFiredThisNight = false;

        if (restoreOnNightChange) Restore();
    }

    private void Restore()
    {
        for (int i = 0; i < objectMoves.Length; i++)
        {
            ObjectMove move = objectMoves[i];

            if (move == null) continue;
            if (move.target == null) continue;

            move.target.position = originalPositions[i];
            move.target.rotation = originalRotations[i];
        }

        foreach (GameObject spawned in spawnedObjects)
        {
            if (spawned != null) Destroy(spawned);
        }

        spawnedObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;

        Play();
    }

    public void Play()
    {
        if (isRunning) return;
        if (oncePerNight && hasFiredThisNight) return;

        if (onlyInNight > 0 && NightManager.Instance != null
            && NightManager.Instance.CurrentNight != onlyInNight) return;

        hasFiredThisNight = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        isRunning = true;

        if (requireOutOfSight)
        {
            yield return StartCoroutine(WaitUntilOutOfSight());
        }

        ApplyMusicBehaviour();

        if (leadIn > 0f) yield return new WaitForSeconds(leadIn);

        SoundUtils.Play(audioSource, scareClip, 0f);

        ApplyFlickerBursts();
        ApplyLightModeChanges();
        ApplyObjectMoves();
        ApplyObjectSpawns();

        SetObjects(activateObjects, true);
        SetObjects(deactivateObjects, false);

        if (plainFlickerLights.Length > 0)
        {
            yield return StartCoroutine(FlickerPlainLights());
        }

        if (!string.IsNullOrEmpty(playerThought)) StartCoroutine(ShowThought());

        yield return new WaitForSeconds(holdAfterSound);

        ReleaseMusic();
        isRunning = false;
    }

    private IEnumerator ShowThought()
    {
        yield return new WaitForSeconds(thoughtDelay);

        if (HintUI.Instance != null)
        {
            HintUI.Instance.ShowTemporary(playerThought, thoughtDuration);
        }
    }


    private IEnumerator WaitUntilOutOfSight()
    {
        Transform point;
        if (sightCheckPoint != null)
        {
            point = sightCheckPoint;
        }
        else
        {
            point = transform;
        }

        float waited = 0f;

        while (waited < sightTimeout)
        {
            if (!IsVisible(point.position)) yield break;

            waited += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private bool IsVisible(Vector3 worldPoint)
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return false;

        Vector3 view = playerCamera.WorldToViewportPoint(worldPoint);

        return view.z > 0f
               && view.x > 0f && view.x < 1f
               && view.y > 0f && view.y < 1f;
    }

    private void ApplyFlickerBursts()
    {
        foreach (LightFlickerBurst burst in flickerBursts)
        {
            if (burst == null) continue;
            if (burst.light == null) continue;

            burst.light.FlickerBurst(burst.duration);
        }
    }

    private void ApplyLightModeChanges()
    {
        foreach (LightModeChange change in lightModeChanges)
        {
            if (change == null) continue;
            if (change.light == null) continue;

            change.light.SetMode(change.newMode);
        }
    }

    private void ApplyObjectMoves()
    {
        foreach (ObjectMove move in objectMoves)
        {
            if (move == null) continue;
            if (move.target == null || move.destination == null) continue;

            if (move.moveDuration <= 0f)
            {
                move.target.position = move.destination.position;
                if (move.matchRotation) move.target.rotation = move.destination.rotation;
            }
            else
            {
                StartCoroutine(MoveSmoothly(move));
            }
        }
    }

    private IEnumerator MoveSmoothly(ObjectMove move)
    {
        Vector3 startPos = move.target.position;
        Quaternion startRot = move.target.rotation;

        float elapsed = 0f;
        while (elapsed < move.moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / move.moveDuration);

            move.target.position = Vector3.Lerp(startPos, move.destination.position, t);
            if (move.matchRotation)
            {
                move.target.rotation = Quaternion.Slerp(startRot, move.destination.rotation, t);
            }

            yield return null;
        }

        move.target.position = move.destination.position;
        if (move.matchRotation) move.target.rotation = move.destination.rotation;
    }

    private void ApplyObjectSpawns()
    {
        foreach (ObjectSpawn spawn in objectSpawns)
        {
            if (spawn == null) continue;
            if (spawn.prefab == null || spawn.spawnPoint == null) continue;

            Transform parent;
            if (spawn.parentToSpawnPoint)
            {
                parent = spawn.spawnPoint;
            }
            else
            {
                parent = null;
            }

            GameObject created = Instantiate(
                spawn.prefab,
                spawn.spawnPoint.position,
                spawn.spawnPoint.rotation,
                parent);

            spawnedObjects.Add(created);
        }
    }

    private void ApplyMusicBehaviour()
    {
        if (MusicPlayer.Instance == null) return;

        switch (musicBehaviour)
        {
            case MusicBehaviour.Duck:
                MusicPlayer.Instance.RequestDuck(this);
                break;
            case MusicBehaviour.Silence:
                MusicPlayer.Instance.RequestSilence(this);
                break;
            case MusicBehaviour.Replace:
                MusicPlayer.Instance.SetEventMusic(eventMusicClip);
                break;
        }
    }

    private void ReleaseMusic()
    {
        if (MusicPlayer.Instance == null) return;

        MusicPlayer.Instance.ReleaseDuck(this);
        MusicPlayer.Instance.ReleaseSilence(this);

        if (musicBehaviour == MusicBehaviour.Replace)
        {
            MusicPlayer.Instance.SetEventMusic(null);
        }
    }

    private IEnumerator FlickerPlainLights()
    {
        float[] original = new float[plainFlickerLights.Length];
        for (int i = 0; i < plainFlickerLights.Length; i++)
        {
            if (plainFlickerLights[i] != null) original[i] = plainFlickerLights[i].intensity;
        }

        float elapsed = 0f;
        while (elapsed < plainFlickerDuration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < plainFlickerLights.Length; i++)
            {
                if (plainFlickerLights[i] == null) continue;

                plainFlickerLights[i].intensity = original[i] * Random.Range(0f, 1f);
            }

            yield return new WaitForSeconds(Random.Range(0.03f, 0.09f));
        }

        for (int i = 0; i < plainFlickerLights.Length; i++)
        {
            if (plainFlickerLights[i] != null) plainFlickerLights[i].intensity = original[i];
        }
    }

    private void SetObjects(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}
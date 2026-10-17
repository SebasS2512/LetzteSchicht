using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    public GameObject On;
    public GameObject Off;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip switchOnClip;
    [SerializeField] private AudioClip switchOffClip;

    private bool isOn;

    void Start()
    {
        On.SetActive(false);
        Off.SetActive(true);
        isOn = false;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            isOn = !isOn;

            if (isOn)
            {
                On.SetActive(true);
                Off.SetActive(false);
                SoundUtils.Play(audioSource, switchOnClip, 0.03f);
            }
            else
            {
                On.SetActive(false);
                Off.SetActive(true);
                SoundUtils.Play(audioSource, switchOffClip, 0.03f);
            }
        }
    }

    public void ForceOff()
    {
        isOn = false;
        if (On != null) On.SetActive(false);
        if (Off != null) Off.SetActive(true);
    }
}
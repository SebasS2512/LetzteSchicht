using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [Tooltip("Name der Menü-Szene (muss in den Build Settings liegen).")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Tooltip("Wartezeit, bevor ein Klick akzeptiert wird.")]
    [SerializeField] private float inputDelay = 1.0f;

    private float timer;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < inputDelay) return;

        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool anyKey  = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        if (clicked || anyKey)
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}

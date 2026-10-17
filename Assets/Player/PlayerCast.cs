using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCast : MonoBehaviour
{
    [Header("Einstellungen")]
    [SerializeField] private Transform pCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float castRadius = 0.25f;
    [SerializeField] private InputActionReference interactionReference;
    [SerializeField] private InputActionReference lockReference;

    private IInteractable currentInteractable;
    private ILockable currentLockable;

    private InteractHighlight currentHighlight;

    private void OnEnable()
    {
        interactionReference.action.Enable();
        interactionReference.action.started += OnInteractPressed;

        lockReference.action.Enable();
        lockReference.action.started += OnLockPressed;
    }

    private void OnDisable()
    {

        interactionReference.action.started -= OnInteractPressed;
        interactionReference.action.Disable();

        lockReference.action.started -= OnLockPressed;
        lockReference.action.Disable();
    }

    private void Update()
    {
        FindTarget();
        UpdatePromptUI();
    }

    private void FindTarget()
    {
        currentInteractable = null;
        currentLockable = null;
        currentHighlight = null;

        if (IsNoteOpen() || PauseMenu.IsPaused) return;

        Ray ray = new Ray(pCamera.position, pCamera.forward);

        bool hasHit = Physics.SphereCast(
            ray, castRadius, out RaycastHit hitInfo, interactionDistance);

        if (!hasHit) return;

        currentInteractable = hitInfo.collider.GetComponentInParent<IInteractable>();
        currentLockable = hitInfo.collider.GetComponentInParent<ILockable>();
        currentHighlight = hitInfo.collider.GetComponentInParent<InteractHighlight>();
    }

    private void UpdatePromptUI()
    {
        if (UIController.Instance == null) return;

        if (currentInteractable == null)
        {
            UIController.Instance.HidePrompt();
            return;
        }

        string displayName = currentInteractable.GetDisplayName();
        InteractionType type = currentInteractable.GetInteractionType();

        ShowPrompt(displayName, type);
    }

    private void ShowPrompt(string displayName, InteractionType type)
    {
        string command = InteractionTypeHelper.GetCommandText(type);

        if (currentLockable != null && currentLockable.CanLockNow())
        {
            if (currentLockable.IsLocked())
            {
                command += "\n[R] Aufschließen";
            }
            else
            {
                command += "\n[R] Abschließen";
            }
        }

        UIController.Instance.ShowPrompt(displayName, command);
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (PauseMenu.IsPaused) return;

        if (IsNoteOpen())
        {
            UIController.Instance.HideNote();
            return;
        }

        if (currentInteractable == null) return;

        currentInteractable.Interact();
        NotifyInteraction(false);
    }

    private void OnLockPressed(InputAction.CallbackContext context)
    {
        if (PauseMenu.IsPaused) return;
        if (IsNoteOpen()) return;

        if (currentLockable == null) return;

        currentLockable.TryLock();
        NotifyInteraction(true);
    }

    private void NotifyInteraction(bool wasLockAction)
    {
        IdleHintSystem.NotifyActivity();

        if (currentHighlight != null) currentHighlight.NotifyInteracted(wasLockAction);
    }

    private bool IsNoteOpen()
    {
        return UIController.Instance != null && UIController.Instance.IsNoteOpen();
    }
}
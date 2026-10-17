using UnityEngine;

public class FlashlightPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject playerFlashlight;

    [SerializeField] private string displayName = "Taschenlampe";

    public string GetDisplayName() => displayName;

    public InteractionType GetInteractionType() => InteractionType.Take;

    public void Interact()
    {
        playerFlashlight.SetActive(true);

        Destroy(gameObject);
    }
}
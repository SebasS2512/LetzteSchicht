using UnityEngine;

public class KeyItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Schlüsselbund";
    [SerializeField] private string keyId = "hausmeister";

    [Header("Sound")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 1f;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Take;

    public void Interact()
    {
        PlayerInventory.Instance.AddKey(keyId);

        SoundUtils.PlayAtPosition(pickupClip, transform.position, pickupVolume);

        Destroy(gameObject);
    }
}
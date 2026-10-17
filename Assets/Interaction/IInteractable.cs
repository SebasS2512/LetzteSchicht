public interface IInteractable
{
    string GetDisplayName();
    InteractionType GetInteractionType();
    void Interact();
}

public interface ILockable
{
    bool CanLockNow();
    bool IsLocked();
    void TryLock();
}
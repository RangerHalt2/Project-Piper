using UnityEngine;

public interface IInteractable
{
    bool canInteract { get; set; }

    void Interact();

}

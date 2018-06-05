using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour, IInteractable
{
    public string slug;

    public void Interact(GameObject interactor)
    {
        Item item = ItemDatabaseManager.Instance.GetItemFromDatabase(slug);

        if (item != null)
        {
            interactor.GetComponent<InventoryController>().AddItem(item);
            //SoundManager.Instance.PlaySound(Resources.Load("Audio/pickup") as AudioClip, transform.position);
            //UIEventHandler.HUDInteractionUnavailable();
            //UIEventHandler.InteractionUnavailable(interactionIconTransform);

            Destroy(gameObject);
        }
    }
}

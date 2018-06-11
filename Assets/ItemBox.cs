using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        ItemBoxUI.Instance.OpenUI(interactor);
    }
}

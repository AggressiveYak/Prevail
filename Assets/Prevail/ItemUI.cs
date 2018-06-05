using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, ISelectHandler
{
    public Item item;
    public Image image;
    public Button button;


    public void OnSelect(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public virtual void SetItem(Item newItem)
    {
        item = newItem;
        image.sprite = Resources.Load<Sprite>("UI/Icons/Items/" + item.slug);

        if (button == null)
        {
            button = GetComponent<Button>();
        }

    }
}

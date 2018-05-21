using System;
using UnityEngine;
using com.ootii.Actors;

public class RoadUI : MonoBehaviour
{
    /// <summary>
    /// Raised when the object becomes active. Initialization can happen here.
    /// </summary>
    void Awake()
    {
    }
    
    /// <summary>
    /// Raised when the object starts being updated
    /// </summary>
    void Start()
    {
        //mMountPoints.AddSkinnedItem("Prefabs/Armor/Pants/Undies_01");
    }

    /// <summary>
    /// Place the buttons and send messages when clicked
    /// </summary>
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 360, 60), "Drag the road pieces to snap them in real-time.");
    }
}

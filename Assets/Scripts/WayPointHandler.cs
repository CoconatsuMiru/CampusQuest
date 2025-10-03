using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointHandler : MonoBehaviour
{
    public GameObject forWayPointsCanvas;  // The canvas to show


    public void SwitchCanvas()
    {
        if (forWayPointsCanvas != null)
            forWayPointsCanvas.SetActive(true);
    }

}

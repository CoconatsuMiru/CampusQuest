using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointHandler : MonoBehaviour
{
    public GameObject forWayPointsCanvas;  // The canvas to show
    public GameObject forCharactersCanvas; // The canvas to hide

    public void SwitchCanvas()
    {
        if (forWayPointsCanvas != null)
            forWayPointsCanvas.SetActive(true);

        if (forCharactersCanvas != null)
            forCharactersCanvas.SetActive(false);
    }
}

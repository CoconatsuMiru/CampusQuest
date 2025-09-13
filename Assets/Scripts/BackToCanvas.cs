using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToCanvas : MonoBehaviour
{
    public GameObject forCharactersCanvas;  // The main canvas to show
    public GameObject forWayPointsCanvas;   // The waypoint canvas to hide

    public void BackToMain()
    {
        if (forCharactersCanvas != null)
            forCharactersCanvas.SetActive(true);

        if (forWayPointsCanvas != null)
            forWayPointsCanvas.SetActive(false);
    }
}

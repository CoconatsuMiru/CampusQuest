using UnityEngine;

public class POIHeightFix : MonoBehaviour
{
    [SerializeField] float yOffset = 0.1f;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y += yOffset;
        transform.position = pos;
    }
}


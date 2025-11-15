using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    public static PlayerPosition Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // <-- prevents destruction on scene load
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}

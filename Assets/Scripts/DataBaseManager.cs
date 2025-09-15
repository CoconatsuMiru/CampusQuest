using Firebase.Database;
using UnityEngine;

public class DataBaseManager : MonoBehaviour
{
    private string userID; 
    void Start()
    {
        userID = SystemInfo.deviceUniqueIdentifier; 
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void CreateUser()
    {

    }
}

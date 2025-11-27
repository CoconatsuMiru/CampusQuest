using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public GameObject damagePopupPrefab;
    public Transform spawnParent; // Canvas

    public void SpawnDamagePopup()
    {
        // Create popup
        GameObject popup = Instantiate(
            damagePopupPrefab,
            transform.position,
            Quaternion.identity,
            spawnParent
        );

        // Random value for testing
        int damageValue = Random.Range(5, 25);

        popup.GetComponent<DamagePopup>().Setup(damageValue);
    }
}

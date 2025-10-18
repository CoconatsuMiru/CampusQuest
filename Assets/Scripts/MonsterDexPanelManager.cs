using UnityEngine;
using UnityEngine.UI;

public class MonsterDexUIManager : MonoBehaviour
{
[Header("UI References")]
[Tooltip("Assign your MonsterDex panel here.")]
[SerializeField] private GameObject monsterDexPanel;


[Tooltip("Assign the button that toggles the MonsterDex.")]
[SerializeField] private Button toggleDexButton;

private bool isOpen = false;

private void Start()
{
    if (monsterDexPanel != null)
        monsterDexPanel.SetActive(false); // Hide on start

    if (toggleDexButton != null)
        toggleDexButton.onClick.AddListener(ToggleDex);
}

private void ToggleDex()
{
    if (monsterDexPanel == null) return;

    isOpen = !isOpen;
    monsterDexPanel.SetActive(isOpen);

    if (isOpen)
        Debug.Log("📖 MonsterDex opened!");
    else
        Debug.Log("❌ MonsterDex closed!");
}


}

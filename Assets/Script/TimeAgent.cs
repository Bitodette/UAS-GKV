using UnityEngine;

public class TimeAgent : MonoBehaviour
{
    [SerializeField] private CropsManager cropsManager;

    public int currentDay = 1;

    void Start()
    {
        if (cropsManager == null)
            cropsManager = FindFirstObjectByType<CropsManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GoToNextDay();
        }
    }

    public void GoToNextDay()
    {
        currentDay++;
        Debug.Log($"--- Hari ke-{currentDay} ---");

        if (cropsManager != null)
            cropsManager.GrowAll();
    }
}

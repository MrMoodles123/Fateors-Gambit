using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public Transform[] spawnPoints; // Assign spawn points in the Inspector

    void Start()
    {
        SetActiveSpawnPoint(0); // Enable only the first spawn point initially
    }

    public void SetActiveSpawnPoint(int index)
    {
        if (index < 0 || index >= spawnPoints.Length)
        {
            Debug.LogWarning($"Invalid spawn index: {index}");
            return;
        }

        // Disable all spawn points
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].gameObject.SetActive(i == index); // Activate only the chosen spawn point
        }

        Debug.Log($"Spawn point {index} activated.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the correct tag
        {
            int spawnIndex = GetSpawnFromCollider(other);
            if (spawnIndex != -1) // Only update if a valid spawn point is found
            {
                SetActiveSpawnPoint(spawnIndex);
            }
        }
    }

    private int GetSpawnFromCollider(Collider collider)
    {
        // Match colliders to specific spawn points
        if (collider.gameObject.name == "Jump (1)")
        {
            Debug.Log("Triggered Jump (1), activating SpawnPoint 1");
            return 1; // Enable the second spawn point
        }
        else if (collider.gameObject.name == "Melee")
        {
            Debug.Log("Triggered Melee, activating SpawnPoint 2");
            return 2; // Enable the third spawn point
        }

        return -1; // No valid match
    }
}

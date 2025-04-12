using UnityEngine;

public class WallBlock : MonoBehaviour
{
    public GameObject portalEffect;  // Assign FX_Portal_Thin_01 in Inspector
    public GameObject[] spawnPoints; // Assign all spawn points in Inspector
    private BoxCollider wallCollider;
    private bool canPassThrough = false;
    private bool hasPassedThrough = false;

    void Start()
    {
        wallCollider = GetComponent<BoxCollider>();

        if (portalEffect != null)
            portalEffect.SetActive(false);

        wallCollider.isTrigger = false;

        // Disable all spawn points except Spawn 1
        SetActiveSpawnPoint(1);
    }

    void Update()
    {
        if (!canPassThrough && !hasPassedThrough)
        {
            EnablePortal();
        }
    }

    private void EnablePortal()
    {
        canPassThrough = true;

        if (portalEffect != null)
            portalEffect.SetActive(true);

        wallCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPassedThrough && other.CompareTag("Player"))
        {
            hasPassedThrough = true;
            Debug.Log("Player passed through the wall");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasPassedThrough && other.CompareTag("Player"))
        {
            DisablePortal();
            Debug.Log("Player exited the trigger");
        }
    }

    private void DisablePortal()
    {
        canPassThrough = false;

        if (portalEffect != null)
            portalEffect.SetActive(false);

        wallCollider.isTrigger = false;
    }

    private void SetActiveSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn points not assigned!");
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                spawnPoints[i].SetActive(i == index); // Enable only Spawn 1
            }
        }

        Debug.Log($"Spawn Point {index} is now active.");
    }
}

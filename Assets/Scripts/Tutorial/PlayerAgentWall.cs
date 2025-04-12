using UnityEngine;

public class PlayerAgentRootWall : MonoBehaviour
{
    public GameObject portalEffect;  
    public GameObject playerAgentRoot;  // The root of the player agent (this will be disabled)

    private BoxCollider wallCollider;
    private bool canPassThrough = false; // Track if the player can pass through
    private bool hasPassedThrough = false;  // Track if the player has passed through already

    void Start()
    {
        // Get the BoxCollider component
        wallCollider = GetComponent<BoxCollider>();

        // Ensure portal effect is off initially
        if (portalEffect != null)
            portalEffect.SetActive(false);

        // Wall starts as solid
        wallCollider.isTrigger = false;
    }

    void Update()
    {
        // If the player agent root is disabled, enable the portal and make wall passable
        if (playerAgentRoot != null && !playerAgentRoot.activeInHierarchy && !canPassThrough)
        {
            EnablePortal();
        }
    }

    private void EnablePortal()
    {
        canPassThrough = true; // Player can pass through now

        if (portalEffect != null)
            portalEffect.SetActive(true); // Show portal FX

        wallCollider.isTrigger = true; // Make the wall passable
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player enters the trigger area and has not passed through yet
        if (!hasPassedThrough && other.CompareTag("Player"))
        {
            hasPassedThrough = true; // Mark the player as passed through
            Debug.Log("Player passed through the wall");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the player exits the trigger and has passed through, disable the portal and make the wall solid
        if (hasPassedThrough && other.CompareTag("Player"))
        {
            DisablePortal();
            Debug.Log("Player exited the trigger");
        }
    }

    private void DisablePortal()
    {
        canPassThrough = false; // Player can no longer pass through

        if (portalEffect != null)
            portalEffect.SetActive(false); // Hide portal FX

        wallCollider.isTrigger = false; // Make wall solid again
    }
}

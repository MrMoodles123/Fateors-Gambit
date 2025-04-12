using UnityEngine;
using UnityEngine.InputSystem;

public class RightMouseButtonDisabledRootWall : MonoBehaviour
{
    public GameObject portalEffect;  
    public GameObject playerAgentRoot; 
    private BoxCollider wallCollider;
    private bool canPassThrough = false; // Track if the player can pass through
    private bool hasPassedThrough = false;  // Track if the player has passed through already
    private bool rightMousePressed = false; // Track if the right mouse button is pressed

    void Start()
    {
        // Get the BoxCollider component on the wall
        wallCollider = GetComponent<BoxCollider>();

        // Ensure portal effect is off initially
        if (portalEffect != null)
            portalEffect.SetActive(false);

        // Wall starts as solid
        wallCollider.isTrigger = false;
    }

    void Update()
    {
        // Check if the right mouse button is pressed and the player's root is disabled
        if (rightMousePressed && playerAgentRoot.activeSelf == false && !hasPassedThrough)
        {
            EnablePortal();
        }

        // Check if the right mouse button is pressed
        if (Mouse.current.rightButton.isPressed && !rightMousePressed)
        {
            rightMousePressed = true; // Right mouse button has been pressed
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
        if (canPassThrough && other.CompareTag("Player") && !hasPassedThrough)
        {
            hasPassedThrough = true; // Mark the player as passed through
            Debug.Log("Player passed through the wall");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the player exits and has passed through, disable the portal and make the wall solid
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

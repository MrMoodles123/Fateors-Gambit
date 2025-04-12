using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;

public class Press3Check : MonoBehaviour
{
    public GameObject portalEffect;  
    private BoxCollider wallCollider;
    private bool canPassThrough = false; // Track if the player can pass through

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
        // Check if only the "1" key is pressed and the portal hasn't been activated yet
        if (Keyboard.current.digit3Key.isPressed && !canPassThrough)
        {
            // Start the coroutine to open the portal with a 2-second delay
            StartCoroutine(OpenPortalWithDelay());
        }

        // Block other number keys (0-9)
        if (Keyboard.current.digit0Key.isPressed ||
            Keyboard.current.digit2Key.isPressed ||
            Keyboard.current.digit1Key.isPressed ||
            Keyboard.current.digit4Key.isPressed ||
            Keyboard.current.digit5Key.isPressed ||
            Keyboard.current.digit6Key.isPressed ||
            Keyboard.current.digit7Key.isPressed ||
            Keyboard.current.digit8Key.isPressed ||
            Keyboard.current.digit9Key.isPressed)
        {
            // Simply return and prevent any other number keys from triggering the portal
            return;
        }
    }

    private IEnumerator OpenPortalWithDelay()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(0f);

        // After 2 seconds, enable the portal and make the wall passable
        EnablePortal();
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
        // If the player enters the trigger area and can pass through
        if (canPassThrough && other.CompareTag("Player"))
        {
            Debug.Log("Player passed through the wall");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the player exits, disable the portal and make the wall solid again
        if (other.CompareTag("Player"))
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

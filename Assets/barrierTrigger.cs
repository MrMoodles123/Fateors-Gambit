using UnityEngine;

public class barrierTrigger : MonoBehaviour
{
    // This method is called when another collider enters the trigger collider attached to the object
    private void OnTriggerEnter(Collider other)
    {
            // Destroy the object
            Destroy(gameObject);
        
    }
}

using UnityEngine;

public class BeanBounce : MonoBehaviour
{
    private Renderer beanRenderer;
    public float bounceHeight = 0.5f; // Max height of the bounce
    public float bounceSpeed = 5f;    // Speed of the bounce

    private float startY; // Stores initial Y position

    // on start, render the bean with random colour
    void Start()
    {
        startY = transform.position.y; // Save the original Y position
        beanRenderer = GetComponent<Renderer>();
        if (beanRenderer != null)
        {
            beanRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startY + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

        // Apply the new Y position, keeping X and Z the same
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

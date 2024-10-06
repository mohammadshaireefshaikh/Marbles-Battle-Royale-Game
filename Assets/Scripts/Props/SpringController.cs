using UnityEngine;
using System.Collections;

public class SpringController : MonoBehaviour
{
    public Transform springTransform; // Reference to the spring (child object)
    public float minHeight = 0f; // Minimum height (compressed position)
    public float maxHeight = 2f; // Maximum height (extended position)
    public float springSpeed = 2f; // Speed at which the spring moves
    public float compressionForce = 10f; // Force applied during compression
    public float damping = 0.9f; // Damping factor to simulate spring mechanics
    public float minDelay = 1f; // Minimum delay before spring compresses
    public float maxDelay = 5f; // Maximum delay before spring compresses

    private Vector3 initialPosition;
    private float velocity = 0f;

    void Start()
    {
        initialPosition = springTransform.localPosition;
        StartCoroutine(SpringMovementRoutine());
    }

    IEnumerator SpringMovementRoutine()
    {
        while (true)
        {
            // Wait for a random amount of time before compressing the spring
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Compress the spring
            yield return StartCoroutine(CompressSpring());

            // Wait for a random amount of time before releasing the spring
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Release the spring
            yield return StartCoroutine(ReleaseSpring());
        }
    }

    IEnumerator CompressSpring()
    {
        while (Mathf.Abs(springTransform.localPosition.y - minHeight) > 0.01f)
        {
            float targetPosition = minHeight;
            float force = compressionForce * (targetPosition - springTransform.localPosition.y);
            velocity += force * Time.deltaTime;
            velocity *= damping; // Apply damping

            springTransform.localPosition += new Vector3(0, velocity * Time.deltaTime, 0);

            yield return null;
        }

        // Stop movement after reaching the compressed state
        velocity = 0;
        springTransform.localPosition = new Vector3(initialPosition.x, minHeight, initialPosition.z);
    }

    IEnumerator ReleaseSpring()
    {
        while (Mathf.Abs(springTransform.localPosition.y - maxHeight) > 0.01f)
        {
            float targetPosition = maxHeight;
            float force = compressionForce * (targetPosition - springTransform.localPosition.y);
            velocity += force * Time.deltaTime;
            velocity *= damping; // Apply damping

            springTransform.localPosition += new Vector3(0, velocity * Time.deltaTime, 0);

            yield return null;
        }

        // Stop movement after reaching the extended state
        velocity = 0;
        springTransform.localPosition = new Vector3(initialPosition.x, maxHeight, initialPosition.z);
    }
}

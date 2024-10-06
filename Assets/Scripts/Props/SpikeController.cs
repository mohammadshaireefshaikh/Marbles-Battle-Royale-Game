using UnityEngine;
using System.Collections;

public class SpikeController : MonoBehaviour
{
    public Transform spikeTransform; // Reference to the spike (child object)
    public float minHeight = 0f; // Minimum height (rest position)
    public float maxHeight = 2f; // Maximum height (extended position)
    public float moveSpeed = 2f; // Speed at which spikes move up and down
    public float minDelay = 1f; // Minimum delay before spikes move
    public float maxDelay = 5f; // Maximum delay before spikes move

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = spikeTransform.localPosition;
        StartCoroutine(SpikeMovementRoutine());
    }

    IEnumerator SpikeMovementRoutine()
    {
        while (true)
        {
            // Wait for a random amount of time before moving spikes
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Move spikes up
            yield return StartCoroutine(MoveSpike(maxHeight));

            // Wait for a random amount of time before moving spikes down
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Move spikes down
            yield return StartCoroutine(MoveSpike(minHeight));
        }
    }

    IEnumerator MoveSpike(float targetHeight)
    {
        Vector3 targetPosition = new Vector3(initialPosition.x, targetHeight, initialPosition.z);

        while (Vector3.Distance(spikeTransform.localPosition, targetPosition) > 0.01f)
        {
            spikeTransform.localPosition = Vector3.MoveTowards(spikeTransform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}

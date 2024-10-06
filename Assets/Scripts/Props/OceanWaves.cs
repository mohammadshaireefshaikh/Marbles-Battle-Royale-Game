using UnityEngine;

public class OceanWaves : MonoBehaviour
{
    public float slideSpeed = 1f; // Speed of sliding movement
    public float bobbingSpeed = 1f; // Speed of bobbing up and down
    public float bobbingHeight = 0.5f; // Height of bobbing up and down
    public float slideDistance = 10f; // Distance to slide before reversing

    private Vector3 originalPosition;
    private bool movingRight = true;

    void Start()
    {
        if (transform.tag == "Tray")
        {
            slideSpeed = Random.Range(0.5f, 5);
        }

        originalPosition = transform.position;
    }

    void Update()
    {
        
        // Calculate the new Y position for the bobbing effect
        float newY = originalPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;

        // Calculate the new X position for the sliding effect
        float newX;
        if (movingRight)
        {
            newX = transform.position.x + slideSpeed * Time.deltaTime;
            if (newX > originalPosition.x + slideDistance)
            {
                newX = originalPosition.x + slideDistance;
                movingRight = false;
            }
        }
        else
        {
            newX = transform.position.x - slideSpeed * Time.deltaTime;
            if (newX < originalPosition.x - slideDistance)
            {
                newX = originalPosition.x - slideDistance;
                movingRight = true;
            }
        }

        // Set the new position
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}

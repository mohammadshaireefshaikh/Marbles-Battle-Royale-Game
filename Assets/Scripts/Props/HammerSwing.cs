using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    public float swingSpeed = 2.0f; // Speed of the swinging motion
    public float swingAngle = 30.0f; // Maximum angle the hammer swings to

    private float startZRotation;

    void Start()
    {
        swingSpeed = Random.Range(0.5f, 3);
        // Record the starting rotation of the hammer on the Z-axis
        startZRotation = transform.localEulerAngles.z;
    }

    void Update()
    {
        // Calculate the swinging angle
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;

        // Apply the rotation to the hammer
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, startZRotation + angle);
    }
}

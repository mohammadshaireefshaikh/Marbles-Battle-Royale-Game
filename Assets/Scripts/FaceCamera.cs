using UnityEngine;
using UnityEngine.SceneManagement;

public class FaceCamera : MonoBehaviour
{
    public Camera cameraToFace;

    void Start()
    {
        if (cameraToFace == null)
            cameraToFace = Camera.main; // Fallback to main camera if not set
    }

    void FixedUpdate()
    {
        if (cameraToFace == null)
            cameraToFace = Camera.main;

        // Calculate the direction to the camera
        Vector3 directionToCamera = cameraToFace.transform.position - transform.position;

        // Create a rotation that faces the camera
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera) ;

        // Apply the rotation, only rotating on the Y axis (around the vertical axis)
        
        if("World1" == SceneManager.GetActiveScene().name)
        {
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y + 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraInScreenshotScene : MonoBehaviour
{
    float accelerate = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        accelerate += Time.smoothDeltaTime*0.25f;

        accelerate = Mathf.Clamp(accelerate, 0, 1);

        transform.position += Vector3.right * Time.smoothDeltaTime * accelerate; 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

using Fusion;

public class LocalCameraHandler : MonoBehaviour
{
    public Transform cameraAnchorPoint;
    public Camera localCamera;
    public GameObject localGun;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //Other components
    NetworkCharacterController networkCharacterController;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        networkCharacterController = GetComponentInParent<NetworkCharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraRotationX = GameManager.instance.cameraViewRotation.x;
        cameraRotationY = GameManager.instance.cameraViewRotation.y;
    }

    void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if (!localCamera.enabled)
            return;

        //Find the Chinemachine camera if we haven't already. 
        if (cinemachineVirtualCamera == null)
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        else 
        {
            if (NetworkPlayer.Local.is3rdPersonCamera)
            {
                if (!cinemachineVirtualCamera.enabled)
                {
                    cinemachineVirtualCamera.Follow = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.LookAt = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.enabled = true;
                    

                    //Sets the layer of the local players model
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("Default"));

                    //Disable the local gun
                    localGun.SetActive(false);
                }

                //Let the camer be handled by cinemachine
                return;
            }
            else 
            {
                if (cinemachineVirtualCamera.enabled)
                {
                    cinemachineVirtualCamera.enabled = false;

                    //Sets the layer of the local players model
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                    //Enable the local gun
                    localGun.SetActive(true);
                }
            }
        }

        //Move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;

        //Calculate rotation
        cameraRotationX += viewInput.y * Time.deltaTime * 60;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

        //Apply rotation
        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);

    }
    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    private void OnDestroy()
    {
        if (cameraRotationX != 0 && cameraRotationY != 0)
        {
            GameManager.instance.cameraViewRotation.x = cameraRotationX;
            GameManager.instance.cameraViewRotation.y = cameraRotationY;
        }
    }
}

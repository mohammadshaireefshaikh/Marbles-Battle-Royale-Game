using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrenadeFireButtonPressed = false;
    bool isRocketLauncherFireButtonPressed = false;

    //Other components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        if (SceneManager.GetActiveScene().name == "Ready")
            return;

        //View input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //Jump
        if (Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        //Fire
        if (Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;

        //Fire
        if (Input.GetButtonDown("Fire2"))
            isRocketLauncherFireButtonPressed = true;

        //Throw grenade
        if (Input.GetKeyDown(KeyCode.G))
            isGrenadeFireButtonPressed = true;

        //Switch camera mode
        if (Input.GetKeyDown(KeyCode.C))
        {
            //Switch the variable on our "local version" of NetworkPlayer
            NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera;

            //Tell the State Authority which camera mode we are using
            NetworkPlayer.Local.RPC_SetCameraMode(NetworkPlayer.Local.is3rdPersonCamera);
        }

        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector);

    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //Camera position
        networkInputData.cameraPosition = localCameraHandler.transform.position;

        //Move data
        networkInputData.movementInput = moveInputVector;

        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        //Fire data
        networkInputData.isFireButtonPressed = isFireButtonPressed;

        //Rocket data
        networkInputData.isRocketLauncherFireButtonPressed = isRocketLauncherFireButtonPressed;

        //Grenade fire data
        networkInputData.isGrenadeFireButtonPressed = isGrenadeFireButtonPressed;

        //Reset variables now that we have read their states
        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrenadeFireButtonPressed = false;
        isRocketLauncherFireButtonPressed = false;

        return networkInputData;
    }
}

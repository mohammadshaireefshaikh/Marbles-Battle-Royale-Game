using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class CharacterMovementHandler : NetworkBehaviour
{
    [Header("Animation")]
    public Animator characterAnimator;

    bool isRespawnRequested = false;

    float walkSpeed = 0;

    //Other components
    NetworkCharacterController networkCharacterController;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    AIHandler aiHandler;

    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
        aiHandler = GetComponent<AIHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (SceneManager.GetActiveScene().name == "Ready")
            return;

        if (Object.HasStateAuthority)
        {
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }

            //Don't update the clients position when they are dead
            if (hpHandler.isDead)
                return;
        }

        Vector2 movementInput = Vector2.zero;
        Vector3 aimForward = Vector3.zero;
        bool isJumppPressed = false;

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            aimForward = networkInputData.aimForwardVector;
            movementInput = networkInputData.movementInput;

            isJumppPressed = networkInputData.isJumpPressed;
        }

        if (Object.HasStateAuthority)
        {
            if (networkPlayer.isBot)
            {
                Vector3 directionToTarget = aiHandler.GetDirectionToTarget(out float distanceToTarget);

                Vector3 inverseDirection = transform.InverseTransformDirection(directionToTarget);

                //Only move towards the target if we are far enough from it. 
                if (distanceToTarget > 3)
                    movementInput = new Vector2(inverseDirection.x, inverseDirection.z);

                //Aim at the target
                aimForward = Vector3.Lerp(transform.forward, directionToTarget, Runner.DeltaTime * 5);
            }

            //Rotate the transform according to the client aim vector
            transform.forward = aimForward;

            //Cancel out rotation on X axis as we don't want our character to tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            //Move
            Vector3 moveDirection = transform.forward * movementInput.y + transform.right * movementInput.x;
            moveDirection.Normalize();

            networkCharacterController.Move(moveDirection);

            //Jump
            if (isJumppPressed)
                networkCharacterController.Jump();

            Vector2 walkVector = new Vector2(networkCharacterController.Velocity.x, networkCharacterController.Velocity.z);
            walkVector.Normalize();

            walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

            characterAnimator.SetFloat("walkSpeed", walkSpeed);


            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName.ToString(), "fell off the world");

                //Respawn();
            }

        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        SetCharacterControllerEnabled(true);
        networkCharacterController.Teleport(Utils.GetRandomSpawnPoint());

        hpHandler.OnRespawned();

        isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterController.enabled = isEnabled;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            networkCharacterController.Teleport(Utils.GetRandomSpawnPoint());
    }
}

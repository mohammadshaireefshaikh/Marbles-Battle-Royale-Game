using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;

    [Networked]
    public NetworkString<_16> nickName { get; set; }

    // Remote Client Token Hash
    [Networked] public int token { get; set; }

    ChangeDetector changeDetector;

    bool isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;

    //AI
    public bool isBot = false;

    //Camera mode
    public bool is3rdPersonCamera { get; set; }

    //Other components
    NetworkInGameMessages networkInGameMessages;

    // Timer variables
    public float gameDuration = 15f; // 3 minutes in seconds
    private float gameTimeRemaining;
    public TextMeshProUGUI timerText; // For showing countdown
    private bool gameEnded = false;

    // Winner/Loser UI Panels
    public GameObject winnerPanel;
    public GameObject loserPanel;

    void Awake()
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    void Start()
    {
        gameTimeRemaining = gameDuration;
        winnerPanel.SetActive(false);
        loserPanel.SetActive(false);
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(nickName):
                    OnNickNameChanged();
                    break;
            }
        }
    }

    void Update()
    {
        if (!gameEnded && SceneManager.GetActiveScene().name == "World1")
        {
            // Countdown the game timer
            gameTimeRemaining -= Time.deltaTime;

            // Update the UI to show the time remaining
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(gameTimeRemaining / 60);
                int seconds = Mathf.FloorToInt(gameTimeRemaining % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            // Check if time has run out
            if (gameTimeRemaining <= 0)
            {
                gameTimeRemaining = 0;
                EndGame();
            }
            else if (Runner.ActivePlayers.ToString() == "1")
            {
                EndGame();
            }
        }
    }

    void EndGame()
    {
        gameEnded = true;

        // Assuming you have a way to get the list of current players in the network session
        var players = FindObjectsOfType<NetworkPlayer>(); // Example to get all NetworkPlayer objects

        if (players.Length == 1)
        {
            // Display winner panel for the last remaining player
            if (Object.HasInputAuthority)
            {
                winnerPanel.SetActive(true);
                StartCoroutine(StartCountdown(winnerPanel)); // Start countdown for the winner
            }
        }
        else
        {
            foreach (var player in players)
            {
                if (player.Object.HasInputAuthority)
                {
                    if (players.Length == 1)
                    {
                        winnerPanel.SetActive(true); // Show winner for the last player
                        StartCoroutine(StartCountdown(winnerPanel)); // Start countdown for the winner
                    }
                    else
                    {
                        loserPanel.SetActive(true); // Show loser for other players
                        StartCoroutine(StartCountdown(loserPanel)); // Start countdown for losers
                    }
                }
            }
        }

        // Additional game-end logic like stopping movement could go here.
    }

    IEnumerator StartCountdown(GameObject panel)
    {
        TextMeshProUGUI countdownText = panel.transform.Find("Countdown").GetComponent<TextMeshProUGUI>();
        float countdownTime = 5f;

        while (countdownTime > 0)
        {
            countdownText.text = "Redirecting in " + Mathf.Ceil(countdownTime).ToString() + "...";
            yield return new WaitForSeconds(1f);
            countdownTime -= 1f;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Redirect to scene with index 0 after countdown finishes
        SceneManager.LoadScene(0);

    }


    public override void Spawned()
    {
        //Set the nick name for players directly, if the nick has already been set there will not be a OnNickNameChanged event
        playerNickNameTM.text = nickName.ToString();
        
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

        if (Object.HasInputAuthority)
        {
            Local = this;

            if (isReadyScene || SceneManager.GetActiveScene().name == "MainMenu")
            {
                Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                //Disable local camera
                localCameraHandler.gameObject.SetActive(false);

                //Disable UI for local player
                localUI.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                //Sets the layer of the local players model
                Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                //Disable main camera
                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);

                //Enable the local camera
                localCameraHandler.localCamera.enabled = true;
                localCameraHandler.gameObject.SetActive(true);

                //Detach camera if enabled
                localCameraHandler.transform.parent = null;

                //Enable UI for local player
                localUI.SetActive(true);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            RPC_SetNickName(GameManager.instance.playerNickName);

            Debug.Log("Spawned local player");
        }
        else
        {
            if (Object.HasStateAuthority && isBot)
                nickName = $"BOT{Random.Range(0, 1000)}";

            //Disable the local camera for remote players
            localCameraHandler.localCamera.enabled = false;
            localCameraHandler.gameObject.SetActive(false);

            //Disable UI for remote player
            localUI.SetActive(false);

            Debug.Log($"{Time.time} Spawned remote player");  
        }

        //Set the Player as a player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);

        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if (playerLeftNetworkObject == Object)
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }
        }

        if (player == Object.InputAuthority)
            Runner.Despawn(Object);
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");
        playerNickNameTM.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if (!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "joined");
            isPublicJoinMessageSent = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCameraMode(bool is3rdPersonCamera, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetCameraMode. is3rdPersonCamera  {is3rdPersonCamera}");
        this.is3rdPersonCamera = is3rdPersonCamera;
    }

    void OnDestroy()
    {
        if (localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Ready")
        {
            if (Object.HasStateAuthority && Object.HasInputAuthority)
                Spawned();

            if (Object.HasStateAuthority)
                GetComponent<CharacterMovementHandler>().RequestRespawn();
        }
    }
}

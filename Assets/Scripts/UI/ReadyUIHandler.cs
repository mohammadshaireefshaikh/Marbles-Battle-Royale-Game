using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;

    bool isReady = false;

    Vector3 desiredCameraPosition = new Vector3(0, 5, 20);

    //Count down
    TickTimer countDownTickTimer = TickTimer.None;

    [Networked]
    byte countDown { get; set; }

    ChangeDetector changeDetector;

    // Start is called before the first frame update
    void Start()
    {
        countDownText.text = "";
    }

    void Update()
    {
        //Wait until the player spawned and NetworkPlayer.Local is populated
        if (NetworkPlayer.Local == null)
            return;

        float lerpSpeed = 0.5f;

        if (!isReady)
        {
            desiredCameraPosition = new Vector3(NetworkPlayer.Local.transform.position.x, 0.95f, 5);
            lerpSpeed = 7;
        }
        else
        {
            desiredCameraPosition = new Vector3(14, 3, 30);
            lerpSpeed = 0.5f;
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, desiredCameraPosition, Time.deltaTime * lerpSpeed);

        if (countDownTickTimer.Expired(Runner))
        {
            StartGame();

            countDownTickTimer = TickTimer.None;
        }
        else if (countDownTickTimer.IsRunning)
        {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        }
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(countDown):
                    OnCountdownChanged();
                    break;
            }
        }
    }

    void StartGame()
    {
        //Lock the session, so no other client can join
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);

            //Check if the player is ready
            if (!gameObjectToTransfer.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection)
                Runner.Disconnect(gameObjectToTransfer.GetComponent<NetworkObject>().InputAuthority);

        }

        //Update scene for the network
        Runner.LoadScene("World1");
    }

    public void OnChangeCharcterBody()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleBody();
    }
    public void OnReady()
    {
        if (isReady)
            isReady = false;
        else isReady = true;

        if (isReady)
            buttonReadyText.text = "NOT READY";
        else
            buttonReadyText.text = "READY";

        if (Runner.IsServer)
        {
            if (isReady)
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, 5);
            else
            {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
        }

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }

    private void OnCountdownChanged()
    {
        if (countDown == 0)
            countDownText.text = $"";
        else countDownText.text = $"Game starts in {countDown}";
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
}

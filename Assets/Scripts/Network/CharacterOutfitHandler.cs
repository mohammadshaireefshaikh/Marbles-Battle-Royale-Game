using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("Character parts")]
    public GameObject playerBody;

    [Header("Ready UI")]
    public Image readyCheckboxImage;

    [Header("Animation")]
    public Animator characterAnimator;

    //List of body part prefabs
    List<GameObject> bodyPrefabs = new List<GameObject>();

    //Other componentes
    NetworkPlayer networkPlayer;

    struct NetworkOutfit : INetworkStruct
    {
        public byte bodyPrefabID;
    }

    [Networked]
    NetworkOutfit networkOutfit { get; set; }

    [Networked]
    public NetworkBool isDoneWithCharacterSelection { get; set; }

    ChangeDetector changeDetector;

    private void Awake()
    {

        //Load all bodies and sort them 
        bodyPrefabs = Resources.LoadAll<GameObject>("Bodyparts/Bodies/").ToList();
        bodyPrefabs = bodyPrefabs.OrderBy(n => n.name).ToList();

        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterAnimator.SetLayerWeight(1, 0.0f);

        if (SceneManager.GetActiveScene().name != "Ready" && !networkPlayer.isBot)
            return;

        NetworkOutfit newOutfit = networkOutfit;

        //Pick a random outfit
        newOutfit.bodyPrefabID = (byte)Random.Range(0, bodyPrefabs.Count);

        //Allow ready up animation layer to show
        characterAnimator.SetLayerWeight(1, 1.0f);

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);

        //Request a random outfit for bots also
        if (networkPlayer.isBot && Object.HasStateAuthority)
        {
            networkOutfit = newOutfit;
            ReplaceBodyParts();
        }
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(isDoneWithCharacterSelection):
                    OnIsDoneWithCharacterSelectionChanged();
                    break;

                case nameof(networkOutfit):
                    OnOutfitChanged();
                    break;
            }
        }
    }

    GameObject ReplaceBodyPart(GameObject currentBodyPart, GameObject prefabNewBodyPart)
    {
        GameObject newPart = Instantiate(prefabNewBodyPart, currentBodyPart.transform.position, currentBodyPart.transform.rotation);
        newPart.transform.parent = currentBodyPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentBodyPart.layer);
        Destroy(currentBodyPart);

        return newPart;
    }

    void ReplaceBodyParts()
    {

        //Replace body
        playerBody = ReplaceBodyPart(playerBody, bodyPrefabs[networkOutfit.bodyPrefabID]);

        GetComponent<HPHandler>().ResetMeshRenderers();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestOutfitChange(NetworkOutfit newNetworkOutfit, RpcInfo info = default)
    {
        Debug.Log($"Received RPC_RequestOutfitChange for player {transform.name}. HeadID {newNetworkOutfit.bodyPrefabID}");

        networkOutfit = newNetworkOutfit;
    }

    private void OnOutfitChanged()
    {
        ReplaceBodyParts();
    }



    

    public void OnCycleBody()
    {
        NetworkOutfit newOutfit = networkOutfit;

        //Pick next head
        newOutfit.bodyPrefabID++;

        if (newOutfit.bodyPrefabID > bodyPrefabs.Count - 1)
            newOutfit.bodyPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    

    public void OnReady(bool isReady)
    {
        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
        {
            RPC_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
         isDoneWithCharacterSelection = isReady;
    }

    private void OnIsDoneWithCharacterSelectionChanged()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        if (isDoneWithCharacterSelection)
        {
            characterAnimator.SetTrigger("Ready");
            readyCheckboxImage.gameObject.SetActive(true);
        }
        else readyCheckboxImage.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Ready")
            readyCheckboxImage.gameObject.SetActive(false);
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
}

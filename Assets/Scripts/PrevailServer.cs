using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using Prevail.Model;
using Prevail.Model.Net;
using System;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class PrevailServer : NetworkManager
{
    //Multi
    public CityGeneratorScript City;

    //Client
    public string PlayerName;
    public Color PlayerColor;

    //Server
    public GameObject playerNetControllerPrefab;
    public GameObject playerNetCharacterPrefab;
    public GameObject bossPrefab;
    
    public Dictionary<string, PlayerNetCharacter> Players;
    //public Dictionary<int, uint> LastAck;
    public static PrevailServer Instance;

    public bool gameInProgress = false;

    #region multi
    public void Start()
    {
        Instance = this;

        ClientScene.RegisterPrefab(playerNetControllerPrefab);
        ClientScene.RegisterPrefab(playerNetCharacterPrefab);
        ClientScene.RegisterPrefab(bossPrefab);
    }
    
    public void FixedUpdate()
    {
        if (NetworkServer.active)
        {
            var state = GameState.GetState();
            var msg = new StateSendMessage(state);

            NetworkServer.SendByChannelToAll(StateSendMessage.MsgId, msg, 2);
        }
    }
    #endregion

    #region client
    public override void OnClientConnect(NetworkConnection conn)
    {
        if (PlayerColor.r == 0 && PlayerColor.g == 0 && PlayerColor.b == 0)
        {
            PlayerColor = new Color(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1f));
        }
        ClientScene.AddPlayer(conn, 0, new PlayerSettingsMessage(PlayerName, PlayerColor, true));
    }
    
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //LastAck.Remove(conn.connectionId);
    }
    #endregion

    #region server
    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkServer.RegisterHandler(StateAckMessage.MsgId, OnStateAck);
        //LastAck = new Dictionary<int, uint>();
        Players = new Dictionary<string, PlayerNetCharacter>();
        City.Generate();
        StartCoroutine(WaitForNetworkServer());
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        //LastAck.Clear();
        Players.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator WaitForNetworkServer()
    {
        while (!NetworkServer.active)
        {
            yield return 0;
        }
        City.GenerateQuirks();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        NetworkServer.SendToClient(conn.connectionId, CitySyncMessage.MsgId, new CitySyncMessage(City.Seed));
    }

    int counter = 0;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader reader)
    {
        var message = new PlayerSettingsMessage(reader);
        if (!message.VR)
        {

            var playerNetControllerObject = (GameObject)Instantiate(playerNetControllerPrefab);
            var playerNetController = playerNetControllerObject.GetComponent<PlayerNetController>();
            NetworkServer.AddPlayerForConnection(conn, playerNetControllerObject, playerControllerId);

            playerNetController.Name = message.Name;
            playerNetController.Color = message.Color;

            GameObject playerNetCharacterObject;
            PlayerNetCharacter playerNetCharacter;

            if (Players.ContainsKey(message.Guid))
            {
                playerNetCharacter = Players[message.Guid];
                playerNetCharacterObject = playerNetCharacter.gameObject;
                Debug.Log("Player Rejoined game");
            }
            else
            {
                var spawns = GameObject.FindObjectsOfType<NetworkStartPosition>();
                var spawn = spawns[counter++ % spawns.Length];
                

                playerNetCharacterObject = (GameObject)Instantiate(playerNetCharacterPrefab, spawn.transform.position, Quaternion.identity);
                playerNetCharacter = playerNetCharacterObject.GetComponent<PlayerNetCharacter>();
                playerNetCharacterObject.AddComponent<NewtonVR.NVRInteractableItem>();

                NetworkServer.Spawn(playerNetCharacterObject);
                
                GameState.TrackedObjects.Add(playerNetCharacterObject);

                Players[message.Guid] = playerNetCharacter;
            }

            playerNetController.Character = playerNetCharacter;
            playerNetCharacter.Controller = playerNetController;
        }
        else
        {
            if (conn.hostId == -1)
            {
                //boss spawn code
                var boss = (GameObject)Instantiate(bossPrefab);
                NetworkServer.Spawn(boss);
                NetworkServer.AddPlayerForConnection(conn, boss, playerControllerId);
                var bossScript = boss.GetComponent<BossController>();
            }
        }
    }
    

    #endregion
}

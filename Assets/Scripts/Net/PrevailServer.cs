using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using Prevail.Model;
using Prevail.Model.Net;
using System;
using UnityEngine.Networking.Match;

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
    
    public Dictionary<string, PlayerNetController> Players;
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

            NetworkServer.SendUnreliableToAll(StateSendMessage.MsgId, msg);
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
        Players = new Dictionary<string, PlayerNetController>();
        City.Generate();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        //LastAck.Clear();
        Players.Clear();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        NetworkServer.SendToClient(conn.connectionId, CitySyncMessage.MsgId, new CitySyncMessage(City.Seed));
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader reader)
    {
        var message = new PlayerSettingsMessage(reader);
        if (!message.VR)
        {
            if (Players.ContainsKey(message.Guid))
            {
                NetworkServer.ReplacePlayerForConnection(conn, Players[message.Guid].gameObject, playerControllerId);
            }
            else
            {
                var playerNetControllerObject = (GameObject)Instantiate(playerNetControllerPrefab);
                var playerNetController = playerNetControllerObject.GetComponent<PlayerNetController>();

                var playerNetCharacterObject = (GameObject)Instantiate(playerNetCharacterPrefab, Vector3.up * 10f, Quaternion.identity);
                var playerNetCharacter = playerNetCharacterObject.GetComponent<PlayerNetCharacter>();

                NetworkServer.Spawn(playerNetCharacterObject);
                NetworkServer.AddPlayerForConnection(conn, playerNetControllerObject, playerControllerId);

                playerNetController.Character = playerNetCharacter;
                playerNetController.Name = message.Name;
                playerNetController.Color = message.Color;

                playerNetCharacter.Controller = playerNetController;

                playerNetController.GameStarted = gameInProgress;

                GameState.TrackedObjects.Add(playerNetCharacterObject);

                Players[message.Guid] = playerNetController;
            }
            //LastAck[conn.connectionId] = 0;
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
    
    public void StartGame()
    {
        gameInProgress = true;
        foreach(var p in Players)
        {
            p.Value.GameStarted = gameInProgress;
        }
    }

    #endregion
}

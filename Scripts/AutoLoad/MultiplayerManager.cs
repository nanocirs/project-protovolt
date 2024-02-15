using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MultiplayerManager : Singleton<MultiplayerManager> {

    // Main menu lobby signals
    [Signal] public delegate void OnServerCreatedEventHandler();
    [Signal] public delegate void OnServerClosedEventHandler();
    [Signal] public delegate void OnServerCreateErrorEventHandler();
    [Signal] public delegate void OnPlayerConnectedEventHandler(int id, string playerName);
    [Signal] public delegate void OnPlayerDisconnectedEventHandler(int id, string playerName);
    [Signal] public delegate void OnPlayerConnectErrorEventHandler();

    // Game lobby signals
    [Signal] public delegate void OnPlayerLoadedEventHandler(int playerId, bool isLocal);

    // In-Game signals
    [Signal] public delegate void OnPlayersReadyEventHandler();
    [Signal] public delegate void OnRaceStartedEventHandler();
    [Signal] public delegate void OnCheckpointConfirmEventHandler(int playedId, int confirmedCheckpoint);
    [Signal] public delegate void OnCarFinishedEventHandler(int playerId, string name, float raceTime);

    private const string DEFAULT_IP = "127.0.0.1";
    private const int PORT = 42010;
    private const int MAX_CONNECTIONS = 12;
    public const int SV_PEER_ID = 1;

    public static bool connected { get; private set; } = false;
    public static int minimumPlayers { get; private set; } = 1;

    private static int playerIterator = 0;

    public static Dictionary<int, int> peerIdplayerIdMap = new Dictionary<int, int>();

    public override void _SingletonReady() {

        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.ServerDisconnected += ServerDisconnected;

    }

#region MAIN_MENU_LOBBY

    public static void CreateServer() {

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

        Error error = peer.CreateServer(PORT, MAX_CONNECTIONS);

        if (error != Error.Ok) {

            instance.EmitSignal(SignalName.OnServerCreateError);

            GD.PrintErr(error);

            return;

        }

        instance.Multiplayer.MultiplayerPeer = peer;

        connected = true;

        int playerId = playerIterator++;
        peerIdplayerIdMap.Add(SV_PEER_ID, playerId);

        GameState.players[playerId] = new PlayerData();
        GameState.players[playerId].playerName = GameState.playerName;
        GameState.players[playerId].peerId = SV_PEER_ID;
        GameState.players[playerId].playerId = playerId;

        instance.EmitSignal(SignalName.OnServerCreated);
        instance.EmitSignal(SignalName.OnPlayerConnected, playerId, GameState.players[playerId].playerName);
        
    }

    public static void JoinServer(string address = "") {
        
        if (address == "") {
            address = DEFAULT_IP;
        }

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        
        Error error = peer.CreateClient(address, PORT);

        if (error != Error.Ok) {

            instance.EmitSignal(SignalName.OnPlayerConnectError);

            GD.PrintErr(error);

            return;

        }

        instance.Multiplayer.MultiplayerPeer = peer;

    }

    public static void DisconnectFromServer() {

        int peerId = instance.Multiplayer.GetUniqueId();

        if (GameState.players.Count() > 0) {
            
            int playerId = peerIdplayerIdMap[peerId];
            string playerName = GameState.players[playerId].playerName;

            instance.Multiplayer.MultiplayerPeer.Close();

            instance.EmitSignal(SignalName.OnPlayerDisconnected, playerId, playerName);

        }

    }

    private void PeerDisconnected(long peerId) {

        int playerId = peerIdplayerIdMap[(int)peerId];

        string playerName = GameState.players[playerId].playerName;

        if (GameState.players[playerId] != null) {
            GameState.players.Remove(playerId);
            peerIdplayerIdMap.Remove((int)peerId);
        }

        EmitSignal(SignalName.OnPlayerDisconnected, playerId, playerName);

    }

    private void ConnectedToServer() {

        RpcId(SV_PEER_ID, "NotifyPeerConnected", GameState.playerName);

        connected = true;

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPeerConnected(string playerName) {

        int playerId = playerIterator++;
        int peerId = instance.Multiplayer.GetRemoteSenderId();

        SendConnectedPlayer(peerId, playerId, playerName);

        foreach (var player in GameState.players) {
            RpcId(peerId, "SendConnectedPlayer", player.Value.peerId, player.Key, player.Value.playerName);
        }

        Rpc("SendConnectedPlayer", peerId, playerId, playerName);

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendConnectedPlayer(int peerId, int playerId, string playerName) {

        if (!peerIdplayerIdMap.ContainsKey(peerId)) {

            peerIdplayerIdMap.Add(peerId, playerId);

            GameState.players[playerId] = new PlayerData();
            GameState.players[playerId].playerName = playerName;
            GameState.players[playerId].peerId = peerId;
            GameState.players[playerId].playerId = playerId;

            EmitSignal(SignalName.OnPlayerConnected, playerId, playerName + " " + playerId);

        }

    }

    private void ConnectionFailed() {

        Multiplayer.MultiplayerPeer = null;

        connected = false;

        GD.Print("Failed to connect to Server");
        
    }

    private void ServerDisconnected() {

        GameState.players.Clear();
        peerIdplayerIdMap.Clear();
        playerIterator = 0;

        EmitSignal(SignalName.OnServerClosed);

        connected = false;

        GD.Print("Disconnected from Server");
        
    }

    public static void LoadMap(string mapFile) {

        string mapPath = "Maps/" + mapFile;

        instance.Rpc("NotifyLoadMap", mapPath);

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyLoadMap(string mapPath) {
        GameStateMachine.instance.LoadScene(mapPath);
    }


#endregion

#region GAME_LOBBY

    public static void PlayerLoaded() {
        if (instance.Multiplayer.IsServer()) {

            int playerId = peerIdplayerIdMap[SV_PEER_ID];

            GameState.playerId = playerId;
            GameState.players[playerId].Restart();

            instance.CallDeferred("OnPlayerLoadedEmit", 0);

        }
        else {
            instance.RpcId(SV_PEER_ID, "NotifyPlayerLoaded");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPlayerLoaded() {

        int clientPlayerId = peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()];

        GameState.players[clientPlayerId].Restart();

        RpcId(Multiplayer.GetRemoteSenderId(), "SetPlayerId", clientPlayerId);

        foreach (var tuple in GameState.players) {
            Rpc("UpdatePlayerList", tuple.Key, tuple.Value.peerId, tuple.Value.carPath);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetPlayerId(int playerId) {
        
        GameState.playerId = playerId;

        CallDeferred("OnPlayerLoadedEmit", 0);   // Let client know server is ready.

        Rpc("EmitPlayerLoaded", GameState.playerId);

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdatePlayerList(int playerId, int peerId, string carPath) {

        GameState.players[playerId].Restart();
        GameState.players[playerId].peerId = peerId;
        GameState.players[playerId].carPath = carPath;

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void EmitPlayerLoaded(int playerId) {

        CallDeferred("OnPlayerLoadedEmit", playerId);

    }

    private void OnPlayerLoadedEmit(int playerId) {
        
        if (GameState.playerId == playerId) {
            EmitSignal(SignalName.OnPlayerLoaded, playerId, true);
        }
        else {
            EmitSignal(SignalName.OnPlayerLoaded, playerId, false);
        }
    }

#endregion

#region IN_GAME

    // PLAYERS READY

    public static void PlayersReady() {

        if (instance.Multiplayer.IsServer()) {
            instance.Rpc("OnPlayersReadyEmit");               
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPlayersReadyEmit() {

        EmitSignal(SignalName.OnPlayersReady);

    }

    // END COUNTDOWN

    public static void Start() {

        if (instance.Multiplayer.IsServer()) {
            instance.Rpc("OnRaceStartedEmit");
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnRaceStartedEmit() {

        EmitSignal(SignalName.OnRaceStarted);

    }

    // CHECKPOINT CONFIRM

    public static void CheckpointConfirm(int checkpointsAdded) {

        if (instance.Multiplayer.IsServer()) {
            instance.Rpc("OnCheckpointConfirmEmit", peerIdplayerIdMap[SV_PEER_ID], checkpointsAdded);
        }
        else {
            instance.RpcId(SV_PEER_ID, "NotifyCheckpointConfirm", checkpointsAdded);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyCheckpointConfirm(int checkpointsAdded) {

        Rpc("OnCheckpointConfirmEmit", peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], checkpointsAdded);
        
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnCheckpointConfirmEmit(int playerId, int checkpointsAdded) {

        GameState.players[playerId].confirmedCheckpoint += checkpointsAdded;
        GameState.players[playerId].currentCheckpoint += checkpointsAdded;

        if (peerIdplayerIdMap[instance.Multiplayer.GetUniqueId()] == playerId) {

            EmitSignal(SignalName.OnCheckpointConfirm, playerId, GameState.players[playerId].confirmedCheckpoint);

        }

    }

    // SEND PLAYER TRANSFORM

    public static void SendPlayerTransform(Transform3D globalTransform, float steering) {
       
        if (instance.Multiplayer.IsServer()) {
            instance.Rpc("UpdateTransforms", peerIdplayerIdMap[SV_PEER_ID], globalTransform, steering);               
        }
        else {
            instance.RpcId(SV_PEER_ID, "NotifyPlayerTransform", globalTransform, steering);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void NotifyPlayerTransform(Transform3D globalTransform, float steering) {

        Rpc("UpdateTransforms", peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], globalTransform, steering);               

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void UpdateTransforms(int playerId, Transform3D globalTransform, float steering) {

        GameState.players[playerId].carTransform = globalTransform;
        GameState.players[playerId].carSteering = steering;     

    }

    // CAR FINISHED

    public static void CarFinished(float raceTime) {

        if (instance.Multiplayer.IsServer()) {
            instance.Rpc("OnCarFinishedEmit", peerIdplayerIdMap[SV_PEER_ID], raceTime);
        }
        else {
            instance.RpcId(SV_PEER_ID, "NotifyCarFinished", raceTime);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyCarFinished(float raceTime) {

        Rpc("OnCarFinishedEmit", peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], raceTime);
        
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnCarFinishedEmit(int playerId, float raceTime) {

        GameState.players[playerId].finished = true;
        GameState.players[playerId].raceTime = raceTime;

        if (playerId != GameState.playerId) {
            EmitSignal(SignalName.OnCarFinished, GameState.players[playerId].playerId, GameState.players[playerId].playerName, raceTime);
        }

    }

#endregion

}

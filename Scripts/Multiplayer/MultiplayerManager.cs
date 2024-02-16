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

    // Car Selection screen signals
    [Signal] public delegate void OnSelectedCarsRequestedEventHandler();

    // Game lobby signals
    [Signal] public delegate void OnPlayerLoadedEventHandler(int playerId, bool isLocal);

    // In-Game signals
    [Signal] public delegate void OnPlayersReadyEventHandler();
    [Signal] public delegate void OnRaceStartedEventHandler();
    [Signal] public delegate void OnPickUpCollectedEventHandler(int playerId, Pickable.PickUpType pickUp);
    [Signal] public delegate void OnPickUpUsedEventHandler(int playerId);
    [Signal] public delegate void OnCheckpointConfirmEventHandler(int playerId, int confirmedCheckpoint);
    [Signal] public delegate void OnCarFinishedEventHandler(int playerId, string name, float raceTime);

    [Export] private GameManager game = null;
    [Export] private CarSelection carSelection = null;

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

#endregion

#region MAP_MANAGER

    // @TODO: This doesn't belong in this file.
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void LoadMap(string mapPath) {

        GameStateMachine.instance.LoadScene(mapPath);
        
    }

#endregion

#region CAR_SELECTION

    public static void RequestSelectedCars() {
        instance.carSelection.RequestSelectedCars();
    }

    public static void SendSelectedCar(string carPath) {
        instance.carSelection.SendSelectedCar(carPath);
    }

#endregion

#region IN_GAME

    public static void PlayerLoaded() {
        instance.game.PlayerLoaded();
    }

    public static void PlayersReady() {
        instance.game.PlayersReady();
    }

    public static void Start() {
        instance.game.Start();
    }

    public static void SendPlayerTransform(Transform3D globalTransform, float steering) {   
        instance.game.SendPlayerTransform(globalTransform, steering);  
    }

    public static void PickUpCollect(CarController car) {
        instance.game.PickUpCollect(car);
    }

    public static void PickUpUse(CarController car) {
        instance.game.PickUpUse(car);
    }

    public static void CheckpointConfirm(int checkpointsAdded) {
        instance.game.CheckpointConfirm(checkpointsAdded);
    }

    public static void CarFinished(float raceTime) {
        instance.game.CarFinished(raceTime);
    }

#endregion

}

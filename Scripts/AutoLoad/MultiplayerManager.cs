using System.Collections.Generic;
using Godot;

public partial class MultiplayerManager : Singleton<MultiplayerManager> {

    [Signal] public delegate void OnServerCreatedEventHandler();
    [Signal] public delegate void OnServerClosedEventHandler();
    [Signal] public delegate void OnPlayerConnectedEventHandler(int id, string playerName);
    [Signal] public delegate void OnPlayerDisconnectedEventHandler(int id, string playerName);

    private const string DEFAULT_IP = "127.0.0.1";
    private const int PORT = 42010;
    private const int MAX_CONNECTIONS = 20;

    private Dictionary<int, string> players = new Dictionary<int, string>();

    private string playerName = "QuePasaShavales";

    public override void _SingletonReady() {

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.ServerDisconnected += ServerDisconnected;

    }

    public void CreateServer() {

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        
        Error error = peer.CreateServer(PORT, MAX_CONNECTIONS);

        if (error != Error.Ok) {
            GD.PrintErr(error);
        }

        Multiplayer.MultiplayerPeer = peer;

        players[1] = playerName;

        EmitSignal(SignalName.OnServerCreated);
        EmitSignal(SignalName.OnPlayerConnected, 1, players[1]);

        if (Multiplayer.IsServer()) {
            GD.Print("Player with ID 1 connected to Server");
        }
    }

    public void JoinServer(string address = "") {
        
        if (address == "") {
            address = DEFAULT_IP;
        }

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        
        Error error = peer.CreateClient(address, PORT);

        if (error != Error.Ok) {
            GD.PrintErr(error);
        }

        Multiplayer.MultiplayerPeer = peer;

    }

    private void PeerConnected(long id) {
        RpcId(id, "RegisterPlayer", playerName);
    }

    private void PeerDisconnected(long id) {

        string peerName = players[(int)id];

        if (players[(int)id] != null) {
            players.Remove((int)id);
        }

        EmitSignal(SignalName.OnPlayerDisconnected, (int)id, peerName);
        
        if (Multiplayer.IsServer()) {
            GD.Print("Player with ID " + id + " disconnected from Server. Current players: " + players.Count);
        }

    }

    private void ConnectedToServer() {

        int playerId = Multiplayer.GetUniqueId();
        players[playerId] = playerName;

        EmitSignal(SignalName.OnPlayerConnected, playerId, playerName);

    }

    private void ConnectionFailed() {

        Multiplayer.MultiplayerPeer = null;

        GD.Print("Failed to connect to Server");
        
    }

    private void ServerDisconnected() {

        players.Clear();
        EmitSignal(SignalName.OnServerClosed);

        GD.Print("Disconnected from Server");
        
    }

    public void DisconnectFromServer() {

        int playerId = Multiplayer.GetUniqueId();
        string peerName = players[playerId];

        GD.PrintErr(players.Count);

        Multiplayer.MultiplayerPeer.Close();
  
        EmitSignal(SignalName.OnPlayerDisconnected, playerId, peerName);

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(string newPlayerName) {

        int playerId = Multiplayer.GetRemoteSenderId();
        players[playerId] = newPlayerName;

        EmitSignal(SignalName.OnPlayerConnected, playerId, newPlayerName);

        if (Multiplayer.IsServer()) {
            GD.Print("Player with ID " + playerId + " connected to Server. Current players: " + players.Count);
        }

    }

    public int GetTotalPlayers() {
        
        return players.Count;

    }
}

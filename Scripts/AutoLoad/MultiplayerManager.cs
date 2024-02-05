using System.Collections.Generic;
using Godot;

public partial class MultiplayerManager : Singleton<MultiplayerManager> {

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

    }

    public void CreateServer() {

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        
        Error error = peer.CreateServer(PORT, MAX_CONNECTIONS);

        if (error != Error.Ok) {
            GD.PrintErr(error);
        }

        Multiplayer.MultiplayerPeer = peer;

        players[1] = "Server";
        
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
        GD.Print("Player with ID " + id + " connected to Server");
    }

    private void PeerDisconnected(long id) {
        GD.Print("Player with ID " + id + " disconnected from Server");
    }

    private void ConnectedToServer() {
        GD.Print("Connected to Server");
    }

    private void ConnectionFailed() {
        GD.Print("Failed to connect to Server");
    }

}

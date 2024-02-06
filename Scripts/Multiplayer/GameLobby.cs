using System.Collections.Generic;
using Godot;

public partial class GameLobby : Node {

    private partial class LobbyPlayer : Node {
        public int id;
        public string name;
    }

    [Signal] public delegate void OnPlayerLoadedEventHandler(int playerId, bool isLocal);

    private Dictionary<int, LobbyPlayer> players = new Dictionary<int, LobbyPlayer>();

    private int playerIterator = 0;
    private int myPlayerId = -1;

    public override void _Ready() {
        
        if (Multiplayer.IsServer()) {
    
            myPlayerId = playerIterator++;

            LobbyPlayer serverPlayer = new LobbyPlayer();
            serverPlayer.id = Multiplayer.MultiplayerPeer.GetUniqueId();
            serverPlayer.name = MultiplayerManager.instance.playerName;

            players[myPlayerId] = serverPlayer;

            CallDeferred("OnPlayerLoadedEmit", myPlayerId);

        }
        else {
            RpcId(1, "NetNotifyConnection", MultiplayerManager.instance.playerName);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NetNotifyConnection(string playerName) {

        int clientPlayerId = playerIterator++;

        LobbyPlayer clientPlayer = new LobbyPlayer();
        clientPlayer.id = Multiplayer.GetRemoteSenderId();
        clientPlayer.name = playerName;

        players[clientPlayerId] = clientPlayer;

        RpcId(clientPlayer.id, "NetSetLobbyId", clientPlayerId);
        
        foreach (var tuple in players) {
            Rpc("NetUpdatePlayerList", tuple.Key, tuple.Value.id, tuple.Value.name);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NetUpdatePlayerList(int playerLobbyId, int playerId, string playerName) {

        LobbyPlayer player = new LobbyPlayer();
        player.id = playerId;
        player.name = playerName;

        players[playerLobbyId] = player;

    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NetSetLobbyId(int lobbyId) {
        
        myPlayerId = lobbyId;

        CallDeferred("OnPlayerLoadedEmit", 0);

        Rpc("NetEmitPlayerLoaded", myPlayerId);

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NetEmitPlayerLoaded(int playerId) {

        CallDeferred("OnPlayerLoadedEmit", playerId);

    }

    private void OnPlayerLoadedEmit(int playerId) {
        
        if (myPlayerId == playerId) {

            EmitSignal(SignalName.OnPlayerLoaded, playerId, true);
        }
        else {

            EmitSignal(SignalName.OnPlayerLoaded, playerId, false);

        }
    }
}

using Godot;

public partial class GameLobby : Node {

    public void PlayerLoaded() {

        if (Multiplayer.IsServer()) {

            int playerId = MultiplayerManager.peerIdplayerIdMap[MultiplayerManager.SV_PEER_ID];

            GameState.playerId = playerId;
            GameState.players[playerId].Restart();

            CallDeferred("OnPlayerLoadedEmit", 0);

        }
        else {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPlayerLoaded");
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPlayerLoaded() {

        int clientPlayerId = MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()];

        GameState.players[clientPlayerId].Restart();

        RpcId(Multiplayer.GetRemoteSenderId(), "SetPlayerId", clientPlayerId);

        foreach (var tuple in GameState.players) {
            Rpc("UpdatePlayerList", tuple.Key, tuple.Value.peerId, tuple.Value.carPath);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetPlayerId(int playerId) {
        
        GameState.playerId = playerId;

        // Let client know server is ready.
        CallDeferred("OnPlayerLoadedEmit", 0);   

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
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPlayerLoaded, playerId, true);
        }
        else {
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPlayerLoaded, playerId, false);
        }

    }
    
}

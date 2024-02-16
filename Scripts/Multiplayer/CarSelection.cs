using Godot;

public partial class CarSelection : Node {

    private int carsNotified = 0;

    public void RequestSelectedCars() {

        if (Multiplayer.IsServer()) {
            Rpc("OnSelectedCarsRequestedEmit");
        }
        
    }

    public void SendSelectedCar(string path) {
        RpcId(MultiplayerManager.SV_PEER_ID, "ReceiveSelectedCars", path);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnSelectedCarsRequestedEmit() {

        MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnSelectedCarsRequested);

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceiveSelectedCars(string path) {

        if (Multiplayer.IsServer()) {

            int playerId = MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()];
            GameState.players[playerId].carPath = path;

            carsNotified++;

            if (carsNotified == GameState.GetTotalPlayers()) {
                MultiplayerManager.instance.Rpc("LoadMap", "Maps/Game.tscn");
            }

        }

    }

}

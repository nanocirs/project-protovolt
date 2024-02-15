using Godot;

public partial class GameManager : Node {

    public void PlayersReady() {

        if (Multiplayer.IsServer()) {
            Rpc("OnPlayersReadyEmit");               
        }

    }

    public void Start() {

        if (Multiplayer.IsServer()) {
            Rpc("OnRaceStartedEmit");
        }

    }

    public void SendPlayerTransform(Transform3D globalTransform, float steering) {
       
        if (Multiplayer.IsServer()) {
            Rpc("UpdateTransforms", MultiplayerManager.peerIdplayerIdMap[MultiplayerManager.SV_PEER_ID], globalTransform, steering);               
        }
        else {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPlayerTransform", globalTransform, steering);
        }

    }

    public void CarFinished(float raceTime) {

        if (Multiplayer.IsServer()) {
            Rpc("OnCarFinishedEmit", MultiplayerManager.peerIdplayerIdMap[MultiplayerManager.SV_PEER_ID], raceTime);
        }
        else {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyCarFinished", raceTime);
        }

    }

    public void PickUpCollect(CarController car) {
        
    }

    public void CheckpointConfirm(int checkpointsAdded) {

        if (Multiplayer.IsServer()) {
            Rpc("OnCheckpointConfirmEmit", MultiplayerManager.peerIdplayerIdMap[MultiplayerManager.SV_PEER_ID], checkpointsAdded);
        }
        else {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyCheckpointConfirm", checkpointsAdded);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPlayersReadyEmit() {

        MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPlayersReady);

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnRaceStartedEmit() {

        MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnRaceStarted);

    }   

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void UpdateTransforms(int playerId, Transform3D globalTransform, float steering) {

        GameState.players[playerId].carTransform = globalTransform;
        GameState.players[playerId].carSteering = steering;     

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnCheckpointConfirmEmit(int playerId, int checkpointsAdded) {

        GameState.players[playerId].confirmedCheckpoint += checkpointsAdded;
        GameState.players[playerId].currentCheckpoint += checkpointsAdded;

        if (MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetUniqueId()] == playerId) {
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnCheckpointConfirm, playerId, GameState.players[playerId].confirmedCheckpoint);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnCarFinishedEmit(int playerId, float raceTime) {

        GameState.players[playerId].finished = true;
        GameState.players[playerId].raceTime = raceTime;

        if (playerId != GameState.playerId) {
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnCarFinished, playerId, GameState.players[playerId].playerName, raceTime);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void NotifyPlayerTransform(Transform3D globalTransform, float steering) {

        Rpc("UpdateTransforms", MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], globalTransform, steering);               

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyCheckpointConfirm(int checkpointsAdded) {
        Rpc("OnCheckpointConfirmEmit", MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], checkpointsAdded);  
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyCarFinished(float raceTime) {

        Rpc("OnCarFinishedEmit", MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()], raceTime);
        
    }
    
}

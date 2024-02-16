using Godot;

public partial class GameManager : Node {

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

    public void PickUpCollect(CarController car) {

        if (Multiplayer.IsServer()) {
            OnPickUpCollectedEmit(car.playerId, Pickable.GetRandomPickUp());
        }
        else if (GameState.playerId == car.playerId) {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPickUpCollected", car.playerId);
        }
    
    }

    public void PickUpUse(CarController car) {

        if (Multiplayer.IsServer()) {
            OnPickUpUsedEmit(car.playerId);
        }
        else if (GameState.playerId == car.playerId) {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPickUpUsed", car.playerId);
        }

    }

    public void CheckpointConfirm(int checkpointsAdded) {

        if (Multiplayer.IsServer()) {
            Rpc("OnCheckpointConfirmEmit", MultiplayerManager.peerIdplayerIdMap[MultiplayerManager.SV_PEER_ID], checkpointsAdded);
        }
        else {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyCheckpointConfirm", checkpointsAdded);
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void EmitPlayerLoaded(int playerId) {

        CallDeferred("OnPlayerLoadedEmit", playerId);

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdatePlayerList(int playerId, int peerId, string carPath) {

        GameState.players[playerId].Restart();
        GameState.players[playerId].peerId = peerId;
        GameState.players[playerId].carPath = carPath;

    }

    private void OnPlayerLoadedEmit(int playerId) {
        
        if (GameState.playerId == playerId) {
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPlayerLoaded, playerId, true);
        }
        else {
            MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPlayerLoaded, playerId, false);
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
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPickUpCollectedEmit(int playerId, Pickable.PickUpType pickUp) {

        MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPickUpCollected, playerId, (int)pickUp);

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPickUpUsedEmit(int playerId) {

        MultiplayerManager.instance.EmitSignal(MultiplayerManager.SignalName.OnPickUpUsed, playerId);

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
    private void NotifyPickUpCollected(int playerId) {

        if (Multiplayer.IsServer()) {
            RpcId(Multiplayer.GetRemoteSenderId(), "OnPickUpCollectedEmit", playerId, (int)Pickable.GetRandomPickUp());
        }

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPickUpUsed(int playerId) {
        
        if (Multiplayer.IsServer()) {
            RpcId(Multiplayer.GetRemoteSenderId(), "OnPickUpUsedEmit", playerId);
        }

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

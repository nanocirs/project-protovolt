using Godot;
using System.Linq;

public partial class GameManagerOnline : GameManagerBase {

    public override void _Ready() {

        base._Ready();
        
        MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;
        MultiplayerManager.instance.OnPlayersReady += StartCountdown;
        MultiplayerManager.instance.OnRaceStarted += Start;
        MultiplayerManager.instance.OnCarFinished += OnCarFinished;
        MultiplayerManager.instance.OnCheckpointConfirm += OnCheckpointConfirm;

        MultiplayerManager.PlayerLoaded();

    }

    public override void _PhysicsProcess(double delta) {

        if (!isValidGame || localCar == null) {
            return;
        }

        MultiplayerManager.SendPlayerTransform(localCar.GlobalTransform, localCar.Steering);

        foreach (var tuple in GameState.players) {

            if (cars.ContainsKey(tuple.Value.playerId) && tuple.Value.playerId != GameState.playerId) {

                cars[tuple.Value.playerId].GlobalTransform = cars[tuple.Value.playerId].GlobalTransform.InterpolateWith(tuple.Value.carTransform, 13.0f * (float)delta);
                cars[tuple.Value.playerId].Steering = tuple.Value.carSteering;

            }

        }  

    }

    private void OnPlayerLoaded(int playerId = 0, bool isLocal = true) {

        LoadPlayer(playerId, isLocal);

        if (GameState.players.Count(element => element.Value.ready) == GameState.players.Count) {
            MultiplayerManager.PlayersReady();
        }
        
    }

    protected override void StartRace() {
        MultiplayerManager.Start();
    }

    protected override void OnPickUpCollected(CarController car) {

        if (Multiplayer.IsServer()) {
            UpdatePlayerPickUp(car.playerId, Pickable.GetRandomPickUp());
        }
        else if (GameState.playerId == car.playerId) {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPickUpCollected", car.playerId);
        }
    
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPickUpCollected(int playerId) {
        
        if (Multiplayer.IsServer()) {
            RpcId(Multiplayer.GetRemoteSenderId(), "UpdatePickUp", playerId, (int)Pickable.GetRandomPickUp());
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdatePickUp(int playerId, Pickable.PickUpType pickUp) {

        UpdatePlayerPickUp(playerId, pickUp);

    }

    protected override void OnPickUpUsed(CarController car)
    {
        if (Multiplayer.IsServer()) {
            RemovePlayerPickUp(car.playerId);
        }
        else if (GameState.playerId == car.playerId) {
            RpcId(MultiplayerManager.SV_PEER_ID, "NotifyPickUpUsed", car.playerId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifyPickUpUsed(int playerId) {
        
        if (Multiplayer.IsServer()) {
            RpcId(Multiplayer.GetRemoteSenderId(), "ActivatePickUp", playerId);
        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ActivatePickUp(int playerId) {

        RemovePlayerPickUp(playerId);

    }

    protected override void OnCheckpointCrossed(CarController car, int checkpointSection) {

        if (car.playerId == GameState.playerId) {

            // Only add current checkpoint if they are crossed in order.
            if (GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap == checkpointSection) {
                MultiplayerManager.CheckpointConfirm(1);
            }

            // Magia en caso de que se decida ir hacia atrás
            else if (GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap > checkpointSection) {
                MultiplayerManager.CheckpointConfirm(-(GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap - checkpointSection));

            }

        }

    }

    private void OnCheckpointConfirm(int playerId, int confirmedCheckpoint) {

        if (playerId == GameState.playerId) {
            // Add lap when crossing first checkpoint of the list (i.e. finish line) AND >>magic<< to add lap only when confirmed checkpoints are coherent with current lap.
            if ((GameState.players[playerId].confirmedCheckpoint - 1) % checkpointsPerLap == 0 && ((GameState.players[playerId].confirmedCheckpoint - 1) / checkpointsPerLap) >= currentLap) {
                OnLapUpdated();
            }

        }

    }

}

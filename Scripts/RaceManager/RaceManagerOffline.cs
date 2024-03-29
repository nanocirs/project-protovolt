public partial class RaceManagerOffline : RaceManagerBase {

    public override void _Ready() {
    
        base._Ready();

        SetupPlayers();
        StartCountdown();

    }

    public override void _PhysicsProcess(double delta) {

        if (!isValidGame || localCar == null) {
            return;
        }

        foreach (var tuple in GameState.players) {
            UpdateCarState(tuple.Key, cars[tuple.Value.playerId].GlobalTransform, cars[tuple.Value.playerId].Steering);
        }  

    }

    private void SetupPlayers() {

        // Player already created on car menu.
        // @TODO: Should they all be created on car menu?
        LoadPlayer(0, true);

        for (int i = 1; i < GameState.maxPlayers; i++) {

            GameState.CreatePlayer(i, GameState.playerName);
            LoadPlayer(i, false);

        }   

    }

    protected override void StartRace() {
        Start();
    }

    protected override void OnPickUpCollected(CarController car) {

        Pickable.PickUpType pickUp = Pickable.GetRandomPickUp();
        UpdatePlayerPickUp(car.playerId, pickUp);

    }

    protected override void OnPickUpUsed(CarController car) {

        PickUpEffect.Activate(car, GameState.players[car.playerId].pickUp);
        RemovePlayerPickUp(car.playerId);
    }

    protected override void OnCheckpointCrossed(CarController car, int checkpointSection) {

        if (GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap == checkpointSection) {
            OnCheckpointConfirm(car.playerId, GameState.players[car.playerId].confirmedCheckpoint + 1);
        }

        // Magia en caso de que se decida ir hacia atrás
        else if (GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap > checkpointSection) {
            OnCheckpointConfirm(car.playerId, GameState.players[car.playerId].confirmedCheckpoint - (GameState.players[car.playerId].confirmedCheckpoint % checkpointsPerLap - checkpointSection)); 
        }

    }

    private void OnCheckpointConfirm(int playerId, int confirmedCheckpoint) {

        GameState.players[playerId].confirmedCheckpoint = confirmedCheckpoint;

        if (playerId == GameState.playerId) {
            // Add lap when crossing first checkpoint of the list (i.e. finish line) AND >>magic<< to add lap only when confirmed checkpoints are coherent with current lap.
            if ((GameState.players[playerId].confirmedCheckpoint - 1) % checkpointsPerLap == 0 && ((GameState.players[playerId].confirmedCheckpoint - 1) / checkpointsPerLap) >= currentLap) {
                OnLapUpdated();
            }

        }

    }

}

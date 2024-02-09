using System.Collections.Generic;
using Godot;

public partial class MapManager : Node {

    [Signal] public delegate void OnCheckpointCrossedEventHandler(int checkpointSection);

    [Export] public int totalLaps { get; private set; } = 3;
    [Export] private PackedScene carScene = null;

    private Node playersNode;
    private Node spawnPointsNode;
    private Node checkpointsNode;

    private Dictionary<int, CarController> cars = new Dictionary<int, CarController>();
    private List<Transform3D> spawnPoints = new List<Transform3D>();
    public List<Checkpoint> checkpoints { get; private set; } = new List<Checkpoint>();

    public int totalCheckpoints { get; private set; } = 0;

    public CarController localCar { get; private set; } = null;

    private bool isValidLevel = true;
    
    public override void _Ready() {

        playersNode = GetNodeOrNull("Players");
        spawnPointsNode = GetNodeOrNull("SpawnPoints");
        checkpointsNode = GetNodeOrNull("Checkpoints");
        
        CheckMapManager();

        if (spawnPointsNode != null) {
            
            foreach (Node3D spawnPoint in spawnPointsNode.GetChildren()) {
                spawnPoints.Add(spawnPoint.GlobalTransform);
            }

        }
        else {
            spawnPoints.Add(new Transform3D());
        }

        if (isValidLevel) {

            foreach (Checkpoint checkpoint in checkpointsNode.GetChildren()) {

                checkpoint.OnCarCrossedCheckpoint += OnCarCrossedCheckpoint;
                checkpoint.checkpointSection = checkpoints.Count;

                checkpoints.Add(checkpoint);

                totalCheckpoints = checkpoints.Count * totalLaps;

            }

            if (MultiplayerManager.connected) {

                MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;

            }
            else {
                OnPlayerLoaded();
            }

        }

    }

    public override void _PhysicsProcess(double delta) {

        if (localCar == null) {
            return;
        }

        if (MultiplayerManager.connected) {

            MultiplayerManager.NotifyPlayerTransform(localCar.GlobalTransform, localCar.Steering);

        }

        foreach (var tuple in GameState.players) {

            if (cars.ContainsKey(tuple.Value.playerId)) {
            
                if (tuple.Value.playerId != localCar.id) {

                    cars[tuple.Value.playerId].GlobalTransform = cars[tuple.Value.playerId].GlobalTransform.InterpolateWith(tuple.Value.carTransform, 13.0f * (float)delta);
                    cars[tuple.Value.playerId].Steering = tuple.Value.carSteering;

                }

            }

        }

    }

    private void OnPlayerLoaded(int peerId = 0, int playerId = 0, bool isLocal = true) {

        if (isValidLevel) {

            if (playerId >= spawnPoints.Count) {
                GD.PrintErr("Not enough spawnpoints. You need at least " + spawnPoints.Count);
                return;
            }

            CarController car = carScene.Instantiate<CarController>();
            playersNode.AddChild(car);

            car.GlobalTransform = spawnPoints[playerId];
            car.SetCarId(playerId);  
            car.SetLocalCar(isLocal);

            if (isLocal) {
                localCar = car;
            }

            cars[playerId] = car;

            if (MultiplayerManager.connected) {

                MultiplayerManager.UpdateCarState(peerId, car.GlobalTransform, car.Steering);

            }

        }

    }

    private void OnCarCrossedCheckpoint(CarController car, int checkpointSection) {
  
        if (car == localCar) {

            EmitSignal(SignalName.OnCheckpointCrossed, checkpointSection);

        }

    }

    public void EnableCar(bool enable) {

        localCar.EnableEngine(enable);

    }

    public int GetCheckpointsPerLap() {
        return checkpoints.Count;
    }

    private void CheckMapManager() {

        if (playersNode == null) {

            isValidLevel = false;
            GD.PrintErr("Map needs a Node called Players.");

        }

        if (checkpointsNode == null) {

            isValidLevel = false;
            GD.PrintErr("Map needs a Node called Checkpoints.");
           
        }

        if (spawnPointsNode == null) {
            
            GD.PushWarning("SpawnPoints not set. Generating a SpawnPoint in origin.");

        }

    }

}

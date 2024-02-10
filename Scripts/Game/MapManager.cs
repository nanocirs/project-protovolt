using System;
using System.Collections.Generic;
using Godot;

public partial class MapManager : Node {

    [Signal] public delegate void OnCheckpointCrossedEventHandler(CarController car, int checkpointSection);

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

                MultiplayerManager.instance.OnPlayerLoaded += LoadPlayer;

            }
            else {
                SetupPlayers();
            }

        }

    }

    public override void _PhysicsProcess(double delta) {

        if (!isValidLevel || localCar == null) {
            return;
        }

        if (MultiplayerManager.connected) {

            MultiplayerManager.SendPlayerTransform(localCar.GlobalTransform, localCar.Steering);

            foreach (var tuple in GameState.players) {

                if (cars.ContainsKey(tuple.Value.playerId) && tuple.Value.playerId != GameState.playerId) {

                    cars[tuple.Value.playerId].GlobalTransform = cars[tuple.Value.playerId].GlobalTransform.InterpolateWith(tuple.Value.carTransform, 13.0f * (float)delta);
                    cars[tuple.Value.playerId].Steering = tuple.Value.carSteering;

                }

            }

        }
        else {
            foreach (var tuple in GameState.players) {
                tuple.Value.carTransform = cars[tuple.Value.playerId].GlobalTransform;
                tuple.Value.carSteering = cars[tuple.Value.playerId].Steering;
            }
        }

    }

    private void SetupPlayers() {

        GameState.players[0] = new PlayerData();
        GameState.players[0].playerId = 0;
        GameState.players[0].playerName = GameState.playerName;

        LoadPlayer(0, 0, true);

        for (int i = 1; i < GameState.maxPlayers; i++) {

            GameState.players[i] = new PlayerData();
            GameState.players[i].playerId = i;
            GameState.players[i].playerName = GameState.playerName;

            LoadPlayer(i, i, false);

        }   

    }

    private void LoadPlayer(int peerId = 0, int playerId = 0, bool isLocal = true) {

        if (playerId >= spawnPoints.Count) {
            GD.PrintErr("Not enough spawnpoints. You need at least " + spawnPoints.Count);
            return;
        }

        CarController car = carScene.Instantiate<CarController>();
        playersNode.AddChild(car);

        car.GlobalTransform = spawnPoints[playerId];
        car.SetLocalCar(isLocal);
        car.playerId = playerId;

        if (isLocal) {
            localCar = car;
        }

        cars[playerId] = car;

        MultiplayerManager.UpdateCarState(playerId, car.GlobalTransform, car.Steering);

    }

    private void OnCarCrossedCheckpoint(CarController car, int checkpointSection) {
  
        EmitSignal(SignalName.OnCheckpointCrossed, car, checkpointSection);

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

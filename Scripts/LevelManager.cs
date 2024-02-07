using System.Collections.Generic;
using Godot;

public partial class LevelManager : Node {

    [Export] private PackedScene carScene = null;

    private Dictionary<int, CarController> cars = new Dictionary<int, CarController>();
    private List<Transform3D> spawnPoints = new List<Transform3D>();

    private Node playersNode;
    private Node spawnPointsNode;

    private CarController myCar = null;

    private bool isValidLevel = true;

    public override void _Ready() {

        playersNode = GetNodeOrNull("Players");
        spawnPointsNode = GetNodeOrNull("SpawnPoints");

        if (spawnPointsNode == null) {
            
            GD.PushWarning("SpawnPoints not set. Generating a SpawnPoint in origin.");

            spawnPoints.Add(new Transform3D());

        }
        else {

            foreach (Node3D spawnPoint in spawnPointsNode.GetChildren()) {
                
                spawnPoints.Add(spawnPoint.GlobalTransform);

            }

        }

        if (playersNode == null) {

            isValidLevel = false;
            GD.PrintErr("Level Scene needs a Node called Players.");

        }

        if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {

            MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;
            MultiplayerManager.NotifyMapLoaded();

        }
        else {
            OnPlayerLoaded(0, 0, true);
        }

    }

    public override void _PhysicsProcess(double delta) {

        if (myCar == null) {
            return;
        }

        if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {

            MultiplayerManager.NotifyPlayerTransform(myCar.GlobalTransform, myCar.Steering);

        }

        foreach (var tuple in MultiplayerManager.players) {

            if (cars.ContainsKey(tuple.Value.playerId)) {
            
                if (tuple.Value.playerId != myCar.id) {

                    cars[tuple.Value.playerId].GlobalTransform = cars[tuple.Value.playerId].GlobalTransform.InterpolateWith(tuple.Value.carTransform, 13.0f * (float)delta);
                    cars[tuple.Value.playerId].Steering = tuple.Value.carSteering;

                }

            }

        }

    }

    private void OnPlayerLoaded(int peerId, int playerId, bool isLocal) {

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
                myCar = car;
            }

            cars[playerId] = car;

            if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {

                MultiplayerManager.UpdateCarState(peerId, car.GlobalTransform, car.Steering);

            }

        }
    }

}

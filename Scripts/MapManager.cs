using System.Collections.Generic;
using Godot;

public partial class MapManager : Node {

    [Signal] public delegate void OnLapUpdatedEventHandler(int currentLap);

    [Export] public int totalLaps = 3;
    [Export] private PackedScene carScene = null;

    private FinishLine finishLine;
    private Node playersNode;
    private Node spawnPointsNode;

    private Dictionary<int, CarController> cars = new Dictionary<int, CarController>();
    private List<Transform3D> spawnPoints = new List<Transform3D>();

    private CarController myCar = null;

    private int myLap = 0;

    private bool isValidLevel = true;
    
    public override void _Ready() {

        finishLine = GetNodeOrNull<FinishLine>("FinishLine");
        playersNode = GetNodeOrNull("Players");
        spawnPointsNode = GetNodeOrNull("SpawnPoints");
        
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

            finishLine.OnCarCrossedFinishLine += OnCarCrossedFinishLine;

            if (MultiplayerManager.connected) {

                MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;
                MultiplayerManager.instance.OnFinishLineCrossed += OnFinishLineCrossed;

            }
            else {

                OnPlayerLoaded();

            }

        }

    }

    public override void _PhysicsProcess(double delta) {

        if (myCar == null) {
            return;
        }

        if (MultiplayerManager.connected) {

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
                myCar = car;
            }

            cars[playerId] = car;

            if (MultiplayerManager.connected) {

                MultiplayerManager.UpdateCarState(peerId, car.GlobalTransform, car.Steering);

            }

        }

    }

    private void OnCarCrossedFinishLine(CarController car) {

        if (car == myCar) {

            if (MultiplayerManager.connected) {
                MultiplayerManager.FinishLineCrossed();
            }
            else {
                OnFinishLineCrossed(car.id);
            }
        }

    }

    private void OnFinishLineCrossed(int playerId) {

        if (playerId == myCar.id) {

            myLap++;

            EmitSignal(SignalName.OnLapUpdated, myLap);

            if (myLap > totalLaps) {

                myCar.EnableEngine(false);

                if (MultiplayerManager.connected) {
                    MultiplayerManager.CarFinished();
                }
            }

        }

    }

    public void EnableCars(bool enable) {

        myCar.EnableEngine(enable);

    }

    private void CheckMapManager() {

        if (finishLine == null) {

            isValidLevel = false;
            GD.PrintErr("Map needs a FinishLine.");

        }

        if (playersNode == null) {

            isValidLevel = false;
            GD.PrintErr("Map needs a Node called Players.");

        }

        if (spawnPointsNode == null) {
            
            GD.PushWarning("SpawnPoints not set. Generating a SpawnPoint in origin.");

        }


    }

}

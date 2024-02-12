using System.Collections.Generic;
using Godot;

public partial class MapManager : Node {

    [Signal] public delegate void OnPickUpConsumedEventHandler(CarController car);
    [Signal] public delegate void OnCheckpointCrossedEventHandler(CarController car, int checkpointSection);

    [Export] public int totalLaps { get; private set; } = 3;

    private Node spawnPointsContainer;
    private Node checkpointsContainer;
    private Node pickablesContainer;

    private List<Transform3D> spawnPoints = new List<Transform3D>();
    public List<Checkpoint> checkpoints { get; private set; } = new List<Checkpoint>();
    public List<Pickable> pickUps { get; private set; } = new List<Pickable>();

    public int totalCheckpoints { get; private set; } = 0;

    private bool isValidMap = true;
    
    public override void _Ready() {

        spawnPointsContainer = GetNodeOrNull("SpawnPoints");
        checkpointsContainer = GetNodeOrNull("Checkpoints");
        pickablesContainer = GetNodeOrNull("Pickables");
        
        if (!IsValidMap()) {
            return;
        }

        if (pickablesContainer != null) {

            foreach (Pickable pickUp in pickablesContainer.GetChildren()) {
                pickUp.OnCarConsumedPickUp += OnCarConsumedPickUp;
            }

        }

        if (spawnPointsContainer != null) {

            foreach (Node3D spawnPoint in spawnPointsContainer.GetChildren()) {
                spawnPoints.Add(spawnPoint.GlobalTransform);
            }

        }
        else {
            spawnPoints.Add(new Transform3D());
        }

        foreach (Checkpoint checkpoint in checkpointsContainer.GetChildren()) {

            checkpoint.OnCarCrossedCheckpoint += OnCarCrossedCheckpoint;
            checkpoint.checkpointSection = GetCheckpointsPerLap();

            checkpoints.Add(checkpoint);

            totalCheckpoints = GetCheckpointsPerLap() * totalLaps;

        }

    }

    private void OnCarConsumedPickUp(CarController car) {
        EmitSignal(SignalName.OnPickUpConsumed, car);
    }

    private void OnCarCrossedCheckpoint(CarController car, int checkpointSection) {
        EmitSignal(SignalName.OnCheckpointCrossed, car, checkpointSection);
    }

    public int GetCheckpointsPerLap() {
        return checkpoints.Count;
    }

    public List<Transform3D> GetSpawnPoints() {
        return spawnPoints;
    }

    private bool IsValidMap() {

        if (checkpointsContainer == null) {
            isValidMap = false;
            GD.PrintErr("Map needs a Node called Checkpoints.");   
        }

        if (pickablesContainer == null) {
            GD.PushWarning("Pickables Node not set.");
        }

        if (spawnPointsContainer == null) {
            GD.PushWarning("SpawnPoints Node not set. Generating a SpawnPoint in origin.");
        }

        return isValidMap;

    }

}

using System.Collections.Generic;
using Godot;

public partial class MapManager : Node {

    [Signal] public delegate void OnCheckpointCrossedEventHandler(CarController car, int checkpointSection);

    [Export] public int totalLaps { get; private set; } = 3;

    private Node spawnPointsNode;
    private Node checkpointsNode;

    private List<Transform3D> spawnPoints = new List<Transform3D>();
    public List<Checkpoint> checkpoints { get; private set; } = new List<Checkpoint>();

    public int totalCheckpoints { get; private set; } = 0;

    private bool isValidLevel = true;
    
    public override void _Ready() {

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

        }

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

    private void CheckMapManager() {

        if (checkpointsNode == null) {

            isValidLevel = false;
            GD.PrintErr("Map needs a Node called Checkpoints.");
           
        }

        if (spawnPointsNode == null) {
            
            GD.PushWarning("SpawnPoints not set. Generating a SpawnPoint in origin.");

        }

    }

}

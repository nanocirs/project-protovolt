using System.Runtime.CompilerServices;
using Godot;

public partial class Checkpoint : Node3D {

    [Signal] public delegate void OnCarCrossedCheckpointEventHandler(CarController car, int section);

    private Area3D areaCheckpoint = null;

    private bool isValidCheckpoint = true;

    public int checkpointSection = 0;

    public override void _Ready() {
        
        areaCheckpoint = GetNodeOrNull<Area3D>("CheckpointArea");

        if (areaCheckpoint == null) {

            isValidCheckpoint = false;
            GD.PrintErr("Checkpoint needs an Area3D (CheckpointArea)");

        }

    }

    private void OnCarEnteredCheckpoint(Node3D car) {

        EmitSignal(SignalName.OnCarCrossedCheckpoint, car as CarController, checkpointSection);

    }

}

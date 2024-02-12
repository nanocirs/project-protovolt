using Godot;

public partial class Checkpoint : Node3D {

    [Signal] public delegate void OnCarCrossedCheckpointEventHandler(CarController car, int section);

    private Area3D areaCheckpoint = null;

    private bool isValidCheckpoint = true;

    public int checkpointSection = 0;

    public Plane plane;

    public override void _Ready() {
        
        areaCheckpoint = GetNodeOrNull<Area3D>("CheckpointArea");

        if (areaCheckpoint == null) {

            isValidCheckpoint = false;
            GD.PrintErr("Checkpoint needs an Area3D (CheckpointArea)");

        }

        plane = CalculatePlane();
        
    }

    private void OnCarEnteredCheckpoint(Node3D car) {

        EmitSignal(SignalName.OnCarCrossedCheckpoint, car as CarController, checkpointSection);

    }

    private Plane CalculatePlane() {

        Vector3 p1 = GlobalPosition;
        Vector3 p2 = GlobalPosition + new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 p3 = GlobalPosition + new Vector3(-Mathf.Cos(GlobalRotation.Y), 0.0f, Mathf.Sin(GlobalRotation.Y));

        return new Plane(p1, p2, p3);
    }

}

using Godot;

[GlobalClass]
public partial class Car : Resource {

    [ExportGroup("Vehicle settings")]
    [Export] public float steerLimit = 15.0f;
    [Export] public float maxRpm = 5000.0f;
    [Export] public float maxTorque = 720.0f;
    [Export] public float mass = 120.0f;
    [Export] public CarController.Traction traction = CarController.Traction.RWD;

}

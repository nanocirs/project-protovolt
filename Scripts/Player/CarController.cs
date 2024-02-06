using Godot;

public partial class CarController : VehicleBody3D
{
    [ExportGroup("Vehicle settings")]
    [Export] float steerLimit = 30.0f;
    [Export] float maxRpm = 500.0f;
    [Export] float maxTorque = 200.0f;

    [ExportGroup("Wheels")]
    [Export] VehicleWheel3D wheelBackLeft = null;
    [Export] VehicleWheel3D wheelBackRight = null;
    [Export] VehicleWheel3D wheelFrontLeft = null;
    [Export] VehicleWheel3D wheelFrontRight = null;

    [ExportGroup("Camera")]
    [Export] Camera3D camera = null;

    private bool isAcceleratePressed = false;
    private bool isDeceleratePressed = false;
    private bool isTurnLeftPressed = false;
    private bool isTurnRightPressed = false;

    private bool isLocalCar = false;
    private bool isValidCar = true;

    public override void _Ready() {

        InputManager.instance.OnUpInput += (bool isPressed) => isAcceleratePressed = isPressed;
        InputManager.instance.OnDownInput += (bool isPressed) => isDeceleratePressed = isPressed;
        InputManager.instance.OnLeftInput += (bool isPressed) => isTurnLeftPressed = isPressed;
        InputManager.instance.OnRightInput += (bool isPressed) => isTurnRightPressed = isPressed;

        camera.Current = false;

        if (wheelBackLeft == null || wheelBackRight == null || wheelFrontLeft == null || wheelFrontRight == null) {

            isValidCar = false;

            GD.PrintErr("There are unassigned wheels in Car");

        }

    }

    public override void _PhysicsProcess(double delta) {
        
        if (!isValidCar) {
            return;
        }

        if (isLocalCar) {

            Steering = Mathf.Lerp(Steering, Input.GetAxis("right", "left") * steerLimit * 2 * Mathf.Pi / 360.0f, 5.0f * (float)delta);
            
            float acceleration = Input.GetAxis("down", "up");

            float rpm = wheelBackLeft.GetRpm();
            wheelBackLeft.EngineForce = acceleration * maxTorque * (1.0f - rpm / maxRpm);

            rpm = wheelBackRight.GetRpm();
            wheelBackRight.EngineForce = acceleration * maxTorque * (1.0f - rpm / maxRpm);

        }
    }

    public void SetLocalCar(bool isLocal) {
        isLocalCar = isLocal;
        camera.Current = isLocal;

    }
}

using Godot;

public partial class CarController : VehicleBody3D
{
    [Signal] public delegate void OnCarPressedUseEventHandler(CarController car);

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

    public int playerId = -1;

    private bool canRace = false;
    private bool isLocalCar = false;
    private bool isValidCar = true;

    private bool isAcceleratePressed = false;
    private bool isBrakePressed = false;
    private bool isSteerLeftPressed = false;
    private bool isSteerRightPressed = false;

    public override void _Ready() {

        camera.Current = false;

        if (wheelBackLeft == null || wheelBackRight == null || wheelFrontLeft == null || wheelFrontRight == null) {

            isValidCar = false;

            GD.PrintErr("There are unassigned wheels in Car");

            return;

        }

        InputManager.instance.OnUpInput += (value) => isAcceleratePressed = value;
        InputManager.instance.OnDownInput += (value) => isBrakePressed = value;
        InputManager.instance.OnLeftInput += (value) => isSteerLeftPressed = value;
        InputManager.instance.OnRightInput += (value) => isSteerRightPressed = value;
        InputManager.instance.OnUseInput += (value) => EmitSignal(SignalName.OnCarPressedUse, this);

    }

    public override void _PhysicsProcess(double delta) {
        
        if (!isValidCar || !isLocalCar) {
            return;
        }
        
        float steeringAxis = isSteerLeftPressed || isSteerRightPressed ? Input.GetAxis("right", "left") : 0.0f;
        float accelerateAxis = isAcceleratePressed || isBrakePressed ? Input.GetAxis("down", "up") : 0.0f;

        Steering = Mathf.Lerp(Steering, steeringAxis * steerLimit * 2 * Mathf.Pi / 360.0f, 5.0f * (float)delta);
        float acceleration = canRace ? accelerateAxis : 0.0f;

        float rpm = wheelBackLeft.GetRpm();
        wheelBackLeft.EngineForce = acceleration * maxTorque * (1.0f - rpm / maxRpm);

        rpm = wheelBackRight.GetRpm();
        wheelBackRight.EngineForce = acceleration * maxTorque * (1.0f - rpm / maxRpm);

    }

    public void SetLocalCar(bool isLocal) {
        isLocalCar = isLocal;
        camera.Current = isLocal;

    }

    public void EnableEngine(bool enable) {
        canRace = enable;
    }

    public async void EffectTurbo() {
        
        maxRpm *= 5.0f;
        maxTorque *= 3.0f;
        Mass *= 3.0f;

        await ToSignal(GetTree().CreateTimer(PickUpEffect.turboDuration), SceneTreeTimer.SignalName.Timeout);

        maxRpm /= 5.0f;
        maxTorque /= 3.0f;
        Mass /= 3.0f;
    }

    public void EffectOil() {}

    public void EffectStickerLaugh() {}

}

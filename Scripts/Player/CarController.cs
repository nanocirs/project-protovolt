using Godot;

public partial class CarController : VehicleBody3D {

    public enum Traction {
        RWD = 0,
        FWD = 1,
        AWD = 2,
    }

    [Signal] public delegate void OnCarPressedUseEventHandler(CarController car);

    private float steerLimit = 30.0f;
    private float maxRpm = 500.0f;
    private float maxTorque = 200.0f;

    VehicleWheel3D wheelBackLeft = null;
    VehicleWheel3D wheelBackRight = null;
    VehicleWheel3D wheelFrontLeft = null;
    VehicleWheel3D wheelFrontRight = null;

    Camera3D camera = null;

    public int playerId = -1;

    private bool canRace = false;
    private bool isLocalCar = false;
    private bool isValidCar = true;

    private bool isAcceleratePressed = false;
    private bool isBrakePressed = false;
    private bool isSteerLeftPressed = false;
    private bool isSteerRightPressed = false;

    public override void _Ready() {

        wheelBackLeft = GetNodeOrNull<VehicleWheel3D>("WheelBL");
        wheelBackRight = GetNodeOrNull<VehicleWheel3D>("WheelBR");
        wheelFrontLeft = GetNodeOrNull<VehicleWheel3D>("WheelFL");
        wheelFrontRight = GetNodeOrNull<VehicleWheel3D>("WheelFR");
        camera = GetNodeOrNull<Camera3D>("Camera3D");
        
        if (!IsValidCar()) {
            return;
        }

        camera?.ClearCurrent();

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

    public void SetCarBrand(string brandPath = "res://Resources/Cars/CarBase.tres") {
        
        if (!ResourceLoader.Exists(brandPath)) {
            brandPath = "res://Resources/Cars/CarBase.tres";
            GD.PushWarning("Invalid path to car brand. Using the default one.");
        }

        Car car = ResourceLoader.Load<Car>(brandPath);

        steerLimit = car.steerLimit;
        maxRpm = car.maxRpm;
        maxTorque = car.maxTorque;
        Mass = car.mass;

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
    
    public void SetLocalCar(bool isLocal) {

        isLocalCar = isLocal;

        if (isLocal) {
            InputManager.instance.OnUpInput += OnAccelerateInput;
            InputManager.instance.OnDownInput += OnBrakeInput;
            InputManager.instance.OnLeftInput += OnSteerLeftInput;
            InputManager.instance.OnRightInput += OnSteerRightInput;
            InputManager.instance.OnUseInput += OnUseInput;
            camera?.MakeCurrent();
        }
        else {
            InputManager.instance.OnUpInput -= OnAccelerateInput;
            InputManager.instance.OnDownInput -= OnBrakeInput;
            InputManager.instance.OnLeftInput -= OnSteerLeftInput;
            InputManager.instance.OnRightInput -= OnSteerRightInput;
            InputManager.instance.OnUseInput -= OnUseInput;
            camera?.ClearCurrent();
        }

    }

    private void OnAccelerateInput(bool value)  { isAcceleratePressed = value; }
    private void OnBrakeInput(bool value)       { isBrakePressed = value; }
    private void OnSteerLeftInput(bool value)   { isSteerLeftPressed = value; }
    private void OnSteerRightInput(bool value)  { isSteerRightPressed = value; }
    private void OnUseInput(bool value)         { EmitSignal(SignalName.OnCarPressedUse, this); }

    public bool IsValidCar() {
        
        if (wheelBackLeft == null) {
            isValidCar = false;
            GD.PrintErr("Unassigned VehicleWheel3D in Car (WheelBL).");
        }

        if (wheelBackRight == null) {
            isValidCar = false;
            GD.PrintErr("Unassigned VehicleWheel3D in Car (WheelBR).");
        }

        if (wheelFrontLeft == null) {
            isValidCar = false;
            GD.PrintErr("Unassigned VehicleWheel3D in Car (WheelFL).");
        }

        if (wheelFrontRight == null) {
            isValidCar = false;
            GD.PrintErr("Unassigned VehicleWheel3D in Car (WheelFR).");
        }

        if (camera == null) {
            GD.PushWarning("Unassigned Camera3D in Car (Camera3D).");
        }

        return isValidCar;

    }

}

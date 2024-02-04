using Godot;

public partial class InputManager : Singleton<InputManager> {

    [Signal] public delegate void OnLeftInputEventHandler(bool isPressed);
    [Signal] public delegate void OnRightInputEventHandler(bool isPressed);
    [Signal] public delegate void OnUpInputEventHandler(bool isPressed);
    [Signal] public delegate void OnDownInputEventHandler(bool isPressed);
    [Signal] public delegate void OnEscInputEventHandler(bool isPressed);

    public override void _Input(InputEvent @event) {
 
        if (@event.IsActionPressed("left"))         EmitSignal(SignalName.OnLeftInput, true);
        else if (@event.IsActionReleased("left"))   EmitSignal(SignalName.OnLeftInput, false);

        if (@event.IsActionPressed("right"))        EmitSignal(SignalName.OnRightInput, true);
        else if (@event.IsActionReleased("right"))  EmitSignal(SignalName.OnRightInput, false);

        if (@event.IsActionPressed("up"))           EmitSignal(SignalName.OnUpInput, true);
        else if (@event.IsActionReleased("up"))     EmitSignal(SignalName.OnUpInput, false);

        if (@event.IsActionPressed("down"))         EmitSignal(SignalName.OnDownInput, true);
        else if (@event.IsActionReleased("down"))   EmitSignal(SignalName.OnDownInput, false);

        if (@event.IsActionPressed("esc"))          EmitSignal(SignalName.OnEscInput, true);
        else if (@event.IsActionPressed("esc"))     EmitSignal(SignalName.OnEscInput, false);

    }
}

using Godot;

public partial class CarMenuOnline : CarMenuBase {

    public float timeToSelect = 20.0f;

    Timer selectionTimer = new Timer();

    public override void _Ready() {
        base._Ready();

        selectionTimer.WaitTime = timeToSelect;
        selectionTimer.OneShot = true;
        selectionTimer.Timeout += ConfirmCarSelection;
        AddChild(selectionTimer);

        selectionTimer.Start();

    }

    protected override void ConfirmCarSelection() {
        
        selectionTimer.QueueFree();

        if (carPath == "") {

            int totalCars = buttonGroup.GetButtons().Count;
            int randomCar = GD.RandRange(0, totalCars - 1);
            
            carPath = buttonGroup.GetButtons()[randomCar].GetMeta("carPath").AsString();
            
        }

        GameState.players[GameState.playerId].carPath = carPath;

        // @TODO: Networkear.
    }

}

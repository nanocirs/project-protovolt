using Godot;

public partial class CarMenuOnline : CanvasLayer {

    [Export] private ButtonGroup buttonGroup = null;

    protected string carPath = "";

    private Timer selectionTimer = null;
    private float timeToSelect = 5.0f;

    private int carsNotified = 0;

    public override void _Ready() {
                
        if (buttonGroup == null) {
            GD.PrintErr("CarMenuOnline doesn't have a valid ButtonGroup.");
            return;
        }
        else {
            buttonGroup.Pressed += SelectedCar;
        }

        if (Multiplayer.IsServer()) {

            selectionTimer = new Timer();
            selectionTimer.WaitTime = timeToSelect;
            selectionTimer.OneShot = true;
        
            AddChild(selectionTimer);

            selectionTimer.Timeout += RequestSelectedCars;

        }

        MultiplayerManager.instance.OnSelectedCarsRequested += SendSelectedCar;

    }

    public void Load() {
        selectionTimer.Start();
        Show();
    }
    
    private void SendSelectedCar() {
        
        if (carPath == "") {
            carPath = GetRandomCarPath();
        }

        MultiplayerManager.SendSelectedCar(carPath);
        
    }

    private void RequestSelectedCars() {
        MultiplayerManager.RequestSelectedCars();
    }

    private void SelectedCar(BaseButton button) {
        carPath = button.GetMeta("carPath").AsString();
    }

    private string GetRandomCarPath() {

        int totalCars = buttonGroup.GetButtons().Count;
        int randomCar = GD.RandRange(0, totalCars - 1);
            
        return buttonGroup.GetButtons()[randomCar].GetMeta("carPath").AsString();

    }

}

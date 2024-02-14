using Godot;

public partial class CarMenuLoader : CanvasLayer {

    CarMenuBase carMenu = null;

    public void Load() {

        carMenu = MultiplayerManager.connected ? new CarMenuOnline() : new CarMenuOffline();
        carMenu.buttonGroup = ResourceLoader.Load<ButtonGroup>("res://Resources/ButtonGroups/CarSelection.tres");
        carMenu.confirmButton = GetNodeOrNull<Button>("Confirm");
        AddChild(carMenu);

        Show();
    }

}

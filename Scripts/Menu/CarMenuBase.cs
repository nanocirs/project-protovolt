using Godot;

public abstract partial class CarMenuBase : CanvasLayer {

    public Button confirmButton = null;
    public ButtonGroup buttonGroup = null;
    
    protected string carPath = "";

    protected abstract void ConfirmCarSelection();

    public override void _Ready() {

        if (confirmButton == null) {
            GD.PrintErr("CarMenu doesn't have a valid Confirm button.");
            return;
        }
        else {
            confirmButton.Hide();
            confirmButton.Pressed += ConfirmCarSelection;
        }

        buttonGroup.Pressed += SelectedCar;

    }

    private void SelectedCar(BaseButton button) {
        carPath = button.GetMeta("carPath").AsString();
        confirmButton.Show();
    }

}

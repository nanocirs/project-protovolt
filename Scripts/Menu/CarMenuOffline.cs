using Godot;

public partial class CarMenuOffline : CanvasLayer {

    [Export] private Button confirmButton = null;
    [Export] private ButtonGroup buttonGroup = null;

    private string carPath = "";

    public override void _Ready() {

        if (buttonGroup != null) {
            buttonGroup.Pressed += SelectedCar;
        }
        else {
            GD.PrintErr("CarMenuOffline doesn't have a valid ButtonGroup.");
            return;
        }

        if (confirmButton != null) {
            confirmButton.Hide();
            confirmButton.Pressed += ConfirmCarSelection;
        }
        else {
            GD.PrintErr("CarMenuOffline doesn't have a valid Confirm button.");
            return;
        }

    }

    private void SelectedCar(BaseButton button) {
        carPath = button.GetMeta("carPath").AsString();
        confirmButton.Show();
    }

    private void ConfirmCarSelection() {

        int playerId = 0;

        GameState.playerId = playerId;

        GameState.CreatePlayer(playerId, GameState.playerName);
        GameState.players[playerId].carPath = carPath;

		GameStateMachine.instance.LoadScene("Maps/Game.tscn");

    }

    private string GetRandomCarPath() {

        int totalCars = buttonGroup.GetButtons().Count;
        int randomCar = GD.RandRange(0, totalCars - 1);
            
        return buttonGroup.GetButtons()[randomCar].GetMeta("carPath").AsString();

    }

}

using Godot;

public partial class GameUI : Node {
    
    private Label countdownLabel = null;

    private bool isValidHud = true;
    
    private float countdownTimer = 0;
    
    public override void _Ready() {
        
        countdownLabel = GetNodeOrNull<Label>("Countdown");

        if (countdownLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Countdown.");

        }

        if (isValidHud) {
            countdownLabel.Hide();
            countdownLabel.Text = "";
        }

    }

    public async void StartCountdown() {

        countdownLabel.Show();

        countdownLabel.Text = "3";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        countdownLabel.Text = "2";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        countdownLabel.Text = "1";

    }

    public async void EndCountdown() {

        countdownLabel.Text = "GO";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        
        countdownLabel.Text = "";
        countdownLabel.Hide();
        
    }
}

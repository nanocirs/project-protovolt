using Godot;

public partial class GameUI : CanvasLayer {
    
    private Label countdownLabel = null;
    private Label lapsLabel = null;

    private bool isValidHud = true;
    
    private float countdownTimer = 0;

    public int totalLaps = 0;
    
    public override void _Ready() {
        
        countdownLabel = GetNodeOrNull<Label>("Countdown");
        lapsLabel = GetNodeOrNull<Label>("Laps");

        CheckHud();

        if (isValidHud) {
            countdownLabel.Hide();
            countdownLabel.Text = "";
            lapsLabel.Text = "Lap 0/" + totalLaps;
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

    public void UpdateLap(int currentLap) {
        lapsLabel.Text = "Lap " + Mathf.Clamp(currentLap, 0, totalLaps) + "/" + totalLaps;
    }

    public void EnableScoreboard() {
        
    }

    private void CheckHud() {

        if (countdownLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Countdown.");

        }

        if (lapsLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Laps.");

        }

    }

}

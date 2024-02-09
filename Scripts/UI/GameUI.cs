using Godot;

public partial class GameUI : CanvasLayer {
    
    [Export] private PackedScene playerNameRowScene = null;

    private Panel scoreboard = null;
    private VBoxContainer scoreContainer = null;
    private Label countdownLabel = null;
    private Label lapsLabel = null;
    private Label positionLabel = null;

    private bool isValidHud = true;

    public int totalLaps = 0;
    private int scorePosition = 1;
    private float countdownTimer = 0;
    
    public override void _Ready() {

        scoreboard = GetNodeOrNull<Panel>("Scoreboard");
        scoreContainer = GetNodeOrNull<VBoxContainer>("Scoreboard/VBoxContainer/ScoreContainer");
        countdownLabel = GetNodeOrNull<Label>("Countdown");
        lapsLabel = GetNodeOrNull<Label>("Laps");
        positionLabel = GetNodeOrNull<Label>("Position");

        CheckHud();

        if (isValidHud) {
            scoreboard.Hide();

            countdownLabel.Hide();
            countdownLabel.Text = "";

            lapsLabel.Text = "Lap 0/" + totalLaps;

            positionLabel.Show();
            positionLabel.Text = "";

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
        scoreboard.Show();
    }

    public void AddScore(string name, float time) {

        //string formattedTime = string.Format("{0:00}:{1:00.000}", (int)(time / 60), time % 60);
        string formattedTime = string.Format("{0:00}:{1:00}.{2:000}", (int)(time / 60), (int)(time % 60), (int)((time - Mathf.Floor(time)) * 1000));

        Label score = playerNameRowScene.Instantiate<Label>();
        score.Text = scorePosition + ". " + name + "    " + formattedTime;

        scoreContainer.AddChild(score);

        scorePosition++;

    }

    public void SetPosition(int position) {
        positionLabel.Text = position.ToString();
    }

    private void CheckHud() {

        if (scoreboard == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Panel called Scoreboard.");

        }

        if (scoreContainer == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a VBoxContainer : Scoreboard/VBoxContainer/ScoreContainer.");

        }

        if (countdownLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Countdown.");

        }

        if (lapsLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Laps.");

        }

        if (positionLabel == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs a Label called Position.");

        }

        if (playerNameRowScene == null) {

            isValidHud = false;
            GD.PrintErr("Hud needs to set up a PlayerNameRow Scene.");

        }

    }

}

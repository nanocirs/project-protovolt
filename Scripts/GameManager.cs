using Godot;

public partial class GameManager : Node {

    [Signal] public delegate void OnStartedCountdownEventHandler();

    [Export] private bool countdownEnabled = true;

    private const float COUNTDOWN_TIME = 3.0f;

    private Node uiNode;
    private MapManager mapManager;

    private float currentTime = 0;
    private int currentLap = 0;

    private bool isRaceStarted = false;

    private bool isValidGame = true;

    public override void _Ready() {
        
        uiNode = GetNodeOrNull("UI");
        mapManager = GetNodeOrNull<MapManager>("Map");

        if (uiNode == null) {

            isValidGame = false;
            GD.PrintErr("Game Scene needs a Node called UI.");

        }

        if (mapManager == null) {

            isValidGame = false;
            GD.PrintErr("Game Scene needs a Node called Map.");

        }

        if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {

            MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;
            MultiplayerManager.instance.OnPlayersReady += StartCountdown;
            MultiplayerManager.instance.OnCountdownEnded += OnCountdownEnded;

            MultiplayerManager.NotifyMapLoaded();

        }
        else {
            OnPlayerLoaded();
        }

    }

    public override void _Process(double delta) {

        if (isRaceStarted) {
            currentTime += (float)delta;
        }       
            
    }

    private void OnPlayerLoaded(int peerId = 0, int playerId = 0, bool isLocal = true) {

        if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {

            if (playerId == MultiplayerManager.players.Count - 1) {
                MultiplayerManager.PlayersReady();
            }

        }
        else {

            StartCountdown();

        }

    }

    private async void StartCountdown() {
        
        if (countdownEnabled) {

            EmitSignal(SignalName.OnStartedCountdown);

            await ToSignal(GetTree().CreateTimer(COUNTDOWN_TIME), SceneTreeTimer.SignalName.Timeout);

            if (MultiplayerManager.connectionStatus == MultiplayerManager.ConnectionStatus.Connected) {
                MultiplayerManager.EndCountdown();
            }
            else {
                OnCountdownEnded();
            }

        }
        else {

            mapManager.EnableCar(true);

        }

    }

    private void OnCountdownEnded() {

        mapManager.EnableCar(true);

    }

}

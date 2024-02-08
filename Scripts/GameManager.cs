using Godot;

public partial class GameManager : Node {

    [Export] private bool countdownEnabled = true;

    private const float COUNTDOWN_TIME = 3.0f;

    private GameUI hud;
    private MapManager mapManager;

    private float currentTime = 0;
    private int currentLap = 0;

    private bool isRaceStarted = false;

    private bool isValidGame = true;

    public override void _Ready() {
        
        hud = GetNodeOrNull<GameUI>("UI");
        mapManager = GetNodeOrNull<MapManager>("Map");

        if (hud == null) {

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
            
            hud.StartCountdown();

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

        hud.EndCountdown();
        mapManager.EnableCar(true);

    }

}

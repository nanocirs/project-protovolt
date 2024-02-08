using Godot;

public partial class GameManager : Node {

    [Export] private bool countdownEnabled = true;

    private const float COUNTDOWN_TIME = 3.0f;

    private GameUI hud;
    private MapManager mapManager;

    private float currentTime = 0;

    private bool isRaceStarted = false;
    private bool isValidGame = true;

    public override void _Ready() {
        
        hud = GetNodeOrNull<GameUI>("UI");
        mapManager = GetNodeOrNull<MapManager>("Map");

        CheckGameManager();

        hud.totalLaps = mapManager.totalLaps;
        hud.CallDeferred("UpdateLap", 0);
        

        mapManager.OnLapUpdated += OnLapUpdated;

        if (MultiplayerManager.connected) {

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

        if (MultiplayerManager.connected) {

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

            if (MultiplayerManager.connected) {
                MultiplayerManager.EndCountdown();
            }
            else {
                OnCountdownEnded();
            }

        }
        else {
            mapManager.EnableCars(true);
        }

    }

    private void OnCountdownEnded() {

        hud.EndCountdown();
        mapManager.EnableCars(true);

    }

    private void OnLapUpdated(int currentLap) {

        hud.UpdateLap(currentLap);

    }

    private void CheckGameManager() {

        if (hud == null) {

            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called UI.");

        }

        if (mapManager == null) {

            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called Map.");

        }

    }

}

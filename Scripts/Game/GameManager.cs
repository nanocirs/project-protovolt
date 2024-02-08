using Godot;

public partial class GameManager : Node {

    [Export] private bool countdownEnabled = true;

    private const float COUNTDOWN_TIME = 3.0f;

    private GameUI hud;
    private MapManager mapManager;

    private int currentPosition = 0;
    private int currentLap = 0;
    private int currentCheckpoint = 0;
    private float currentTime = 0.0f;

    private bool isRaceStarted = false;
    private bool isValidGame = true;

    // @TODO: Hay que sacar esto de aquí
    private string disconnected_name = "Client";

    public override void _Ready() {
        
        hud = GetNodeOrNull<GameUI>("UI");
        mapManager = GetNodeOrNull<MapManager>("Map");

        CheckGameManager();

        if (isValidGame) {

            hud.totalLaps = mapManager.totalLaps;
            hud.UpdateLap(0);

            mapManager.OnCheckpointCrossed += OnCheckpointCrossed;


            if (MultiplayerManager.connected) {

                MultiplayerManager.instance.OnPlayerLoaded += OnPlayerLoaded;
                MultiplayerManager.instance.OnPlayersReady += StartCountdown;
                MultiplayerManager.instance.OnCountdownEnded += OnCountdownEnded;
                MultiplayerManager.instance.OnCarFinished += OnCarFinished;
                MultiplayerManager.instance.OnCheckpointConfirm += OnCheckpointConfirm;

                MultiplayerManager.NotifyMapLoaded();

            }
            else {
                OnPlayerLoaded();
            }

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
            OnCountdownEnded();
        }

    }

    private void OnCountdownEnded() {

        hud.EndCountdown();
        mapManager.EnableCar(true);
        
        isRaceStarted = true;

    }

    private void OnLapUpdated() {

        currentLap++;

        hud.UpdateLap(currentLap);

        if (currentLap > mapManager.totalLaps) {

            mapManager.EnableCar(false);
            isRaceStarted = false;

            if (MultiplayerManager.connected) {
                MultiplayerManager.CarFinished(currentTime);
            }
            else {
                OnCarFinished(mapManager.localCar.id, disconnected_name, currentTime);
            }
        }

    }

    private void OnCheckpointCrossed(int checkpointSection) {

        // Only add current checkpoint if they are crossed in order.
        if (currentCheckpoint % mapManager.GetCheckpointsPerLap() == checkpointSection) {

            if (MultiplayerManager.connected) {
                MultiplayerManager.CheckpointConfirm();
            }
            else {
                OnCheckpointConfirm(currentCheckpoint + 1);
            }
        
        }

    }

    private void OnCheckpointConfirm(int confirmedCheckpoint) {

        // Add lap when crossing first checkpoint of the list (i.e. finish line)      
        if (currentCheckpoint % mapManager.GetCheckpointsPerLap() == 0) {

            OnLapUpdated();

        }

        currentCheckpoint = confirmedCheckpoint;

    }

    private void OnCarFinished(int playerId, string name, float raceTime) {

        hud.AddScore(name, raceTime);

        if (playerId == mapManager.localCar.id) {

            hud.EnableScoreboard();

        }

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

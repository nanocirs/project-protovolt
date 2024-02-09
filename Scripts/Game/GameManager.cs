using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameManager : Node {

    [Export(PropertyHint.Range, "1,12")] private const int maxPlayers = 12;
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
    private string offline_name = "Client";

    private List<PlayerData> playersByPosition = new List<PlayerData>();

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

        if (!isValidGame) {
            return;
        }

        if (isRaceStarted) {
            currentTime += (float)delta;

            hud.SetPosition(GetRacePosition());

        }

    }

    private void OnPlayerLoaded(int peerId = 0, int playerId = 0, bool isLocal = true) {

        if (MultiplayerManager.connected) {

            if (playerId == GameState.players.Count - 1) {
                MultiplayerManager.PlayersReady();
            }

        }
        else {
            StartCountdown();
        }
        
    }

    private async void StartCountdown() {

        hud.SetPosition(GetRacePosition());

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
                OnCarFinished(GameState.playerId, offline_name, currentTime);
            }
        }

    }

    private void OnCheckpointCrossed(CarController car, int checkpointSection) {

        if (MultiplayerManager.connected) {

            if (car.playerId == GameState.playerId) {
                // Only add current checkpoint if they are crossed in order.
                if (currentCheckpoint % mapManager.GetCheckpointsPerLap() == checkpointSection) {
                    MultiplayerManager.CheckpointConfirm();
                }

            }

        }
        else {
            if (GameState.players[car.playerId].currentCheckpoint % mapManager.GetCheckpointsPerLap() == checkpointSection) {
                OnCheckpointConfirm(car.playerId, GameState.players[car.playerId].currentCheckpoint + 1);
            }
        }

    }

    private void OnCheckpointConfirm(int playerId, int confirmedCheckpoint) {

        if (!MultiplayerManager.connected) {

            GameState.players[playerId].currentCheckpoint = confirmedCheckpoint;

        }

        // @TODO: rework to move currentCheckpoint to PlayerData players.
        currentCheckpoint = confirmedCheckpoint;

        if (playerId == GameState.playerId) {

            // Add lap when crossing first checkpoint of the list (i.e. finish line)      
            if ((confirmedCheckpoint - 1) % mapManager.GetCheckpointsPerLap() == 0) {
                OnLapUpdated();
            }

        }

    }

    private int GetRacePosition() {

        playersByPosition = OrderPlayersByPosition();

        for (int i = 0; i < playersByPosition.Count; i++) {

            if (playersByPosition[i].playerId == GameState.playerId) {
                return i + 1;
            }

        }

        return 0;

    }

    private List<PlayerData> OrderPlayersByPosition() {

        return GameState.players.Values.OrderByDescending(player => player.currentCheckpoint)
                                       .ThenBy(player => CalculateDistanceToNextCheckpoint(player.carTransform.Origin, player.currentCheckpoint)).ToList();

    }

    private float CalculateDistanceToNextCheckpoint(Vector3 position, int currentCheckpoint) {
        
        int sectionNumber = currentCheckpoint % mapManager.GetCheckpointsPerLap();

        return mapManager.checkpoints[sectionNumber].plane.DistanceTo(position);

    }

    private void OnCarFinished(int playerId, string name, float raceTime) {

        hud.AddScore(name, raceTime);

        if (playerId == GameState.playerId) {

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

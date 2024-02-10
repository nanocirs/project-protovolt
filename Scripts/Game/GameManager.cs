using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameManager : Node {

    [Export(PropertyHint.Range, "1,12")] private const int maxPlayers = 12;
    [Export] private bool countdownEnabled = true;
    [Export] private PackedScene carScene = null;

    private const float COUNTDOWN_TIME = 3.0f;

    private GameUI hud;
    private MapManager mapManager;
    private Node playersNode;

    public CarController localCar { get; private set; } = null;

    private int currentPosition = 0;
    private int currentLap = 0;

    private float currentTime = 0.0f;

    private bool isRaceStarted = false;
    private bool isValidGame = true;

    private Dictionary<int, CarController> cars = new Dictionary<int, CarController>();
    private List<PlayerData> playersByPosition = new List<PlayerData>();

    public override void _Ready() {
        
        hud = GetNodeOrNull<GameUI>("UI");
        mapManager = GetNodeOrNull<MapManager>("Map");
        playersNode = GetNodeOrNull("Players");

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

    public override void _PhysicsProcess(double delta) {

        if (localCar == null) {
            return;
        }

        if (MultiplayerManager.connected) {

            MultiplayerManager.SendPlayerTransform(localCar.GlobalTransform, localCar.Steering);

            foreach (var tuple in GameState.players) {

                if (cars.ContainsKey(tuple.Value.playerId) && tuple.Value.playerId != GameState.playerId) {

                    cars[tuple.Value.playerId].GlobalTransform = cars[tuple.Value.playerId].GlobalTransform.InterpolateWith(tuple.Value.carTransform, 13.0f * (float)delta);
                    cars[tuple.Value.playerId].Steering = tuple.Value.carSteering;

                }

            }

        }
        else {

            foreach (var tuple in GameState.players) {
                UpdateCarState(tuple.Key, cars[tuple.Value.playerId].GlobalTransform, cars[tuple.Value.playerId].Steering);
            }

        }

    }

    private void OnPlayerLoaded(int playerId = 0, bool isLocal = true) {

        if (MultiplayerManager.connected) {

            LoadPlayer(playerId, isLocal);
            
            if (playerId == GameState.players.Count - 1) {
                MultiplayerManager.PlayersReady();
            }

        }
        else {
            SetupPlayers();
            StartCountdown();
        }
        
    }

    private void LoadPlayer(int playerId = 0, bool isLocal = true) {

        if (playerId >= mapManager.GetSpawnPoints().Count) {
            GD.PrintErr("Not enough spawnpoints. You need at least " + mapManager.GetSpawnPoints().Count);
            return;
        }

        CarController car = carScene.Instantiate<CarController>();
        playersNode.AddChild(car);

        car.GlobalTransform = mapManager.GetSpawnPoints()[playerId];
        car.SetLocalCar(isLocal);
        car.playerId = playerId;

        if (isLocal) {
            localCar = car;
        }

        cars[playerId] = car;

        UpdateCarState(playerId, car.GlobalTransform, car.Steering);

    }

    private void SetupPlayers() {

        GameState.players[0] = new PlayerData();
        GameState.players[0].playerId = 0;
        GameState.players[0].playerName = GameState.playerName;

        LoadPlayer(0, true);

        for (int i = 1; i < GameState.maxPlayers; i++) {

            GameState.players[i] = new PlayerData();
            GameState.players[i].playerId = i;
            GameState.players[i].playerName = GameState.playerName;

            LoadPlayer(i, false);

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
        EnableCar(true);
        
        isRaceStarted = true;

    }

    private void OnLapUpdated() {

        currentLap++;

        hud.UpdateLap(currentLap);

        if (currentLap > mapManager.totalLaps) {

            EnableCar(false);
            isRaceStarted = false;

            if (MultiplayerManager.connected) {
                MultiplayerManager.CarFinished(currentTime);
            }
            else {
                OnCarFinished(GameState.playerId, GameState.playerName, currentTime);
            }
        }

    }

    private void OnCheckpointCrossed(CarController car, int checkpointSection) {

        if (MultiplayerManager.connected) {

            if (car.playerId == GameState.playerId) {
                // Only add current checkpoint if they are crossed in order.
                if (GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() == checkpointSection) {
                    MultiplayerManager.CheckpointConfirm(1);
                }
                // Magia en caso de que se decida ir hacia atrás
                else if (GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() > checkpointSection) {
                    MultiplayerManager.CheckpointConfirm(-(GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() - checkpointSection));

                }

            }

        }
        else {
            if (GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() == checkpointSection) {
                OnCheckpointConfirm(car.playerId, GameState.players[car.playerId].confirmedCheckpoint + 1);
            }
            // Magia en caso de que se decida ir hacia atrás
            else if (GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() > checkpointSection) {
                OnCheckpointConfirm(car.playerId, GameState.players[car.playerId].confirmedCheckpoint - (GameState.players[car.playerId].confirmedCheckpoint % mapManager.GetCheckpointsPerLap() - checkpointSection)); 
            }

        }

    }

    private void OnCheckpointConfirm(int playerId, int confirmedCheckpoint) {

        if (MultiplayerManager.connected) {


        }
        else {
            GameState.players[playerId].confirmedCheckpoint = confirmedCheckpoint;
        }

        if (playerId == GameState.playerId) {
            // Add lap when crossing first checkpoint of the list (i.e. finish line) AND >>magic<< to add lap only when confirmed checkpoints are coherent with current lap.
            if ((GameState.players[playerId].confirmedCheckpoint - 1) % mapManager.GetCheckpointsPerLap() == 0 && ((GameState.players[playerId].confirmedCheckpoint - 1) / mapManager.GetCheckpointsPerLap()) >= currentLap) {
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

        return GameState.players.Values.OrderByDescending(player => player.confirmedCheckpoint)
                                       .ThenBy(player => CalculateDistanceToNextCheckpoint(player.carTransform.Origin, player.confirmedCheckpoint)).ToList();

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

    public void EnableCar(bool enable) {
        localCar.EnableEngine(enable);
    }

    public void UpdateCarState(int playerId, Transform3D carTransform, float steering) {
        GameState.players[playerId].carTransform = carTransform;
        GameState.players[playerId].carSteering = steering;
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

        if (playersNode == null) {

            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called Players.");

        }
    }

}

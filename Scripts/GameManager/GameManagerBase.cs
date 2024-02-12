using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class GameManagerBase : Node {

    [ExportGroup("Game Settings")]
    [Export(PropertyHint.Range, "1,12")] public int maxPlayers = 12;
    [Export] public bool countdownEnabled = true;
    
    public PackedScene carScene = ResourceLoader.Load<PackedScene>("res://Prefabs/Player/Car.tscn");

    public GameUI hud = null;
    public MapManager map = null;
    public Node playersNode = null;

    protected const float COUNTDOWN_TIME = 3.0f;
    
    public CarController localCar { get; private set; } = null;

    protected int currentLap = 0;
    protected int currentPosition = 0;
    protected float currentTime = 0.0f;

    protected bool isRaceStarted = false;
    protected bool isValidGame = true;

    protected Dictionary<int, CarController> cars = new Dictionary<int, CarController>();
    protected List<PlayerData> playersByPosition = new List<PlayerData>();

    protected int checkpointsPerLap = 0;

    protected abstract void OnPickUpCollected(CarController car);
    protected abstract void OnCheckpointCrossed(CarController car, int checkpointSection);
    protected abstract void StartRace();

    public override void _Ready() {

        if (hud == null || map == null || playersNode == null) {

            hud = GetNodeOrNull<GameUI>("UI");
            map = GetNodeOrNull<MapManager>("Map");
            playersNode = GetNodeOrNull("Players");

            if (!IsValidGame()) {
                return;
            }
        }

        checkpointsPerLap = map.GetCheckpointsPerLap();

        hud.totalLaps = map.totalLaps;
        hud.UpdateLap(0);

        map.OnPickUpConsumed += OnPickUpCollected;
        map.OnCheckpointCrossed += OnCheckpointCrossed;

    }

    public override void _Process(double delta) {

        if (!isValidGame) {
            return;
        }
        
        if (isRaceStarted) {
            currentTime += (float)delta;

            UpdatePosition();

        }

    }

    protected void LoadPlayer(int playerId = 0, bool isLocal = true) {

        if (playerId >= map.GetSpawnPoints().Count) {
            GD.PrintErr("Not enough spawnpoints. You need at least " + map.GetSpawnPoints().Count);
            return;
        }

        CarController car = carScene.Instantiate<CarController>();
        playersNode.AddChild(car);

        car.GlobalTransform = map.GetSpawnPoints()[playerId];
        car.SetLocalCar(isLocal);
        car.playerId = playerId;

        if (isLocal) {
            localCar = car;
        }

        cars[playerId] = car;

        UpdateCarState(playerId, car.GlobalTransform, car.Steering);

        GameState.players[playerId].ready = true;

    }

    protected async void StartCountdown() {

        hud.SetPosition(GetRacePosition());

        if (countdownEnabled) {
            
            hud.StartCountdown();
            await ToSignal(GetTree().CreateTimer(COUNTDOWN_TIME), SceneTreeTimer.SignalName.Timeout);

        }
        
        StartRace();

    }

    protected void Start() {

        hud.EndCountdown();
        EnableCar(true);
        
        isRaceStarted = true;

    }
    
    protected void UpdatePlayerPickUp(int playerId, PickUp.PickUpType pickUp) {

        GameState.players[playerId].hasPickUp = true;
        GameState.players[playerId].pickUp = pickUp;

        if (playerId == GameState.playerId) {
            hud.SetItem(pickUp);
        }

    }

    protected void OnLapUpdated() {

        currentLap++;

        hud.UpdateLap(currentLap);

        if (currentLap > map.totalLaps) {

            OnCarFinished(GameState.playerId, GameState.playerName, currentTime);
            EnableCar(false);
            isRaceStarted = false;

        }

    }

    protected void OnCarFinished(int playerId, string name, float raceTime) {

        hud.AddScore(name, raceTime);

        if (playerId == GameState.playerId) {

            hud.EnableScoreboard();

        }

    }

    protected int GetRacePosition() {

        playersByPosition = OrderPlayersByPosition();

        for (int i = 0; i < playersByPosition.Count; i++) {

            if (playersByPosition[i].playerId == GameState.playerId) {
                return i + 1;
            }

        }

        return 0;

    }

    protected void EnableCar(bool enable) {
        localCar.EnableEngine(enable);
    }

    protected void UpdateCarState(int playerId, Transform3D carTransform, float steering) {
        GameState.players[playerId].carTransform = carTransform;
        GameState.players[playerId].carSteering = steering;
    }

    private void UpdatePosition() {
        hud.SetPosition(GetRacePosition());
    }

    protected List<PlayerData> OrderPlayersByPosition() {

        return GameState.players.Values.OrderByDescending(player => player.confirmedCheckpoint)
                                       .ThenBy(player => CalculateDistanceToNextCheckpoint(player.carTransform.Origin, player.confirmedCheckpoint)).ToList();

    }

    protected float CalculateDistanceToNextCheckpoint(Vector3 position, int currentCheckpoint) {
        
        int sectionNumber = currentCheckpoint % checkpointsPerLap;

        return map.checkpoints[sectionNumber].plane.DistanceTo(position);

    }

    private bool IsValidGame() {

        if (hud == null) {
            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called UI.");
        }

        if (map == null) {
            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called Map.");
        }

        if (playersNode == null) {
            isValidGame = false;
            GD.PrintErr("GameManager needs a Node called Players.");
        }

        if (carScene == null) {
            isValidGame = false;
            GD.PrintErr("GameManager needs a Car Scene set up.");

        }

        return isValidGame;

    }

}

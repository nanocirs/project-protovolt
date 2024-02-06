using System.Collections.Generic;
using Godot;

public partial class LevelManager : Node {

    [Export] private PackedScene carScene = null;

    private GameLobby lobby;
    private Node playersNode;
    private Node spawnPointsNode;

    private CarController myCar = null;

    private List<Transform3D> spawnPoints = new List<Transform3D>();
    private Dictionary<int, Transform3D> carTransforms = new Dictionary<int, Transform3D>();
    private Dictionary<int, CarController> cars = new Dictionary<int, CarController>();

    private bool isValidLevel = true;

    public override void _Ready() {

        lobby = GetNodeOrNull<GameLobby>("Lobby");
        playersNode = GetNodeOrNull("Players");
        spawnPointsNode = GetNodeOrNull("SpawnPoints");

        lobby.OnPlayerLoaded += OnPlayerLoaded;

        if (spawnPointsNode == null) {
            
            GD.PushWarning("SpawnPoints not set. Generating a SpawnPoint in origin.");

            spawnPoints.Add(new Transform3D());

        }
        else {

            foreach (Node3D spawnPoint in spawnPointsNode.GetChildren()) {
                
                spawnPoints.Add(spawnPoint.GlobalTransform);

            }

        }

        if (playersNode == null) {

            isValidLevel = false;
            GD.PrintErr("Level Scene needs a Node called Players.");

        }

        if (lobby == null) {
            
            isValidLevel = false;
            GD.PrintErr("Level Scene needs a Node called Lobby.");

        }

    }

    public override void _PhysicsProcess(double delta) {

        if (myCar == null) {
            return;
        }

        if (Multiplayer.IsServer()) {

            NetNotifyTransform(myCar.id, myCar.GlobalTransform);

        }
        else {

            RpcId(1, "NetNotifyTransform", myCar.id, myCar.GlobalTransform);

        }

        foreach (var tuple in carTransforms) {
            
            if (tuple.Key != myCar.id) {
                cars[tuple.Key].GlobalTransform = cars[tuple.Key].GlobalTransform.InterpolateWith(tuple.Value, 5.0f * (float)delta);
            }

        }

    }

    private void OnPlayerLoaded(int playerId, bool isLocal) {

        if (isValidLevel) {

            CarController car = carScene.Instantiate<CarController>();
            car.SetCarId(playerId);  
            GD.Print(playerId);
            playersNode.AddChild(car);

            car.GlobalTransform = spawnPoints[playerId];
            car.SetLocalCar(isLocal);

            if (isLocal) {
                myCar = car;
            }

            carTransforms[playerId] = car.GlobalTransform;
            cars[playerId] = car;

        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void NetNotifyTransform(int playerId, Transform3D globalTransform) {

        Rpc("NetUpdateTransforms", playerId, globalTransform);        

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void NetUpdateTransforms(int playerId, Transform3D globalTransform) {
        
        if (myCar == null) {
            return;
        }

        if (playerId != myCar.id) {
            
            carTransforms[playerId] = globalTransform;

        }

    }

}

using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Godot;

public partial class LevelManager : Node {

    [Export] private PackedScene carScene = null;

    private GameLobby lobby;
    private Node playersNode;
    private Node spawnPointsNode;

    private List<Transform3D> spawnPoints = new List<Transform3D>();

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

    private void OnPlayerLoaded(int playerId, bool isLocal) {

        if (isValidLevel) {

            CarController car = carScene.Instantiate<CarController>();
            car.GlobalTransform = spawnPoints[playerId];
            
            playersNode.AddChild(car);
            car.SetLocalCar(isLocal);

        }
    }
}

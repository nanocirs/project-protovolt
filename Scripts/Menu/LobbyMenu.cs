using System.Collections.Generic;
using System.Data.Common;
using Godot;

public partial class LobbyMenu : CanvasLayer {

    [Export] private VBoxContainer playersContainer = null;
    [Export] private PackedScene playerNameRowScene = null;
    [Export] private Button playButton = null;

    private Dictionary<int, LineEdit> playerRows = new Dictionary<int, LineEdit>();

    private bool isServer = false;
    private bool isSceneValid = true;

    public override void _Ready() {
        
        MultiplayerManager.instance.OnServerCreated += OnServerCreated;
        MultiplayerManager.instance.OnServerClosed += OnServerClosed;
        MultiplayerManager.instance.OnPlayerConnected += OnPlayerConnected;
        MultiplayerManager.instance.OnPlayerDisconnected += OnPlayerDisconnected;

        if (playerNameRowScene == null) {
            isSceneValid = false;
            GD.PrintErr("PackedScene (PlayerNameRowScene) not assigned in Lobby Menu.");
        }

        if (playersContainer == null) {
            isSceneValid = false;
            GD.PrintErr("VBoxContainer (PlayersContainer) not assigned in Lobby Menu.");
        }

        if (playButton != null) {
            playButton.Hide();
        }
        else {
            isSceneValid = false;
            GD.PrintErr("Button (PlayButton) not assigned in Lobby Menu.");
        }

    }

    private void OnPlayPressed() {
        // @TODO: Check if enough players joined to start the game.
        // @TODO: Make all clients load the map.
        GD.Print("PLAY");
    }

    private void OnDisconnectPressed() {
        MultiplayerManager.instance.DisconnectFromServer();
    }


    private void OnServerCreated() {

        if (isSceneValid) {
            playButton.Show();
        }

    }

    private void OnServerClosed() {

        GetParent<MainMenu>().SetMenuPage(MainMenu.MenuPage.Main);

        if (isSceneValid) {

            foreach (var row in playerRows) {
                playerRows[row.Key].QueueFree();
                playerRows.Remove(row.Key);
            }

            playButton.Hide();

        }
    }

    private void OnPlayerConnected(int id, string name) {
        
        if (isSceneValid) {

            LineEdit playerName = playerNameRowScene.Instantiate<LineEdit>();
            playerName.Text = name;
            playersContainer.AddChild(playerName);

            playerRows.Add(id, playerName);

        }
    }

    private void OnPlayerDisconnected(int id, string name) {

        if (isSceneValid) {

            playerRows[id].QueueFree();
            playerRows.Remove(id);

        }

    }

}

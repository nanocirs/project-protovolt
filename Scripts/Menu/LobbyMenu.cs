using System.Collections.Generic;
using Godot;

public partial class LobbyMenu : CanvasLayer {

    [Export] private VBoxContainer playersContainer = null;
    [Export] private PackedScene playerNameRowScene = null;
    [Export] private Button playButton = null;
    
    // @TODO: Segregate to another class (LobbySettings?)
    private int minimumPlayers = 1;
    private string mapFile = "Map1.tscn";
    //

    private Dictionary<int, LineEdit> playerRows = new Dictionary<int, LineEdit>();

    private bool isSceneValid = true;

    public override void _Ready() {
        
        MultiplayerManager.instance.OnServerCreated += OnServerCreated;
        MultiplayerManager.instance.OnServerClosed += OnServerClosed;
        MultiplayerManager.instance.OnPlayerConnected += OnPlayerConnected;
        MultiplayerManager.instance.OnPlayerDisconnected += OnPlayerDisconnected;
        MultiplayerManager.instance.OnServerCreateError += OnLobbyError;
        MultiplayerManager.instance.OnPlayerConnectError += OnLobbyError;

        if (playerNameRowScene == null) {
            isSceneValid = false;
            GD.PrintErr("PackedScene (PlayerNameRowScene) not assigned in Lobby Menu.");
        }

        if (playersContainer == null) {
            isSceneValid = false;
            GD.PrintErr("VBoxContainer (PlayersContainer) not assigned in Lobby Menu.");
        }

        if (playButton != null) {
            playButton.Disabled = true;
            playButton.Hide();
        }
        else {
            isSceneValid = false;
            GD.PrintErr("Button (PlayButton) not assigned in Lobby Menu.");
        }

    }

    private void OnPlayPressed() {

        MultiplayerManager.instance.Rpc("LoadMap", mapFile);
        
        GD.Print("Game starting...");

    }

    private void OnDisconnectPressed() {
        MultiplayerManager.instance.DisconnectFromServer();
        GetParent<MainMenuManager>().SetMenuPage(MainMenuManager.MenuPage.Main);
    }


    private void OnServerCreated() {

        if (isSceneValid) {
            playButton.Show();
        }

    }

    private void OnServerClosed() {

        GetParent<MainMenuManager>().SetMenuPage(MainMenuManager.MenuPage.Main);

        if (isSceneValid) {

            foreach (var row in playerRows) {
                playerRows[row.Key].QueueFree();
                playerRows.Remove(row.Key);
            }

            playButton.Hide();

            if (MultiplayerManager.instance.GetTotalPlayers() >= minimumPlayers) {
                playButton.Disabled = false;
            }
            else {
                playButton.Disabled = true;
            }

        }
    }

    private void OnPlayerConnected(int id, string name) {
        
        if (isSceneValid) {

            LineEdit playerName = playerNameRowScene.Instantiate<LineEdit>();
            playerName.Text = name;

            playersContainer.AddChild(playerName);
            playerRows[id] = playerName;

            if (MultiplayerManager.instance.GetTotalPlayers() >= minimumPlayers) {
                playButton.Disabled = false;
            }
            else {
                playButton.Disabled = true;
            }

        }


    }

    private void OnPlayerDisconnected(int id, string name) {

        if (isSceneValid) {

            playerRows[id].QueueFree();
            playerRows.Remove(id);

            if (MultiplayerManager.instance.GetTotalPlayers() >= minimumPlayers) {
                playButton.Disabled = false;
            }
            else {
                playButton.Disabled = true;
            }

        }

    }

    private void OnLobbyError() {
        GetParent<MainMenuManager>().SetMenuPage(MainMenuManager.MenuPage.Main);
    }

}

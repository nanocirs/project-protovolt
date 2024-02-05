using Godot;

public partial class MainMenu : Node {

    public enum MenuPage {
        Main = 0,
        Lobby = 1,
    }

    [Export] private CanvasLayer canvasMainMenu = null;
    [Export] private LobbyMenu canvasLobby = null;

    private bool isSceneValid = true;

    public override void _Ready() {

        if (canvasMainMenu == null) {
            isSceneValid = false;
            GD.PrintErr("CanvasLayer (CanvasMainMenu) not assigned in Main Menu.");
        }

        if (canvasLobby == null) {
            isSceneValid = false;
            GD.PrintErr("CanvasLayer (CanvasLobby) not assigned in Main Menu.");
        }

        SetMenuPage(MenuPage.Main);
        
    }

    public void SetMenuPage(MenuPage page) {

        if (isSceneValid) {

            canvasMainMenu.Hide();
            canvasLobby.Hide();

            switch (page) {
                case MenuPage.Main:
                    canvasMainMenu.Show();
                    break;
                case MenuPage.Lobby:
                    canvasLobby.Show();
                    break;
                default:
                    canvasMainMenu.Show();
                    break;
            }

        }

    }

    private void OnPlayPressed() {
		GameStateMachine.instance.LoadScene("Game.tscn");
	}

	private void OnConnectPressed() {
        
        SetMenuPage(MenuPage.Lobby);

        MultiplayerManager.instance.JoinServer();

	}

	private void OnHostPressed() {

        SetMenuPage(MenuPage.Lobby);

        MultiplayerManager.instance.CreateServer();

	}

}

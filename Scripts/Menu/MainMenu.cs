using Godot;

public partial class MainMenu : CanvasLayer {

    private void OnPlayPressed() {
		GameStateMachine.instance.LoadScene("Game.tscn");
	}

	private void OnConnectPressed() {
        
        GetParent<MainMenuManager>().SetMenuPage(MainMenuManager.MenuPage.Lobby);

        MultiplayerManager.instance.JoinServer();

	}

	private void OnHostPressed() {

        GetParent<MainMenuManager>().SetMenuPage(MainMenuManager.MenuPage.Lobby);

        MultiplayerManager.instance.CreateServer();

	}

}

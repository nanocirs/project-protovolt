using Godot;

public partial class MainMenu : Node
{
	private void OnPlayPressed() {
		GameStateMachine.instance.LoadScene("Game.tscn");
	}

	private void OnConnectPressed() {
        MultiplayerManager.instance.JoinServer();
	}

	private void OnHostPressed() {
        MultiplayerManager.instance.CreateServer();
	}

}

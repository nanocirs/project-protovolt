using Godot;

public partial class MainMenu : Node
{
	private void OnPlayPressed() {
		GameStateMachine.instance.LoadScene("Game.tscn");
	}

}

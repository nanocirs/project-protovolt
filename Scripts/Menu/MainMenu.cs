using Godot;

public partial class MainMenu : Node
{
	private void _on_button_pressed() {
		GameStateMachine.instance.LoadScene("Game.tscn");
	}
}

using Godot;

public partial class EntryScene : Node {

	public override void _Ready() {

		GameStateMachine.instance.Start(this, GameStateMachine.State.MainMenu);

	}    
}

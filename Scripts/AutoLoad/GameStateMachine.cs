using Godot;

public enum GameState {
	Intro = 0,
	MainMenu = 1,
	Loading = 2,
	Playing = 3,
}

public partial class GameStateMachine : Singleton<GameStateMachine> {

	private Node _entryScene = null;
	private Node _currentScene = null;

	public void Start(Node entryScene, GameState startingState) {

		_entryScene = entryScene;


		switch (startingState) {
			case GameState.Intro:
			case GameState.MainMenu:
				LoadScene("MainMenu.tscn");
				break;
			case GameState.Loading:
			case GameState.Playing:

			default:
				LoadScene("MainMenu.tscn");
				break;
		}
	}

	public void LoadScene(string sceneFile) {

		if (_currentScene != null) {
			_currentScene.GetParent().RemoveChild(_currentScene);
		}

		string fullPath = "res://Scenes/" + sceneFile;

		if (ResourceLoader.Load(fullPath) is PackedScene scene) {
			Node sceneInstance = scene.Instantiate();
			_entryScene.AddChild(sceneInstance);
			
			_currentScene = sceneInstance;
		}
	}
}

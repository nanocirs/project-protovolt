using Godot;

public partial class GameStateMachine : Singleton<GameStateMachine> {

    public enum State {
        Intro = 0,
        MainMenu = 1,
        Loading = 2,
        Playing = 3,
    }

	private Node _entryScene = null;
	private Node _currentScene = null;

	public void Start(Node entryScene, State startingState) {

		_entryScene = entryScene;

		switch (startingState) {
			case State.Intro:
			case State.MainMenu:
				LoadScene("Menu/MainMenu.tscn");
				break;
			case State.Loading:
			case State.Playing:
            	LoadScene("Maps/Game.tscn");
                break;
			default:
				LoadScene("Menu/MainMenu.tscn");
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

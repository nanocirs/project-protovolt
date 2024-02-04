using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public enum GameState {
	MainMenu = 0,
	Loading = 1,
	Intro = 2,
	Playing = 3,
}

public partial class GameStateMachine : Singleton<GameStateMachine> {

	private Node _entryScene = null;

	public void Start(Node entryScene, GameState startingState) {

		_entryScene = entryScene;

		switch (startingState) {
			case GameState.MainMenu:
				LoadScene("MainMenu.tscn");
				break;
			case GameState.Loading:
			case GameState.Intro:
			case GameState.Playing:

			default:
				LoadScene("MainMenu.tscn");
				break;
		}
	}

	public void LoadScene(string sceneFile) {

		string fullPath = "res://Scenes/" + sceneFile;

		if (ResourceLoader.Load(fullPath) is PackedScene scene) {
			Node sceneInstance = scene.Instantiate();
			_entryScene.AddChild(sceneInstance);
		}
	}
}

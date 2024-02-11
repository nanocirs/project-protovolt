using Godot;

public partial class GameManagerLoader : Node {

    public bool multiplayer = false;

    [Export(PropertyHint.Range, "1,12")] private const int maxPlayers = 12;
    [Export] protected bool countdownEnabled = true;
    [Export] private PackedScene carScene = null;

    public override void _Ready() {

        GameManagerBase game = MultiplayerManager.connected ? new GameManagerOnline() : new GameManagerOffline();

        game.maxPlayers = maxPlayers;
        game.countdownEnabled = countdownEnabled;
        game.carScene = carScene;
        game.hud = GetNodeOrNull<GameUI>("UI");
        game.map = GetNodeOrNull<MapManager>("Map");
        game.playersNode = GetNodeOrNull("Players");

        AddChild(game);

    }

}

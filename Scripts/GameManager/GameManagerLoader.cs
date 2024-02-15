using Godot;

public partial class GameManagerLoader : Node {

    [ExportGroup("Game Settings")]
    [Export(PropertyHint.Range, "1,12")] private const int maxPlayers = 12;
    [Export] protected bool countdownEnabled = true;

    [Export] public GameUI hud = null;
    [Export] public MapManager map = null;
    [Export] public Node playersNode = null;

    public override void _Ready() {

        GameManagerBase game = MultiplayerManager.connected ? new GameManagerOnline() : new GameManagerOffline();

        game.maxPlayers = maxPlayers;
        game.countdownEnabled = countdownEnabled;
        game.hud = hud;
        game.map = map;
        game.playersNode = playersNode;

        AddChild(game);

    }

}

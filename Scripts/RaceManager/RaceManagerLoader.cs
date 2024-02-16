using Godot;

public partial class RaceManagerLoader : Node {

    [ExportGroup("Game Settings")]
    [Export(PropertyHint.Range, "1,12")] private const int maxPlayers = 12;
    [Export] protected bool countdownEnabled = true;

    [Export] public GameUI hud = null;
    [Export] public MapManager map = null;
    [Export] public Node playersContainer = null;

    public override void _Ready() {

        RaceManagerBase game = MultiplayerManager.connected ? new RaceManagerOnline() : new RaceManagerOffline();

        game.maxPlayers = maxPlayers;
        game.countdownEnabled = countdownEnabled;
        game.hud = hud;
        game.map = map;
        game.playersContainer = playersContainer;

        AddChild(game);

    }

}

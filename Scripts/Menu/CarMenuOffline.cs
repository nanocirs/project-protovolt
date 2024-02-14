public partial class CarMenuOffline : CarMenuBase {
    
    protected override void ConfirmCarSelection() {

        int playerId = 0;

        GameState.playerId = playerId;

        GameState.CreatePlayer(playerId, GameState.playerName);
        GameState.players[playerId].carPath = carPath;

		GameStateMachine.instance.LoadScene("Maps/Game.tscn");

    }

}

using System.Collections.Generic;

public partial class GameState : Singleton<GameState> {

    public static int playerId = -1;

    public static Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    public static int GetTotalPlayers() {
        
        return players.Count;

    }

}

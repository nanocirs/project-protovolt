using System.Collections.Generic;

public partial class GameState : Singleton<GameState> {

    public static int maxPlayers = 12;
    public static int playerId = 0;

    public static string playerName = "KPS";

    public static Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    public static int GetTotalPlayers() {
        
        return players.Count;

    }

    public static void Clear() {
        players.Clear();
        playerId = 0;
    }

}

using System.Collections.Generic;
using Godot;

public partial class GameState : Singleton<GameState> {

    public class PlayerData {
        public int playerId; 
        public string playerName;
        public bool finished;
        public int currentCheckpoint;
        public int racePosition;
        public float raceTime;
        public Transform3D carTransform;
        public float carSteering;
    }

    public static Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    public static int GetTotalPlayers() {
        
        return players.Count;

    }
}

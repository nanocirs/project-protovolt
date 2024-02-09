using Godot;

public class PlayerData {

    public int playerId; 
    public string playerName;
    public int currentCheckpoint;
    public float raceTime;
    public bool finished;
    public Transform3D carTransform;
    public float carSteering;

    public void Restart() {
        
        currentCheckpoint = 0;
        raceTime = 0.0f;
        finished = false;
        
    }

}

using Godot;

public class PlayerData {

    public int playerId; 
    public string playerName;
    public bool finished;
    public int currentCheckpoint;
    public int racePosition;
    public float raceTime;
    public Transform3D carTransform;
    public float carSteering;

    public void Restart() {
        
        this.currentCheckpoint = 0;
        this.racePosition = 0;
        this.raceTime = 0.0f;
        this.finished = false;
        
    }

}

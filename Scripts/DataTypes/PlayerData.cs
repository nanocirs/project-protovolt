using Godot;

public class PlayerData {

    public int playerId; 
    public int peerId;
    public string playerName;
    public bool ready;
    public bool finished;
    public float raceTime;
    public int confirmedCheckpoint;
    public int currentCheckpoint;
    public Transform3D carTransform;
    public float carSteering;

    public void Restart() {
        confirmedCheckpoint = 0;
        currentCheckpoint = 0;
        raceTime = 0.0f;
        finished = false;
        ready = false;
    }

}

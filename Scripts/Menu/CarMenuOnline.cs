using Godot;

public partial class CarMenuOnline : CanvasLayer {

    [Export] private ButtonGroup buttonGroup = null;

    protected string carPath = "";

    private Timer selectionTimer = null;
    private float timeToSelect = 5.0f;

    private int carsNotified = 0;

    public override void _Ready() {
                
        if (buttonGroup == null) {
            GD.PrintErr("CarMenuOnline doesn't have a valid ButtonGroup.");
            return;
        }
        else {
            buttonGroup.Pressed += SelectedCar;
        }

        if (Multiplayer.IsServer()) {

            selectionTimer = new Timer();
            selectionTimer.WaitTime = timeToSelect;
            selectionTimer.OneShot = true;
        
            AddChild(selectionTimer);

            selectionTimer.Timeout += CollectSelectedCars;

        }

    }

    public void Load() {
        selectionTimer.Start();
        Show();
    }

    private void SelectedCar(BaseButton button) {
        carPath = button.GetMeta("carPath").AsString();
    }

    private string GetRandomCarPath() {

        int totalCars = buttonGroup.GetButtons().Count;
        int randomCar = GD.RandRange(0, totalCars - 1);
            
        return buttonGroup.GetButtons()[randomCar].GetMeta("carPath").AsString();

    }

    private void CollectSelectedCars() {

        if (Multiplayer.IsServer()) {

            if (carPath == "") {
                carPath = GetRandomCarPath();
            }

            GameState.players[GameState.playerId].carPath = carPath;

            carsNotified++;

            if (carsNotified == GameState.GetTotalPlayers()) {
                Rpc("LoadMap", "Maps/Game.tscn");
            }
            else {
                Rpc("RequestSelectedCars");
            }

        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestSelectedCars() {
        RpcId(MultiplayerManager.SV_PEER_ID, "NotifySelectedCar", carPath);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NotifySelectedCar(string path) {
        
        if (Multiplayer.IsServer()) {
            
            int playerId = MultiplayerManager.peerIdplayerIdMap[Multiplayer.GetRemoteSenderId()];

            if (path == "") {
                path = GetRandomCarPath();
            }

            GameState.players[playerId].carPath = path;

            carsNotified++;

            if (carsNotified == GameState.GetTotalPlayers()) {
                Rpc("LoadMap", "Maps/Game.tscn");           
            }

        }

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void LoadMap(string mapPath) {
        GameStateMachine.instance.LoadScene(mapPath);
    }

}

using System;
using Godot;

public partial class PickUp : Node3D {

    public enum PickUpType {
        Turbo = 0,
        Oil = 1,
        StickerLaugh = 2,
    }

    [Signal] public delegate void OnCarConsumedPickUpEventHandler(CarController car);

    [Export] float respawnTime = 10.0f;

    private Area3D pickUpArea = null;

    private bool isValidPickUp = true;

    public override void _Ready() {
 
        pickUpArea = GetNodeOrNull<Area3D>("PickUpArea");

        if (pickUpArea == null) {
            isValidPickUp = false;
            GD.PrintErr("PickUp needs an Area3D (PickUpArea)");
        }

    }

    public async void Consume() {
        pickUpArea.Monitoring = false;
        Hide();

        await ToSignal(GetTree().CreateTimer(respawnTime), SceneTreeTimer.SignalName.Timeout);
        Respawn();

    }

    public void Respawn() {
        pickUpArea.Monitoring = true;
        Show();
    }

    private void OnCarPickedUp(Node3D carBody) {

        CarController car = carBody as CarController;

        if (!GameState.players[car.playerId].hasPickUp) {
            CallDeferred("Consume");
            EmitSignal(SignalName.OnCarConsumedPickUp, car);
        }
    
    }

    public static PickUpType GetRandomPickUp() {

        int typeCount = Enum.GetNames(typeof(PickUpType)).Length;

        return (PickUpType)GD.RandRange(0, typeCount - 1);

    }

}

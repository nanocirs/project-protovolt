using Godot;

public partial class PickUpEffect : Node {

    public static float turboDuration = 5.0f;
    
    public static void Activate(CarController car, Pickable.PickUpType pickUpType) {

        switch (pickUpType) {

            case Pickable.PickUpType.Turbo:
                car.EffectTurbo();
                break;
            case Pickable.PickUpType.Oil:
                car.EffectOil();
                break;
            case Pickable.PickUpType.StickerLaugh:
                car.EffectStickerLaugh();
                break;
            case Pickable.PickUpType.Missile:
                break;
            case Pickable.PickUpType.Bomb:
                break;
            case Pickable.PickUpType.Disabler:
                break;
            default:
                GD.PushWarning("PickUp " + pickUpType + " doesn't have a valid implementation.");
                break;

        }

    }

}

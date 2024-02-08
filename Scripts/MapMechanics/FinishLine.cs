using System;
using System.Collections.Generic;
using Godot;

public partial class FinishLine : Node {

    [Signal] public delegate void OnFinishLineCrossedEventHandler(CarController car);

    private Area3D areaFirst = null;
    private Area3D areaLast = null;

    private bool isValidFinishLine = true;

    private List<Node3D> detectedLegalCars = new List<Node3D>();

    public override void _Ready() {
        
        areaFirst = GetNodeOrNull<Area3D>("AreaFirst");
        areaLast = GetNodeOrNull<Area3D>("AreaLast");

        if (areaFirst == null || areaLast == null) {

            isValidFinishLine = false;
            GD.Print("FinishLine needs two Area3D (AreaFirst and AreaLast)");

        }

    }

    private void OnCarEnteredAreaFirst(Node3D car) {

        detectedLegalCars.Add(car);
        
    }

    private void OnCarEnteredAreaLast(Node3D car) {
        
        if (detectedLegalCars.Contains(car)) {

            detectedLegalCars.Remove(car);
            EmitSignal(SignalName.OnFinishLineCrossed, car as CarController);

        }
    }

}

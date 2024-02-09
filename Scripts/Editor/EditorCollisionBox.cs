using Godot;

[Tool]
public partial class EditorCollisionBox : CollisionShape3D {

    [Export] private Color color = new Color(0, 0.757f, 0.349f, 0.757f);
    [Export] private bool VisibleInGame = false;

    public override void _Ready() {

        if (Engine.IsEditorHint() || VisibleInGame) {

            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = new BoxMesh();
            AddChild(meshInstance);

            meshInstance.Mesh.Set("size", Shape.Get("size"));

            StandardMaterial3D material = new StandardMaterial3D();
            material.Set("albedo_color", color);
            material.Set("transparency", 1);

            meshInstance.MaterialOverride = material;
            
        }
    }

}

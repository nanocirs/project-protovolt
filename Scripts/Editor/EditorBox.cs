using Godot;

[Tool]
public partial class EditorBox : Node3D {
    
    [Export] private bool VisibleInGame = false;
    [Export] private Color color = new Color(0.0f, 0.424f, 1.0f);
    [Export] private Vector3 size = new Vector3(1.0f, 1.0f, 1.0f);

    public override void _Ready() {
        
        if (Engine.IsEditorHint() || VisibleInGame) {

            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = new BoxMesh();
            AddChild(meshInstance);

            meshInstance.Mesh.Set("size", size);

            StandardMaterial3D material = new StandardMaterial3D();
            material.Set("albedo_color", color);
            material.Set("transparency", 1);

            meshInstance.MaterialOverride = material;
            
        }

    }
}

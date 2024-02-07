using Godot;

[Tool]
public partial class EditorBox : Node {
    
    [Export] private PackedScene boxScene = null;

    public override void _Ready() {

        if (Engine.IsEditorHint()) {
            Node box = boxScene.Instantiate();
            AddChild(box);
        }

    }
}

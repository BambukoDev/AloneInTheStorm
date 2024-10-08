using Godot;
using System.Collections.Generic;
using ImGuiNET;

public partial class WorldObject : RigidBody3D {
    
    [ExportGroup("Data")]
    [Export]
    public string name = "Object";
    [Export]
    public string options = "";
    
    [ExportGroup("Physics")]
    [Export]
    public float float_force = 1.0f;
    public static float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble();

    public Godot.Collections.Array<Node> edges;

    public override void _Ready() {
        edges = GetNode<Node3D>("Edges").GetChildren();
    }

    public override void _Process(double delta) {
        ImGui.Begin($"{this.Name} Config");
        // var cube = GetNode<RigidBody3D>("/root/Game/TestBody");
        // var cubepos = new System.Numerics.Vector3(cube.Position.X, cube.Position.Y, cube.Position.Z);
        // ImGui.InputFloat3("Position", ref cubepos);
        // cube.Position = new Vector3(cubepos.X, cubepos.Y, cubepos.Z);
        if (ImGui.Button("Despawn")) QueueFree();
        ImGui.End();
    }
}

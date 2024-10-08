using Godot;

public partial class WaterBehaviour : Area3D {
    [Export]
    public float water_drag = 0.05f;
    [Export]
    public float water_angular_drag = 0.05f;
    [Export]
    public Vector3 water_movement = Vector3.Zero;


    public override void _PhysicsProcess(double delta) {
        var bodies = GetOverlappingBodies();
        foreach (var obj in bodies) {
            var body = obj as WorldObject;
            if (body != null) {
                foreach (Marker3D edge in body.edges) {
                    float depth = GlobalPosition.Y - edge.GlobalPosition.Y;
                    if (depth > 0) {
                        body.ApplyForce(Vector3.Up * body.float_force * depth * WorldObject.gravity / body.Mass, edge.Position);
                    }
                }
                body.LinearVelocity *= 1 - water_drag;
                body.AngularVelocity *= 1 - water_angular_drag;
                body.ApplyForce(water_movement);
            }
        }
    }
}

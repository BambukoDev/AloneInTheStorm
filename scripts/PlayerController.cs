using Godot;
using System;
using ImGuiGodot;
using ImGuiNET;

public partial class PlayerController : CharacterBody3D {
	const float SPEED = 5.0f;
    const float JUMP_VELOCITY = 4.5f;

    float gravity = 9.8f;

    Node3D face;
    Camera3D camera;

    RayCast3D pickup;
    Node3D hold;
    const float PICKUP_RANGE = 3;
    RigidBody3D pickedObject = null;

    public override void _Ready() {
        GD.Print("PlayerController: Ready()");
        face = GetNode<Node3D>("Face");
        camera = face.GetNode<Camera3D>("Camera3D");
        pickup = camera.GetNode<RayCast3D>("RayCast3D");
        hold = camera.GetNode<Node3D>("Hold");

        pickup.TargetPosition = new Vector3(0, 0, -PICKUP_RANGE);
        hold.Position = new Vector3(0, 0, -PICKUP_RANGE);

        pickedObject = GetNode<RigidBody3D>("/root/Game/TestBody");
	}

	public override void _PhysicsProcess(double delta) {
        Vector3 velocity = Velocity;
        
        if (!IsOnFloor()) {
            velocity.Y -= (float)(gravity * delta);
        }

        if (Input.IsActionPressed("game_jump") && IsOnFloor()) {
            velocity.Y = JUMP_VELOCITY;
        }

        Vector2 inputDir = Input.GetVector("game_left", "game_right", "game_foward", "game_backward");
        Vector3 direction = (face.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        // TODO: Add inertia
        if (direction != Vector3.Zero) {
            velocity.X = direction.X * SPEED;
            velocity.Z = direction.Z * SPEED;
        } else {
            velocity.X = Mathf.MoveToward(velocity.X, 0, SPEED);
            velocity.Z = Mathf.MoveToward(velocity.X, 0, SPEED);
        }

        Velocity = velocity;

        MoveAndSlide();

        if (pickedObject != null) { 
            pickedObject.Position = hold.GlobalPosition;
            pickedObject.LinearVelocity = Vector3.Zero;
        }
	}

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is InputEventMouseButton) Input.MouseMode = Input.MouseModeEnum.Captured;
        else if (@event.IsActionPressed("ui_cancel")) Input.MouseMode = Input.MouseModeEnum.Visible;

        if (Input.MouseMode == Input.MouseModeEnum.Captured) {
            if (@event is InputEventMouseMotion) {
                var motion = @event as InputEventMouseMotion;
                face.RotateY(-motion.Relative.X * 0.005f);
                camera.RotateX(-motion.Relative.Y * 0.005f);
                camera.Rotation = new Vector3(Mathf.Clamp((float)camera.Rotation.X, (float)Mathf.DegToRad(-90), (float)Mathf.DegToRad(90)), camera.Rotation.Y, camera.Rotation.Z); 
            }
        }

        if (@event is InputEventKey) {
            var key = @event as InputEventKey;
            if (key.IsActionPressed("game_interact")) {
                var obj = pickup.GetCollider() as RigidBody3D;
                if (pickedObject == null) pickedObject = obj;
                else pickedObject = null;
            }
        }
    }

    public override void _Process(double delta) {
        ImGui.Begin("Player Config");
        var pos = new System.Numerics.Vector3(Position.X, Position.Y, Position.Z);
        ImGui.InputFloat3("Position", ref pos);
        Position = new Vector3(pos.X, pos.Y, pos.Z);
        ImGui.End();
    }
}

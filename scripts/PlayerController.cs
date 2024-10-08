using Godot;
using System;
using ImGuiGodot;
using ImGuiNET;

public partial class PlayerController : CharacterBody3D {
	const float SPEED = 5.0f;
    const float JUMP_VELOCITY = 4.5f;

    PackedScene block = ResourceLoader.Load<PackedScene>("res://assets/objects/TestBody.tscn");

    float gravity = 9.8f;

    Node3D face;
    Camera3D camera;

    RayCast3D pickup;
    Node3D hold;
    const float PICKUP_RANGE = 3;
    RigidBody3D pickedObject = null;

    public override void _Ready() {
        face = GetNode<Node3D>("Face");
        camera = face.GetNode<Camera3D>("Camera3D");
        pickup = camera.GetNode<RayCast3D>("RayCast3D");
        hold = camera.GetNode<Node3D>("Hold");

        pickup.TargetPosition = new Vector3(0, 0, -PICKUP_RANGE);
        hold.Position = new Vector3(0, 0, -PICKUP_RANGE);

        //pickedObject = GetNode<RigidBody3D>("/root/Game/TestBody");
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
            pickedObject.LinearVelocity = (hold.GlobalPosition - pickedObject.Position)*4;
            Vector3 rotation = Vector3.Zero - pickedObject.Rotation;
            pickedObject.ApplyTorque(rotation.Normalized() * 4.0f * (float)delta);
            pickedObject.AngularDamp = 2f;
            // pickedObject.AngularVelocity = pickedObject.AngularVelocity.Lerp(Vector3.Zero, 0.01f);
            // pickedObject.Position = hold.GlobalPosition;
            // pickedObject.LinearVelocity = Vector3.Zero;
        }
	}

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is InputEventMouseButton) Input.MouseMode = Input.MouseModeEnum.Captured;
        else if (@event.IsActionPressed("ui_cancel")) Input.MouseMode = Input.MouseModeEnum.Visible;

        if (Input.MouseMode == Input.MouseModeEnum.Captured) {
            // rotate mode
            if (Input.IsMouseButtonPressed(MouseButton.Right)) {
                if (@event is InputEventMouseMotion) {
                    pickedObject.AngularVelocity = Vector3.Zero;
                    var motion = @event as InputEventMouseMotion;
                    pickedObject.RotateY(motion.Relative.X * 0.01f);
                    pickedObject.RotateZ(motion.Relative.Y * 0.01f);
                    pickedObject.Rotation = new Vector3(pickedObject.Rotation.X, pickedObject.Rotation.Y, pickedObject.Rotation.Z); 
                }
            } else {
                if (@event is InputEventMouseMotion) {
                    var motion = @event as InputEventMouseMotion;
                    face.RotateY(-motion.Relative.X * 0.005f);
                    camera.RotateX(-motion.Relative.Y * 0.005f);
                    camera.Rotation = new Vector3(Mathf.Clamp((float)camera.Rotation.X, (float)Mathf.DegToRad(-90), (float)Mathf.DegToRad(90)), camera.Rotation.Y, camera.Rotation.Z); 
                }
                if (@event is InputEventMouseButton) {
                    var mouse = @event as InputEventMouseButton;
                    if (mouse.ButtonIndex is MouseButton.WheelUp) {
                        hold.Position = new Vector3(0, 0, hold.Position.Z - 0.1f);
                    }
                    if (mouse.ButtonIndex is MouseButton.WheelDown) {
                        hold.Position = new Vector3(0, 0, hold.Position.Z + 0.1f);
                    }
                    //hold.Position.Clamp(new Vector3(0, 0, -PICKUP_RANGE*2), new Vector3(0, 0, -1));
                }
            }
        }

        if (@event is InputEventKey) {
            var key = @event as InputEventKey;
            if (key.IsActionPressed("game_interact")) {
                var obj = pickup.GetCollider() as WorldObject;
                if (pickedObject == null && obj != null) {
                    pickedObject = obj;
                    hold.Position = new Vector3(0, 0, -PICKUP_RANGE);
                    // hold.Position = new Vector3(0, 0, -camera.Position.DistanceTo(pickedObject.Position));
                }
                else pickedObject = null;
            }
        }
        if (@event.IsActionPressed("game_throw")) {
            if (pickedObject != null) {
                Vector3 diff = hold.GlobalPosition - camera.GlobalPosition;
                pickedObject.LinearVelocity = diff*4;
                StopHoldingObject();
            }
        }
    }

    private void StopHoldingObject() {
        pickedObject.AngularDamp = 0f;
        pickedObject = null;
    }

    public override void _Process(double delta) {
        ImGui.Begin("Player Config");
        if (ImGui.Button("Reset position")) Position = new Vector3(0, 10, 0);
        var pos = new System.Numerics.Vector3(Position.X, Position.Y, Position.Z);
        ImGui.InputFloat3("Position", ref pos);
        Position = new Vector3(pos.X, pos.Y, pos.Z);
        if (ImGui.Button("Spawn test cube")) {
            var cube = block.Instantiate<WorldObject>();
            cube.Position = hold.GlobalPosition;
            GetNode("/root/Game").AddChild(cube);
        }
        ImGui.End();

        if (Input.IsKeyPressed(Key.X)) {
            camera.Fov = 25f;
        } else {
            camera.Fov = 75f;
        }
    }
}

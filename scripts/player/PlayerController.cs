
using Godot;

public partial class PlayerController : CharacterBody3D
{
	[ExportGroup("Movement")]
	[Export] public float WalkSpeed = 5.0f;
	[Export] public float SprintSpeed = 8.0f;
	[Export] public float CrouchSpeed = 2.5f;
	[Export] public float JumpVelocity = 5.0f;
	[Export] public float Gravity = 18.0f;

	[ExportGroup("Mouse")]
	[Export] public float MouseSensitivity = 0.002f;

	[ExportGroup("Crouch")]
	[Export] public float StandHeight = 1.8f;
	[Export] public float CrouchHeight = 1.1f;
	[Export] public float StandCameraY = 1.65f;
	[Export] public float CrouchCameraY = 0.95f;

	[ExportGroup("Interaction")]
	[Export] public float InteractionDistance = 3.0f;

	private Camera3D _camera = null!;
	private CollisionShape3D _collision = null!;
	private CapsuleShape3D _capsule = null!;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera3D");
		_collision = GetNode<CollisionShape3D>("CollisionShape3D");

		// 玩家使用独立的胶囊碰撞体
		_capsule = (CapsuleShape3D)_collision.Shape.Duplicate();
		_collision.Shape = _capsule;

		_capsule.Height = StandHeight;
		_collision.Position = new Vector3(0, StandHeight / 2, 0);
		_camera.Position = new Vector3(0, StandCameraY, 0);

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Esc 释放鼠标
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		// 左键重新捕获
		if (@event is InputEventMouseButton button &&
			button.Pressed &&
			button.ButtonIndex == MouseButton.Left)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		// 鼠标视角
		if (@event is InputEventMouseMotion motion &&
			Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			RotateY(-motion.Relative.X * MouseSensitivity);

			_camera.RotateX(-motion.Relative.Y * MouseSensitivity);

			Vector3 rotation = _camera.Rotation;
			rotation.X = Mathf.Clamp(
				rotation.X,
				Mathf.DegToRad(-89),
				Mathf.DegToRad(89)
			);

			_camera.Rotation = rotation;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// 重力
		if (!IsOnFloor())
		{
			velocity.Y -= Gravity * (float)delta;
		}
		else
		{
			// 跳跃
			if (Input.IsActionJustPressed("jump"))
				velocity.Y = JumpVelocity;
		}

		// WASD 移动
		Vector2 input = Input.GetVector(
			"move_left",
			"move_right",
			"move_forward",
            "move_backward"
		);

		// 根据人物朝向计算移动方向
		Vector3 direction =
			(Transform.Basis * new Vector3(input.X, 0, input.Y))
			.Normalized();

		// 速度选择
		bool crouching = Input.IsActionPressed("crouch");

		float speed = WalkSpeed;

		if (crouching)
			speed = CrouchSpeed;
		else if (Input.IsActionPressed("sprint"))
			speed = SprintSpeed;

		// 直接移动，没有惯性
		velocity.X = direction.X * speed;
		velocity.Z = direction.Z * speed;

		Velocity = velocity;
		MoveAndSlide();

		// 蹲下
		float targetHeight = crouching ? CrouchHeight : StandHeight;
		float targetCameraY = crouching ? CrouchCameraY : StandCameraY;

		// 简单版本：立即改变碰撞体高度
		_capsule.Height = targetHeight;
		_collision.Position = new Vector3(0, targetHeight / 2, 0);

		Vector3 cameraPos = _camera.Position;
		cameraPos.Y = Mathf.MoveToward(
			cameraPos.Y,
			targetCameraY,
			5.0f * (float)delta
		);
		_camera.Position = cameraPos;

		// 互动
		if (Input.IsActionJustPressed("interact"))
			TryInteract();
	}

	private void TryInteract()
	{
		Vector3 from = _camera.GlobalPosition;
		Vector3 to = from -
			_camera.GlobalTransform.Basis.Z * InteractionDistance;

		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		query.CollideWithAreas = true;

		var result = GetWorld3D()
			.DirectSpaceState.IntersectRay(query);

		if (result.Count == 0)
			return;

		if (result["collider"].AsGodotObject() is Node target)
		{
			GD.Print("互动目标: " + target.Name);
		}
	}
}

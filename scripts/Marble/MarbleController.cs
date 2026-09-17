using Godot;

public partial class MarbleController : RigidBody3D
{
	[Export] public float MoveForce = 12.0f;
	[Export] public float MaxSpeed = 8.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 force = new Vector3(input.X, 0, input.Y) * MoveForce;

		if (LinearVelocity.Length() < MaxSpeed)
		{
			ApplyCentralForce(force);
		}
	}
}

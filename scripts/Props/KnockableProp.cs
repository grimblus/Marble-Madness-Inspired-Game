using Godot;

// A loose physics prop the marble can knock over (scenes/props/). Physics
// tuning - mass, friction, damping, layers - lives on the prop's base scene
// (e.g. VaseBase.tscn). Props start awake so one placed slightly above a tile
// drops onto it, then Godot's auto-sleep keeps it still until it's hit.
//
// Two things make hits read as "knocked it over":
// - Centre of mass: props are authored with their origin at the base, and
//   Godot's automatic centre of mass sits at the collision shape's origin -
//   the very bottom - so a fallen prop would swing back upright like a
//   weeble. It's moved up to a realistic height instead.
// - Topple push: the marble (0.5 m) strikes low on a vase's rounded body, so
//   plain physics mostly slides the prop or tips it back onto the marble.
//   When the marble arrives, the prop also gets a push high up in the
//   marble's direction of travel, scaled by the marble's speed. The
//   collision itself still takes speed off the marble as normal.
public partial class KnockableProp : RigidBody3D
{
	// Props that fall below this height (knocked off the level) are removed,
	// so they don't keep falling and simulating forever.
	[Export] public float FallLimitY = -50.0f;

	// Centre of mass height as a fraction of the collision shape's height.
	[Export] public float CenterOfMassHeight = 0.45f;

	// Marble hits slower than this (m/s) only nudge the prop physically.
	[Export] public float MinToppleSpeed = 1.0f;

	// Size of the extra push, as a fraction of mass x marble speed.
	// Higher = props fly further; 0 = plain physics only.
	[Export] public float ToppleStrength = 0.4f;

	// Where the push is applied, as a fraction of the shape's height.
	[Export] public float ToppleHeight = 0.65f;

	// The marble is detected by a copy of the prop's collision shapes scaled
	// up by this much, so it's caught just before contact - while its
	// velocity is still its approach speed, not what's left after the bounce.
	private const float HitAreaScale = 1.15f;

	private float _shapeHeight;

	public override void _Ready()
	{
		Aabb bounds = GetShapeBounds();
		_shapeHeight = bounds.Size.Y;

		Vector3 centre = bounds.GetCenter();
		CenterOfMassMode = CenterOfMassModeEnum.Custom;
		CenterOfMass = new Vector3(centre.X, bounds.Position.Y + _shapeHeight * CenterOfMassHeight, centre.Z);

		AddHitArea();
	}

	public override void _PhysicsProcess(double delta)
	{
		// A sleeping prop is resting on something, so it can't be falling.
		if (Sleeping)
		{
			return;
		}

		if (GlobalPosition.Y < FallLimitY)
		{
			QueueFree();
		}
	}

	private void OnHitAreaBodyEntered(Node3D body)
	{
		if (body is not RigidBody3D marble || !marble.IsInGroup("marble"))
		{
			return;
		}

		Vector3 travel = marble.LinearVelocity with { Y = 0.0f };
		if (travel.Length() < MinToppleSpeed)
		{
			return;
		}

		// Along the marble's travel, scaled by its speed and the prop's mass.
		Vector3 push = travel * Mass * ToppleStrength;
		ApplyImpulse(push, GlobalBasis.Y * _shapeHeight * ToppleHeight);
	}

	private void AddHitArea()
	{
		var area = new Area3D
		{
			Name = "HitArea",
			CollisionLayer = 0,
			CollisionMask = 2, // Marble
			Monitorable = false,
		};

		foreach (Node child in GetChildren())
		{
			if (child is CollisionShape3D { Shape: not null } shape)
			{
				area.AddChild(new CollisionShape3D
				{
					Shape = shape.Shape,
					Transform = shape.Transform.ScaledLocal(Vector3.One * HitAreaScale),
				});
			}
		}

		area.BodyEntered += OnHitAreaBodyEntered;
		AddChild(area);
	}

	// Combined bounds of the prop's collision shapes, in the prop's own space.
	private Aabb GetShapeBounds()
	{
		Aabb? bounds = null;
		foreach (Node child in GetChildren())
		{
			if (child is CollisionShape3D { Shape: not null } shape)
			{
				Aabb box = shape.Transform * shape.Shape.GetDebugMesh().GetAabb();
				bounds = bounds?.Merge(box) ?? box;
			}
		}

		return bounds ?? new Aabb(Vector3.Zero, Vector3.One);
	}
}

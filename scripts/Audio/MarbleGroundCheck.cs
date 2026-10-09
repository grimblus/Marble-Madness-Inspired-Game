using Godot;

// Ground detection shared by the marble's sound nodes (rolling and impact),
// so they agree on what "on the ground" means without each repeating the
// raycast.
public static class MarbleGroundCheck
{
	// Reads the radius from the marble's sphere collider (including its
	// scale), so changing the marble's size doesn't break the ground check.
	public static float GetRadius(RigidBody3D marble)
	{
		foreach (Node child in marble.GetChildren())
		{
			if (child is CollisionShape3D { Shape: SphereShape3D sphere } shape)
			{
				return sphere.Radius * shape.Scale.X;
			}
		}

		return 0.25f;
	}

	// Short ray straight down from the marble's centre, reaching `reach`
	// metres, against whatever the marble itself collides with. Returns the
	// surface normal if it hits.
	public static bool TryGetGroundNormal(RigidBody3D marble, float reach, out Vector3 normal)
	{
		Vector3 from = marble.GlobalPosition;
		Vector3 to = from + Vector3.Down * reach;

		var query = PhysicsRayQueryParameters3D.Create(from, to, marble.CollisionMask);
		query.Exclude = new Godot.Collections.Array<Rid> { marble.GetRid() };

		var hit = marble.GetWorld3D().DirectSpaceState.IntersectRay(query);
		normal = hit.Count > 0 ? (Vector3)hit["normal"] : Vector3.Up;
		return hit.Count > 0;
	}
}

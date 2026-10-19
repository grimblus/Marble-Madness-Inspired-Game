using Godot;
using System.Collections.Generic;

// Fades out backdrop props that come between the camera and the marble, so
// the tiles around the marble are never hidden behind a statue or vase. The
// see-through marble shows the marble itself through props, but not the
// floor it's rolling on; this clears the view of the area around it.
//
// Each top-level child of the current level's "Backdrop" node is treated as
// one prop. A prop fades when its bounding box covers a circle around the
// marble on screen and part of it is nearer the camera than the marble.
// Only the backdrop is affected, so tiles, vases and hazards stay solid.
public partial class BackdropFader : Node
{
	[Export] public NodePath CameraPath = "../CameraRig/Camera3D";
	[Export] public NodePath MarblePath = "../Marble";

	// Searched for the level's Backdrop node; levels are loaded under it.
	[Export] public NodePath LevelRootPath = "../LevelLoader";

	// Radius, in metres, of the area around the marble that's kept clear.
	[Export] public float ClearRadius = 3.0f;

	// Transparency of a faded prop (0 = solid, 1 = invisible).
	[Export] public float FadedTransparency = 0.8f;

	// How quickly props fade out and back in, in transparency per second.
	[Export] public float FadeSpeed = 4.0f;

	private sealed class Prop
	{
		public readonly List<GeometryInstance3D> Meshes = new();
		public Vector3[] Corners;
		public float Transparency;
	}

	private Camera3D _camera;
	private Node3D _marble;
	private Node _levelRoot;
	private Node3D _backdrop;
	private readonly List<Prop> _props = new();

	public override void _Ready()
	{
		_camera = GetNodeOrNull<Camera3D>(CameraPath);
		_marble = GetNodeOrNull<Node3D>(MarblePath);
		_levelRoot = GetNodeOrNull(LevelRootPath);
	}

	public override void _Process(double delta)
	{
		if (_camera == null || _marble == null || _levelRoot == null)
		{
			return;
		}

		// The level (and with it the backdrop) can be swapped at any time,
		// so look it up again whenever the cached one is gone.
		if (!IsInstanceValid(_backdrop))
		{
			FindProps();
			if (_backdrop == null)
			{
				return;
			}
		}

		// Camera space: x/y across the screen, z towards the camera. The
		// camera is orthographic, so screen overlap is a plain x/y test.
		Transform3D toCamera = _camera.GlobalTransform.AffineInverse();
		Vector3 marble = toCamera * _marble.GlobalPosition;
		float step = FadeSpeed * (float)delta;

		foreach (Prop prop in _props)
		{
			float target = Blocks(prop, toCamera, marble) ? FadedTransparency : 0.0f;
			if (Mathf.IsEqualApprox(prop.Transparency, target))
			{
				continue;
			}

			prop.Transparency = Mathf.MoveToward(prop.Transparency, target, step);
			foreach (GeometryInstance3D mesh in prop.Meshes)
			{
				mesh.Transparency = prop.Transparency;
			}
		}
	}

	private bool Blocks(Prop prop, Transform3D toCamera, Vector3 marble)
	{
		Vector3 min = Vector3.Inf;
		Vector3 max = -Vector3.Inf;
		foreach (Vector3 corner in prop.Corners)
		{
			Vector3 p = toCamera * corner;
			min = min.Min(p);
			max = max.Max(p);
		}

		// Nothing of the prop is in front of the marble.
		if (max.Z <= marble.Z)
		{
			return false;
		}

		// Distance on screen from the marble to the nearest point of the box.
		float dx = Mathf.Max(Mathf.Max(min.X - marble.X, marble.X - max.X), 0.0f);
		float dy = Mathf.Max(Mathf.Max(min.Y - marble.Y, marble.Y - max.Y), 0.0f);
		return dx * dx + dy * dy < ClearRadius * ClearRadius;
	}

	private void FindProps()
	{
		_props.Clear();
		_backdrop = _levelRoot.FindChild("Backdrop", true, false) as Node3D;
		if (_backdrop == null)
		{
			return;
		}

		// Backdrop props don't move, so their bounds are measured once.
		foreach (Node child in _backdrop.GetChildren())
		{
			var prop = new Prop();
			Aabb? bounds = null;
			foreach (Node node in child.FindChildren("*", "GeometryInstance3D", true, false))
			{
				var mesh = (GeometryInstance3D)node;
				prop.Meshes.Add(mesh);
				Aabb box = mesh.GlobalTransform * mesh.GetAabb();
				bounds = bounds?.Merge(box) ?? box;
			}
			if (child is GeometryInstance3D self)
			{
				prop.Meshes.Add(self);
				Aabb box = self.GlobalTransform * self.GetAabb();
				bounds = bounds?.Merge(box) ?? box;
			}
			if (!bounds.HasValue)
			{
				continue;
			}

			prop.Corners = new Vector3[8];
			for (int i = 0; i < 8; i++)
			{
				prop.Corners[i] = bounds.Value.GetEndpoint(i);
			}
			_props.Add(prop);
		}
	}
}

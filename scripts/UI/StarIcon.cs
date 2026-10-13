using Godot;

// A five-point star drawn in code (no image needed), either filled gold or an
// empty outline. Used by the result screen. Set Filled from code or the
// Inspector; size it with the Control's Custom Minimum Size.

public partial class StarIcon : Control
{
	private bool _filled;

	[Export]
	public bool Filled
	{
		get => _filled;
		set
		{
			_filled = value;
			QueueRedraw();
		}
	}

	[Export] public Color FilledColor { get; set; } = new Color("B89A63");
	[Export] public Color FilledOutline { get; set; } = new Color("8F7440");
	[Export] public Color EmptyColor { get; set; } = new Color(0.145f, 0.145f, 0.145f, 0.05f);
	[Export] public Color EmptyOutline { get; set; } = new Color("C4BBA8");

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		Vector2 center = Size / 2.0f;
		float outer = Mathf.Min(Size.X, Size.Y) / 2.0f - 2.0f;
		float inner = outer * 0.45f;

		var points = new Vector2[10];
		for (int i = 0; i < 10; i++)
		{
			float angle = -Mathf.Pi / 2.0f + i * Mathf.Pi / 5.0f;
			float radius = i % 2 == 0 ? outer : inner;
			points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}

		DrawColoredPolygon(points, Filled ? FilledColor : EmptyColor);

		var outline = new Vector2[11];
		points.CopyTo(outline, 0);
		outline[10] = points[0];
		DrawPolyline(outline, Filled ? FilledOutline : EmptyOutline, 2.0f, true);
	}
}

using Godot;

// A five-point star drawn in code (no image needed), either filled gold or an
// empty outline. Used by the result screen. Set Filled from code or the
// Inspector; size it with the Control's Custom Minimum Size.

public partial class StarIcon : Control
{
	private bool _filled;

	// Animation state, driven by tweens (see PlayShine).
	private float _appear = 1.0f; // 0..1 pop-in size (1 = fully drawn)
	private float _sweep;         // 0..1 progress of the glint across the star
	private Tween _popTween;
	private Tween _loopTween;

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

	// Seconds of rest between glints.
	[Export] public float ShineInterval { get; set; } = 2.2f;

	// Pops a filled star in after `delay` seconds, then keeps it shining:
	// a glint sweeps across it with a soft glow and two sparkles.
	public void PlayShine(float delay = 0.0f)
	{
		_popTween?.Kill();
		_loopTween?.Kill();

		_appear = 0.0f;
		_sweep = 0.0f;
		QueueRedraw();

		_popTween = CreateTween();
		_popTween.TweenInterval(delay);
		_popTween.TweenMethod(Callable.From<float>(SetAppear), 0.0f, 1.0f, 0.45f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
		_popTween.TweenCallback(Callable.From(StartShineLoop));
	}

	private void StartShineLoop()
	{
		_loopTween = CreateTween().SetLoops();
		_loopTween.TweenMethod(Callable.From<float>(SetSweep), 0.0f, 1.0f, 0.8f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		_loopTween.TweenInterval(ShineInterval);
	}

	private void SetAppear(float value)
	{
		_appear = value;
		QueueRedraw();
	}

	private void SetSweep(float value)
	{
		_sweep = value;
		QueueRedraw();
	}

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
		float baseOuter = Mathf.Min(Size.X, Size.Y) / 2.0f - 2.0f;

		// Empty stars, and the empty slot behind a filled star while it pops in.
		if (!Filled || _appear < 1.0f)
		{
			DrawStarShape(StarPoints(center, baseOuter), EmptyColor, EmptyOutline);
		}

		if (!Filled)
		{
			return;
		}

		float outer = baseOuter * _appear;
		if (outer <= 0.5f)
		{
			return;
		}

		// 0 at the start and end of a glint, 1 in the middle.
		float envelope = Mathf.Sin(_sweep * Mathf.Pi);

		// Soft glow behind the star.
		if (envelope > 0.01f)
		{
			DrawCircle(center, baseOuter * 1.3f, new Color(1.0f, 0.9f, 0.6f, 0.22f * envelope));
		}

		Vector2[] points = StarPoints(center, outer);
		DrawStarShape(points, FilledColor, FilledOutline);

		// The glint: a slanted white band sweeping left to right, clipped to the star.
		if (_sweep > 0.0f && _sweep < 1.0f)
		{
			float x = Mathf.Lerp(center.X - outer * 1.5f, center.X + outer * 1.5f, _sweep);
			float halfWidth = outer * 0.22f;
			float slant = outer * 0.5f;
			float top = center.Y - outer * 1.2f;
			float bottom = center.Y + outer * 1.2f;

			var band = new[]
			{
				new Vector2(x - halfWidth + slant, top),
				new Vector2(x + halfWidth + slant, top),
				new Vector2(x + halfWidth - slant, bottom),
				new Vector2(x - halfWidth - slant, bottom)
			};

			foreach (var piece in Geometry2D.IntersectPolygons(points, band))
			{
				if (piece.Length >= 3)
				{
					DrawColoredPolygon(piece, new Color(1.0f, 1.0f, 1.0f, 0.8f));
				}
			}
		}

		// Two small sparkles near the star while it glints.
		if (envelope > 0.01f)
		{
			DrawSparkle(center + new Vector2(outer * 0.95f, -outer * 0.95f), outer * 0.4f * envelope, envelope);
			DrawSparkle(center + new Vector2(-outer * 1.0f, -outer * 0.25f), outer * 0.28f * envelope, envelope);
		}
	}

	private static Vector2[] StarPoints(Vector2 center, float outer)
	{
		float inner = outer * 0.45f;
		var points = new Vector2[10];
		for (int i = 0; i < 10; i++)
		{
			float angle = -Mathf.Pi / 2.0f + i * Mathf.Pi / 5.0f;
			float radius = i % 2 == 0 ? outer : inner;
			points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		return points;
	}

	private void DrawStarShape(Vector2[] points, Color fill, Color outlineColor)
	{
		DrawColoredPolygon(points, fill);

		var outline = new Vector2[11];
		points.CopyTo(outline, 0);
		outline[10] = points[0];
		DrawPolyline(outline, outlineColor, 2.0f, true);
	}

	// A small four-point sparkle.
	private void DrawSparkle(Vector2 position, float size, float alpha)
	{
		var points = new Vector2[8];
		for (int i = 0; i < 8; i++)
		{
			float angle = i * Mathf.Pi / 4.0f;
			float radius = i % 2 == 0 ? size : size * 0.22f;
			points[i] = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		DrawColoredPolygon(points, new Color(1.0f, 0.97f, 0.85f, alpha));
	}
}

using System.Collections.Generic;
using Godot;

// Draws the stylised museum art used by the level select screen (the backdrop
// and the placeholder level previews). Every method draws onto the CanvasItem
// it is given, so call these from that item's _Draw().

public static class GalleryArt
{
	public static readonly Color Wall = new Color("EDE7DA");
	public static readonly Color Skirting = new Color("E3DCCB");
	public static readonly Color Floor = new Color("E8E2D4");
	public static readonly Color FloorLine = new Color("D3CAB7");
	public static readonly Color ArchFill = new Color("F9F7F2");
	public static readonly Color Gold = new Color("B89A63");
	public static readonly Color Pedestal = new Color("F3EEE3");
	public static readonly Color PedestalLine = new Color("D9D0BD");
	public static readonly Color Stone = new Color("CFC8BA");
	public static readonly Color Cyan = new Color("35C7D9");
	public static readonly Color CyanDark = new Color("1B8FA3");
	public static readonly Color CyanLight = new Color("8BE3EE");

	// An arch standing on baseY: straight sides with a semicircle on top.
	public static void DrawArch(CanvasItem c, float left, float baseY, float width, float height,
		float strokeWidth, bool fill)
	{
		float r = width / 2.0f;
		float springY = baseY - height + r; // where the curve starts

		var points = new List<Vector2>
		{
			new Vector2(left, baseY),
			new Vector2(left, springY)
		};
		const int steps = 16;
		for (int i = 1; i < steps; i++)
		{
			float a = Mathf.Pi * i / steps;
			points.Add(new Vector2(left + r - Mathf.Cos(a) * r, springY - Mathf.Sin(a) * r));
		}
		points.Add(new Vector2(left + width, springY));
		points.Add(new Vector2(left + width, baseY));

		Vector2[] array = points.ToArray();
		if (fill)
		{
			c.DrawColoredPolygon(array, ArchFill);
		}
		c.DrawPolyline(array, Gold, strokeWidth, true);
	}

	// A pedestal with a rough stone bust on top. (left, top) is the pedestal's top-left corner.
	public static void DrawPedestal(CanvasItem c, float left, float top, float width, float height)
	{
		var rect = new Rect2(left, top, width, height);
		c.DrawRect(rect, Pedestal);
		c.DrawRect(rect, PedestalLine, false, 2.0f);

		float cx = left + width / 2.0f;
		c.DrawColoredPolygon(new[]
		{
			new Vector2(left + width * 0.05f, top),
			new Vector2(cx - width * 0.25f, top - height * 0.18f),
			new Vector2(cx, top - height * 0.24f),
			new Vector2(cx + width * 0.25f, top - height * 0.18f),
			new Vector2(left + width * 0.95f, top)
		}, Stone);
		c.DrawCircle(new Vector2(cx, top - height * 0.36f), width * 0.29f, Stone);
	}

	public static void DrawEllipse(CanvasItem c, Vector2 center, float rx, float ry, Color color)
	{
		const int steps = 24;
		var points = new Vector2[steps];
		for (int i = 0; i < steps; i++)
		{
			float a = Mathf.Tau * i / steps;
			points[i] = center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
		}
		c.DrawColoredPolygon(points, color);
	}

	// The cyan marble with its shadow, swirls and a highlight.
	public static void DrawMarble(CanvasItem c, Vector2 center, float r)
	{
		DrawEllipse(c, center + new Vector2(r * 0.14f, r * 0.95f), r * 1.1f, r * 0.2f,
			new Color(0.145f, 0.145f, 0.145f, 0.18f));

		c.DrawCircle(center, r, Cyan);

		c.DrawPolyline(Quad(center, r, new Vector2(-0.86f, -0.10f), new Vector2(-0.14f, -0.79f), new Vector2(0.72f, -0.24f)),
			CyanDark, r * 0.1f, true);
		c.DrawPolyline(Quad(center, r, new Vector2(-0.79f, 0.55f), new Vector2(0.0f, 0.10f), new Vector2(0.70f, 0.55f)),
			CyanLight, r * 0.09f, true);

		DrawEllipse(c, center + new Vector2(-0.41f * r, -0.52f * r), r * 0.28f, r * 0.17f,
			new Color(1.0f, 1.0f, 1.0f, 0.75f));
	}

	// Points along a quadratic curve, given in units of the marble's radius.
	private static Vector2[] Quad(Vector2 center, float r, Vector2 p0, Vector2 p1, Vector2 p2)
	{
		const int steps = 12;
		var points = new Vector2[steps + 1];
		for (int i = 0; i <= steps; i++)
		{
			float t = (float)i / steps;
			Vector2 p = p0 * ((1 - t) * (1 - t)) + p1 * (2 * (1 - t) * t) + p2 * (t * t);
			points[i] = center + p * r;
		}
		return points;
	}

	// Wall, skirting board, floor and the perspective lines on the floor.
	// width x height is the art's own coordinate space; wallBottom is where the wall meets the floor.
	public static void DrawRoom(CanvasItem c, float width, float height, float wallBottom)
	{
		float skirting = height * 0.025f;
		c.DrawRect(new Rect2(0, 0, width, wallBottom), Wall);
		c.DrawRect(new Rect2(0, wallBottom - skirting, width, skirting), Skirting);
		c.DrawRect(new Rect2(0, wallBottom, width, height - wallBottom), Floor);

		float vanishX = width / 2.0f;
		float lineWidth = height / 400.0f;
		foreach (float f in new[] { -0.7f, -0.45f, -0.2f, 0.0f, 0.2f, 0.45f, 0.7f })
		{
			c.DrawLine(new Vector2(vanishX, wallBottom), new Vector2(vanishX + f * width, height),
				FloorLine, lineWidth, true);
		}

		float span = height - wallBottom;
		foreach (float f in new[] { 0.1f, 0.25f, 0.5f, 0.75f })
		{
			float y = wallBottom + span * f;
			c.DrawLine(new Vector2(0, y), new Vector2(width, y), FloorLine, lineWidth, true);
		}
	}
}

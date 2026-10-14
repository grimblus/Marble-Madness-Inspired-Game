using Godot;

// The picture on a level card. Shows Screenshot if one is assigned, otherwise a
// stylised placeholder scene (which varies with Variant so each level looks a
// little different). When Locked, it is veiled and shows a padlock.

public partial class LevelPreview : Control
{
	private const float ArtWidth = 464.0f;
	private const float ArtHeight = 236.0f;

	private static readonly float[] ArchX = { 60, 190, 340, 120, 280 };
	private static readonly float[] MarbleX = { 300, 190, 120, 340, 250 };

	private Texture2D _screenshot;
	private int _variant;
	private bool _locked;

	[Export]
	public Texture2D Screenshot
	{
		get => _screenshot;
		set
		{
			_screenshot = value;
			QueueRedraw();
		}
	}

	public int Variant
	{
		get => _variant;
		set
		{
			_variant = value;
			QueueRedraw();
		}
	}

	public bool Locked
	{
		get => _locked;
		set
		{
			_locked = value;
			QueueRedraw();
		}
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
		if (Screenshot != null)
		{
			DrawScreenshot();
		}
		else
		{
			DrawPlaceholder();
		}

		if (Locked)
		{
			DrawLockOverlay();
		}
	}

	// Fills the control with the screenshot, cropping the edges to keep its proportions.
	private void DrawScreenshot()
	{
		Vector2 textureSize = Screenshot.GetSize();
		float scale = Mathf.Max(Size.X / textureSize.X, Size.Y / textureSize.Y);
		Vector2 sourceSize = Size / scale;
		Vector2 sourcePosition = (textureSize - sourceSize) / 2.0f;

		DrawTextureRectRegion(Screenshot, new Rect2(Vector2.Zero, Size), new Rect2(sourcePosition, sourceSize));
	}

	private void DrawPlaceholder()
	{
		float scale = Mathf.Max(Size.X / ArtWidth, Size.Y / ArtHeight);
		Vector2 offset = (Size - new Vector2(ArtWidth, ArtHeight) * scale) / 2.0f;
		DrawRect(new Rect2(Vector2.Zero, Size), GalleryArt.Wall);
		DrawSetTransform(offset, 0.0f, new Vector2(scale, scale));

		int v = Mathf.PosMod(Variant, ArchX.Length);

		GalleryArt.DrawRoom(this, ArtWidth, ArtHeight, 158.0f);
		GalleryArt.DrawArch(this, ArchX[v], 158, 80, 58 + 40, 5, true);
		GalleryArt.DrawPedestal(this, 396, 130, 40, 28);
		GalleryArt.DrawArch(this, 60, 224, 32, 16 + 16, 5, false);
		GalleryArt.DrawMarble(this, new Vector2(MarbleX[v], 196), 20);

		DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
	}

	private void DrawLockOverlay()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.97f, 0.96f, 0.94f, 0.62f));

		Vector2 c = Size / 2.0f;
		Color ink = new Color("252525");

		// Shackle, body and keyhole.
		DrawArc(new Vector2(c.X, c.Y - 4), 10.0f, Mathf.Pi, Mathf.Tau, 16, ink, 2.5f, true);
		DrawRect(new Rect2(c.X - 15, c.Y - 4, 30, 24), ink, false, 2.5f);
		DrawCircle(new Vector2(c.X, c.Y + 8), 2.5f, ink);
	}
}

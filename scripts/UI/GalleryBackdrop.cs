using Godot;

// Full-screen museum backdrop for the menus (wall, floor, arches, pedestals and
// the marble), drawn in code. It scales to cover the whole screen at any window size.
public partial class GalleryBackdrop : Control

{
	private const float ArtWidth = 1440.0f;
	private const float ArtHeight = 810.0f;

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		// Fill everything first, then draw the art scaled to cover the control.
		DrawRect(new Rect2(Vector2.Zero, Size), GalleryArt.Wall);

		float scale = Mathf.Max(Size.X / ArtWidth, Size.Y / ArtHeight);
		Vector2 offset = (Size - new Vector2(ArtWidth, ArtHeight) * scale) / 2.0f;
		DrawSetTransform(offset, 0.0f, new Vector2(scale, scale));

		GalleryArt.DrawRoom(this, ArtWidth, ArtHeight, 520.0f);

		// Far arches, then the big arches on each side.
		GalleryArt.DrawArch(this, 620, 520, 90, 165, 5, true);
		GalleryArt.DrawArch(this, 730, 520, 90, 165, 5, true);
		GalleryArt.DrawArch(this, 90, 520, 200, 290, 8, true);
		GalleryArt.DrawArch(this, 1150, 520, 200, 290, 8, true);

		GalleryArt.DrawPedestal(this, 350, 420, 90, 100);
		GalleryArt.DrawPedestal(this, 1000, 420, 90, 100);

		// Gold arch lying on the floor, and the marble.
		GalleryArt.DrawArch(this, 170, 720, 90, 95, 10, false);
		GalleryArt.DrawMarble(this, new Vector2(1230, 696), 58);

		DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
	}
}

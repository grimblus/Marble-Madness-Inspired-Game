using Godot;

// Trigger volume placed below a level's geometry (scenes/triggers/KillZone.tscn),
// or anywhere else a fall should be fatal. Anything with a public Die()
// method is called and left to respawn itself - this is the same entry
// point a future hazard (moving or environmental) should call on contact,
// so a kill zone and a hazard both send the marble back to its last
// checkpoint the same way, with no shared type dependency between them.
public partial class KillZone : Area3D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body.HasMethod("Die"))
		{
			body.Call("Die");
		}
	}
}

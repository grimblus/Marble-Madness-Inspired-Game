using Godot;

// Wet-floor hazard (scenes/hazards/stationary_hazard.tscn): a puddle beside a
// knock-over warning sign. Rolling through the puddle makes the marble lose
// control for a moment (MarbleController.GetHit), while it keeps its speed.
public partial class StationaryHazard : Node3D
{
	// Connected to the Puddle area's body_entered signal in the scene.
	public void OnPuddleEnter(Node3D body)
	{
		if (body is MarbleController marble)
		{
			marble.GetHit();
		}
	}
}

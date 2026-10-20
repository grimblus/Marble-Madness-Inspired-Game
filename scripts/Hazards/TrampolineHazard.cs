using Godot;

// Trampoline hazard (scenes/hazards/trampoline_hazard.tscn). Rolling onto it
// launches the marble upwards (MarbleController.GetHitTramp), so levels can
// put ledges and gaps that are only reachable with a bounce. The bounce sound
// is a child AudioStreamPlayer played from the same body_entered signal.
public partial class TrampolineHazard : Area3D
{
	// Connected to this area's own body_entered signal in the scene.
	private void OnBodyEntered(Node3D body)
	{
		if (body is MarbleController marble)
		{
			marble.GetHitTramp(GlobalPosition);
		}
	}
}

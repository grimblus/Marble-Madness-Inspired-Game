using Godot;

// Wind blower hazard (scenes/hazards/blower_hazard.tscn): a well that blows
// the marble away along its wind cone (BlowArea). The force logic is shared
// with the vacuum well in RadialForceHazard.
public partial class BlowerHazard : RadialForceHazard
{
	// Push at the blower's mouth, in newtons. Fades out towards BlowRadius.
	[Export] public float BlowForce = 20.0f;

	// Horizontal distance, in metres, at which the push fades to nothing.
	[Export] public float BlowRadius = 5.0f;

	protected override float Force => BlowForce;
	protected override float Radius => BlowRadius;
	protected override float Direction => 1.0f;

	// Connected to BlowArea's body_entered and body_exited in the scene.
	public void OnBlowerEnter(Node3D body) => Track(body);
	public void OnBlowerExit(Node3D body) => Untrack(body);
}

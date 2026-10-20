using Godot;

// Vacuum well hazard (scenes/hazards/vacuum_hazard.tscn): a well that pulls
// the marble in along its wind cone (VacuumArea), so a careless line drags
// it off the edge. The force logic is shared with the blower in
// RadialForceHazard.
public partial class VacuumHazard : RadialForceHazard
{
	// Pull at the well's mouth, in newtons. Fades out towards VacuumRadius.
	[Export] public float VacuumForce = 20.0f;

	// Horizontal distance, in metres, at which the pull fades to nothing.
	[Export] public float VacuumRadius = 5.0f;

	protected override float Force => VacuumForce;
	protected override float Radius => VacuumRadius;
	protected override float Direction => -1.0f;

	// Connected to VacuumArea's body_entered and body_exited in the scene.
	public void OnVacuumEnter(Node3D body) => Track(body);
	public void OnVacuumExit(Node3D body) => Untrack(body);
}

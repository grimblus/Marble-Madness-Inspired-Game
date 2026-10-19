using Godot;
using System;

public partial class MovingHazard : RigidBody3D
{
	[Export] public Curve3D Pattern;
	[Export] public float PatternSpeed = 0.75f;
	[Export] public Path3D PatternPath;
	private Vector3 spawnPosition;
	private float spawnRotation;
	private float distanceTravelled = 0.0f;
	private float patternLength;
	private MarbleController marble;
	
	//so the path3d increases with scale
	private Transform3D pathTransform;

	// Shifts the whole path so it starts exactly where the hazard was placed
	// in the level. The path's first point doesn't sit on the Path3D's
	// origin, so following the raw path made every hazard jump up off the
	// floor when the level started.
	private Vector3 pathStartOffset;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (PatternPath != null && PatternPath.Curve != null)
		{
			Pattern = PatternPath.Curve;
			pathTransform = PatternPath.GlobalTransform;
		}
		else
		{
			pathTransform = GlobalTransform;
		}
		
		if (Pattern != null)
			patternLength = Pattern.GetBakedLength();
			
		if (PatternPath != null)
		{
			Pattern = PatternPath.Curve;
			patternLength = Pattern.GetBakedLength();
		}
		
		Freeze = true;
		FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
		ContactMonitor = true;
		MaxContactsReported = 4;
		
		spawnPosition = GlobalPosition;
		spawnRotation = GlobalRotation.Y;

		if (Pattern != null && patternLength > 0)
		{
			pathStartOffset = GlobalPosition - pathTransform * Pattern.SampleBaked(0.0f);
		}
		
		marble = GetNode<MarbleController>("/root/Main/Marble");
				
		AddToGroup("enemies");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (Pattern == null || patternLength <= 0)
			return;

		distanceTravelled += PatternSpeed * (float)delta;

		if (distanceTravelled >= patternLength)
		{
			distanceTravelled = 0f;
			return;
		}
		
		Vector3 localOffset = Pattern.SampleBaked(distanceTravelled);
		Vector3 rotatedOffset = localOffset.Rotated(Vector3.Up, spawnRotation);

		//GlobalPosition = spawnPosition + rotatedOffset;
		GlobalPosition = pathTransform * Pattern.SampleBaked(distanceTravelled) + pathStartOffset;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnBodyEntered(Node3D body)
	{
		if (body is MarbleController marbleController)
		{
			marbleController.GetHitPhys(GlobalPosition);
		}
	}
}

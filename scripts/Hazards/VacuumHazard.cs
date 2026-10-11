using Godot;
using System;

public partial class VacuumHazard : RigidBody3D
{
	[Export] public Area3D VacuumArea;
	[Export] public float VacuumForce = 20.0f;
	[Export] public float VacuumRadius = 5.0f;
	
	private MarbleController marble;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Freeze = true;
		FreezeMode = RigidBody3D.FreezeModeEnum.Static;
		
		marble = GetNode<MarbleController>("/root/Main/Marble");
				
		AddToGroup("enemies");
		
		//VacuumArea.BodyEntered += OnVacuumEnter;
		//VacuumArea.BodyExited += OnVacuumExit;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnVacuumEnter(Node3D body)
	{
		if (body is MarbleController marbleController)
		{
			marble = marbleController;
		}
	}
	
	public void OnVacuumExit(Node3D body)
	{
		if (body == marble)
		{
			marble = null;
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (marble == null)
		{
			return;
		}

		Vector3 toVacuum = GlobalPosition - marble.GlobalPosition;
		toVacuum.Y = 0;
		float distance = toVacuum.Length();

		if (distance < 0.01f)
		{
			return;
		}

		//Closer player is, stronger the suc
		float strength = Mathf.Clamp(1.0f - distance / VacuumRadius, 0.0f, 1.0f);

		marble.Sleeping = false;
		marble.ApplyCentralForce(toVacuum.Normalized() * VacuumForce * strength);
	}
}

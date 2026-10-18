using Godot;
using System;

public partial class TrampolineHazard : Area3D
{
	private MarbleController marble;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		marble = GetNode<MarbleController>("/root/Main/Marble");
				
		AddToGroup("enemies");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnBodyEntered(Node3D body)
	{
		if (body is MarbleController marbleController)
		{
			marbleController.GetHitTramp(GlobalPosition);
		}
	}
}

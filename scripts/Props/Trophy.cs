using Godot;
using System;

public partial class Trophy : Area3D
{
	[Export]
	public string levelId  { get; set; } = "level_01";
	
	[Signal]
	public delegate void CollectedEventHandler(int value);
	
	private bool collected = false;
	
	public override void _Ready() {
		BodyEntered += OnBodyEntered;
		
		if (TrophyManager.Instance.HasTrophy(levelId)) {
			QueueFree();
		}
	}

	private void OnBodyEntered(Node3D body) {
		if (collected || body is not MarbleController) {
			return;
		}

		collected = true;

		TrophyManager.Instance.UnlockTrophy(levelId);

		QueueFree();
	}
}

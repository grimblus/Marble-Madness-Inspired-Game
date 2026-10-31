using Godot;
using System;

public partial class Trophy : Area3D
{
	[Export] public string levelId  { get; set; } = "level_01";
	[Export] public float rotationSpeed { get; set; } = 1.5f;
	[Export] public float floatHeight { get; set; } = 0.15f;
	[Export] public float floatSpeed { get; set; } = 2.0f;

	private float initialY;
	private float elapsedTime;

	private Node3D winnerCup;
	
	[Signal]
	public delegate void CollectedEventHandler(int value);
	
	private bool collected = false;
	
	public override void _Ready() {
		winnerCup = GetNode<Node3D>("WinnerCup");
		initialY = winnerCup.Position.Y;
		
		BodyEntered += OnBodyEntered;
		
		if (TrophyManager.Instance.HasTrophy(levelId)) {
			QueueFree();
		}
	}
	
	public override void _Process(double delta)
	{
		elapsedTime += (float)delta;
		
		winnerCup.RotateY((float)delta * rotationSpeed);
		
		Vector3 position = winnerCup.Position;
		position.Y = initialY + Mathf.Sin(elapsedTime * floatSpeed) * floatHeight;

		winnerCup.Position = position;
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

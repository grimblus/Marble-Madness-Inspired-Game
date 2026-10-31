using Godot;
using System;
using System.Collections.Generic;

public partial class TrophyManager : Node
{
	public static TrophyManager Instance { get; private set; }
	
	[Signal]
	public delegate void TrophyUnlockedEventHandler(string levelId);
	
	private readonly HashSet<string> collectedTrophies = new();
	
	public override void _EnterTree() {
		Instance = this;
	}
	
	public override void _ExitTree() {
		if (Instance == this) {
			Instance = null;
		}
	}
	
	public bool HasTrophy(string levelId) {
		return collectedTrophies.Contains(levelId);
	}
	
	public void UnlockTrophy(string levelId) {
		if (!collectedTrophies.Add(levelId)) {
			return;
		}
		
		EmitSignal(SignalName.TrophyUnlocked, levelId);
	}
}

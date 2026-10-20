using Godot;

// Add to any scene that should have background music and set Track.
// When the scene loads it asks the MusicPlayer autoload to play the track;
// the player ignores the request if that track is already playing, so a
// Retry reload doesn't restart the music from the beginning.
public partial class MusicCue : Node
{
	// Music for this scene. Leave empty to keep whatever is already playing.
	[Export] public AudioStream Track;

	public override void _Ready()
	{
		GetNode<MusicPlayer>("/root/MusicPlayer").Play(Track);
	}
}

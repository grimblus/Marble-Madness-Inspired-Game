using Godot;

// Autoloaded singleton (see project.godot [autoload]) that owns background
// music. It sits above the current scene, so music carries on through
// scene changes and Retry reloads instead of restarting each time.
// Scenes don't play music themselves - they include a MusicCue naming their
// track, and this crossfades to it (or does nothing if that track is
// already playing). It deliberately doesn't listen for win/lose, so the
// level's music keeps playing behind the win and game over screens.
public partial class MusicPlayer : Node
{
	private const float FadeSeconds = 1.5f;

	// Two players so the outgoing track can fade out while the new one fades in.
	private AudioStreamPlayer _current;
	private AudioStreamPlayer _previous;
	private Tween _fade;

	public override void _Ready()
	{
		// Keep playing (and fading) while the tree is paused, e.g. behind a pause menu.
		ProcessMode = ProcessModeEnum.Always;

		_current = new AudioStreamPlayer();
		_previous = new AudioStreamPlayer();
		AddChild(_current);
		AddChild(_previous);
	}

	public void Play(AudioStream track)
	{
		if (track == null || (_current.Playing && _current.Stream == track))
		{
			return;
		}

		SetLooping(track);

		// A fade still in progress is simply taken over from wherever it got to.
		_fade?.Kill();
		(_current, _previous) = (_previous, _current);

		_current.Stream = track;
		_current.VolumeLinear = 0.0f;
		_current.Play();

		AudioStreamPlayer outgoing = _previous;
		_fade = CreateTween().SetParallel();
		_fade.TweenProperty(_current, "volume_linear", 1.0f, FadeSeconds);
		if (outgoing.Playing)
		{
			_fade.TweenProperty(outgoing, "volume_linear", 0.0f, FadeSeconds);
		}
		_fade.Chain().TweenCallback(Callable.From(outgoing.Stop));
	}

	// Music should always loop. Setting it here means a track dropped into
	// assets/audio/music works without anyone remembering to tick Loop in
	// its import settings.
	private static void SetLooping(AudioStream track)
	{
		switch (track)
		{
			case AudioStreamMP3 mp3:
				mp3.Loop = true;
				break;
			case AudioStreamOggVorbis ogg:
				ogg.Loop = true;
				break;
		}
	}
}

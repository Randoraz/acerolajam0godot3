using Godot;
using System;

public partial class MusicController : Node
{
	private static AudioStreamPlayer audioPlayer;
	private static AudioStreamPlayer click;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		click = GetNode<AudioStreamPlayer>("ClickAudio");
		Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public static void Play()
	{
		audioPlayer.Play();
	}

	public static void Stop()
	{
		audioPlayer.Stop();
	}

	public static void PlayClick()
	{
		click.Play();
	}
}

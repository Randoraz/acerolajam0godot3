using Godot;
using System;

public partial class TitleScreen : CanvasLayer
{
	private bool fullscreen = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("exit_game"))
		{
			GetTree().Quit(0);
		}

		if (Input.IsActionJustPressed("toggle_screen"))
		{
			if (fullscreen)
			{
				OS.WindowFullscreen = false;
				OS.WindowSize = new Vector2(1280, 720);
				fullscreen = false;
			}
			else
			{
				OS.WindowFullscreen = true;
				fullscreen = true;
			}
		}
	}
	
	private void OnStartButtonPressed()
	{
		MusicController.PlayClick();
		GetTree().ChangeScene("res://scenes/main.tscn");
	}
}

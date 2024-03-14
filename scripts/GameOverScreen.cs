using Godot;
using System;

public partial class GameOverScreen : CanvasLayer
{
	[Signal]
	public delegate void GameOverTimeOutEventHandler();
	[Signal]
	public delegate void RestartButtonPressedEventHandler();

	private AnimatedSprite slashSprite;
	private Label gameOverLabel;

	private AudioStreamPlayer slashAudio;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		slashSprite = GetNode<AnimatedSprite>("SlashScreen");
		gameOverLabel = GetNode<Label>("Label");

		slashAudio = GetNode<AudioStreamPlayer>("SlashAudio");

		gameOverLabel.Text = "You were knocked out!\r\nTry again!";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public void ShowGameOverScreen(bool showSlash = false)
	{
		Show();

		if(showSlash)
		{
			slashAudio.Play();
			slashSprite.Show();
			slashSprite.Frame = 0;
			slashSprite.Play();
		}
		else
		{
			gameOverLabel.Text = "You won!";
			slashSprite.Hide();
		}
	}
	
	private void OnRestartButtonPressed()
	{
		MusicController.PlayClick();
		EmitSignal("RestartButtonPressedEventHandler");
	}
	
	private void OnGameOverTimeOut()
	{
		EmitSignal("GameOverTimeOutEventHandler");
	}
	
	private void OnAnimationFinished()
	{
		slashSprite.Hide();
		MusicController.Play();
	}
}

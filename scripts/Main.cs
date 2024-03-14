using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class Main : Node2D
{
	[Export]
	public PackedScene SuspectScene;
	[Export]
	public PackedScene PoliceScene;

	private Position2D suspectPos;
	private Suspect suspect;
	private Position2D policePos;
	private Police police;

	private AudioStreamPlayer suspectAudio;
	public AudioStreamPlayer policeAudio;

	private Light2D lampLight;
	private CanvasModulate dimmer;

	private Dictionary<string, (int, string[])> dialog;
	
	private HUD hud;
	private GameOverScreen gameOverScreen;

	private bool fullscreen = OS.WindowFullscreen;
	private bool gameStarted = false;
	private bool gameOver = false;

	private string reportResult;
	private string gameEnding;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dialog = DialogScript.Dialog;
		
		hud = GetNode<HUD>("HUD");
		gameOverScreen = GetNode<GameOverScreen>("GameOverScreen");

		suspectPos = GetNode<Position2D>("SuspectPos");
		suspect = SuspectScene.Instance() as Suspect;
		suspect.Position = suspectPos.Position;

		suspectAudio = GetNode<AudioStreamPlayer>("SuspectAudio");
		policeAudio = GetNode<AudioStreamPlayer>("PoliceAudio");

		lampLight = GetNode<Light2D>("LampLight");
		dimmer = GetNode<CanvasModulate>("Dimmer");

		AddChild(suspect);
		HandleGameStart();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
#if DEBUG
		if (Input.IsActionJustPressed("main_action"))
		{
			UpdateDialog("Thank you. That's all I needed to know. Your collaboration is appreciated.");
		}
#endif
		if (Input.IsActionJustPressed("exit_game"))
		{
			GetTree().Quit(0);
		}

		if (Input.IsActionJustPressed("evidence"))
		{
			if (!gameStarted)
			{
				gameStarted = true;
				hud.ShowSuspectDialog();
				hud.ShowDialogOptions();
				UpdateDialog("Init");
				hud.CurrentOptions = dialog["Init"].Item2.Skip(1).ToArray();
			}

			hud.ToggleEvidencePage();
		}

		if (Input.IsActionJustPressed("next_page"))
		{
			hud.NextPage();
		}

		if (Input.IsActionJustPressed("previous_page"))
		{
			hud.PreviousPage();
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
	
	private void UpdateDialog(string option)
	{
		if(dialog.ContainsKey(option))
		{
			suspectAudio.Play();

			// If player chose an instant game over option
			if(dialog[option].Item1 == -1)
			{
				hud.UpdateSuspicion(100);
				HandleGameOver(option);
				return;
			}

			hud.UpdateSuspicion(dialog[option].Item1);

			if(hud.Suspicion >= 100)
			{
				HandleGameOver();
				return;
			}

			hud.UpdateSuspectDialog(dialog[option].Item2[0]);
			hud.ClearDialogOptions();
			hud.CurrentOptions = dialog[option].Item2.Skip(1).ToArray();
			// hud.AddDialogOptions(dialog[option].Item2.Skip(1).ToArray());
		}
	}

	private void HandleGameStart()
	{
		hud.UpdateSuspicion(-100);
		gameOverScreen.Hide();
		hud.ToggleEvidencePage();
	}

	private void HandleGameOver(string option = "GameOver")
	{
		gameOver = true;

		// Adjust lights
		lampLight.Hide();
		dimmer.Color = new Color(0.4f, 0.4f, 0.4f, 1);

		// Pitch down audio
		suspectAudio.PitchScale = 0.5f;

		// Stop music
		MusicController.Stop();

		// UI adjustments
		hud.HideDialogOptions();
		hud.SlowDownLabelTimer(0);
		hud.UpdateSuspectDialog(dialog[option].Item2[0]);
		suspect.PlayAnimation("suspectDPG");
	}
	
	private void DialogOptionClicked(string option)
	{
		UpdateDialog(option);
		
		//switch(index)
		//{
			//case 0:
				//hud.ShowInvestigatorDialog("First option clicked");
				//break;
			//case 1:
				//hud.ShowInvestigatorDialog("Second option clicked");
				//break;
			//case 2:
				//hud.ShowInvestigatorDialog("Third option clicked");
				//break;
			//default:
				//break;
		//}
	}
	
	private void OnRestartButtonPressed()
	{
		GetTree().ChangeScene("res://scenes/main.tscn");
	}
	
	private void OnFinalDialogDone()
	{
		suspectAudio.Stop();

		if(gameOver)
		{
			gameOverScreen.GetNode<Timer>("GameOverTimer").Start();
		}
		else
		{
			GetNode<Timer>("ReportTimer").Start();
		}
	}
	
	private void OnGameOverTimeOut()
	{
		gameOverScreen.ShowGameOverScreen(true);
	}

	private void OnReportTimeOut()
	{
		if (!string.IsNullOrEmpty(gameEnding))
		{
			gameOverScreen.ShowGameOverScreen(gameEnding == "bad");
			return;
		}

		hud.ShowReport();
		suspect.Hide();
		hud.HideSuspectDialog();
		hud.HideDialogOptions();
	}

	private void OnReportSubmitted(string result)
	{
		reportResult = result;

		policePos = GetNode<Position2D>("PolicePos");
		police = PoliceScene.Instance() as Police;
		police.Position = policePos.Position;
		AddChild(police);

		police.Connect("ReadingFinishEventHandler", this, "OnPoliceReadingFinished");

		hud.HideReport();
		hud.ShowPoliceDialog();
		hud.CurrentPoliceDialog = police.FirstDialog;
		hud.UpdatePoliceDialog();
	}
	
	private void OnPoliceDialogFinished()
	{
		if(!string.IsNullOrEmpty(gameEnding))
		{
			GetNode<Timer>("ReportTimer").Start();
			return;
		}
		else
		{
			if (reportResult == "human")
			{
				gameEnding = "good";
				hud.CurrentPoliceDialog = police.GoodDialog;
			}
			else
			{
				gameEnding = "bad";
				hud.CurrentPoliceDialog = police.BadDialog;
			}

			police.PlayAnimation("policeReading");
		}
	}

	private void OnPoliceReadingFinished()
	{
		police.PlayAnimation("policeDPG");
		hud.UpdatePoliceDialog();

		if(gameEnding == "bad")
		{
			// Adjust lights
			lampLight.Hide();
			dimmer.Color = new Color(0.4f, 0.4f, 0.4f, 1);

			// Pitch down audio
			policeAudio.PitchScale = 0.5f;

			// Stop music
			MusicController.Stop();

			// UI adjustments
			hud.HideDialogOptions();
			hud.SlowDownLabelTimer(1);
		}
	}

	private void OnEvidenceTogglePressed()
	{
		if(!gameStarted)
		{
			gameStarted = true;
			hud.ShowSuspectDialog();
			hud.ShowDialogOptions();
			UpdateDialog("Init");
			hud.CurrentOptions = dialog["Init"].Item2.Skip(1).ToArray();
		}

		hud.ToggleEvidencePage();
	}

	private void OnSuspectStoppedTalking()
	{
		suspectAudio.Stop();
	}
}

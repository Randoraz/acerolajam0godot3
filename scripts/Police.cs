using Godot;
using System;

public partial class Police : Node2D
{
	[Signal]
	public delegate void ReadingFinishEventHandler();
	
	private AnimatedSprite animatedSprite;
	private Timer readingTimer;

	public string[] FirstDialog = new string[]
	{
		"Good work, detective.",
		"I'll check your report now."
	};
	public string[] GoodDialog = new string[]
	{
		"Great job! You learn fast, detective.",
		"Keep up the good work for the sake of all of us."
	};
	public string[] BadDialog = new string[]
	{
		"I hoped for more, detective.",
		"We can't allow one of us to be exposed."
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		animatedSprite.Play("police");

		readingTimer = GetNode<Timer>("ReadingTimer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public void PlayAnimation(string animationName)
	{
		animatedSprite.Play(animationName);

		if(animationName == "policeReading")
		{
			readingTimer.Start();
		}
	}
	private void OnReadingTimeout()
	{
		PlayAnimation("police");
		EmitSignal("ReadingFinishEventHandler");
	}
}

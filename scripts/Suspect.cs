using Godot;
using System;

public partial class Suspect : Node2D
{
	private AnimatedSprite animatedSprite2D;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite>("AnimatedSprite");
		animatedSprite2D.Play("suspect1");
		// string[] suspectTypes = animatedSprite2D.SpriteFrames.GetAnimationNames();
		// animatedSprite2D.Play(suspectTypes[GD.Randi() % suspectTypes.Length]);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public void PlayAnimation(string animationName)
	{
		animatedSprite2D.Play(animationName);
	}
}

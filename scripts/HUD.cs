using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HUD : CanvasLayer
{
	[Signal]
	public delegate void DialogOptionClickedEventHandler(string option);
	[Signal]
	public delegate void FinalDialogDoneEventHandler();
	[Signal]
	public delegate void ReportSubmittedEventHandler(string result);
	[Signal]
	public delegate void PoliceDialogFinishedEventHandler();
	[Signal]
	public delegate void ToggleEvidenceEventHandler();
	[Signal]
	public delegate void SuspectStoppedTalkingEventHandler();

	private Label suspectDialog;
	private Label policeDialog;
	private ItemList dialogOptions;
	private TextureProgress progressBar;
	private Node evidencePage;
	private Node report;
	private Button evidenceButton;

	private Timer policeLabelTimer;
	private Timer policePauseTimer;
	private Timer labelTimer;

	private Tween tween;

	private TextureRect[] evidencePageList;
	private TextureRect[] reportPageList;
	private int currentPageIndex = 0;
	private bool showingReport = false;
	public string[] CurrentOptions;
	public string[] CurrentPoliceDialog;

	private AudioStreamPlayer policeAudio;

	private int suspicion = 0;
	private string selectedResult = "";
	public int Suspicion {
		get { return suspicion; }
		private set
		{
			if (value < 0)
			{
				suspicion = 0;
			}
			else if (value > 100)
			{
				suspicion = 100;
			}
			else
			{
				suspicion = value;
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		suspectDialog = GetNode<Label>("SuspectDialog");
		policeDialog = GetNode<Label>("PoliceDialog");
		dialogOptions = GetNode<ItemList>("DialogOptions");
		progressBar = GetNode<TextureProgress>("SuspicionBar");
		evidencePage = GetNode<Node>("EvidencePage");
		evidenceButton = GetNode<Button>("EvidenceButton");
		report = GetNode<Node>("Report");
		labelTimer = GetNode<Timer>("LabelTimer");
		policePauseTimer = GetNode<Timer>("PolicePauseTimer");
		policeLabelTimer = GetNode<Timer>("PoliceLabelTimer");

		tween = GetNode<Tween>("Tween");

		suspectDialog.Hide();
		dialogOptions.Hide();

		GetNode<Button>("Report/Page2/SubmitReportButton").Hide();

		HidePoliceDialog();

		Godot.Collections.Array list = evidencePage.GetChildren();
		evidencePageList = new TextureRect[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			Node node = (Node)list[i];
			evidencePageList[i] = (TextureRect)node;
		}
			
		foreach (var page in evidencePageList)
		{
			page.Hide();
		}

		//reportPageList = report.GetChildren().ToList().Select(node => GetNode<TextureRect>($"Report/{node.Name}")).ToArray();
		list = report.GetChildren();
		reportPageList = new TextureRect[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			Node node = (Node)list[i];
			reportPageList[i] = (TextureRect)node;
		}
		foreach (var page in reportPageList)
		{
			page.Hide();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}

	public void ToggleEvidencePage()
	{
		if(!evidenceButton.IsVisibleInTree())
		{
			return;
		}

		TextureRect page = evidencePageList[currentPageIndex];

		if (page.IsVisibleInTree())
		{
			page.Hide();
		}
		else
		{
			page.Show();
		}
	}

	public void ShowReport()
	{
		evidenceButton.Hide();
		currentPageIndex = 0;
		showingReport = true;
		reportPageList[currentPageIndex].Show();
	}

	public void HideReport()
	{
		showingReport = false;
		reportPageList[currentPageIndex].Hide();
	}

	public void NextPage()
	{
		if (currentPageIndex + 1 >= (showingReport ? reportPageList.Count() : evidencePageList.Count()) ||
		   (!evidencePageList[currentPageIndex].IsVisibleInTree() && !reportPageList[currentPageIndex].IsVisibleInTree()))
		{
			return;
		}

		MusicController.PlayClick();

		string pageName = showingReport ? reportPageList[currentPageIndex].Name : evidencePageList[currentPageIndex].Name;
		TextureRect page = showingReport ? GetNode<TextureRect>($"Report/{pageName}") : GetNode<TextureRect>($"EvidencePage/{pageName}");
		page.Hide();

		currentPageIndex += 1;
		pageName = evidencePageList[currentPageIndex].Name;
		page = showingReport ? GetNode<TextureRect>($"Report/{pageName}") : GetNode<TextureRect>($"EvidencePage/{pageName}");
		page.Show();
	}

	public void PreviousPage()
	{
		if (currentPageIndex - 1 < 0 ||
		   (!evidencePageList[currentPageIndex].IsVisibleInTree() && !reportPageList[currentPageIndex].IsVisibleInTree()))
		{
			return;
		}

		MusicController.PlayClick();

		string pageName = showingReport ? reportPageList[currentPageIndex].Name : evidencePageList[currentPageIndex].Name;
		TextureRect page = showingReport ? GetNode<TextureRect>($"Report/{pageName}") : GetNode<TextureRect>($"EvidencePage/{pageName}");
		page.Hide();

		currentPageIndex -= 1;
		pageName = showingReport ? reportPageList[currentPageIndex].Name : evidencePageList[currentPageIndex].Name;
		page = showingReport ? GetNode<TextureRect>($"Report/{pageName}") : GetNode<TextureRect>($"EvidencePage/{pageName}");
		page.Show();
	}
	
	public void UpdateSuspectDialog(string text)
	{
		suspectDialog.Text = text;
		suspectDialog.VisibleCharacters = 0;
		labelTimer.Start();
	}

	public void UpdatePoliceDialog()
	{
		// Since UpdatePoliceDialog is called in this script instead of Main, audio must be controlled here
		policeAudio = GetParent<Main>().policeAudio;

		policeAudio.Play();
		policeDialog.Text = CurrentPoliceDialog[0];
		policeDialog.VisibleCharacters = 0;

		CurrentPoliceDialog = CurrentPoliceDialog.Skip(1).ToArray();

		policeLabelTimer.Start();
	}

	public void ShowSuspectDialog()
	{
		suspectDialog.Show();
	}

	public void HideSuspectDialog()
	{
		suspectDialog.Hide();
	}

	public void ShowPoliceDialog()
	{
		policeDialog.Show();
	}

	public void HidePoliceDialog()
	{
		policeDialog.Hide();
	}

	public void ClearDialogOptions()
	{
		dialogOptions.Clear();
	}
	
	public void AddDialogOptions()
	{
		for(int i = 0; i < CurrentOptions.Length; i++)
		{
			dialogOptions.AddItem(CurrentOptions[i], null);
			dialogOptions.SetItemTooltipEnabled(i, false);
		}
	}

	public void ShowDialogOptions()
	{
		dialogOptions.Show();
	}

	public void HideDialogOptions()
	{
		dialogOptions.Hide();
	}

	public void UpdateSuspicion(int value)
	{
		tween.InterpolateProperty(progressBar, "value", suspicion, suspicion + value, 0.5f);
		if (!tween.IsActive())
		{
			tween.Start();
		}


		Suspicion = suspicion += value;
	}

	public void SlowDownLabelTimer(int label)
	{
		switch(label)
		{
			case 0:
				// Suspect
				labelTimer.WaitTime = 0.1f;
				return;
			case 1:
				// Police
				policeLabelTimer.WaitTime = 0.1f;
				return;
			default:
				return;
		}
		
	}
	
	private void OnDialogOptionClicked(int index)
	{
		EmitSignal("DialogOptionClickedEventHandler", dialogOptions.GetItemText(index));
	}

	private void OnLabelTimeout()
	{
		suspectDialog.VisibleCharacters += 1;

		if(suspectDialog.PercentVisible >= 1)
		{
			if(suspicion < 100 && CurrentOptions.Length > 0)
			{
				AddDialogOptions();
				EmitSignal("SuspectStoppedTalkingEventHandler");
			}
			else
			{
				EmitSignal("FinalDialogDoneEventHandler");
			}

			labelTimer.Stop();
		}
	}
	private void OnPoliceLabelTimeout()
	{
		policeDialog.VisibleCharacters += 1;

		if (policeDialog.PercentVisible >= 1)
		{
			if(CurrentPoliceDialog.Length > 0)
			{
				policePauseTimer.Start();
			}
			else
			{
				EmitSignal("PoliceDialogFinishedEventHandler");
			}
			
			policeAudio.Stop();
			policeLabelTimer.Stop();
		}
	}

	private void OnButtonGroupPressed()
	{
		BaseButton button = GetNode<CheckBox>("Report/Page2/HumanCheckBox").Group.GetPressedButton();
		GetNode<Button>("Report/Page2/SubmitReportButton").Show();
		if (button.Name == "HumanCheckBox")
		{
			selectedResult = "human";
		}
		else
		{
			selectedResult = "doppelganger";
		}
	}
	
	private void OnSubmitReportPressed()
	{
		MusicController.PlayClick();
		EmitSignal("ReportSubmittedEventHandler", selectedResult);
	}
	
	private void OnPolicePauseTimeout()
	{
		UpdatePoliceDialog();
	}

	private void OnEvidenceButtonPressed()
	{
		MusicController.PlayClick();
		EmitSignal("ToggleEvidenceEventHandler");
	}
}

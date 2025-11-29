using System;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BingusNametagsPlusPlus.Components;

public static class UIManager
{
	public static bool showingUI;
	
	public static void Update()
	{
		if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
			showingUI = !showingUI;
	}

	private const int WindowX = 10;
	private const int WindowY = 10;

	private const int WindowSizeX = 310;
	private const int WindowSizeY = 400;

	private const int WindowPadding = 10;

	private static int WindowStartX => WindowX + WindowPadding;
	private static int WindowStartY => (WindowY + WindowPadding) + 50;

	private const string Credits = "Beta Testers:\n" +
	                               "\tMedievalz\n" +
	                               "\tMonky\n" +
	                               "\ttfsdemon\n" +
	                               "\tSalty0591\n" +
	                               "\tLinear\n" +
	                               "\tAriel (The Mysterious Person)\n";

	private static readonly GUIContent[] Pages =
	{
		new("General", "Most common settings"),
		new("About", "About BingusNametags++")
	};
	
	private static int _pageSelected;

	private static float lastWavingFrameSwitch;
	private static bool waving = true;

	public static void OnGUI()
	{
		if (!showingUI)
			return;

		// Window
		GUI.Box(new Rect(WindowX, WindowY, WindowSizeX, WindowSizeY), "");
		GUI.Label(
			new Rect(WindowX + 100, WindowY + 5, 120, 20),
			"BingusNametags++"
		);

		_pageSelected = GUI.Toolbar(new Rect(WindowX + 5, WindowY + 30, WindowSizeX - WindowPadding, 20), _pageSelected, Pages);

		switch (_pageSelected)
		{
			case 0:
				Config.ShowingNametags = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 150, 20),
					Config.ShowingNametags,
					new GUIContent("Show Nametags", "Toggle the visibility of all nametags")
				);

				if (Config.ShowingNametags)
				{
					Config.ShowingName = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 35, 80, 20),
						Config.ShowingName,
						new GUIContent("Name", "Display usernames on nametags")
					);

					Config.ShowingPlatform = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 60, 80, 20),
						Config.ShowingPlatform,
						new GUIContent("Platform", "Display the platform on nametags")
					);

					Config.NametagScale = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 95, 185, 20),
						Config.NametagScale, 2f, 12f);
					Config.NametagYOffset =
						GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 120, 185, 20),
							Config.NametagYOffset, 0.5f, 5f);

					Config.ShowInFirstPerson = GUI.Toggle(
						new Rect(WindowStartX + 70, WindowStartY + 155, 100, 20),
						Config.ShowInFirstPerson,
						new GUIContent("First Person", "Display the nametag in VR (first person)"));

					Config.ShowInThirdPerson = GUI.Toggle(
						new Rect(WindowStartX + 175, WindowStartY + 155, 100, 20),
						Config.ShowInThirdPerson,
						new GUIContent("Third Person", "Display the nametag on your PC (third person)"));

					// Labels
					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 90, 80, 20),
						new GUIContent("Font Size", "Change how large the nametag text is")
					);

					GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 90, 30, 20), $"{Config.NametagScale}");

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 115, 80, 20),
						new GUIContent("Y Offset", "Change the offset of the nametag")
					);

					GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 115, 30, 20), $"{Config.NametagYOffset}");

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 155, 70, 20),
						new GUIContent("Display", "Change how nametags are displayed")
					);
				}

				break;
			case 1:
				GUI.Label(new Rect(WindowStartX, WindowStartY, WindowSizeX - (WindowPadding * 2), 20), $"BingusNametags++ {Main.Instance.Info.Metadata.Version}");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 25, WindowSizeX - (WindowPadding * 2), 20), $"(C) Copyright 2025 - 2026 SirKingBinx / Bingus");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 50, WindowSizeX - (WindowPadding * 2), 20), $"MIT License");

				if (Time.time > lastWavingFrameSwitch + 1)
				{
					lastWavingFrameSwitch = Time.time;
					waving = !waving;
				}

				var waveText = waving ? "|˶˙ᵕ˙ )ﾉﾞ" : "|˶˙ᵕ˙ )_.";

				GUI.Label(new Rect(WindowStartX, WindowStartY + 85, 150, 20), waveText);

				GUI.TextArea(
					new Rect(
						WindowStartX, WindowStartY + 110, 
						WindowSizeX - (WindowPadding + 5), WindowSizeY - (WindowStartY + 130)),
					Credits
					);
				
				break;
			default:
				_pageSelected = 0;
				break;
		}
		
		// Saving / other stuff like that
		if (GUI.Button(
			    new Rect(WindowStartX, WindowY + WindowSizeY - 25, 100, 20),
			    new GUIContent("Refresh", "Refresh the config file and font (save & reload)")))
		{
			Config.SavePrefs();
			Config.LoadPrefs();
		}

		if (GUI.Button(new Rect(WindowStartX + 105, WindowY + WindowSizeY - 25, 75, 20),
			    new GUIContent("Apply",
				    "Apply the changes and save the config manually (instead of relying on autosave)")))
			Config.SavePrefs();

		// X button
		if (GUI.RepeatButton(new Rect(WindowX + 285, WindowY + 5, 20, 20), new GUIContent("X", "Close the window (press right shift to reopen)")))
			showingUI = false;

		// Tooltip display
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			var actualY = Math.Abs(Mouse.current.position.ReadValue().y - Screen.height);
			GUIStyle.none.CalcMinMaxWidth(new GUIContent(GUI.tooltip), out var min, out var max);
			GUI.Box(new Rect(Mouse.current.position.ReadValue().x + 10, actualY, min + 20, 25), GUI.tooltip);
		}
		
		// Reset it so the tooltip is gone when your mouse leaves an element
		GUI.tooltip = "";
	}
}
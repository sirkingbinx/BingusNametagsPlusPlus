using System;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace BingusNametagsPlusPlus.Components;

public static class UIManager
{
	public static bool showingUI;
	
	public static void Update()
	{
		if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
			showingUI = !showingUI;
	}

	private static int WindowX = 10;
	private static int WindowY = 10;

	private const int WindowPadding = 10;

	private static int WindowStartX => WindowX + WindowPadding;
	private static int WindowStartY => WindowX + WindowPadding;

	public static void OnGUI()
	{
		if (!showingUI)
			return;
		
		// Window
		GUI.Box(new Rect(WindowX, WindowY, 310, 225), "");
		GUI.Label(
			new Rect(WindowX + 100, WindowY + 5, 120, 20),
			new GUIContent("BingusNametags++", $"Version {Main.Instance.Info.Metadata.Version} | Made by Bingus / SirKingBinx")
		);

		// Controls
		Config.ShowingNametags = GUI.Toggle(
			new Rect(WindowStartX, WindowY + 30, 200, 20),
			Config.ShowingNametags,
			new GUIContent("Show Nametags", "Toggle the visibility of all nametags")
		);

		if (Config.ShowingNametags)
		{
			Config.ShowingName = GUI.Toggle(
				new Rect(WindowStartX, WindowStartY + 45, 80, 20), 
				Config.ShowingName,
				new GUIContent("Name", "Display usernames on nametags")
			);
			
			Config.ShowingPlatform = GUI.Toggle(
				new Rect(WindowStartX, WindowStartY + 70, 80, 20), 
				Config.ShowingPlatform,
				new GUIContent("Platform", "Display the platform on nametags")
			);
			
			Config.NametagScale = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 105, 185, 20), Config.NametagScale, 2f, 12f);
			Config.NametagYOffset = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 130, 185, 20), Config.NametagYOffset, 0.5f, 5f);

			Config.ShowInFirstPerson = GUI.Toggle(
				new Rect(WindowStartX + 70, WindowStartY + 165, 100, 20),
				Config.ShowInFirstPerson, 
				new GUIContent("First Person", "Display the nametag in VR (first person)"));
			
			Config.ShowInThirdPerson = GUI.Toggle(
				new Rect(WindowStartX + 175, WindowStartY + 165, 100, 20),
				Config.ShowInThirdPerson, 
				new GUIContent("Third Person", "Display the nametag on your PC (third person)"));

			if (GUI.Button(
				    new Rect(WindowStartX, WindowStartY + 190, 100, 20),
				    new GUIContent("Refresh", "Refresh the config file and font (save & reload)")))
			{
				Config.SavePrefs();
				Config.LoadPrefs();
			}
			
			if (GUI.Button(new Rect(WindowStartX + 105, WindowStartY + 190, 75, 20), new GUIContent("Apply", "Apply the changes and save the config manually (instead of relying on autosave)") ) )
				Config.SavePrefs();

			// Labels
			GUI.Label(
				new Rect(WindowStartX, WindowStartY + 100, 80, 20),
				new GUIContent("Font Size", "Change how large the nametag text is")
			);
			
			GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 100, 30, 20), $"{Config.NametagScale}");
		
			GUI.Label(
				new Rect(WindowStartX, WindowStartY + 125, 80, 20),
				new GUIContent("Y Offset", "Change the offset of the nametag")
			);
			
			GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 125, 30, 20), $"{Config.NametagYOffset}");
		
			GUI.Label(
				new Rect(WindowStartX, WindowStartY + 165, 70, 20),
				new GUIContent("Display", "Change how nametags are displayed")
			);
		}

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
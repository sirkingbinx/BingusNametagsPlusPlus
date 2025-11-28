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
		GUI.Box(new Rect(WindowX, WindowY, 310, 190), $"BingusNametags++ v{Main.Instance.Info.Metadata.Version}");

		// Controls
		Config.ShowingNametags = GUI.Toggle(new Rect(WindowStartX, WindowStartY + 10, 200, 20), Config.ShowingNametags, "Show Nametags");

		if (Config.ShowingNametags)
		{
			Config.ShowingName = GUI.Toggle(new Rect(WindowStartX, WindowStartY + 35, 80, 20), Config.ShowingName, "Name");
			Config.ShowingPlatform = GUI.Toggle(new Rect(WindowStartX, WindowStartY + 60, 80, 20), Config.ShowingPlatform, "Platform");
			Config.NametagScale = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 105, 185, 20), Config.NametagScale, 2f, 12f);
			Config.NametagYOffset = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 130, 185, 20), Config.NametagYOffset, 0.5f, 5f);

			Config.ShowInFirstPerson = GUI.Toggle(new Rect(WindowStartX + 70, WindowStartY + 155, 100, 20), Config.ShowInFirstPerson, "First Person");
			Config.ShowInThirdPerson = GUI.Toggle(new Rect(WindowStartX + 175, WindowStartY + 155, 100, 20), Config.ShowInThirdPerson, "Third Person");
			
			// Labels
			GUI.Label(new Rect(WindowStartX, WindowStartY + 100, 80, 20), "Font Size");
			GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 100, 30, 20), $"{Config.NametagScale}");
		
			GUI.Label(new Rect(WindowStartX, WindowStartY + 130, 80, 20), "Y Offset");
			GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 130, 30, 20), $"{Config.NametagYOffset}");
		
			GUI.Label(new Rect(WindowStartX, WindowStartY + 155, 70, 20), "Display");
		}

		// X button
		if (GUI.RepeatButton(new Rect(WindowX + 285, WindowStartY, 20, 20), "X"))
			showingUI = false;
	}
}
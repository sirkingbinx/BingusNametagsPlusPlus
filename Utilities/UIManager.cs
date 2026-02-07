using System;
using System.Collections.Generic;
using System.Diagnostics;
using BingusNametagsPlusPlus.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BingusNametagsPlusPlus.Utilities;

/*
 * Dear contributor:
 * I would rather kill myself then clean this code up. Please do it for me.
 * - Bingus
 */
public static class UIManager
{
	private static float WindowX = 10;
	private static float WindowY = 10;

	private const int WindowSizeX = 420;
	private const int WindowSizeY = 400;

	private const float WindowPadding = 10;

	private const string Credits =
		"Beta Testers:\n" +
		"\tMedievalz\n" +
		"\tMonky\n" +
		"\ttfsdemon\n" +
		"\tSalty0591\n" +
		"\tLinear\n" +
		"\tGolden\n" +
		"\tJosh\n" +
		"\tAriel (The Mysterious Person)\n" +
		"\tCrazykid\n";

	public static bool ShowingUI;

	private static GUIContent[] Pages =
	[
		new("Nametag", "Nametag behaviour settings"),
		new("Icons", "Change how icons behave"),
		new("Network", "Change how your nametag looks to other people"),
		new("Plugins", "Enable/disable all nametags"),
		new("About", "About BingusNametags++")
	];
	

	private static int _pageSelected;

	private static float _lastWavingFrameSwitch;
	private static bool _waving = true;

	private static float WindowStartX => WindowX + WindowPadding;
	private static float WindowStartY => WindowY + WindowPadding + 50f;

	private static BGWindowState WindowState = BGWindowState.Normal;

	private static BingusNametagsPlugin? CurrentlyInspectedNametag;

	public static void Update()
	{
		if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
			ShowingUI = !ShowingUI;
	}

	public static void DrawNormal()
	{
		_pageSelected = GUI.Toolbar(new Rect(WindowX + 5, WindowY + 30, WindowSizeX - WindowPadding, 20), _pageSelected, Pages.AsArray());

		switch (_pageSelected)
		{
			case 0:
				ConfigManager.Nametags = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 150, 20),
					ConfigManager.Nametags,
					new GUIContent("Nametags", "Show nametags")
				);

				if (ConfigManager.Nametags)
				{
					ConfigManager.FirstPersonEnabled = GUI.Toggle(
						new Rect(WindowStartX + 70, WindowStartY + 25, 100, 20),
						ConfigManager.FirstPersonEnabled,
						new GUIContent("First Person", "Display the nametag in VR (first person)")
					 );

					ConfigManager.ThirdPersonEnabled = GUI.Toggle(
						new Rect(WindowStartX + 175, WindowStartY + 25, 100, 20),
						ConfigManager.ThirdPersonEnabled,
						new GUIContent("Third Person", "Display the nametag on your PC (third person)")
					);

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 25, 70, 20),
						new GUIContent("Display", "Change how nametags are displayed")
					);

					ConfigManager.Scale = GUI.HorizontalSlider(new Rect(WindowStartX + 90, WindowStartY + 50, WindowSizeX - 140, 20),
						ConfigManager.Scale, 2f, 12f);

					ConfigManager.Offset =
						GUI.HorizontalSlider(new Rect(WindowStartX + 90, WindowStartY + 75, WindowSizeX - 140, 20),
							ConfigManager.Offset, 0f, 3.5f);

					ConfigManager.SanitizeNicknames = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 100, 250, 20),
						ConfigManager.SanitizeNicknames,
						new GUIContent("Sanitize Nicknames", "Prevents invalid usernames from being displayed on nametags (eg. spaces, cuss words, etc.). This is the username displayed on the gorilla's chest.")
					);

					ConfigManager.GFriendsIntegration = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 125, 300, 20),
						ConfigManager.GFriendsIntegration,
						new GUIContent("Match Nametag Color", "Make the color of the nametag the same color as the nametag on their chest, allowing support for GorillaFriends and Very Important Monke subscribers.")
					);

					// Labels
					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 45, 80, 20),
						new GUIContent("Size", "Change how large the nametag text is")
					);

					GUI.Label(new Rect(WindowStartX + WindowSizeX - 50, WindowStartY + 45, 30, 20), $"{ConfigManager.Scale}");

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 70, 80, 20),
						new GUIContent("Offset", "Change the Y axis offset of the nametag")
					);

					GUI.Label(new Rect(WindowStartX + WindowSizeX - 50, WindowStartY + 70, 30, 20), $"{ConfigManager.Offset}");
				}

				break;
			case 1:
				ConfigManager.Icons = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 250, 20),
					ConfigManager.Icons,
					new GUIContent("Icons", "Enable icons in the nametag")
				);

				if (ConfigManager.Icons)
				{
					ConfigManager.UserIcons = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 25, 250, 20),
						ConfigManager.UserIcons,
						new GUIContent("Special User Icons",
							"Display custom icons for known people like developers, beta testers, and whoever I want")
					);

					ConfigManager.PlatformIcons = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 50, 250, 20),
						ConfigManager.PlatformIcons,
						new GUIContent("Platform Icons", "Display icons representing the icon of a user")
					);
				}

				break;
			case 2:
				var propsToggle = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 175, 20),
					ConfigManager.CustomNametags,
					new GUIContent("Custom Nametags", "Customize how nametags look to other people")
				);

				if (propsToggle != ConfigManager.CustomNametags && !ConfigManager.CustomNametags)
				{
					Ask("By enabling this feature, your nametag style will be networked with properties.\n\nProperties are commonly used in mod checkers to detect what mods you have installed. Small, very immature children may harass you over your properties.\n\nAre you sure you want to enable Custom Nametags?",
						["Yes", "No"],
						result => ConfigManager.CustomNametags = (result == "Yes")
					);
				} else if (propsToggle != ConfigManager.CustomNametags)
				{
					ConfigManager.CustomNametags = false;
				}

				if (ConfigManager.CustomNametags)
				{
					ConfigManager.NetworkColor = GUI.TextArea(
						new Rect(WindowStartX + 75, WindowStartY + 25, 200, 20),
						ConfigManager.NetworkColor
					);

					GUI.Label(new Rect(WindowStartX, WindowStartY + 25, 75, 20),
						new GUIContent("Hex Code",
							"Custom hex code for your nametag (use a color picker to determine this.)"));

					ConfigManager.NetworkBold = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 50, 175, 20),
						ConfigManager.NetworkBold,
						new GUIContent("Bold", "Nametag text is bolded")
					);

					ConfigManager.NetworkItalic = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 75, 175, 20),
						ConfigManager.NetworkItalic,
						new GUIContent("Italics", "Nametag text is italicized")
					);

					ConfigManager.NetworkUnderline = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 100, 175, 20),
						ConfigManager.NetworkUnderline,
						new GUIContent("Underlined", "Nametag text is underlined")
					);
				}

				ConfigManager.ViewOtherCustomStyles = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + (ConfigManager.CustomNametags ? 125 : 25), 175, 20),
					ConfigManager.ViewOtherCustomStyles,
					new GUIContent("View Other Nametag Styles", "View custom nametag styles for other users. This doesn't enable custom nametags for yourself.")
				);

				break;
			case 3:
				if (GUI.Button(
					new Rect(WindowStartX, WindowStartY, WindowSizeX - 20, 20),
					new GUIContent("Open Nametags Folder", "You can place .dlls of nametag files here to have them loaded manually by BingusNametags++.")
				))
				{
					PluginManager.OpenNametagsFolder();
				}

				var startingIndex = WindowStartY + 30;

				foreach (var plugin in PluginManager.Plugins)
				{
					var currently = GUI.Toggle(
						new Rect(WindowStartX, startingIndex, WindowSizeX - 120, 20),
						plugin.Metadata.Enabled,
						new GUIContent($"{plugin.Metadata.Name}", plugin.Metadata.Description)
					);

					if (currently != plugin.Metadata.Enabled && plugin.Metadata.Enabled)
						PluginManager.DisablePlugin(plugin);
					else if (currently != plugin.Metadata.Enabled)
						PluginManager.EnablePlugin(plugin);

					var inspect = GUI.Button(
						new Rect(WindowX + WindowSizeX - 110, startingIndex, 100, 20),
						new GUIContent("Inspect", "View information about this nametag"));

					if (inspect)
					{
						CurrentlyInspectedNametag = plugin.Metadata;
						WindowState = BGWindowState.Inspector;
					}

					startingIndex += 25;
				}

				break;
			case 4:
				GUI.Label(new Rect(WindowStartX, WindowStartY, WindowSizeX - WindowPadding * 2, 20),
					$"BingusNametags++ v{Constants.Version}-{Constants.Channel.AsString()}");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 20, WindowSizeX - WindowPadding * 2, 20),
					"(C) Copyright 2025 - 2026 Bingus/SirKingBinx");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 40, WindowSizeX - WindowPadding * 2, 20),
					"MIT License");

				if (Time.time > _lastWavingFrameSwitch + 1)
				{
					_lastWavingFrameSwitch = Time.time;
					_waving = !_waving;
				}

				var waveText = _waving ? "|˶˙ᵕ˙ )ﾉﾞ" : "|˶˙ᵕ˙ )_.";

				GUI.Label(new Rect(WindowStartX, WindowStartY + 70, 150, 20), waveText);

				GUI.TextArea(
					new Rect(
						WindowStartX, WindowStartY + 110,
						WindowSizeX - (WindowPadding + 5), WindowSizeY - (WindowStartY + 130)),
					Credits
				);

				if (GUI.Button(
						new Rect(WindowStartX, WindowY + WindowSizeY - 25, 150, 20),
						new GUIContent("Data Folder", "Opens the location of the BingusNametags++ data folder"))
				   )
					Process.Start(new ProcessStartInfo
					{
						FileName = Constants.BingusNametagsData,
						UseShellExecute = true,
						Verb = "open"
					});

				break;
			default:
				_pageSelected = 0;
				break;
		}

		if (GUI.Button(
			new Rect(WindowX + WindowSizeX - 190, WindowY + WindowSizeY - 25, 100, 20),
			new GUIContent("Refresh", "Reload all configuration. Any unsaved changes will be lost!"))
		)
			ConfigManager.LoadPrefs();

		if (GUI.Button(new Rect(WindowX + WindowSizeX - 80, WindowY + WindowSizeY - 25, 75, 20),
				new GUIContent("Apply",
					"Save the current nametags configuration. Auto-saves!")))
			ConfigManager.SavePrefs();

		#region Debug Stuff
		if (Constants.Channel != ReleaseChannel.Stable)
		{
			GUI.Label(
				new Rect(WindowStartX, WindowY + WindowSizeY + WindowPadding, WindowSizeX - WindowPadding * 2, 20),
				$"Plugin updates polled: {Main.UpdateNametags?.GetInvocationList().Length}"
			);

			var fps = 1.0f / Time.deltaTime;

			GUI.Label(
				new Rect(WindowStartX, WindowY + WindowSizeY + WindowPadding + 20, WindowSizeX - WindowPadding * 2, 20),
				$"FPS: {Mathf.Floor(fps)}"
			);
		}
		#endregion
	}

	private static string promptQuestion = "";
	private static List<string> promptButtons = [ ];
	private static Action<string> promptCallback = delegate { };

	private static void DrawPrompt()
	{
		GUI.Label(
			new Rect(WindowX + 10, WindowY + 30, WindowSizeX - 20, WindowSizeY - 60),
			promptQuestion
		);

		var buttonIndex = 0;

		foreach (var answerOption in promptButtons)
		{
			if (GUI.Button(new Rect(WindowStartX + (buttonIndex * 155), WindowY + WindowSizeY - 25, 150, 20), answerOption))
			{
				promptCallback(answerOption);
				WindowState = BGWindowState.Normal;
			}

			buttonIndex++;
		}
	}

	public static void Ask(string question, List<string> answers, Action<string> callback)
	{
		promptQuestion = question;
		promptButtons = answers;
		promptCallback = callback;
		WindowState = BGWindowState.Prompt;
	}

	private static Vector2 scrollPosition;

	public static void DrawPluginInspector()
	{
		if (CurrentlyInspectedNametag == null)
		{
			LogManager.Log("No CurrentlyInspectedNametag (uhoh) - sincerely, the UIManager");
			WindowState = BGWindowState.Normal;
			return;
		}

		GUILayout.BeginArea(new Rect(WindowStartX, WindowY + WindowPadding, WindowSizeX - (WindowPadding * 2), WindowSizeY - (WindowPadding * 2)));

		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(WindowSizeX - (WindowPadding * 2)), GUILayout.Height(250));

		// Metadata
		GUILayout.Label(
			CurrentlyInspectedNametag.Name
		);

		GUILayout.Label(
			$"by {CurrentlyInspectedNametag.Author}"
		);

		GUILayout.Label(
			CurrentlyInspectedNametag.Description
		);

		GUILayout.Label(
			"This plugin contains the following nametags:"
		);

		foreach (var nametagMeta in CurrentlyInspectedNametag.Nametags.Keys)
		{
			GUILayout.Label($"- {nametagMeta.Name} (Offset: {nametagMeta.Offset})");
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();

		if (GUI.Button(new Rect(WindowX + (WindowSizeX - 105), WindowY + WindowSizeY - 25, 100, 20),
				new GUIContent("Close",
					"Return to the Plugins tab")))
			WindowState = BGWindowState.Normal;
	}

	private static Vector2 mousePosition;

	public static void SafeDraw(Action drawingThing)
	{
		try
		{
			drawingThing();
		}
		catch (Exception ex)
		{
			ex.Report();
			GUI.Label(
				new Rect(WindowStartX, WindowStartY, WindowSizeX - WindowPadding * 2, WindowSizeY - WindowPadding * 2),
				$"An error occured while drawing UI.\n\n\tSource: {drawingThing.Method.Name}\n\tMessage: {ex.Message}\n\tTrace:\n\t{ex.StackTrace}\n\noops"
			);
		}
	}
	
	public static void OnGUI()
	{
		if (!ShowingUI)
			return;

		// Window
		GUI.Box(new Rect(WindowX, WindowY, WindowSizeX, WindowSizeY), "");
		GUI.Label(
			new Rect(WindowX + ((WindowSizeX / 2 - (WindowSizeX % 2)) - 75), WindowY + 5, 150, 20),
			"BingusNametags++"
		);

		switch (WindowState)
		{
			default:
			case BGWindowState.Normal:
				SafeDraw(DrawNormal);
				break;
			case BGWindowState.Prompt:
				SafeDraw(DrawPrompt);
				break;
			case BGWindowState.Inspector:
				SafeDraw(DrawPluginInspector);
				break;
		}

		mousePosition = Mouse.current.position.ReadValue();

		// X button
		if (GUI.Button(new Rect(WindowX + WindowSizeX - 25, WindowY + 5, 20, 20), new GUIContent("X", "Close")))
			ShowingUI = false;

		// Tooltip display
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			var actualY = Math.Abs(mousePosition.y - Screen.height);
			GUIStyle.none.CalcMinMaxWidth(new GUIContent(GUI.tooltip), out var min, out _);
			GUI.Box(new Rect(mousePosition.x + 10, actualY, min + 20, 25), GUI.tooltip);
		}

		// Reset it so the tooltip is gone when your mouse leaves an element
		GUI.tooltip = "";
	}

	enum BGWindowState
	{
		Normal,
		Prompt,
		Inspector
	}
}
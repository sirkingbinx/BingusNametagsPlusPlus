using System;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BingusNametagsPlusPlus.Components;

public static class UIManager
{
	private const int WindowX = 10;
	private const int WindowY = 10;

	private const int WindowSizeX = 420;
	private const int WindowSizeY = 400;

	private const int WindowPadding = 10;

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

	private static bool _showingUI;

	private static readonly GUIContent[] Pages =
	[
		new("Nametag", "Nametag behaviour settings"),
		new("Text", "Text display settings"),
		new("Icons", "Change how icons behave"),
		new("Network", "Change how your nametag looks to other people"),
        new("Plugins", "Enable/disable all nametags"),
        new("About", "About BingusNametags++")
	];

	private static int _pageSelected;

	private static float _lastWavingFrameSwitch;
	private static bool _waving = true;

	private static int WindowStartX => WindowX + WindowPadding;
	private static int WindowStartY => WindowY + WindowPadding + 50;

	public static void Update()
	{
		if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
			_showingUI = !_showingUI;
	}

	public static void OnGUI()
	{
		if (!_showingUI)
			return;

		// Window
		GUI.Box(new Rect(WindowX, WindowY, WindowSizeX, WindowSizeY), "");
		GUI.Label(
			new Rect(WindowX + ((WindowSizeX / 2 - (WindowSizeX % 2)) - 60), WindowY + 5, 120, 20),
			"BingusNametags++"
		);

		_pageSelected = GUI.Toolbar(new Rect(WindowX + 5, WindowY + 30, WindowSizeX - WindowPadding, 20), _pageSelected,
			Pages);

		switch (_pageSelected)
		{
			case 0:
				Config.ShowingNametags = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 150, 20),
					Config.ShowingNametags,
					new GUIContent("Nametags", "Show nametags")
				);

				if (Config.ShowingNametags)
				{
					Config.ShowInFirstPerson = GUI.Toggle(
						new Rect(WindowStartX + 70, WindowStartY + 25, 100, 20),
						Config.ShowInFirstPerson,
						new GUIContent("First Person", "Display the nametag in VR (first person)")
                     );

                    Config.ShowInThirdPerson = GUI.Toggle(
                        new Rect(WindowStartX + 175, WindowStartY + 25, 100, 20),
                        Config.ShowInThirdPerson,
                        new GUIContent("Third Person", "Display the nametag on your PC (third person)")
                    );

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 25, 70, 20),
						new GUIContent("Display", "Change how nametags are displayed")
					);
                }

				break;
			case 1:
				Config.NametagScale = GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 5, 185, 20),
					Config.NametagScale, 2f, 12f);

				Config.NametagYOffset =
					GUI.HorizontalSlider(new Rect(WindowStartX + 70, WindowStartY + 30, 185, 20),
						Config.NametagYOffset, 0.5f, 5f);

				// Labels
				GUI.Label(
					new Rect(WindowStartX, WindowStartY, 80, 20),
					new GUIContent("Size", "Change how large the nametag text is")
				);

				GUI.Label(new Rect(WindowStartX + 260, WindowStartY, 30, 20), $"{Config.NametagScale}");

				GUI.Label(
					new Rect(WindowStartX, WindowStartY + 25, 80, 20),
					new GUIContent("Offset", "Change the Y axis offset of the nametag")
				);

				GUI.Label(new Rect(WindowStartX + 260, WindowStartY + 25, 30, 20), $"{Config.NametagYOffset}");

				break;
			case 2:
				Config.GlobalIconsEnabled = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 250, 20),
					Config.GlobalIconsEnabled,
					new GUIContent("Icons", "Enable icons in the nametag")
				);

				if (Config.GlobalIconsEnabled)
				{
					Config.UserCustomIcons = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 25, 250, 20),
						Config.UserCustomIcons,
						new GUIContent("Special User Icons",
							"Display custom icons for known people like developers, beta testers, and whoever I want")
					);

					Config.ShowingPlatform = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 50, 250, 20),
						Config.ShowingPlatform,
						new GUIContent("Platform Icons", "Display icons representing the icon of a user")
					);
				}

				break;
			case 3:
				Config.CustomNametags = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 175, 20),
					Config.CustomNametags,
					new GUIContent("Custom Nametags", "View custom nametag styles for other users (Networked!)")
				);

				Config.NametagColor = GUI.TextArea(
					new Rect(WindowStartX + 75, WindowStartY + 25, 200, 20),
					Config.NametagColor
				);

				GUI.Label(new Rect(WindowStartX, WindowStartY + 25, 75, 20),
					new GUIContent("Hex Code",
						"Custom hex code for your nametag (use a color picker to determine this.)"));

				Config.NetworkBold = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 50, 175, 20),
					Config.NetworkBold,
					new GUIContent("Bold", "Nametag text is bolded")
				);

				Config.NetworkItalic = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 75, 175, 20),
					Config.NetworkItalic,
					new GUIContent("Italics", "Nametag text is italicized")
				);

				Config.NetworkUnderline = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 100, 175, 20),
					Config.NetworkUnderline,
					new GUIContent("Underlined", "Nametag text is underlined")
				);

				Networking.DoNetworking = GUI.Toggle(
                    new Rect(WindowStartX, WindowStartY + 130, 175, 20),
                    Networking.DoNetworking,
                    new GUIContent("Properties", "You can disable the property managing custom nametags from being networked so other people can not see that BingusNametags++ is installed. For privacy conscience users, you can use this to get little timmy off your back.")
                );

                break;
			case 4:
                var startingIndex = WindowStartY;

                foreach (var plugin in Main.Plugins)
                {
                    plugin.Enabled = GUI.Toggle(
                        new Rect(WindowStartX, startingIndex, 175, 20),
						plugin.Enabled,
                        new GUIContent(plugin.Name, plugin.Description)
                    );

                    startingIndex += 25;
                }

                break;
			case 5:
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

				break;
			default:
				_pageSelected = 0;
				break;
		}

		// Saving / other stuff like that
		if (GUI.Button(
			    new Rect(WindowStartX, WindowY + WindowSizeY - 25, 100, 20),
			    new GUIContent("Refresh", "Save all settings, reload fonts and configuration settings")))
		{
			Config.SavePrefs();
			Config.LoadPrefs();
		}

		if (GUI.Button(new Rect(WindowStartX + 105, WindowY + WindowSizeY - 25, 75, 20),
			    new GUIContent("Apply",
				    "Save the current nametags configuration (This is not required, changes are auto-saved upon exiting)")))
			Config.SavePrefs();

		// X button
		if (GUI.RepeatButton(new Rect(WindowX + WindowSizeX - 25, WindowY + 5, 20, 20),
			    new GUIContent("X", "Close the window (press right shift to reopen)")))
			_showingUI = false;

		// Tooltip display
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			var actualY = Math.Abs(Mouse.current.position.ReadValue().y - Screen.height);
			GUIStyle.none.CalcMinMaxWidth(new GUIContent(GUI.tooltip), out var min, out _);
			GUI.Box(new Rect(Mouse.current.position.ReadValue().x + 10, actualY, min + 20, 25), GUI.tooltip);
		}

		// Reset it so the tooltip is gone when your mouse leaves an element
		GUI.tooltip = "";
	}
}
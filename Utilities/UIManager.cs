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

	public static bool ShowingUI = false;

	private static int _pageSelected;

	private static float _lastWavingFrameSwitch;
	private static bool _waving = true;

	private static float WindowStartX => WindowX + WindowPadding;
	private static float WindowStartY => WindowY + WindowPadding + 50f;

	private static BGWindowState WindowState = BGWindowState.Normal;

	private static BingusNametagsPlugin? CurrentlyInspectedNametag;

	public static void DrawNormal()
	{
		GUIContent[] pages =
		[
			new(LocalizationManager.GetString("t6"), LocalizationManager.GetString("t11")),
			new(LocalizationManager.GetString("t7"), LocalizationManager.GetString("t12")),
			new(LocalizationManager.GetString("t8"), LocalizationManager.GetString("t13")),
			new(LocalizationManager.GetString("t9"), LocalizationManager.GetString("t14")),
			new(LocalizationManager.GetString("t10"), LocalizationManager.GetString("t15"))
		];

		_pageSelected = GUI.Toolbar(new Rect(WindowX + 5, WindowY + 30, WindowSizeX - WindowPadding, 20), _pageSelected, pages.AsArray());

		switch (_pageSelected)
		{
			case 0:
				Config.Current.Nametags = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 150, 20),
					Config.Current.Nametags,
					new GUIContent(LocalizationManager.GetString("t16"), LocalizationManager.GetString("t16", 1))
				);

				if (Config.Current.Nametags)
				{
					Config.Current.FirstPersonEnabled = GUI.Toggle(
						new Rect(WindowStartX + 70, WindowStartY + 25, 100, 20),
						Config.Current.FirstPersonEnabled,
						new GUIContent(LocalizationManager.GetString("t17"), LocalizationManager.GetString("t17", 1))
					 );

					Config.Current.ThirdPersonEnabled = GUI.Toggle(
						new Rect(WindowStartX + 175, WindowStartY + 25, 100, 20),
						Config.Current.ThirdPersonEnabled,
						new GUIContent(LocalizationManager.GetString("t18"), LocalizationManager.GetString("t18", 1))
					);

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 25, 70, 20),
						new GUIContent(LocalizationManager.GetString("t19"), LocalizationManager.GetString("t19", 1))
					);

					Config.Current.Scale = GUI.HorizontalSlider(new Rect(WindowStartX + 90, WindowStartY + 50, WindowSizeX - 140, 20),
						Config.Current.Scale, 2f, 12f);

					Config.Current.Offset =
						GUI.HorizontalSlider(new Rect(WindowStartX + 90, WindowStartY + 75, WindowSizeX - 140, 20),
							Config.Current.Offset, 0f, 3.5f);

					Config.Current.SanitizeNicknames = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 100, 250, 20),
						Config.Current.SanitizeNicknames,
						new GUIContent(LocalizationManager.GetString("t20"), LocalizationManager.GetString("t24"))
					);

					Config.Current.GFriendsIntegration = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 125, 300, 20),
						Config.Current.GFriendsIntegration,
						new GUIContent(LocalizationManager.GetString("t21"), LocalizationManager.GetString("t25"))
					);

					// Labels
					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 45, 80, 20),
						new GUIContent(LocalizationManager.GetString("t22"), LocalizationManager.GetString("t22", 1))
					);

					GUI.Label(new Rect(WindowStartX + WindowSizeX - 50, WindowStartY + 45, 30, 20), $"{Config.Current.Scale}");

					GUI.Label(
						new Rect(WindowStartX, WindowStartY + 70, 80, 20),
						new GUIContent(LocalizationManager.GetString("t23"), LocalizationManager.GetString("t23", 1))
					);

					GUI.Label(new Rect(WindowStartX + WindowSizeX - 50, WindowStartY + 70, 30, 20), $"{Config.Current.Offset}");
				}

				break;
			case 1:
				Config.Current.Icons = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 250, 20),
					Config.Current.Icons,
					new GUIContent(LocalizationManager.GetString("t26"), LocalizationManager.GetString("t26", 1))
				);

				if (Config.Current.Icons)
				{
					Config.Current.UserIcons = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 25, 250, 20),
						Config.Current.UserIcons,
						new GUIContent(LocalizationManager.GetString("t27"), LocalizationManager.GetString("t27", 1))
					);

					Config.Current.PlatformIcons = GUI.Toggle(
						new Rect(WindowStartX + 10, WindowStartY + 50, 250, 20),
						Config.Current.PlatformIcons,
						new GUIContent(LocalizationManager.GetString("t28"), LocalizationManager.GetString("t28", 1))
					);
				}

				break;
			case 2:
				var propsToggle = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY, 175, 20),
					Config.Current.CustomNametags,
					new GUIContent(LocalizationManager.GetString("t29"), LocalizationManager.GetString("t29", 1))
				);

				if (propsToggle != Config.Current.CustomNametags && !Config.Current.CustomNametags)
				{
					Ask(LocalizationManager.GetString("t30"),
						[LocalizationManager.GetString("Yes"), LocalizationManager.GetString("No")],
						result => Config.Current.CustomNametags = (result == LocalizationManager.GetString("Yes"))
					);
				} else if (propsToggle != Config.Current.CustomNametags)
				{
					Config.Current.CustomNametags = false;
				}

				if (Config.Current.CustomNametags)
				{
					Config.Current.NetworkColor = GUI.TextArea(
						new Rect(WindowStartX + 75, WindowStartY + 25, 200, 20),
						Config.Current.NetworkColor
					);

					GUI.Label(new Rect(WindowStartX, WindowStartY + 25, 75, 20),
						new GUIContent(LocalizationManager.GetString("t31"), LocalizationManager.GetString("t31", 1)));

					Config.Current.NetworkBold = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 50, 175, 20),
						Config.Current.NetworkBold,
						LocalizationManager.GetString("t32")
					);

					Config.Current.NetworkItalic = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 75, 175, 20),
						Config.Current.NetworkItalic,
						LocalizationManager.GetString("t33")
					);

					Config.Current.NetworkUnderline = GUI.Toggle(
						new Rect(WindowStartX, WindowStartY + 100, 175, 20),
						Config.Current.NetworkUnderline,
						LocalizationManager.GetString("t34")
					);
				}

				Config.Current.ViewOtherCustomStyles = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 25 + (Config.Current.CustomNametags ? 100 : 0), 175, 20),
					Config.Current.ViewOtherCustomStyles,
					new GUIContent(LocalizationManager.GetString("t35"), LocalizationManager.GetString("t35", 1))
				);

				GUI.Label(new Rect(WindowStartX, WindowStartY + 50 + (Config.Current.CustomNametags ? 100 : 0), WindowSizeX - WindowPadding * 2, 20),
					LocalizationManager.GetString("t36") + ':');
				
				var autoUpdFull = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 70 + (Config.Current.CustomNametags ? 100 : 0), WindowSizeX - WindowPadding * 2, 20),
					Config.Current.AutoUpdateMode == 0,
					new GUIContent(LocalizationManager.GetString("t37"), LocalizationManager.GetString("t37", 1))
				);

				var autoUpdPrompt = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 90 + (Config.Current.CustomNametags ? 100 : 0), WindowSizeX - WindowPadding * 2, 20),
					Config.Current.AutoUpdateMode == 1,
					new GUIContent(LocalizationManager.GetString("t38"), LocalizationManager.GetString("t38", 1))
				);

				var autoUpdOff = GUI.Toggle(
					new Rect(WindowStartX, WindowStartY + 110 + (Config.Current.CustomNametags ? 100 : 0), WindowSizeX - WindowPadding * 2, 20),
					Config.Current.AutoUpdateMode == 2,
					new GUIContent(LocalizationManager.GetString("t39"), LocalizationManager.GetString("t39", 1))
				);

				if (autoUpdFull)
					Config.Current.AutoUpdateMode = 0;
				if (autoUpdPrompt)
					Config.Current.AutoUpdateMode = 1;
				if (autoUpdOff)
					Config.Current.AutoUpdateMode = 2;

				break;
			case 3:
				if (GUI.Button(
					new Rect(WindowStartX, WindowStartY, WindowSizeX - 20, 20),
					new GUIContent(LocalizationManager.GetString("t40"), LocalizationManager.GetString("t40", 1))
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
						new GUIContent(LocalizationManager.GetString("t41"), LocalizationManager.GetString("t41", 1)));

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
					$"BingusNametags++");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 20, WindowSizeX - WindowPadding * 2, 20),
					$"v{Constants.Version}-{(Constants.Channel == ReleaseChannel.Stable ? "stable" : "beta")}");
				GUI.Label(new Rect(WindowStartX, WindowStartY + 40, WindowSizeX - WindowPadding * 2, 20),
					"(C) Copyright 2025 - 2026 Bingus/SirKingBinx / MIT License");

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
					$"""
{LocalizationManager.GetString("t3")}:
- Medievalz
- Monky
- tfsdemon
- salty
- Linear
- Golden
- Josh
- Ariel
- Crazykid
- nebwella

{LocalizationManager.GetString("t4")}:
- sirkingbinx
	
Join the Discord:
https://discord.gg/SYCpaKjyU6
"""
				);

				if (GUI.Button(new Rect(WindowStartX, WindowY + WindowSizeY - 25, 150, 20),
				  	new GUIContent(LocalizationManager.GetString("t42"), LocalizationManager.GetString("t42", 1))))
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
			new Rect(WindowX + 5, WindowY + 5, 100, 20),
			new GUIContent(LocalizationManager.Languages[LocalizationManager.CurrentLanguageIndex]))
		)
		{
			LocalizationManager.CurrentLanguageIndex = (LocalizationManager.CurrentLanguageIndex + 1) % LocalizationManager.Languages.Length;
			LocalizationManager.SetLanguage(LocalizationManager.CurrentLanguageIndex);
		}

		if (GUI.Button(
			new Rect(WindowX + WindowSizeX - 190, WindowY + WindowSizeY - 25, 100, 20),
			new GUIContent(LocalizationManager.GetString("t43"), LocalizationManager.GetString("t43", 1)))
		)
			Config.LoadPrefs();

		if (GUI.Button(new Rect(WindowX + WindowSizeX - 80, WindowY + WindowSizeY - 25, 75, 20),
				new GUIContent(LocalizationManager.GetString("t44"), LocalizationManager.GetString("t44", 1))))
			Config.SavePrefs();

        #region Debug Stuff
#pragma warning disable CS0162
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
#pragma warning restore CS0162
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
		ShowingUI = true;
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
			LocalizationManager.GetString("t45") + ":"
		);

		foreach (var nametagMeta in CurrentlyInspectedNametag.Nametags.Keys)
		{
			float currentOffset = Config.GetNametagOffset(CurrentlyInspectedNametag, nametagMeta);

            GUILayout.Label($"- {nametagMeta.Name} ({LocalizationManager.GetString("t46")}: {currentOffset})");
            float newOffset = GUILayout.HorizontalSlider(currentOffset, -5f, 5f);

			if (currentOffset != newOffset)
				Config.SaveNametagOffset(CurrentlyInspectedNametag, nametagMeta, MathF.Round(newOffset, 2));
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();

		if (GUI.Button(new Rect(WindowX + (WindowSizeX - 105), WindowY + WindowSizeY - 25, 100, 20),
				new GUIContent(LocalizationManager.GetString("t48"), LocalizationManager.GetString("t47"))))
			WindowState = BGWindowState.Normal;
	}

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
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
            ShowingUI = !ShowingUI;

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

        var mousePosition = Mouse.current.position.ReadValue();

		// X button
		if (GUI.Button(new Rect(WindowX + WindowSizeX - 25, WindowY + 5, 20, 20), new GUIContent("X", LocalizationManager.GetString("t48"))))
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
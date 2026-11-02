using System;
using System.Collections.Generic;
using System.Diagnostics;
using BingusNametagsPlusPlus.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

public static class UIManager
{
    public static bool ShowingUI;
    private static GameObject? canvasObject;
    private static GameObject? ownedEvents;
    private static RectTransform window = null!;
    private static RectTransform body = null!;
    private static RawImage liveImage = null!;
    private static Text saveStatus = null!;
    private static readonly Vector2 WindowSize = new(552, 460);
    private static Vector2 windowPosition = new(16, -16);
    private const float PreviewWidth = 360;
    private static Font font = null!;
    private static EventSystem? previousEvents;
    private static bool inputCaptured;
    private static bool previousEventsEnabled;
    private static float nextPreviewErrorLog;
    private static Sprite? rounded;
    private static int page;
    private static bool rebuild;
    private static BingusNametagsPlugin? inspected;
    private static string? question;
    private static List<string> answers = [];
    private static Action<string> callback = delegate { };
    private static readonly Color Ink = UITheme.Text;
    private static readonly Color Quiet = UITheme.Muted;
    private static readonly Color Blue = UITheme.Accent;
    private static readonly Color Panel = UITheme.Panel;

    private static string T(string key) => LocalizationManager.GetString(key);

    private static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }

    private static Sprite Rounded()
    {
        if (rounded != null)
            return rounded;
        var texture = new Texture2D(32, 32)
        {
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float dx = Mathf.Max(4 - x, x - 27, 0), dy = Mathf.Max(4 - y, y - 27, 0);
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(4.5f - Mathf.Sqrt(dx * dx + dy * dy))));
            }

        texture.Apply();
        rounded = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
        return rounded;
    }

    private static Image Surface(RectTransform rect, Color color, Color? border = null)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = Rounded();
        image.type = Image.Type.Sliced;
        image.color = color;
        if (border.HasValue)
        {
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = border.Value;
            outline.effectDistance = new Vector2(1, -1);
        }

        return image;
    }

    private static Text Label(Transform parent, string text, float x, float y, float w, float h, int size = 15, Color? color = null)
    {
        var label = Rect(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.color = color ?? Ink;
        label.raycastTarget = false;
        label.supportRichText = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    private static Button Button(Transform parent, string text, float x, float y, float w, float h, Action action, bool primary = false, bool selected = false)
    {
        var rect = Rect(parent, text, x, y, w, h);
        var image = Surface(rect, Color.white, UITheme.InputBorder);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = primary ? UITheme.Primary : selected ? UITheme.Selected : UITheme.Input;
        colors.highlightedColor = primary ? UITheme.Hover : UITheme.Selected;
        colors.pressedColor = UITheme.Primary;
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = UITheme.Panel;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        // Set the initial tint before the canvas is shown.
        image.CrossFadeColor(colors.normalColor, 0, true, true);
        button.onClick.AddListener(() => action());
        var label = Label(rect, text, 8, 0, w - 16, h, 13, primary ? Color.white : Ink);
        label.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private static RectTransform Scroll(Transform parent, float x, float y, float w, float h)
    {
        var viewport = Rect(parent, "Scrollable settings", x, y, w, h);
        Surface(viewport, Panel, UITheme.Border);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect(viewport, "Content", 0, 0, w - 12, h);
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32;
        var rail = Rect(viewport, "Scroll rail", w - 7, 4, 4, h - 8);
        var railImage = Surface(rail, UITheme.Input);
        var handle = Rect(rail, "Scroll handle", 0, 0, 4, 36);
        var handleImage = Surface(handle, Quiet);
        var scrollbar = rail.gameObject.AddComponent<Scrollbar>();
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scroll.verticalScrollbar = scrollbar;
        return content;
    }

    private static float row;

    private static void Heading(string text)
    {
        Label(body, text, 10, row, 366, 24, 15);
        row += 28;
    }

    private static void Note(string text, float height = 44)
    {
        Label(body, text, 10, row, 366, height, 13, Quiet);
        row += height + 4;
    }

    private static void Switch(string key, bool value, Action<bool> changed, Action? inspect = null)
    {
        var track = Rect(body, key, 10, row, inspect == null ? 366 : 266, 40);
        Surface(track, new Color(1, 1, 1, 0));
        var toggle = track.gameObject.AddComponent<Toggle>();
        var title = Label(track, T(key), 0, 2, inspect == null ? 300 : 208, 36, 13, UITheme.Label);
        title.alignment = TextAnchor.MiddleLeft;
        var pill = Rect(track, "Switch", inspect == null ? 320 : 220, 8, 42, 24);
        var background = Surface(pill, value ? UITheme.Switch : UITheme.InputBorder);
        var knob = Rect(pill, "Knob", value ? 22 : 4, 4, 16, 16);
        var knobImage = Surface(knob, Ink);
        knobImage.raycastTarget = false;
        toggle.targetGraphic = background;
        toggle.SetIsOnWithoutNotify(value);
        toggle.onValueChanged.AddListener(on =>
        {
            background.color = on ? UITheme.Switch : UITheme.InputBorder;
            knob.anchoredPosition = new Vector2(on ? 22 : 4, -4);
            changed(on);
            Dirty();
        });
        if (inspect != null)
            Button(body, T("t41"), 286, row + 4, 86, 32, inspect);
        row += 40;
    }

    private static void Range(string key, float value, float min, float max, Action<float> changed)
    {
        Label(body, T(key), 10, row, 278, 24, 13);
        var number = Label(body, value.ToString("0.00"), 304, row, 72, 24, 13, Blue);
        number.alignment = TextAnchor.MiddleRight;
        var rect = Rect(body, key, 10, row + 25, 362, 24);
        var slider = rect.gameObject.AddComponent<Slider>();
        var background = Rect(rect, "Track", 0, 9, 362, 5);
        Surface(background, UITheme.Border);
        var fillArea = Rect(rect, "Fill area", 0, 9, 362, 5);
        var fill = Rect(fillArea, "Fill", 0, 0, 362, 5);
        Surface(fill, Blue);
        var handleArea = Rect(rect, "Handle area", 8, 0, 346, 24);
        var handle = Rect(handleArea, "Handle", 0, 4, 16, 16);
        var handleImage = Surface(handle, Ink);
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(v =>
        {
            v = (float)Math.Round(v, 2);
            number.text = v.ToString("0.00");
            changed(v);
            Dirty();
        });
        row += 58;
    }

    private static void ActionRow(string key, Action action)
    {
        Button(body, T(key), 10, row, 362, 32, action);
        row += 38;
    }

    private static void Dirty()
    {
        if (saveStatus != null)
            saveStatus.text = T("Canvas.Unsaved");
    }

    private static void Build()
    {
        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
            Object.Destroy(canvasObject);
        }

        font = UITheme.Font;
        canvasObject = new GameObject("BingusNametags++ UI", typeof(RectTransform));
        canvasObject.SetActive(false);
        canvasObject.AddComponent<Canvas>();
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        Object.DontDestroyOnLoad(canvasObject);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 31000;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = DisplayScale;
        if (ownedEvents == null)
        {
            ownedEvents = new GameObject("BingusNametags UI input");
            ownedEvents.SetActive(false);
            ownedEvents.AddComponent<EventSystem>();
            ownedEvents.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            Object.DontDestroyOnLoad(ownedEvents);
        }

        window = Rect(canvasObject.transform, "Settings window", 0, 0, WindowSize.x, WindowSize.y);
        window.anchoredPosition = windowPosition;
        Surface(window, UITheme.Background, UITheme.Border);
        var dragBar = Rect(window, "Drag window", 0, 0, WindowSize.x - 40, 36);
        Surface(dragBar, new Color(1, 1, 1, 0));
        dragBar.gameObject.AddComponent<WindowDragHandle>().Initialize(window, position => windowPosition = position, PreviewWidth + 12);
        Label(dragBar, "BingusNametags", 12, 7, 120, 24, 15);
        Label(dragBar, "++", 132, 7, 28, 24, 15, Blue);
        Button(window, "x", 516, 6, 28, 28, () => ShowingUI = false);
        var nav = Rect(window, "Navigation", 8, 44, 124, 362);
        Surface(nav, Panel, UITheme.Border);
        for (int i = 0; i < 5; i++)
        {
            int selected = i;
            Button(nav, T("t" + (6 + i)), 6, 6 + 38 * i, 112, 32, () =>
            {
                page = selected;
                inspected = null;
                rebuild = true;
            }, selected: i == page);
        }

        Label(nav, "v" + Constants.Version, 10, 292, 104, 20, 11, Quiet);
        LanguageDropdown(nav);
        Label(window, inspected?.Name ?? T("t" + (6 + page)), 146, 44, 390, 26, 20);
        body = Scroll(window, 140, 76, 404, 330);
        row = 10;
        BuildSettings();
        body.sizeDelta = new Vector2(392, Mathf.Max(row + 10, 330));
        BuildPreview();
        saveStatus = Label(window, "", 12, 419, 280, 34, 11, Quiet);
        Button(window, T("t43"), 302, 418, 112, 32, () =>
        {
            Config.LoadPrefs();
            LocalizationManager.SetLanguage(Config.Current.SetLanguage);
            rebuild = true;
        });
        Button(window, T("t44"), 422, 418, 122, 32, () =>
        {
            Config.SavePrefs();
            saveStatus.text = T("Canvas.Saved");
        }, true);
        if (question != null)
            BuildPrompt();
        rebuild = false;
    }

    private static void LanguageDropdown(Transform parent)
    {
        var rect = Rect(parent, "Language", 6, 320, 112, 32);
        var background = Surface(rect, UITheme.Input, UITheme.InputBorder);
        var dropdown = rect.gameObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = background;
        var colors = dropdown.colors;
        colors.highlightedColor = new Color(0.85f, 0.92f, 1);
        colors.pressedColor = colors.highlightedColor;
        colors.selectedColor = Color.white;
        dropdown.colors = colors;
        var caption = Label(rect, "", 8, 0, 82, 32, 13);
        caption.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = caption;
        Label(rect, "▾", 92, 0, 16, 32, 13, Quiet).alignment = TextAnchor.MiddleCenter;
        // Open above the selector so the list stays inside the window.
        const float listHeight = 224;
        var template = Rect(rect, "Language list", 0, -listHeight - 4, 112, listHeight);

        // Unity's default dropdown canvas sorts below this window.
        var windowCanvas = canvasObject!.GetComponent<Canvas>();
        var listCanvas = template.gameObject.AddComponent<Canvas>();
        listCanvas.overrideSorting = true;
        listCanvas.sortingLayerID = windowCanvas.sortingLayerID;
        listCanvas.sortingOrder = windowCanvas.sortingOrder + 1;
        template.gameObject.AddComponent<GraphicRaycaster>();

        Surface(template, Panel, UITheme.Border);
        var content = Scroll(template, 0, 0, 112, listHeight);
        // The template content starts with a single option.
        content.sizeDelta = new Vector2(100, 28);
        var item = Rect(content, "Language option", 0, 0, 100, 28);
        var itemBackground = Surface(item, UITheme.Input);
        var toggle = item.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = itemBackground;
        var itemColors = toggle.colors;
        itemColors.highlightedColor = new Color(0.8f, 0.9f, 1);
        itemColors.pressedColor = itemColors.highlightedColor;
        toggle.colors = itemColors;
        var marker = Rect(item, "Selected", 0, 4, 2, 20);
        toggle.graphic = Surface(marker, Blue);
        var itemLabel = Label(item, "", 8, 0, 88, 28, 13);
        itemLabel.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabel;
        dropdown.template = template;
        template.gameObject.SetActive(false);
        dropdown.AddOptions(new List<string>(LocalizationManager.Languages));
        dropdown.SetValueWithoutNotify(LocalizationManager.CurrentLanguageIndex);
        dropdown.onValueChanged.AddListener(index =>
        {
            LocalizationManager.SetLanguage(index);
            rebuild = true;
        });
    }

    private static void BuildSettings()
    {
        var c = Config.Current;
        if (inspected != null)
        {
            Note(string.Format(T("UI.Author"), inspected.Author));
            Note(inspected.Description, 92);
            Heading(T("t45"));
            foreach (var tag in inspected.Nametags.Keys)
            {
                var captured = tag;
                Range(tag.Name, Config.GetNametagOffset(inspected, tag), -5, 5, v => Config.SaveNametagOffset(inspected!, captured, v));
            }

            ActionRow("Canvas.Back", () =>
            {
                inspected = null;
                rebuild = true;
            });
            return;
        }

        switch (page)
        {
            case 0:
                Switch("t16", c.Nametags, v => c.Nametags = v);
                Switch("t17", c.FirstPersonEnabled, v => c.FirstPersonEnabled = v);
                Switch("t18", c.ThirdPersonEnabled, v => c.ThirdPersonEnabled = v);
                row += 8;
                Heading(T("Canvas.Placement"));
                Range("t22", c.Scale, 2, 12, v => c.Scale = v);
                Range("t23", c.Offset, 0, 3.5f, v => c.Offset = v);
                Switch("t20", c.SanitizeNicknames, v => c.SanitizeNicknames = v);
                Switch("t21", c.GFriendsIntegration, v => c.GFriendsIntegration = v);
                break;
            case 1:
                Switch("t26", c.Icons, v => c.Icons = v);
                Switch("t27", c.UserIcons, v => c.UserIcons = v);
                Switch("t28", c.PlatformIcons, v => c.PlatformIcons = v);
                break;
            case 2:
                Switch("t29", c.CustomNametags, v =>
                {
                    if (!v)
                    {
                        c.CustomNametags = false;
                        rebuild = true;
                        return;
                    }

                    string yes = T("Yes");
                    Ask(T("t30"), [yes, T("No")], answer =>
                    {
                        c.CustomNametags = answer == yes;
                        rebuild = true;
                    });
                });
                if (c.CustomNametags)
                {
                    Label(body, T("t31"), 10, row, 362, 24, 13);
                    row += 26;
                    var inputRect = Rect(body, "Hex color", 10, row, 362, 32);
                    Surface(inputRect, UITheme.Input, UITheme.InputBorder);
                    var input = inputRect.gameObject.AddComponent<InputField>();
                    input.selectionColor = new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.35f);
                    input.textComponent = Label(inputRect, "", 8, 3, 346, 26, 13);
                    input.text = c.NetworkColor;
                    input.characterLimit = 7;
                    var validation = Label(body, "", 10, row + 36, 362, 32, 12, Quiet);
                    input.onValueChanged.AddListener(v =>
                    {
                        c.NetworkColor = v;
                        validation.text = Config.ValidHexCode(v) ? "" : T("Preview.InvalidColor");
                        Dirty();
                    });
                    row += 70;
                    Switch("t32", c.NetworkBold, v => c.NetworkBold = v);
                    Switch("t33", c.NetworkItalic, v => c.NetworkItalic = v);
                    Switch("t34", c.NetworkUnderline, v => c.NetworkUnderline = v);
                }

                Switch("t35", c.ViewOtherCustomStyles, v => c.ViewOtherCustomStyles = v);
                row += 8;
                Heading(T("t36"));
                for (int i = 0; i < 3; i++)
                {
                    int mode = i;
                    Button(body, T("t" + (37 + i)), 10, row, 362, 32, () =>
                    {
                        c.AutoUpdateMode = mode;
                        rebuild = true;
                    }, selected: c.AutoUpdateMode == i);
                    row += 38;
                }

                break;
            case 3:
                ActionRow("t40", PluginManager.OpenNametagsFolder);
                foreach (var plugin in PluginManager.Plugins)
                {
                    var p = plugin;
                    Switch(p.Metadata.Name, p.Metadata.Enabled, on =>
                    {
                        if (on)
                            PluginManager.EnablePlugin(p);
                        else
                            PluginManager.DisablePlugin(p);
                    }, () =>
                    {
                        inspected = p.Metadata;
                        rebuild = true;
                    });
                }

                break;
            case 4:
                Heading("BingusNametags++");
                Note("© 2025–2026 Bingus/SirKingBinx · MIT");
                Heading(T("t3"));
                Note("Medievalz, Monky, tfsdemon, salty, Linear, Golden, Josh, Ariel, Crazykid, nebwella", 100);
                Heading(T("t4"));
                Note("sirkingbinx");
                ActionRow("UI.Community", () => Open("https://discord.gg/SYCpaKjyU6"));
                ActionRow("t42", () => Open(Constants.BingusNametagsData));
                break;
        }
    }

    private static void Open(string path) => Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });

    private static void BuildPreview()
    {
        // Keep the preview outside the window.
        var stage = Rect(window, "Rendered nametags", WindowSize.x + 12, 44, PreviewWidth, PreviewWidth * 640 / 1080);
        liveImage = stage.gameObject.AddComponent<RawImage>();
        liveImage.raycastTarget = false;
        liveImage.color = Color.clear;
    }

    private static void BuildPrompt()
    {
        var overlay = Rect(window, "Modal backdrop", 0, 36, 552, 424);
        Surface(overlay, new Color(0.01f, 0.02f, 0.04f, 0.94f));
        var modal = Rect(overlay, "Confirmation", 8, 8, 536, 408);
        Surface(modal, Panel);
        var content = Scroll(modal, 12, 12, 512, 320);
        var text = Label(content, question!, 10, 10, 476, 600, 15);
        text.rectTransform.sizeDelta = new Vector2(476, text.preferredHeight + 20);
        content.sizeDelta = new Vector2(500, Mathf.Max(320, text.preferredHeight + 30));
        float width = 512f / Math.Max(1, answers.Count);
        for (int i = 0; i < answers.Count; i++)
        {
            string answer = answers[i];
            Button(modal, answer, 12 + i * width, 348, width - 8, 36, () =>
            {
                var onAnswer = callback;
                question = null;
                rebuild = true;
                onAnswer(answer);
            }, i == 0);
        }
    }

    public static void Ask(string text, List<string> options, Action<string> onAnswer)
    {
        question = text;
        answers = options;
        callback = onAnswer;
        ShowingUI = true;
        rebuild = true;
    }

    public static void Tick()
    {
        if (!ShowingUI)
        {
            if (canvasObject != null)
                canvasObject.SetActive(false);
            RestoreInput();
            NametagPreview.Release();
            return;
        }

        if (canvasObject == null || rebuild)
            Build();
        canvasObject!.SetActive(true);
        canvasObject.GetComponent<CanvasScaler>().scaleFactor = DisplayScale;
        WindowDragHandle.Clamp(window, PreviewWidth + 12);
        windowPosition = window.anchoredPosition;
        if (!inputCaptured)
        {
            previousEvents = EventSystem.current;
            previousEventsEnabled = previousEvents != null && previousEvents.enabled;
            if (previousEvents != null)
                previousEvents.enabled = false;
            inputCaptured = true;
            ownedEvents!.SetActive(true);
        }

        try
        {
            var texture = NametagPreview.Render();
            liveImage.texture = texture;
            liveImage.color = texture == null ? Color.clear : Color.white;
        }
        catch (Exception ex)
        {
            liveImage.color = Color.clear;
            if (Time.unscaledTime >= nextPreviewErrorLog)
            {
                UnityEngine.Debug.LogException(ex);
                nextPreviewErrorLog = Time.unscaledTime + 10;
            }
        }
    }

    private static float DisplayScale => Mathf.Min(1f, Screen.width / (WindowSize.x + PreviewWidth + 44), Screen.height / (WindowSize.y + 32));

    private static void RestoreInput()
    {
        if (!inputCaptured)
            return;

        if (ownedEvents != null)
            ownedEvents.SetActive(false);
        if (previousEvents != null)
            previousEvents.enabled = previousEventsEnabled;
        previousEvents = null;
        inputCaptured = false;
    }

    public static void Release()
    {
        RestoreInput();
        NametagPreview.Release();
        if (canvasObject != null)
            Object.Destroy(canvasObject);
        if (ownedEvents != null)
            Object.Destroy(ownedEvents);
        canvasObject = null;
        ownedEvents = null;
    }
}

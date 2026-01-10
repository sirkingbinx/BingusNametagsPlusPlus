using System.Collections.Generic;
using BingusNametagsPlusPlus.Utilities;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Classes;

/// <summary>
/// PlayerNametag represents the nametag of a single player.
/// </summary>
public class PlayerNametag(VRRig player, GameObject firstPerson, GameObject thirdPerson)
{
    private readonly List<string> _styles = [];
    private readonly Dictionary<string, string> _valueStyles = [];

    /// <summary>
    /// The owner of the nametag.
    /// </summary>
    public VRRig Owner => player;

    /// <summary>
    /// Adds a rich text style to the nametag.
    /// </summary>
    /// <param name="tag">The rich text tag to add.</param>
    public void AddStyle(string tag)
    {
        if (!_styles.Contains(tag))
            _styles.Add(tag);
    }

    /// <summary>
    /// Adds a rich text style with a value to the nametag.
    /// </summary>
    /// <param name="tag">The rich text tag to add.</param>
    /// /// <param name="value">The value of the tag.</param>
    public void AddStyle(string tag, string value)
    { 
        if (_valueStyles.TryGetValue(tag, out var v) && (v == value))
            return;

        _valueStyles.Remove(tag);
        _valueStyles.Add(tag, value);
    }

    /// <summary>
    /// Removes a rich text style from the nametag.
    /// </summary>
    /// <param name="tag">The rich text tag to remove.</param>
    public void RemoveStyle(string tag)
    {
        _styles.Remove(tag);
        _valueStyles.Remove(tag);
    }

    private string _text = "";

    /// <summary>
    /// The text of the nametag.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            var tmpFirst = firstPerson.GetComponent<TextMeshPro>();
            var tmpThird = thirdPerson.GetComponent<TextMeshPro>();

            var start = "";
            var end = "";

            foreach (var style in _styles)
            {
                start += $"<{style}>";
                end += $"</{style}>";
            }

            foreach (var vstyle in _valueStyles)
            {
                start += $"<{vstyle.Key}={vstyle.Value}>";
                end += $"</{vstyle.Key}>";
            }

            tmpFirst.text = $"{start}{value}{end}";
            tmpThird.text = $"{start}{value}{end}";

            _text = value;
        }
    }

    /// <summary>
    /// The size of the nametag.
    /// </summary>
    public float Size
    {
        get => firstPerson.GetComponent<TextMeshPro>().fontSize;
        set => firstPerson.GetComponent<TextMeshPro>().fontSize = value;
    }

    /// <summary>
    /// The sprite sheet of the nametag. Sprite sheets are used to provide icons to nametags.
    ///
    /// By default, the sprite sheet includes three icons: "steam", "oculus", and "meta". You can use them like this:
    /// <code>
    /// &lt;sprite name="meta"&gt;
    /// </code>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/Sprites.html"/>
    /// </summary>
    public TMP_SpriteAsset SpriteSheet
    {
        get => firstPerson.GetComponent<TextMeshPro>().spriteAsset;
        set
        {
            firstPerson.GetComponent<TextMeshPro>().spriteAsset = value;
            thirdPerson.GetComponent<TextMeshPro>().spriteAsset = value;
        }
    }

    internal void UpdateSettings(float offset)
    {
        firstPerson.GetComponent<TextMeshPro>().fontSize = ConfigManager.Scale;
        thirdPerson.GetComponent<TextMeshPro>().fontSize = ConfigManager.Scale;

        firstPerson.transform.localPosition = new Vector3(0f, ConfigManager.Offset + offset, 0f);
        thirdPerson.transform.localPosition = new Vector3(0f, ConfigManager.Offset + offset, 0f);

        firstPerson.SetActive(ConfigManager.FirstPersonEnabled);
        thirdPerson.SetActive(ConfigManager.ThirdPersonEnabled);

        if (!(ConfigManager.CustomNametags && player.OwningNetPlayer.GetPlayerRef().CustomProperties.TryGetValue("BingusNametags++", out var rawData)))
            return;

        var data = (Dictionary<string, object>)rawData;
        if (data == null)
            return;

        var color = (string)data["Color"];
        var bold = (bool)data["isBold"];
        var italic = (bool)data["isItalic"];
        var underlined = (bool)data["isUnderlined"];

        if (ConfigManager.ValidHexCode(color))
            AddStyle("color", $"#{color}");

        if (bold) 
            AddStyle("b");
        else
            RemoveStyle("b");

        if (italic)
            AddStyle("i");
        else
            RemoveStyle("i");

        if (underlined)
            AddStyle("u");
        else
            RemoveStyle("u");
    }

    internal void Destroy()
    {
        firstPerson.Destroy();
        thirdPerson.Destroy();
    }
}
using System.Collections.Generic;
using BingusNametagsPlusPlus.Utilities;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Classes;

public class PlayerNametag(VRRig player, GameObject firstPerson, GameObject thirdPerson)
{
    private readonly List<string> _styles = [];
    private readonly Dictionary<string, string> _valueStyles = [];

    public VRRig Owner => player;

    public void AddStyle(string tag)
    {
        if (!_styles.Contains(tag))
            _styles.Add(tag);
    }

    public void AddStyle(string tag, string value)
    { 
        if (_valueStyles.TryGetValue(tag, out var v) && (v == value))
            return;

        _valueStyles.Remove(tag);
        _valueStyles.Add(tag, value);
    }

    public void RemoveStyle(string tag)
    {
        _styles.Remove(tag);
        _valueStyles.Remove(tag);
    }

    private string _text = "";

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

    internal void UpdateSettings(float offset)
    {
        firstPerson.GetComponent<TextMeshPro>().fontSize = Config.NametagScale;
        thirdPerson.GetComponent<TextMeshPro>().fontSize = Config.NametagScale;

        firstPerson.transform.localPosition = new Vector3(0f, Config.NametagYOffset + offset, 0f);
        thirdPerson.transform.localPosition = new Vector3(0f, Config.NametagYOffset + offset, 0f);

        firstPerson.SetActive(Config.ShowInFirstPerson);
        thirdPerson.SetActive(Config.ShowInThirdPerson);

        if (!(Config.CustomNametags && player.OwningNetPlayer.GetPlayerRef().CustomProperties.TryGetValue("BingusNametags++", out var rawData)))
            return;

        var data = (Dictionary<string, object>)rawData;
        if (data == null)
            return;

        var color = (string)data["Color"];
        var bold = (bool)data["isBold"];
        var italic = (bool)data["isItalic"];
        var underlined = (bool)data["isUnderlined"];

        if (Config.ValidHexCode(color))
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
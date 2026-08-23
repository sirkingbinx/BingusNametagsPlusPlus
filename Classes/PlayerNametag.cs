using System.Collections.Generic;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Utilities;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Classes;

/// <summary>
/// PlayerNametag represents the nametag of a single player.
/// </summary>
public class PlayerNametag(VRRig player, GameObject nametag, GameObject tpNametag)
{
#region api
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

    private TextMeshPro? fpTmp;
    private TextMeshPro? tpTmp;

    /// <summary>
    /// The text of the nametag.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            fpTmp ??= nametag.GetComponent<TextMeshPro>();
            tpTmp ??= tpNametag.GetComponent<TextMeshPro>();

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

            string text = $"{start}{value}{end}";

            fpTmp.text = text;
            tpTmp.text = text;

            _text = value;
        }
    }

    /// <summary>
    /// The size of the nametag.
    /// </summary>
    public float Size
    {
        get => PluginScale;
        set => PluginScale = value;
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
        get => nametag.GetComponent<TextMeshPro>().spriteAsset;
        set
        {
            nametag.GetComponent<TextMeshPro>().spriteAsset = value;
            tpNametag.GetComponent<TextMeshPro>().spriteAsset = value;
        }
    }
#endregion

#region not api
    internal float PluginScale = 1f;

    internal void UpdateSettings(BingusNametagsPlugin plugin, BingusNametagsNametag pNametag)
    {
        if (nametag.activeSelf != Config.Current.Nametags)
        {
            nametag.SetActive(Config.Current.Nametags);
            tpNametag.SetActive(Config.Current.Nametags);
        }
        
        if (!(nametag.activeSelf || tpNametag.activeSelf))
            return;

        float offset = Config.GetNametagOffset(plugin, pNametag);

        nametag.GetComponent<TextMeshPro>().fontSize = Config.Current.Scale * PluginScale;
        nametag.transform.localPosition = new Vector3(0f, Config.Current.Offset + offset, 0f);

        tpNametag.GetComponent<TextMeshPro>().fontSize = Config.Current.Scale * PluginScale;
        tpNametag.transform.localPosition = new Vector3(0f, Config.Current.Offset + offset, 0f);

        if (nametag.activeSelf != Config.Current.FirstPersonEnabled)
            nametag.SetActive(Config.Current.FirstPersonEnabled);
        if (tpNametag.activeSelf != Config.Current.ThirdPersonEnabled)
            tpNametag.SetActive(Config.Current.ThirdPersonEnabled);
    }

    internal void Destroy()
    {
        nametag.Destroy();
        tpNametag.Destroy();
    }
#endregion
}

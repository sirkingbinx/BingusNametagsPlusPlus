using System.Collections.Generic;
using BingusNametagsPlusPlus.Utilities;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Classes;

/// <summary>
/// PlayerNametag represents the nametag of a single player.
/// </summary>
public class PlayerNametag(VRRig player, GameObject nametag)
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

    /// <summary>
    /// The text of the nametag.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            var tmp = nametag.GetComponent<TextMeshPro>();

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

            tmp.text = $"{start}{value}{end}";

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
        set => nametag.GetComponent<TextMeshPro>().spriteAsset = value;
    }
#endregion

#region not api
    internal float PluginScale = 1f;

    internal void UpdateSettings(float offset)
    {
        if (nametag.activeSelf != Config.Current.Nametags)
            nametag.SetActive(Config.Current.Nametags);
        
        if (!nametag.activeSelf)
            return;

        nametag.GetComponent<TextMeshPro>().fontSize = Config.Current.Scale * PluginScale;
        nametag.transform.localPosition = new Vector3(0f, Config.Current.Offset + offset, 0f);

        if (Config.Current.FirstPersonEnabled && Config.Current.ThirdPersonEnabled)
            nametag.layer = 0;
        else if (Config.Current.FirstPersonEnabled && !Config.Current.ThirdPersonEnabled)
            nametag.layer = LayerMask.NameToLayer("FirstPersonOnly");
        else if (!Config.Current.FirstPersonEnabled && Config.Current.ThirdPersonEnabled)
            nametag.layer = LayerMask.NameToLayer("MirrorOnly");
    }

    internal void Destroy()
    {
        nametag.Destroy();
    }
#endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Nametags;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

internal static class NametagPreview
{
    private static GameObject? root;
    private static Camera? camera;
    private static RenderTexture? texture;
    private static VRRig? owner;
    private static float nextFrame;
    private static readonly Dictionary<BingusNametagsNametag, PlayerNametag> Tags = new();
    internal static int Count => Tags.Count;
    internal static int Failures { get; private set; }

    internal static Texture? Render()
    {
        if (Main.NametagDefault == null || !GorillaTagger.hasInstance)
            return null;
        var rig = GorillaTagger.Instance.offlineVRRig;
        if (rig == null)
            return null;
        if (owner != rig)
            Release();
        owner = rig;
        if (texture != null && Time.unscaledTime < nextFrame)
            return texture;
        nextFrame = Time.unscaledTime + 1f / 30f;
        if (root == null)
        {
            root = new GameObject("BingusNametags++ UI preview")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            root.transform.position = new Vector3(0, -10000, 0);
            var cameraObject = new GameObject("Preview camera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0.8f, -5);
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.orthographic = true;
            camera.orthographicSize = 0.3f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10;
            camera.cullingMask = 1 << 31;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            texture = new RenderTexture(1080, 640, 16, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.Create();
            camera.targetTexture = texture;
            camera.aspect = 1080f / 640f;
        }

        var enabled = PluginManager.EnabledPlugins.SelectMany(p => p.Metadata.Nametags.Select(t => (Plugin: p.Metadata, Tag: t.Key, Update: t.Value))).ToList();
        foreach (var stale in Tags.Keys.Where(t => !enabled.Any(e => e.Tag == t)).ToList())
        {
            Tags[stale].Destroy();
            Tags.Remove(stale);
        }

        Failures = 0;
        foreach (var entry in enabled)
        {
            if (!Tags.TryGetValue(entry.Tag, out var tag))
            {
                var obj = Object.Instantiate(Main.NametagDefault, root.transform, false);
                obj.hideFlags = HideFlags.HideAndDontSave;
                tag = new PlayerNametag(rig, obj);
                Tags.Add(entry.Tag, tag);
            }

            var go = tag.GameObject;
            go.SetActive(Config.Current.Nametags);
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.renderer.forceRenderingOff = true;
            if (!Config.Current.Nametags)
                continue;

            try
            {
                entry.Update(tag);
                if (entry.Update.Method.DeclaringType == typeof(DefaultNametag))
                {
                    tag.RemoveStyle("color");
                    tag.RemoveStyle("b");
                    tag.RemoveStyle("i");
                    tag.RemoveStyle("u");
                    if (Config.Current.GFriendsIntegration)
                        tag.AddStyle("color", "#" + ColorUtility.ToHtmlStringRGB(rig.playerText1.color));
                    if (Config.Current.CustomNametags)
                    {
                        if (NametagAppearance.TryParseColor(Config.Current.NetworkColor, out var color))
                            tag.AddStyle("color", "#" + ColorUtility.ToHtmlStringRGB(color));
                        if (Config.Current.NetworkBold)
                            tag.AddStyle("b");
                        if (Config.Current.NetworkItalic)
                            tag.AddStyle("i");
                        if (Config.Current.NetworkUnderline)
                            tag.AddStyle("u");
                    }

                    tag.Text = tag.Text;
                }
            }
            catch (Exception)
            {
                Failures++;
                tag.Text = LocalizationManager.GetString("Preview.PluginError");
            }

            go.layer = 31;
            go.transform.localPosition = new Vector3(0, Config.Current.Offset + Config.GetNametagOffset(entry.Plugin, entry.Tag), 0);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * 0.25f;
            tmp.fontSize = Config.Current.Scale * tag.PluginScale;
            if (Config.Current.CustomFont != null)
                tmp.font = Config.Current.CustomFont;
            tmp.ForceMeshUpdate();
            foreach (var child in go.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = 31;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                renderer.forceRenderingOff = true;
        }

        Bounds? visibleBounds = null;
        foreach (var tag in Tags.Values.Where(t => t.GameObject.activeSelf))
        {
            var tmp = tag.GameObject.GetComponent<TextMeshPro>();
            var local = tmp.textBounds;
            if (local.size.x <= 0 || local.size.y <= 0)
                continue;

            var world = new Bounds(tmp.transform.TransformPoint(local.center), Vector3.Scale(local.size, tmp.transform.lossyScale));
            if (visibleBounds.HasValue)
            {
                var combined = visibleBounds.Value;
                combined.Encapsulate(world);
                visibleBounds = combined;
            }
            else
                visibleBounds = world;
        }

        var bounds = visibleBounds ?? new Bounds(root.transform.position, Vector3.zero);
        camera!.transform.position = new Vector3(bounds.center.x, bounds.center.y, root.transform.position.z - 5);
        camera.orthographicSize = Mathf.Max(0.3f, bounds.extents.y * 1.12f, bounds.extents.x / camera.aspect * 1.12f);
        try
        {
            foreach (var tag in Tags.Values)
                foreach (var renderer in tag.GameObject.GetComponentsInChildren<Renderer>(true))
                    renderer.forceRenderingOff = false;
            if (GraphicsSettings.currentRenderPipeline == null)
                camera.Render();
            else
            {
                var request = new RenderPipeline.StandardRequest
                {
                    destination = texture
                };
                if (!RenderPipeline.SupportsRenderRequest(camera, request))
                    throw new InvalidOperationException("The active pipeline does not support nametag preview rendering.");
                RenderPipeline.SubmitRenderRequest(camera, request);
            }
        }
        finally
        {
            foreach (var tag in Tags.Values)
                foreach (var renderer in tag.GameObject.GetComponentsInChildren<Renderer>(true))
                    renderer.forceRenderingOff = true;
        }

        return texture;
    }

    internal static void Release()
    {
        if (camera != null)
            camera.targetTexture = null;
        if (texture != null)
        {
            texture.Release();
            Object.Destroy(texture);
        }

        if (root != null)
            Object.Destroy(root);
        Tags.Clear();
        root = null;
        camera = null;
        texture = null;
        owner = null;
        nextFrame = 0;
    }
}

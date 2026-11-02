using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BingusNametagsPlusPlus.Classes;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

internal static class CustomIconService
{
    private const string Origin = "https://bntags.sirkingbinx.dev";
    private const int Capacity = 256;
    private const int Cell = 64;
    private const int AtlasSize = 1024;
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(12),
        MaxResponseContentBufferSize = 2 * 1024 * 1024
    };
    private static readonly Dictionary<string, List<Icon>> Players = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> Installed = new(StringComparer.Ordinal);
    private static readonly HashSet<TMP_SpriteAsset> BoundSheets = new();
    private static TMP_SpriteAsset? sprites;
    private static Texture2D? atlas;
    private static CancellationTokenSource? cancellation;
    private static Coroutine? routine;
    private static float nextPoll;
    private static float nextRosterCheck;
    private static float lastAttempt = -100;
    private static float retry = 10;
    private static string currentPlayer = "";
    private static string roster = "";
    private static string session = "";
    private static DateTime renewAt;
    private static int revision;

    private sealed class Icon
    {
        [JsonProperty("id")]
        public string Id = "";
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("imageUrl")]
        public string ImageUrl = "";
        [JsonProperty("sha256")]
        public string Hash = "";
    }

    private sealed class Login
    {
        [JsonProperty("token")]
        public string Token = "";
        [JsonProperty("icons")]
        public List<Icon> Icons = [];
    }

    private sealed class Lookup
    {
        [JsonProperty("players")]
        public Dictionary<string, List<Icon>> Players = new();
    }

    private sealed class Result
    {
        public string Token = "";
        public bool LoggedIn;
        public bool RebuildAtlas;
        public int ImageFailures;
        public Dictionary<string, List<Icon>> Players = new();
        public Dictionary<string, byte[]> Images = new();
    }

    // Networking
    internal static void Tick(Main main)
    {
        if (!Main.PluginEnabled || Main.NametagDefault == null || NetworkSystem.Instance == null)
            return;

        float now = Time.unscaledTime;
        if (now < nextRosterCheck)
            return;

        nextRosterCheck = now + 5;
        string id;
        try
        {
            id = NetworkSystem.Instance.GetMyUserID();
        }
        catch
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(id))
            return;

        id = id.ToUpperInvariant();
        if (currentPlayer != id)
        {
            Release(main);
            currentPlayer = id;
        }

        if (routine != null || now - lastAttempt < 10)
            return;

        var ids = VRRigCache.ActiveRigs.Where(r => r != null && r.Creator != null).Select(r => r.Creator.UserId).Where(s => !string.IsNullOrWhiteSpace(s)).Append(id).Select(s => s.ToUpperInvariant()).Distinct().OrderBy(s => s).Take(100).ToArray();
        string nextRoster = string.Join(";", ids);
        if (now < nextPoll && roster == nextRoster)
            return;

        roster = nextRoster;
        lastAttempt = now;
        cancellation = new CancellationTokenSource();
        routine = main.StartCoroutine(Poll(id, ids, cancellation.Token));
    }

    private static IEnumerator Poll(string id, string[] ids, CancellationToken ct)
    {
        var cache = Path.Combine(Constants.BingusNametagsData, "icon-cache");
        var task = Fetch(id, ids, session, DateTime.UtcNow >= renewAt, Installed.ToHashSet(), cache, ct);
        // Fetch may outlive the coroutine when the mod is disabled.
        _ = task.ContinueWith(t =>
        {
            _ = t.Exception;
        }, TaskContinuationOptions.OnlyOnFaulted);
        while (!task.IsCompleted)
            yield return null;
        try
        {
            var result = task.GetAwaiter().GetResult();
            session = result.Token;
            if (result.LoggedIn)
                renewAt = DateTime.UtcNow.AddMinutes(30);
            if (result.RebuildAtlas)
                ClearAtlas();
            foreach (var image in result.Images)
            {
                try
                {
                    Install(image.Key, image.Value);
                }
                catch (Exception ex)
                {
                    LogManager.Log("Custom icon image could not be loaded: " + ex.Message);
                }
            }

            foreach (var entry in result.Players)
                Players[entry.Key] = entry.Value;
            // Drop players who left the room.
            foreach (var old in Players.Keys.Where(k => !ids.Contains(k)).ToArray())
                Players.Remove(old);
            if (result.ImageFailures > 0)
                LogManager.Log("Some custom icon images were unavailable; the remaining icons were updated.");
            retry = 10;
            nextPoll = Time.unscaledTime + (result.ImageFailures > 0 ? 30 : 120);
        }
        catch (Exception ex)
        {
            if (!ct.IsCancellationRequested)
                LogManager.Log("Custom icon service unavailable; retrying later: " + ex.Message);
            session = "";
            nextPoll = Time.unscaledTime + retry;
            retry = Mathf.Min(retry * 2, 300);
        }
        finally
        {
            routine = null;
            cancellation?.Dispose();
            cancellation = null;
        }
    }

    private static async Task<Result> Fetch(string id, string[] ids, string oldToken, bool renew, HashSet<string> installed, string cache, CancellationToken ct)
    {
        var result = new Result
        {
            Token = oldToken
        };
        if (renew || string.IsNullOrEmpty(oldToken))
        {
            var login = await Post<Login>("/me", new { playerId = id }, "", ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(login.Token))
                throw new IOException("Missing client session");
            result.Token = login.Token;
            result.LoggedIn = true;
            result.Players[id] = login.Icons;
        }

        var lookup = await Post<Lookup>("/icons", new { playerIds = ids }, result.Token, ct).ConfigureAwait(false);
        foreach (var pair in lookup.Players.Where(p => ids.Contains(p.Key, StringComparer.OrdinalIgnoreCase)))
            result.Players[pair.Key] = pair.Value ?? [];
        try
        {
            Directory.CreateDirectory(cache);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        var desired = result.Players.Values.SelectMany(x => x).Where(Valid).GroupBy(x => x.Hash).Select(g => g.First()).Take(Capacity).ToArray();
        if (installed.Count + desired.Count(i => !installed.Contains(i.Hash)) > Capacity)
        {
            result.RebuildAtlas = true;
            installed.Clear();
        }

        var wanted = desired.Where(i => !installed.Contains(i.Hash));
        foreach (var icon in wanted)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                result.Images[icon.Hash] = await LoadImage(icon, cache, ct).ConfigureAwait(false);
            }
            catch (Exception ex)when (ex is IOException || ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
            {
                result.ImageFailures++;
            }
        }

        // Trim the image cache.
        try
        {
            foreach (var stale in new DirectoryInfo(cache).EnumerateFiles("*.png").OrderByDescending(f => f.LastWriteTimeUtc).Skip(Capacity))
                try
                {
                    stale.Delete();
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return result;
    }

    private static async Task<byte[]> LoadImage(Icon icon, string cache, CancellationToken ct)
    {
        var path = Path.Combine(cache, icon.Hash + ".png");
        try
        {
            if (File.Exists(path) && new FileInfo(path).Length <= 300000)
            {
                var cached = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
                if (Digest(cached) == icon.Hash)
                {
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                    return cached;
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        // Keep image requests on the icon server.
        using var response = await Http.GetAsync(Origin + icon.ImageUrl, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (data.Length > 300000 || Digest(data) != icon.Hash)
            throw new IOException("Invalid custom icon download");
        try
        {
            await File.WriteAllBytesAsync(path, data, ct).ConfigureAwait(false);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return data;
    }

    private static async Task<T> Post<T>(string path, object payload, string bearer, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Origin + path);
        request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        request.Headers.UserAgent.ParseAdd("BingusNametagsPlusPlus/" + Constants.Version);
        if (!string.IsNullOrEmpty(bearer))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        using var response = await Http.SendAsync(request, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new IOException("Custom icon session expired");
        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)) ?? throw new IOException("Invalid custom icon response");
    }

    private static string Digest(byte[] data)
    {
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
    }

    private static bool Valid(Icon i) => i != null && Regex.IsMatch(i.Hash ?? "", "^[a-f0-9]{64}$") && i.ImageUrl == "/images/" + i.Hash + ".png";
    // Sprites
    private static void Install(string hash, byte[] data)
    {
        if (Installed.Contains(hash) || Installed.Count >= Capacity)
            return;

        // Check dimensions before decoding.
        if (data.Length < 24 || data[0] != 137 || data[1] != 80 || data[2] != 78 || data[3] != 71)
            throw new IOException("Not a PNG");
        uint width = ((uint)data[16] << 24) | ((uint)data[17] << 16) | ((uint)data[18] << 8) | data[19];
        uint height = ((uint)data[20] << 24) | ((uint)data[21] << 16) | ((uint)data[22] << 8) | data[23];
        if (width == 0 || height == 0 || width > 256 || height > 256)
            throw new IOException("Icon dimensions exceed limits");
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!ImageConversion.LoadImage(source, data))
                throw new IOException("Unable to decode PNG");
            if (sprites == null)
            {
                atlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, false)
                {
                    name = "BNTags icon atlas",
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp
                };
                atlas.SetPixels32(new Color32[AtlasSize * AtlasSize]);
                sprites = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                sprites.name = "BNTags custom icons";
                sprites.hideFlags = HideFlags.HideAndDontSave;
                // TMP upgrades unversioned assets on the first table lookup.
                sprites.spriteInfoList = [];
                sprites.spriteSheet = atlas;
                sprites.faceInfo = Main.NametagDefault!.GetComponent<TextMeshPro>().spriteAsset.faceInfo;
                sprites.material = new Material(Shader.Find("TextMeshPro/Sprite") ?? Shader.Find("UI/Default"))
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    mainTexture = atlas
                };
                // Run the upgrade before adding any glyphs.
                sprites.UpdateLookupTables();
            }

            int index = Installed.Count, x = (index % 16) * Cell, y = (index / 16) * Cell;
            // Leave a transparent border between atlas cells.
            var pixels = new Color[Cell * Cell];
            float aspect = source.width / (float)source.height;
            int w = aspect >= 1 ? Cell - 4 : Mathf.Max(1, Mathf.RoundToInt((Cell - 4) * aspect));
            int h = aspect <= 1 ? Cell - 4 : Mathf.Max(1, Mathf.RoundToInt((Cell - 4) / aspect));
            for (int py = 0; py < h; py++)
                for (int px = 0; px < w; px++)
                    pixels[(py + (Cell - h) / 2) * Cell + px + (Cell - w) / 2] = source.GetPixelBilinear((px + .5f) / w, (py + .5f) / h);
            atlas!.SetPixels(x, y, Cell, Cell, pixels);
            atlas.Apply(false);
            // Match the bundled badge's spacing and baseline.
            var baseSheet = Main.NametagDefault!.GetComponent<TextMeshPro>().spriteAsset;
            var reference = baseSheet.spriteCharacterTable.FirstOrDefault(c => c.name == "beta") ?? baseSheet.spriteCharacterTable.First();
            var metrics = IconGlyphLayout.ScaleMetrics(reference.glyph.metrics, Cell - 4);
            var glyph = new TMP_SpriteGlyph
            {
                index = (uint)index,
                glyphRect = new GlyphRect(x + 2, y + 2, Cell - 4, Cell - 4),
                metrics = metrics,
                scale = reference.glyph.scale,
                atlasIndex = 0
            };
            // Explicit face metrics use point-size scaling.
            if (sprites!.faceInfo.pointSize > 0)
                glyph.scale *= reference.glyph.metrics.height / metrics.height;
            // Resolve lazy lookup tables before mutating either table.
            var characters = sprites.spriteCharacterTable;
            sprites.spriteGlyphTable.Add(glyph);
            characters.Add(new TMP_SpriteCharacter((uint)(0xF0000 + index), sprites, glyph) { name = "bntags-" + hash, scale = reference.scale });
            sprites.UpdateLookupTables();
            Installed.Add(hash);
            revision++;
        }
        finally
        {
            Object.Destroy(source);
        }
    }

    internal static string Prefix(PlayerNametag tag)
    {
        if (!Config.Current.Icons || !Config.Current.UserIcons || sprites == null)
            return "";
        string id = tag.Owner == GorillaTagger.Instance.offlineVRRig ? currentPlayer : tag.Owner.Creator?.UserId ?? "";
        if (!Players.TryGetValue(id, out var icons))
            return "";
        var sheet = tag.SpriteSheet;
        if (sheet == null)
            return "";
        sheet.fallbackSpriteAssets ??= [];
        if (!sheet.fallbackSpriteAssets.Contains(sprites))
            sheet.fallbackSpriteAssets.Add(sprites);
        BoundSheets.Add(sheet);
        if (tag.CustomIconRevision != revision)
        {
            tag.GameObject.GetComponent<TextMeshPro>().havePropertiesChanged = true;
            tag.CustomIconRevision = revision;
        }

        return string.Concat(icons.Where(Valid).Where(i => Installed.Contains(i.Hash)).Take(16).Select(i => $"<sprite name=\"bntags-{i.Hash}\" tint=0>"));
    }

    private static void ClearAtlas()
    {
        foreach (var sheet in BoundSheets)
            if (sheet != null)
                sheet.fallbackSpriteAssets?.Remove(sprites!);
        BoundSheets.Clear();
        if (sprites != null)
        {
            Object.Destroy(sprites.material);
            Object.Destroy(sprites);
        }

        if (atlas != null)
            Object.Destroy(atlas);
        sprites = null;
        atlas = null;
        Installed.Clear();
        revision++;
    }

    internal static void Release(Main main)
    {
        cancellation?.Cancel();
        if (routine != null)
            main.StopCoroutine(routine);
        cancellation?.Dispose();
        cancellation = null;
        routine = null;
        ClearAtlas();
        Players.Clear();
        session = "";
        roster = "";
        currentPlayer = "";
        renewAt = default;
        nextPoll = 0;
        nextRosterCheck = 0;
        lastAttempt = -100;
        retry = 10;
    }
}

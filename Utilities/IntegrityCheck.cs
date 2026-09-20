using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SirKingBinx;

public static class IntegrityCheck
{
    // This is a way to help detect and prevent the VMT worm thing.
    // Compares checksums over the internet and stops execution if running right.

    // Build in a debug config to bypass.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<string> GetSha256FromUrlAsync(string url)
    {
        using var httpClient = new HttpClient();
        using Stream stream = await httpClient.GetStreamAsync(url);
        using var sha256 = SHA256.Create();

        byte[] hashBytes = sha256.ComputeHash(stream);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetCurrentSha256()
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        if (string.IsNullOrEmpty(assemblyPath))
            assemblyPath = AppDomain.CurrentDomain.BaseDirectory;

        using FileStream stream = File.OpenRead(assemblyPath);
        using SHA256 sha256 = SHA256.Create();
        
        byte[] hashBytes = sha256.ComputeHash(stream);

        StringBuilder sb = new StringBuilder();

        foreach (byte b in hashBytes)
            sb.Append(b.ToString("x2"));
        
        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<bool> TestIntegritySHA256(string sourceUrl)
    {
        string latestSha256 = await GetSha256FromUrlAsync(sourceUrl);
        string currentSha256 = GetCurrentSha256();

        return latestSha256 == currentSha256;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<bool> TestIntegrityStrongName(string publicKey)
    {
        try
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;

            bool wasVerified = false;
            bool isSignedAndValid = StrongNameSignatureVerificationEx(dllPath, true, ref wasVerified);

            return isSignedAndValid;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("mscoree.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern bool StrongNameSignatureVerificationEx(
        string wszFilePath,
        bool fForceVerification,
        ref bool pfWasVerified
    );
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using Debug = UnityEngine.Debug;

namespace BingusNametagsPlusPlus.Utilities
{
    public class NametagLoader
    {
        public static List<string> pluginFailures = [ ];

        public static string BingusNametagsPath =>
            Path.Combine(Constants.BingusNametagsData, "nametags");

        public static void LoadFromDefaultFolder() =>
            LoadNametagsFromFolder(BingusNametagsPath);

        public static void OpenNametagsFolder() =>
            Process.Start(new ProcessStartInfo
            {
                FileName = BingusNametagsPath,
                UseShellExecute = true,
                Verb = "open"
            });

        public static void LoadNametagsFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return;
            }

            foreach (var file in Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    LoadNametagsFromAssembly(assembly);
                }
                catch (Exception ex)
                {
                    Debug.Log($"The file {file} could not be loaded correctly: {ex.Message}");
                    pluginFailures.Add($"[{file}]: {ex.Message}");
                }
            }
        }

        public static void LoadNametagsFromAssembly(Assembly assembly)
        {
            var nametagTypes = assembly.GetTypes()
                .Where(type => typeof(IBaseNametag).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var nametagType in nametagTypes)
            {
                try
                {
                    if (Activator.CreateInstance(nametagType) is not IBaseNametag nametag)
                        return;

                    Debug.Log($"Loaded nametag {nametag.Name}");

                    Main.Plugins.Add(nametag);

                    Main.UpdateNametags += () =>
                    {
                        Main.Nametags.TryAdd(nametag, new Dictionary<VRRig, PlayerNametag>());
                        nametag.Update(Main.Nametags[nametag], nametag.Offset);
                    };
                }
                catch (Exception ex)
                {
                    pluginFailures.Add($"[{nametagType.Name}]: {ex.Message}");
                }
            }
        }
    }
}
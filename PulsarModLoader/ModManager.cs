﻿using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarModLoader
{
    public class ModManager
    {
        public delegate void ModLoaded(string name, PulsarMod mod);
        public delegate void ModUnloaded(PulsarMod mod);
        public event ModLoaded OnModSuccessfullyLoaded;
        public event ModUnloaded OnModUnloaded;
        public FileVersionInfo PMLVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        private readonly Dictionary<string, PulsarMod> activeMods;
        private readonly HashSet<string> modDirectories;

        private static ModManager _instance = null;

        public static ModManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModManager();
                }

                return _instance;
            }
        }

        public ModManager()
        {
            Logger.Info($"Starting {PMLVersionInfo.ProductName} v{PMLVersionInfo.FileVersion}");

            activeMods = new Dictionary<string, PulsarMod>();
            modDirectories = new HashSet<string>();

            // Add mods directories to AppDomain so mods referencing other as-yet-unloaded mods don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveModsDirectory);

            // Force Photon's static constructor to run so patching its methods doesn't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);
        }

        public PulsarMod GetMod(string name)
        {
            if (activeMods.TryGetValue(name, out PulsarMod mod))
            {
                return mod;
            }
            else
            {
                return null;
            }
        }

        public bool IsModLoaded(string name)
        {
            return activeMods.ContainsKey(name);
        }

        public IEnumerable<PulsarMod> GetAllMods()
        {
            return activeMods.Values;
        }

        public void LoadModsDirectory(string modsDir)
        {
            OnModSuccessfullyLoaded += Events.EventHelper.RegisterEventHandlers;
            Logger.Info($"Attempting to load mods from {modsDir}");

            // Manage directories
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
            }
            modDirectories.Add(modsDir);

            // Load mods
            foreach (string assemblyPath in Directory.GetFiles(modsDir, "*.dll"))
            {
                if (Path.GetFileName(assemblyPath) != "0Harmony.dll")
                {
                    LoadMod(assemblyPath);
                }
            }

            Logger.Info($"Finished loading {activeMods.Count} mods!");
        }

        private Assembly ResolveModsDirectory(object sender, ResolveEventArgs args)
        {
            // Search for dependency in every mods directory loaded so far
            foreach (string modsDir in modDirectories)
            {
                string assemblyPath = Path.Combine(modsDir, new AssemblyName(args.Name).Name + ".dll");

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            // Failed to find dependency!  Assemblies missing from mods directory?
            return null;
        }

        public PulsarMod LoadMod(string assemblyPath)
        {

            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                Type modType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarMod)));

                if (modType != null)
                {
                    PulsarMod mod = Activator.CreateInstance(modType) as PulsarMod;
                    activeMods.Add(mod.Name, mod);
                    OnModSuccessfullyLoaded?.Invoke(mod.Name, mod);

                    Logger.Info($"Loaded mod: {mod.Name} Version {mod.Version} Author: {mod.Author}");
                    return mod;
                }
                else
                {
                    Logger.Info($"Skipping {Path.GetFileName(assemblyPath)}; couldn't find mod entry point.");

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to load mod: {Path.GetFileName(assemblyPath)}\n{e}");

                return null;
            }
        }

        internal void UnloadMod(PulsarMod mod, ref Harmony harmony)
        {
            activeMods.Remove(mod.Name); // Removes selected mod from activeMods
            harmony.UnpatchAll(mod.HarmonyIdentifier()); // Removes all patches from selected mod
            OnModUnloaded?.Invoke(mod);
            Logger.Info($"Unloaded mod: {mod.Name} Version {mod.Version} Author: {mod.Author}");
            GC.Collect();
        }
    }
}
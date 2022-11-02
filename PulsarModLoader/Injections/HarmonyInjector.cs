using HarmonyLib;

using System.Reflection;

using PulsarModLoader.Utilities;

using UnityEngine;

namespace PulsarModLoader.Injections
{
    public static class HarmonyInjector
    {
        public static void InitializeHarmony()
        {
            Utilities.Logger.Info("Loading Harmony 6");
            try
            {
                Harmony.DEBUG = true;
                Utilities.Logger.Info($"Harmony.DEBUG {Harmony.DEBUG}");
                var harmony = new Harmony("wiki.pulsar.pml");
                Utilities.Logger.Info(harmony.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Utilities.Logger.Info("Patched methods ");
                foreach (MethodBase m in Harmony.GetAllPatchedMethods())
                {
                    Utilities.Logger.Info($"\t{m.FullDescription()}");

                    HarmonyLib.Patches info = Harmony.GetPatchInfo(m);
                    Utilities.Logger.Info($"\t\t{info}");
                    Utilities.Logger.Info($"\t\t\tPrefixes: {info.Prefixes.Count}");
                    foreach (Patch patch in info.Prefixes)
                    {
                        Utilities.Logger.Info($"\t\t\t\t{patch.PatchMethod.FullDescription()}");

                    }
                    Utilities.Logger.Info($"\t\t\tTranspilers: {info.Transpilers.Count}");
                    foreach (Patch patch in info.Transpilers)
                    {
                        Utilities.Logger.Info($"\t\t\t\t{patch.PatchMethod.FullDescription()}");

                    }
                    Utilities.Logger.Info($"\t\t\tPostfixes: {info.Postfixes.Count}");
                    foreach (Patch patch in info.Postfixes)
                    {
                        Utilities.Logger.Info($"\t\t\t\t{patch.PatchMethod.FullDescription()}");

                    }
                }

            }
            catch (System.Exception e)
            {
                Utilities.Logger.Info($"Failed to inject {e.Message} - {e}");
            }
        }
    }
}

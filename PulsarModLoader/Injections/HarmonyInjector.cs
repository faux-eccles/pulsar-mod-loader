using HarmonyLib;
using System.Reflection;

using PulsarModLoader.Utilities;

namespace PulsarModLoader.Injections
{
    public static class HarmonyInjector
    {
        public static void InitializeHarmony()
        {
            Logger.Info("Loading Harmony 6");
            try
            {
                Harmony.DEBUG = true;
                Logger.Info($"Harmony.DEBUG {Harmony.DEBUG}");
                var harmony = new Harmony("wiki.pulsar.pml");
                Logger.Info(harmony.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Logger.Info("Patched methods ");
                foreach (MethodBase m in harmony.GetPatchedMethods())
                {
                    Logger.Info(m.FullDescription());
                }
            } catch (System.Exception e)
            {
                Logger.Info($"Failed to inject {e.Message} - {e}");
            }
        }
    }
}

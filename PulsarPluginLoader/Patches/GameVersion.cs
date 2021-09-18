﻿using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.UI;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class GameVersion
    {
        static readonly string PMLVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        static void Postfix(PLNetworkManager __instance, Text ___CurrentVersionLabel)
        {
            PLGlobal.SafeLabelSetText(___CurrentVersionLabel, $"{___CurrentVersionLabel.text}\nPML {PMLVersion}");
        }
    }
}

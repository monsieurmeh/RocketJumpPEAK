using BepInEx;
using HarmonyLib;
using PEAKLib.Core;
using PEAKLib.Items;
using System.Collections.Generic;
using PEAKLib.Items.UnityEditor;
using UnityEngine;


namespace RocketJump
{
    [BepInAutoPlugin]
    [BepInDependency(ItemsPlugin.Id)]
    [BepInDependency(CorePlugin.Id)]
    public partial class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            this.LoadBundleWithName("rocketjump", BuildPrefab);
            Logger.LogInfo($"RocketJump, locked and loaded...");
        }


        private void BuildPrefab(PeakBundle bundle)
        {
            bundle.Mod.RegisterContent();
            bundle.LoadAsset<UnityItemContent>("PeakRocketLauncher");
        }
    }
}

using BepInEx;
using HarmonyLib;
using PEAKLib.Core;
using PEAKLib.Items;
using System.Collections.Generic;
using PEAKLib.Items.UnityEditor;
using UnityEngine;
using System;


namespace RocketJump
{
    [BepInAutoPlugin]
    [BepInDependency(ItemsPlugin.Id)]
    [BepInDependency(CorePlugin.Id)]
    public partial class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            LocalizedText.mainTable["NAME_ROCKET_LAUNCHER"] = ["Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher", "Rocket Launcher"];

            this.LoadBundleWithName("rocketjump.peakbundle", BuildPrefab);
        }


        private void BuildPrefab(PeakBundle bundle)
        {
            try
            {
                bundle.Mod.RegisterContent();
                UnityItemContent launcherContent = bundle.LoadAsset<UnityItemContent>("Launcher");
                if (launcherContent == null)
                {
                    Logger.LogError("Could not load launcher item content!");
                    return;
                }

                RocketLauncher launcherScript = launcherContent.ItemPrefab.AddComponent<RocketLauncher>();
                Action_RocketLauncher_OnFire launcherTrigger = launcherContent.ItemPrefab.AddComponent<Action_RocketLauncher_OnFire>();
                launcherTrigger.launcher = launcherScript;
                launcherTrigger.OnCastFinished = true;

                GameObject rocket = bundle.LoadAsset<GameObject>("RocketPrefab");
                if (rocket == null)
                {
                    Logger.LogError("Could not load rocket prefab!");
                    return;
                }

                Rocket rocketScript = rocket.AddComponent<Rocket>();
                launcherScript.rocketPrefab = rocket;


                Logger.LogInfo($"RocketJump, locked and loaded...");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
}

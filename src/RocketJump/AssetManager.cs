using BepInEx.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using BepInEx;
using System.Reflection;

namespace RocketJump
{

    internal class AssetManager
    {
        internal const string BundlePath = "RocketJump.assets";

        internal Transform? root;
        internal ManualLogSource logger;
        internal Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();
        internal Dictionary<string, UnityEngine.Object> assetDict = new Dictionary<string, UnityEngine.Object>();

        internal AssetManager(ManualLogSource logger)
        {
            this.logger = logger;
        }


        internal bool Initialize()
        {
            try
            {
                if (!LoadAssetBundle(out AssetBundle assetBundle))
                {
                    logger.LogError($"Could not load asset bundle");
                    return false;
                }

                if (!ParseAssetBundle(assetBundle))
                {
                    logger.LogError($"Could not parse asset bundle");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.LogFatal($"Critical initialization failure: {e}");
            }

            return true;
        }

        internal bool LoadAssetBundle(out AssetBundle assetBundle)
        {
            assetBundle = null!;
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream stream = asm.GetManifestResourceStream(BundlePath);
            if (stream == null)
            {
                logger.LogError("Embedded AssetBundle not found!");
                return false;
            }

            byte[] bundleData = new byte[stream.Length];
            stream.Read(bundleData, 0, bundleData.Length);

            assetBundle = AssetBundle.LoadFromMemory(bundleData);
            return assetBundle != null;
        }


        internal bool ParseAssetBundle(AssetBundle assetBundle)
        {
            string[] assetNames = assetBundle.GetAllAssetNames();

            foreach (string assetName in assetNames)
            {
                if (assetName is null)
                {
                    logger.LogWarning($"Null asset name detected");
                    continue;
                }

                UnityEngine.Object obj = assetBundle.LoadAsset(assetName);
                if (obj is null)
                {
                    logger.LogError($"Could not load asset {assetName}: null");
                    continue;
                }

                if (obj is GameObject gameObject && !prefabDict.TryAdd(assetName, gameObject))
                {
                    logger.LogWarning($"Duplicate prefab name detected: {assetName}");
                    continue;
                }
                else if (!assetDict.TryAdd(assetName, obj))
                {
                    logger.LogWarning($"Duplicate asset name detected: {assetName}");
                    continue;
                }

                logger.LogInfo($"Asset loaded: {assetName}");
            }

            return true;
        }

        internal bool TryGetPrefabByName(string name, out GameObject prefab) => prefabDict.TryGetValue(name, out prefab);
        internal bool TryGetAssetByName(string name, out UnityEngine.Object asset) => assetDict.TryGetValue(name, out asset);
    }
}
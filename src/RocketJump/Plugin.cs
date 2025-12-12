using BepInEx;

namespace RocketJump
{

    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        AssetManager assetManager = null!;

        private void Awake()
        {
            AssetManager assetManager = new AssetManager(Logger);
            if (!assetManager.Initialize())
            {
                Logger.LogError("AssetManager init failure!");
                return;
            }

            Logger.LogInfo($"RocketJump, locked and loaded...");
        }
    }
}

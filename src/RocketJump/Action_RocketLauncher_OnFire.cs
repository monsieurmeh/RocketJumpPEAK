using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketJump
{
    internal class Action_RocketLauncher_OnFire : ItemAction
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Action_RocketLauncher_OnFire");
        public RocketLauncher launcher;

        public override void RunAction()
        {
            if (launcher == null)
            {
                logger.LogError("null launcher!");
                return;
            }
            launcher.MaybeFire();
        }
    }
}

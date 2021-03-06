﻿using System;
using UnityEngine;

namespace Kethane
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal class ScaledSpaceFix : MonoBehaviour
    {
        public void Start()
        {
            if (ScaledSpace.Instance == null || ScaledSpace.Instance.scaledSpaceTransforms == null) { return; }
            ScaledSpace.Instance.scaledSpaceTransforms.RemoveAll(t => t == null);
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                ScaledSpace.Instance.scaledSpaceTransforms.RemoveAll(t => !FlightGlobals.Bodies.Exists(b => b.name == t.name));
            }
        }
    }
}

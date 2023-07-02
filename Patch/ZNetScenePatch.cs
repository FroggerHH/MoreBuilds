using System.Collections.Generic;
using HarmonyLib;
using ItemManager;
using UnityEngine;
using static MoreBuilds.Plugin;
using static Heightmap;
using static Heightmap.Biome;
using static ZoneSystem;
using static ZoneSystem.ZoneVegetation;

namespace MoreBuilds;

[HarmonyPatch]
public class ZNetScenePatch
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
    public static void Patch(ZNetScene __instance)
    {
    }
}
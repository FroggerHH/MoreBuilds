using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemManager;
using ServerSync;
using UnityEngine;
using MoreBiomes;
using PieceManager;
using CraftingTable = PieceManager.CraftingTable;

namespace MoreBuilds;

[BepInPlugin(ModGUID, ModName, ModVersion)]
internal class Plugin : BaseUnityPlugin
{
    #region values

    internal const string ModName = "MoreBuilds", ModVersion = "1.0.0", ModGUID = "com.Frogger." + ModName;
    internal static Harmony harmony = new(ModGUID);
    internal static Plugin _self;
    internal static AssetBundle bundleDesertbuilds;
    internal static AssetBundle bundleJungle;

    #endregion

    #region tools

    public static void Debug(string msg)
    {
        _self.Logger.LogInfo(msg);
    }

    public static void DebugError(object msg, bool showWriteToDev)
    {
        if (showWriteToDev)
        {
            msg += "Write to the developer and moderator if this happens often.";
        }

        _self.Logger.LogError(msg);
    }

    public static void DebugWarning(string msg, bool showWriteToDev)
    {
        if (showWriteToDev)
        {
            msg += "Write to the developer and moderator if this happens often.";
        }

        _self.Logger.LogWarning(msg);
    }

    #endregion

    #region ConfigSettings

    #region tools

    static string ConfigFileName = $"com.Frogger.{ModName}.cfg";
    DateTime LastConfigChange;

    public static readonly ConfigSync configSync = new(ModName)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigEntry<T> configEntry = _self.Config.Bind(group, name, value, description);

        SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    private ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    void SetCfgValue<T>(Action<T> setter, ConfigEntry<T> config)
    {
        setter(config.Value);
        config.SettingChanged += (_, _) => setter(config.Value);
    }

    public enum Toggle
    {
        On = 1,
        Off = 0
    }

    #endregion

    #region configs

    #endregion

    #endregion

    #region Config

    private void SetupWatcher()
    {
        FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, ConfigFileName);
        fileSystemWatcher.Changed += ConfigChanged;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - LastConfigChange).TotalSeconds <= 5.0)
        {
            return;
        }

        LastConfigChange = DateTime.Now;
        try
        {
            Config.Reload();
        }
        catch
        {
            DebugError("Can't reload Config", true);
        }
    }

    private void UpdateConfiguration()
    {
        Debug("Configuration Received");
    }

    #endregion

    private void Awake()
    {
        _self = this;
        harmony.PatchAll();

        #region Config

        configSync.AddLockingConfigEntry(config("Main", "Lock Configuration", Toggle.On,
            "If on, the configuration is locked and can be changed by server admins only."));

        #endregion

        bundleDesertbuilds = PrefabManager.RegisterAssetBundle("desertbuilds");
        //bundleJungle = PrefabManager.RegisterAssetBundle("junglebuilds");

        Item sandstone = new(bundleDesertbuilds, "Sandstone");
        sandstone.Name
            .English("Sandstone")
            .English("Песчаник");
        sandstone.Description
            .English(
                "Sandstone is a building material obtained from a clastic rock consisting of compressed sand bound with minerals. It has strength and durability, which makes it ideal for use in the construction of various structures.")
            .Russian(
                "Песчаник - это строительный материал, получаемый из обломочной горной породы, состоящей из сжатого песка, связанного минеральными веществами. Он обладает прочностью и долговечностью, что делает его идеальным для использования в строительстве различных сооружений.");
        // sandstone["SandToSandstone 2 to 1"].RequiredItems.Add("Sand", 2);
        // sandstone["SandToSandstone 2 to 1"].CraftAmount = 1;
        // sandstone["SandToSandstone 2 to 1"].Crafting.Add(CraftingTable.StoneCutter, 0);
        // sandstone["SandToSandstone 2 to 1"].RequireOnlyOneIngredient = true;
        //
        // sandstone["SandToSandstone 8 to 5"].RequiredItems.Add("Sand", 8);
        // sandstone["SandToSandstone 8 to 5"].CraftAmount = 5;
        // sandstone["SandToSandstone 8 to 5"].Crafting.Add(CraftingTable.StoneCutter, 0);
        // sandstone["SandToSandstone 8 to 5"].RequireOnlyOneIngredient = true;
        //
        // sandstone["SandToSandstone 3 to 1"].RequiredItems.Add("Sand", 3);
        // sandstone["SandToSandstone 3 to 1"].Crafting.Add(CraftingTable.Inventory, 0);
        // sandstone["SandToSandstone 3 to 1"].RequireOnlyOneIngredient = true;
        sandstone.Crafting.Add("piece_sandstonecutter", 0);
        sandstone.RequiredItems.Add("Sand", 3);
        sandstone.CraftAmount = 2;

        Item sand = new(bundleDesertbuilds, "Sand");
        sand.Name
            .English("Sand")
            .Russian("Песок");
        sand.Description
            .English("");
        new Conversion(sandstone)
        {
            Input = "Sand",
            Piece = ConversionPiece.Smelter
        };


        // BuildPiece sandstonecutter = new(bundleDesertbuilds, "piece_sandstonecutter");
        // sandstonecutter.Category.Add(BuildPieceCategory.Crafting);
        // sandstonecutter.Crafting.Set(CraftingTable.Workbench);
        // sandstonecutter.RequiredItems.Add("Wood", 10, true);
        // sandstonecutter.RequiredItems.Add("Iron", 2, true);
        // sandstonecutter.RequiredItems.Add("Sandstone", 4, true);
        // sandstonecutter.Name
        //     .Russian("Уплотнитель песка")
        //     .English("Sand sealer");
        // sandstonecutter.Description
        //     .English("");

        BuildPiece sandstone_floor_2x2 = new(bundleDesertbuilds, "sandstone_floor_2x2");
        sandstone_floor_2x2.Category.Add(BuildPieceCategory.Building);
        sandstone_floor_2x2.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_floor_2x2.RequiredItems.Add("Sandstone", 6, true);
        sandstone_floor_2x2.Name
            .Russian("Пол из песчаника 2x2")
            .English("Sandstone floor 2x2");
        sandstone_floor_2x2.Description
            .English("");

        BuildPiece sandstone_wall_1x1 = new(bundleDesertbuilds, "sandstone_wall_1x1");
        sandstone_wall_1x1.Category.Add(BuildPieceCategory.Building);
        sandstone_wall_1x1.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_wall_1x1.RequiredItems.Add("Sandstone", 3, true);
        sandstone_wall_1x1.Name
            .Russian("Стена из песчаника 1x1")
            .English("Sandstone wall 1x1");
        sandstone_wall_1x1.Description
            .English("");

        BuildPiece sandstone_pile = new(bundleDesertbuilds, "sandstone_pile");
        sandstone_pile.Category.Add(BuildPieceCategory.Building);
        sandstone_pile.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_pile.RequiredItems.Add("Sandstone", 50, true);
        sandstone_pile.Name
            .Russian("Груда песчаника")
            .English("Sandstone Pile");
        sandstone_pile.Description
            .English("");

        BuildPiece sandstone_ColumnSquare = new(bundleDesertbuilds, "sandstone_ColumnSquare");
        sandstone_ColumnSquare.Category.Add(BuildPieceCategory.Building);
        sandstone_ColumnSquare.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_ColumnSquare.RequiredItems.Add("Sandstone", 4, true);
        sandstone_ColumnSquare.Name
            .Russian("Квадратная колонна из песчаника")
            .English("Square sandstone column");
        sandstone_ColumnSquare.Description
            .English("");

        BuildPiece sandstone_ColumnRound = new(bundleDesertbuilds, "sandstone_ColumnRound");
        sandstone_ColumnRound.Category.Add(BuildPieceCategory.Building);
        sandstone_ColumnRound.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_ColumnRound.RequiredItems.Add("Sandstone", 4, true);
        sandstone_ColumnRound.Name
            .Russian("Круглая колонна из песчаника")
            .English("Round sandstone column");
        sandstone_ColumnRound.Description
            .English("");

        BuildPiece sandstone_ColumnBroken = new(bundleDesertbuilds, "sandstone_ColumnBroken");
        sandstone_ColumnBroken.Category.Add(BuildPieceCategory.Building);
        sandstone_ColumnBroken.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_ColumnBroken.RequiredItems.Add("Sandstone", 4, true);
        sandstone_ColumnBroken.Name
            .Russian("Разрушенная колонна из песчаника")
            .English("Sandstone column broken");
        sandstone_ColumnBroken.Description
            .English("");

        BuildPiece sandstone_PathPost = new(bundleDesertbuilds, "sandstone_PathPost");
        sandstone_PathPost.Category.Add(BuildPieceCategory.Building);
        sandstone_PathPost.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_PathPost.RequiredItems.Add("Sandstone", 4, true);
        sandstone_PathPost.Name
            .Russian("Столб из песчаника")
            .English("Sandstone pillar");
        sandstone_PathPost.Description
            .English("");


        BuildPiece sandstone_wall_tall = new(bundleDesertbuilds, "sandstone_wall_tall");
        sandstone_wall_tall.Category.Add(BuildPieceCategory.Building);
        sandstone_wall_tall.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_wall_tall.RequiredItems.Add("Sandstone", 5, true);
        sandstone_wall_tall.Name
            .Russian("Высокая стена из песчаника")
            .English("Tall sandstone wall");
        sandstone_wall_tall.Description
            .English(""); 

        BuildPiece sandstone_PyramidBase = new(bundleDesertbuilds, "sandstone_PyramidBase");
        sandstone_PyramidBase.Category.Add(BuildPieceCategory.Building);
        sandstone_PyramidBase.Crafting.Set(CraftingTable.StoneCutter);
        sandstone_PyramidBase.RequiredItems.Add("Sandstone", 5, true);
        sandstone_PyramidBase.Name
            .Russian("Основание пирамиды из песчаника")
            .English("Sandstone pyramid base");
        sandstone_PyramidBase.Description
            .English("");

        BuildPiece piece_chestSandstone = new(bundleDesertbuilds, "piece_chestSandstone");
        piece_chestSandstone.Category.Add(BuildPieceCategory.Building);
        piece_chestSandstone.Crafting.Set(CraftingTable.StoneCutter);
        piece_chestSandstone.RequiredItems.Add("Sandstone", 15, true);
        piece_chestSandstone.RequiredItems.Add("FineWood", 10, true);
        piece_chestSandstone.Name
            .Russian("Сундук из песчаника")
            .English("Sandstone chest");
        piece_chestSandstone.Description
            .English("");
    }
}
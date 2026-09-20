using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DmaBase;
using DmaBase.DMA;
using DmaBase.Misc;
using DmaBase.Misc.Config;
using DmaBase.Unity;
using DmaBase.Unity.LowLevel;
using DmaBase.DMA.ScatterAPI;
using ClickableTransparentOverlay;
using ImGuiNET;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace ScpslApp
{

    // ═══════════════════════════════════════════════════════════
    //  ROOM CONFIG (Blacklist & Custom Names)
    // ═══════════════════════════════════════════════════════════
    public static class RoomConfig
    {
        public static readonly HashSet<RoomName> BlacklistedRooms = new()
        {
            RoomName.Unnamed,
            RoomName.Pocket,
            RoomName.EzOfficeSmall,
            RoomName.EzOfficeLarge,
            RoomName.EzOfficeStoried,
            RoomName.LczClassDSpawn,
            RoomName.EzCollapsedTunnel,
            RoomName.EzRedroom,
            RoomName.Outside,
            RoomName.EzEvacShelter,

        };

        private static readonly Dictionary<RoomName, string> RoomDisplayNames = new()
        {
            { RoomName.LczClassDSpawn, "Class-D Spawn" },
            { RoomName.LczComputerRoom, "Computer Room" },
            { RoomName.LczCheckpointA, "Checkpoint A" },
            { RoomName.LczCheckpointB, "Checkpoint B" },
            { RoomName.LczToilets, "Toilets" },
            { RoomName.LczArmory, "Armory" },
            { RoomName.Lcz173, "173" },
            { RoomName.LczGlassroom, "Glassroom" },
            { RoomName.Lcz330, "Candy" },
            { RoomName.Lcz914, "914" },
            { RoomName.LczGreenhouse, "Greenhouse" },
            { RoomName.LczAirlock, "Airlock" },
            { RoomName.HczCheckpointToEntranceZone, "Checkpoint" },
            { RoomName.HczCheckpointA, "Checkpoint A" },
            { RoomName.HczCheckpointB, "Checkpoint B" },
            { RoomName.HczWarhead, "Warhead" },
            { RoomName.Hcz049, "049" },
            { RoomName.Hcz079, "079" },
            { RoomName.Hcz096, "096" },
            { RoomName.Hcz106, "106" },
            { RoomName.Hcz939, "939" },
            { RoomName.HczMicroHID, "Micro" },
            { RoomName.HczArmory, "Armory" },
            { RoomName.HczServers, "Server" },
            { RoomName.HczTesla, "Tesla" },
            { RoomName.HczTestroom, "Test Room" },
            { RoomName.Hcz127, "127" },
            { RoomName.HczAcroamaticAbatement, "Waterfall" },
            { RoomName.HczWaysideIncinerator, "Furnace" },
            { RoomName.HczRampTunnel, "Ramp Tunnel" },
            { RoomName.EzCollapsedTunnel, "Collapsed Tunnel" },
            { RoomName.EzGateA, "Gate A" },
            { RoomName.EzGateB, "Gate B" },
            { RoomName.EzRedroom, "Red Room" },
            { RoomName.EzEvacShelter, "Evac Shelter" },
            { RoomName.EzIntercom, "Intercom Room" },
            { RoomName.EzOfficeStoried, "Two-Story Office" },
            { RoomName.EzOfficeLarge, "Large Office" },
            { RoomName.EzOfficeSmall, "Small Office" },
            { RoomName.Outside, "Surface Zone" },
            { RoomName.Pocket, "Pocket Dimension" }
        };

        public static bool IsBlacklisted(RoomName name) => BlacklistedRooms.Contains(name);

        public static FacilityZone GetZone(RoomName name)
        {
            string s = name.ToString();
            if (s.StartsWith("Lcz")) return FacilityZone.LightContainment;
            if (s.StartsWith("Hcz")) return FacilityZone.HeavyContainment;
            if (s.StartsWith("Ez")) return FacilityZone.Entrance;
            if (name == RoomName.Outside) return FacilityZone.Surface;
            if (name == RoomName.Pocket) return FacilityZone.Other;
            return FacilityZone.None;
        }

        public static string GetDisplayName(RoomName name)
        {
            if (RoomDisplayNames.TryGetValue(name, out string? customName) && customName != null)
                return customName;

            return name.ToString();
        }
    }

    public enum ItemType : int
    {
        None = -1,
        KeycardJanitor = 0,
        KeycardScientist = 1,
        KeycardResearchCoordinator = 2,
        KeycardZoneManager = 3,
        KeycardGuard = 4,
        KeycardMTFPrivate = 5,
        KeycardContainmentEngineer = 6,
        KeycardMTFOperative = 7,
        KeycardMTFCaptain = 8,
        KeycardFacilityManager = 9,
        KeycardChaosInsurgency = 10,
        KeycardO5 = 11,
        Radio = 12,
        GunCOM15 = 13,
        Medkit = 14,
        Flashlight = 15,
        MicroHID = 16,
        SCP500 = 17,
        SCP207 = 18,
        Ammo12gauge = 19,
        GunE11SR = 20,
        GunCrossvec = 21,
        Ammo556x45 = 22,
        GunFSP9 = 23,
        GunLogicer = 24,
        GrenadeHE = 25,
        GrenadeFlash = 26,
        Ammo44cal = 27,
        Ammo762x39 = 28,
        Ammo9x19 = 29,
        GunCOM18 = 30,
        SCP018 = 31,
        SCP268 = 32,
        Adrenaline = 33,
        Painkillers = 34,
        Coin = 35,
        ArmorLight = 36,
        ArmorCombat = 37,
        ArmorHeavy = 38,
        GunRevolver = 39,
        GunAK = 40,
        GunShotgun = 41,
        SCP330 = 42,
        SCP2176 = 43,
        SCP244a = 44,
        SCP244b = 45,
        SCP1853 = 46,
        ParticleDisruptor = 47,
        GunCom45 = 48,
        SCP1576 = 49,
        Jailbird = 50,
        AntiSCP207 = 51,
        GunFRMG0 = 52,
        GunA7 = 53,
        Lantern = 54,
        SCP1344 = 55,
        Snowball = 56,
        Coal = 57,
        SpecialCoal = 58,
        SCP1507Tape = 59,
        DebugRagdollMover = 60,
        SurfaceAccessPass = 61,
        GunSCP127 = 62,
        KeycardCustomTaskForce = 63,
        KeycardCustomSite02 = 64,
        KeycardCustomManagement = 65,
        KeycardCustomMetalCase = 66,
        MarshmallowItem = 67,
        SCP1509 = 68,
        Scp021J = 69
    }

    public enum ItemCategory : byte
    {
        Keycard,
        Weapon,
        Ammo,
        Armor,
        Medical,
        ScpItem,
        Utility
    }

    [Flags]
    public enum DoorPermissionFlags : ushort
    {
        None = 0,
        Checkpoints = 1,
        ExitGates = 2,
        Intercom = 4,
        AlphaWarhead = 8,
        ContainmentLevelOne = 16,
        ContainmentLevelTwo = 32,
        ContainmentLevelThree = 64,
        ArmoryLevelOne = 128,
        ArmoryLevelTwo = 256,
        ArmoryLevelThree = 512,
        ScpOverride = 1024,
        All = 65535
    }

    public enum AmmoCaliber : byte
    {
        None = 0,
        Cal9x19,
        Cal556,
        Cal762,
        Gauge12,
        Cal44
    }

    public enum ArmorTier : byte
    {
        None = 0,
        Light = 1,
        Combat = 2,
        Heavy = 3
    }

    public sealed class LocalInventoryState
    {
        public readonly List<ItemType> HeldItems = new();
        public readonly HashSet<ItemType> HeldItemSet = new();
        public DoorPermissionFlags CombinedKeycardPermissions = DoorPermissionFlags.None;
        public int HighestKeycardTier = -1;
        public ArmorTier HighestArmorTier = ArmorTier.None;
        public bool HasBestArmor => HighestArmorTier == ArmorTier.Heavy;
        public readonly HashSet<AmmoCaliber> CarriedWeaponCalibers = new();
        public readonly Dictionary<AmmoCaliber, ushort> AmmoCounts = new();

        public int SlotCount => HeldItems.Count;
        public bool IsFull => SlotCount >= 8;

        public bool IsAmmoFull(AmmoCaliber caliber)
        {
            if (caliber == AmmoCaliber.None) return false;
            ushort current = AmmoCounts.TryGetValue(caliber, out var c) ? c : (ushort)0;
            ushort max = ItemConfig.GetMaxAmmoCapacity(caliber, HighestArmorTier);
            return current >= max;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ITEM CONFIG (Custom Names & Categories)
    // ═══════════════════════════════════════════════════════════
    public static class ItemConfig
    {
        private static readonly Dictionary<ItemType, string> ItemDisplayNames = new()
        {
            // Keycards (without "Keycard" suffix)
            { ItemType.KeycardJanitor, "Janitor" },
            { ItemType.KeycardScientist, "Scientist" },
            { ItemType.KeycardResearchCoordinator, "Research Coordinator" },
            { ItemType.KeycardZoneManager, "Zone Manager" },
            { ItemType.KeycardGuard, "Guard" },
            { ItemType.KeycardMTFPrivate, "MTF Private" },
            { ItemType.KeycardContainmentEngineer, "Containment Engineer" },
            { ItemType.KeycardMTFOperative, "MTF Operative" },
            { ItemType.KeycardMTFCaptain, "MTF Captain" },
            { ItemType.KeycardFacilityManager, "Facility Manager" },
            { ItemType.KeycardChaosInsurgency, "Chaos Insurgency" },
            { ItemType.KeycardO5, "O5" },
            { ItemType.KeycardCustomTaskForce, "Custom Task Force" },
            { ItemType.KeycardCustomSite02, "Custom Site-02" },
            { ItemType.KeycardCustomManagement, "Custom Management" },
            { ItemType.KeycardCustomMetalCase, "Metal Case" },

            // Weapons & Firearms
            { ItemType.GunCOM15, "COM-15" },
            { ItemType.GunCOM18, "COM-18" },
            { ItemType.GunCom45, "COM-45" },
            { ItemType.GunFSP9, "FSP-9" },
            { ItemType.GunCrossvec, "Crossvec" },
            { ItemType.GunE11SR, "E-11 SR" },
            { ItemType.GunAK, "AK" },
            { ItemType.GunShotgun, "Shotgun" },
            { ItemType.GunRevolver, "Revolver" },
            { ItemType.GunLogicer, "Logicer" },
            { ItemType.GunFRMG0, "FR-MG-0" },
            { ItemType.GunA7, "A7" },
            { ItemType.GunSCP127, "127" },
            { ItemType.MicroHID, "Micro" },
            { ItemType.ParticleDisruptor, "Particle Disruptor" },
            { ItemType.Jailbird, "Jailbird" },

            // Ammo
            { ItemType.Ammo9x19, "9mm" },
            { ItemType.Ammo556x45, "5.56" },
            { ItemType.Ammo762x39, "7.62" },
            { ItemType.Ammo12gauge, "12ga" },
            { ItemType.Ammo44cal, ".44 Cal" },

            // Armor
            { ItemType.ArmorLight, "Light Armor" },
            { ItemType.ArmorCombat, "Combat Armor" },
            { ItemType.ArmorHeavy, "Heavy Armor" },

            // Medical
            { ItemType.Medkit, "Medkit" },
            { ItemType.Painkillers, "Pills" },
            { ItemType.Adrenaline, "Adrenaline" },
            { ItemType.SCP500, "SCP Pills" },

            // SCP Items
            { ItemType.SCP018, "BouncyBall" },
            { ItemType.SCP207, "Cola" },
            { ItemType.AntiSCP207, "Anti Scp Cola" },
            { ItemType.SCP268, "Hat" },
            { ItemType.SCP330, "Candy" },
            { ItemType.SCP2176, "Ghostlight" },
            { ItemType.SCP244a, "Vase" },
            { ItemType.SCP244b, "Vase" },
            { ItemType.SCP1853, "Performance Enhancer" },
            { ItemType.SCP1576, "Gramophone" },
            { ItemType.SCP1344, "Goggles" },
            { ItemType.SCP1507Tape, "1507 Tape" },
            { ItemType.SCP1509, "Machete" },
            { ItemType.Scp021J, "021-J" },

            // Utility & Throwables
            { ItemType.GrenadeHE, "Grenade" },
            { ItemType.GrenadeFlash, "Flashbang" },
            { ItemType.Radio, "Radio" },
            { ItemType.Flashlight, "Flashlight" },
            { ItemType.Lantern, "Lantern" },
            { ItemType.Coin, "Coin" },
            { ItemType.SurfaceAccessPass, "SurfaceAccess" },
            { ItemType.Snowball, "Snowball" },
            { ItemType.Coal, "Coal" },
            { ItemType.SpecialCoal, "Special Coal" },
            { ItemType.MarshmallowItem, "Marshmallow" },
            { ItemType.DebugRagdollMover, "Ragdoll Mover" }
        };

        public static string GetDisplayName(ItemType item)
        {
            if (ItemDisplayNames.TryGetValue(item, out string? customName) && customName != null)
                return customName;

            return item.ToString();
        }

        public static ItemCategory GetCategory(ItemType item) => item switch
        {
            ItemType.KeycardJanitor or ItemType.KeycardScientist or ItemType.KeycardResearchCoordinator
            or ItemType.KeycardZoneManager or ItemType.KeycardGuard or ItemType.KeycardMTFPrivate
            or ItemType.KeycardContainmentEngineer or ItemType.KeycardMTFOperative or ItemType.KeycardMTFCaptain
            or ItemType.KeycardFacilityManager or ItemType.KeycardChaosInsurgency or ItemType.KeycardO5
            or ItemType.KeycardCustomTaskForce or ItemType.KeycardCustomSite02 or ItemType.KeycardCustomManagement
            or ItemType.KeycardCustomMetalCase or ItemType.SurfaceAccessPass => ItemCategory.Keycard,

            ItemType.GunCOM15 or ItemType.GunCOM18 or ItemType.GunCom45 or ItemType.GunFSP9
            or ItemType.GunCrossvec or ItemType.GunE11SR or ItemType.GunAK or ItemType.GunShotgun
            or ItemType.GunRevolver or ItemType.GunLogicer or ItemType.GunFRMG0 or ItemType.GunA7
            or ItemType.GunSCP127 or ItemType.MicroHID or ItemType.ParticleDisruptor or ItemType.Jailbird => ItemCategory.Weapon,

            ItemType.Ammo9x19 or ItemType.Ammo556x45 or ItemType.Ammo762x39 or ItemType.Ammo12gauge or ItemType.Ammo44cal => ItemCategory.Ammo,

            ItemType.ArmorLight or ItemType.ArmorCombat or ItemType.ArmorHeavy => ItemCategory.Armor,

            ItemType.Medkit or ItemType.Painkillers or ItemType.Adrenaline or ItemType.SCP500 => ItemCategory.Medical,

            ItemType.SCP018 or ItemType.SCP207 or ItemType.AntiSCP207 or ItemType.SCP268
            or ItemType.SCP330 or ItemType.SCP2176 or ItemType.SCP244a or ItemType.SCP244b
            or ItemType.SCP1853 or ItemType.SCP1576 or ItemType.SCP1344 or ItemType.SCP1507Tape
            or ItemType.SCP1509 or ItemType.Scp021J => ItemCategory.ScpItem,

            _ => ItemCategory.Utility
        };

        public static uint GetColor(ItemType item) => GetCategory(item) switch
        {
            ItemCategory.Keycard => 0xFFFF8CB1, // ABGR Violet
            ItemCategory.Weapon => 0xFF5555FF,  // ABGR Orange-Red
            ItemCategory.Ammo => 0xFF20D7FF,    // ABGR Gold/Yellow
            ItemCategory.Armor => 0xFFFF904A,   // ABGR Steel Blue
            ItemCategory.Medical => 0xFF70E330, // ABGR Emerald Green
            ItemCategory.ScpItem => 0xFFB469FF, // ABGR Magenta/Pink
            _ => 0xFFD8D8D8                     // ABGR Silver/White
        };

        public static DoorPermissionFlags GetKeycardPermissions(ItemType item) => item switch
        {
            ItemType.KeycardJanitor => DoorPermissionFlags.ContainmentLevelOne,
            ItemType.KeycardScientist => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ContainmentLevelTwo,
            ItemType.KeycardResearchCoordinator => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ContainmentLevelThree,
            ItemType.KeycardZoneManager => DoorPermissionFlags.Checkpoints,
            ItemType.KeycardGuard => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ContainmentLevelOne | DoorPermissionFlags.ArmoryLevelOne,
            ItemType.KeycardMTFPrivate => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelTwo,
            ItemType.KeycardMTFOperative => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelTwo,
            ItemType.KeycardMTFCaptain => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.Intercom | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelThree,
            ItemType.KeycardContainmentEngineer => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ContainmentLevelThree,
            ItemType.KeycardFacilityManager => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.Intercom | DoorPermissionFlags.AlphaWarhead | DoorPermissionFlags.ContainmentLevelThree,
            ItemType.KeycardChaosInsurgency => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.Intercom | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelTwo,
            ItemType.KeycardO5 => DoorPermissionFlags.All,
            ItemType.SurfaceAccessPass => DoorPermissionFlags.ExitGates,
            ItemType.KeycardCustomTaskForce => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelTwo,
            ItemType.KeycardCustomSite02 => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.Intercom | DoorPermissionFlags.AlphaWarhead | DoorPermissionFlags.ContainmentLevelThree,
            ItemType.KeycardCustomManagement => DoorPermissionFlags.Checkpoints | DoorPermissionFlags.ExitGates | DoorPermissionFlags.Intercom | DoorPermissionFlags.ContainmentLevelTwo | DoorPermissionFlags.ArmoryLevelThree,
            ItemType.KeycardCustomMetalCase => DoorPermissionFlags.All,
            _ => DoorPermissionFlags.None
        };

        public static int GetKeycardTier(ItemType item) => item switch
        {
            ItemType.KeycardO5 or ItemType.KeycardCustomMetalCase => 6,
            ItemType.KeycardFacilityManager or ItemType.KeycardMTFCaptain or ItemType.KeycardCustomManagement or ItemType.KeycardCustomSite02 => 5,
            ItemType.KeycardMTFOperative or ItemType.KeycardChaosInsurgency or ItemType.KeycardCustomTaskForce => 4,
            ItemType.KeycardMTFPrivate or ItemType.KeycardContainmentEngineer => 3,
            ItemType.KeycardGuard or ItemType.KeycardResearchCoordinator => 2,
            ItemType.KeycardScientist or ItemType.KeycardZoneManager => 1,
            ItemType.KeycardJanitor or ItemType.SurfaceAccessPass => 0,
            _ => -1
        };

        public static ArmorTier GetArmorTier(ItemType item) => item switch
        {
            ItemType.ArmorHeavy => ArmorTier.Heavy,
            ItemType.ArmorCombat => ArmorTier.Combat,
            ItemType.ArmorLight => ArmorTier.Light,
            _ => ArmorTier.None
        };

        public static AmmoCaliber GetWeaponCaliber(ItemType item) => item switch
        {
            ItemType.GunCOM15 or ItemType.GunCOM18 or ItemType.GunCom45 or ItemType.GunFSP9 or ItemType.GunCrossvec => AmmoCaliber.Cal9x19,
            ItemType.GunE11SR or ItemType.GunFRMG0 => AmmoCaliber.Cal556,
            ItemType.GunAK or ItemType.GunLogicer or ItemType.GunA7 => AmmoCaliber.Cal762,
            ItemType.GunShotgun => AmmoCaliber.Gauge12,
            ItemType.GunRevolver => AmmoCaliber.Cal44,
            _ => AmmoCaliber.None
        };

        public static AmmoCaliber GetAmmoCaliber(ItemType item) => item switch
        {
            ItemType.Ammo9x19 => AmmoCaliber.Cal9x19,
            ItemType.Ammo556x45 => AmmoCaliber.Cal556,
            ItemType.Ammo762x39 => AmmoCaliber.Cal762,
            ItemType.Ammo12gauge => AmmoCaliber.Gauge12,
            ItemType.Ammo44cal => AmmoCaliber.Cal44,
            _ => AmmoCaliber.None
        };

        public static ushort GetMaxAmmoCapacity(AmmoCaliber caliber, ArmorTier armor) => (caliber, armor) switch
        {
            (AmmoCaliber.Cal9x19, ArmorTier.Heavy) => 100,
            (AmmoCaliber.Cal9x19, ArmorTier.Combat) => 85,
            (AmmoCaliber.Cal9x19, ArmorTier.Light) => 65,
            (AmmoCaliber.Cal9x19, _) => 30,

            (AmmoCaliber.Cal556, ArmorTier.Heavy) => 120,
            (AmmoCaliber.Cal556, ArmorTier.Combat) => 105,
            (AmmoCaliber.Cal556, ArmorTier.Light) => 80,
            (AmmoCaliber.Cal556, _) => 40,

            (AmmoCaliber.Cal762, ArmorTier.Heavy) => 120,
            (AmmoCaliber.Cal762, ArmorTier.Combat) => 105,
            (AmmoCaliber.Cal762, ArmorTier.Light) => 80,
            (AmmoCaliber.Cal762, _) => 40,

            (AmmoCaliber.Gauge12, ArmorTier.Heavy) => 44,
            (AmmoCaliber.Gauge12, ArmorTier.Combat) => 36,
            (AmmoCaliber.Gauge12, ArmorTier.Light) => 28,
            (AmmoCaliber.Gauge12, _) => 14,

            (AmmoCaliber.Cal44, ArmorTier.Heavy) => 56,
            (AmmoCaliber.Cal44, ArmorTier.Combat) => 48,
            (AmmoCaliber.Cal44, ArmorTier.Light) => 36,
            (AmmoCaliber.Cal44, _) => 18,

            _ => 0
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  CONFIG DATA
    // ═══════════════════════════════════════════════════════════
    public sealed class ConfigData
    {
        // Player ESP
        public bool EspEnabled { get; set; } = true;
        public bool BoundingBox { get; set; } = true;
        public bool SkeletonEsp { get; set; } = true;
        public bool HeadCircleEsp { get; set; } = true;
        public float HeadCircleRadius { get; set; } = 5.0f;
        public bool NameTags { get; set; } = true;
        public bool HealthBar { get; set; } = true;
        public bool ShowScpIcons { get; set; } = true;
        public float ScpIconSize { get; set; } = 28.0f;
        public float MaxDistance { get; set; } = 250f;

        // World ESP
        public bool ShowRoomEsp { get; set; } = true;
        public bool RoomCurrentZoneOnly { get; set; } = true;
        public bool ShowGeneratorEsp { get; set; } = true;
        public bool GenCurrentZoneOnly { get; set; } = true;

        // Item ESP
        public bool ItemEspEnabled { get; set; } = true;
        public float ItemMaxDistance { get; set; } = 30f;
        public bool ShowItemIcons { get; set; } = true;
        public bool ShowItemText { get; set; } = true;
        public float ItemIconSize { get; set; } = 24.0f;

        // Category Filters
        public bool ShowKeycards { get; set; } = true;
        public bool ShowWeapons { get; set; } = true;
        public bool ShowAmmo { get; set; } = true;
        public bool ShowArmor { get; set; } = true;
        public bool ShowMedical { get; set; } = true;
        public bool ShowScpItems { get; set; } = true;
        public bool ShowUtility { get; set; } = true;

        // Smart Culling
        public bool FilterMaxAmmo { get; set; } = true;
        public bool FilterSmartAmmo { get; set; } = false;
        public bool FilterBestArmor { get; set; } = true;
        public bool FilterInferiorArmor { get; set; } = true;
        public bool FilterInferiorKeycards { get; set; } = true;
        public bool FilterDuplicateKeycards { get; set; } = true;
        public bool FilterDuplicateUtility { get; set; } = false;
        public int FilterMinKeycardTier { get; set; } = 0;
        public int FilterMinArmorTier { get; set; } = 0;

        // HUD
        public bool ShowHud { get; set; } = false;
        public bool HudShowTitle { get; set; } = true;
        public bool HudShowAlive { get; set; } = true;
        public bool HudShowDead { get; set; } = true;
        public bool HudShowWarhead { get; set; } = true;
        public bool HudShowGenerators { get; set; } = true;
        public bool HudShowRound { get; set; } = true;
        public int HudX { get; set; } = 10;
        public int HudY { get; set; } = 10;
        public bool ShowWatermark { get; set; } = false;
        public bool VSync { get; set; } = true;

        // Preferences
        public bool AutoSave { get; set; } = true;
        [JsonConverter(typeof(JsonStringEnumConverter<UnityKeyCode>))]
        public UnityKeyCode MenuKey { get; set; } = UnityKeyCode.F1;
        public bool StreamerMode { get; set; } = false;
        public bool ShowConsole { get; set; } = false;
        public bool ShowConsoleOnStart { get; set; } = false;

        // Memory Writes
        public bool MasterMemWritesEnabled { get; set; } = false;
        public bool WorldFovChangerEnabled { get; set; } = false;
        public float CustomWorldFov { get; set; } = 90.0f;
        public bool ZoomFovEnabled { get; set; } = false;
        public float ZoomFov { get; set; } = 35.0f;
        [JsonConverter(typeof(JsonStringEnumConverter<UnityKeyCode>))]
        public UnityKeyCode ZoomKey { get; set; } = UnityKeyCode.Mouse1;
        public bool ViewmodelFovChangerEnabled { get; set; } = false;
        public float CustomViewmodelFov { get; set; } = 68.0f;
        public bool NoSwayEnabled { get; set; } = false;
        public bool NoRecoilEnabled { get; set; } = false;
        public float RecoilIntensity { get; set; } = 0.0f;
        public bool AutoBunnyhopEnabled { get; set; } = false;

        // Viewangle Aim (Memory Aimbot)
        public bool ViewangleAimEnabled { get; set; } = false;
        public float ViewangleAimFov { get; set; } = 25.0f;
        public float ViewangleAimSmoothness { get; set; } = 0.20f;
        public int ViewangleAimTargetBone { get; set; } = 0; // 0 = Head, 1 = Chest
        public float ViewangleAimHeadOffset { get; set; } = 0.0f;
        public float ViewangleAimChestOffset { get; set; } = 0.0f;
        public bool ViewangleAimTeamCheck { get; set; } = true;
        public bool ViewangleAimDrawFov { get; set; } = true;
        [JsonConverter(typeof(JsonStringEnumConverter<UnityKeyCode>))]
        public UnityKeyCode ViewangleAimKey { get; set; } = UnityKeyCode.Mouse1;
        public bool ViewangleAimRequireKey { get; set; } = true;
        public int ViewangleAimKeyMode { get; set; } = 0; // 0 = Hold, 1 = Toggle

        // Visual Enhancements (No Flash, No Fog, Brightness, Gun Flashlight, TPV)
        public bool NoFlashEnabled { get; set; } = false;
        public bool LessFogEnabled { get; set; } = false;
        public bool FullNoFogEnabled { get; set; } = false;
        public float LessFogClearness { get; set; } = 3.0f;
        public bool LessFogScp244Enabled { get; set; } = false;
        public bool BrightnessEnabled { get; set; } = false;
        public float BrightnessEv { get; set; } = 1.5f;

        // Gun Flashlight Enhancement
        public bool GunFlashlightEnabled { get; set; } = false;
        public float GunFlashlightSpotAngle { get; set; } = 80.0f;
        public float GunFlashlightIntensityMult { get; set; } = 1.0f;
        public float GunFlashlightRange { get; set; } = 60.0f;

        // Instant ADS (Aim-Down-Sights)
        public bool InstantAdsEnabled { get; set; } = false;

        // Third-Person View (TPV) / Corner Peek Mode
        public bool TpvEnabled { get; set; } = false;
        public float TpvDistance { get; set; } = 2.2f;
        public float TpvHeight { get; set; } = 0.4f;
        public float TpvShoulderOffset { get; set; } = 0.45f;
        [JsonConverter(typeof(JsonStringEnumConverter<UnityKeyCode>))]
        public UnityKeyCode TpvKey { get; set; } = UnityKeyCode.F4;
        public bool TpvRequireKey { get; set; } = true;
        public int TpvKeyMode { get; set; } = 0; // 0 = Toggle, 1 = Hold


        [JsonIgnore]
        public bool NoFogEnabled { get => LessFogEnabled; set => LessFogEnabled = value; }
        [JsonIgnore]
        public bool NoFogAtmosphericEnabled { get => FullNoFogEnabled; set => FullNoFogEnabled = value; }
        [JsonIgnore]
        public bool NoFogScp244Enabled { get => LessFogScp244Enabled; set => LessFogScp244Enabled = value; }
        [JsonIgnore]
        public bool NoSmokeEnabled { get => LessFogEnabled; set => LessFogEnabled = value; }
        [JsonIgnore]
        public bool NoSmokeScp244Enabled { get => LessFogScp244Enabled; set => LessFogScp244Enabled = value; }
    }

    public sealed class ScpslConfig : IConfig
    {
        public LowLevelCache LowLevelCache { get; } = new();
        public bool MemWritesEnabled => (GameReader.MasterMemWritesEnabled && (GameReader.WorldFovChangerEnabled || GameReader.ViewmodelFovChangerEnabled || GameReader.NoSwayEnabled || GameReader.NoRecoilEnabled || GameReader.AutoBunnyhopEnabled || GameReader.ViewangleAimEnabled || GameReader.NoFlashEnabled || GameReader.NoFogEnabled || GameReader.BrightnessEnabled || GameReader.GunFlashlightEnabled || GameReader.InstantAdsEnabled || GameReader.TpvEnabled))
            || GameReader.IsRestoring
            || GameReader.HasPendingRestores;
        public int MonitorWidth => 1920;
        public int MonitorHeight => 1080;
        public void Save() => GameReader.ScpslOverlay.SaveConfig();
        public Task SaveAsync()
        {
            GameReader.ScpslOverlay.SaveConfig();
            return Task.CompletedTask;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  DMA ATTACH
    // ═══════════════════════════════════════════════════════════
    public sealed class ScpslMemory : MemDMABase
    {
        private const uint ImageScnMemExecute = 0x20000000;
        private readonly string _processName;
        private readonly object _writeGuardLock = new();

        public static ScpslMemory? Instance { get; private set; }

        public ScpslMemory(string processName) : base(FpgaAlgo.Auto, useMemMap: true)
        {
            _processName = processName;
            Instance = this;
            Log.WriteLine($"Searching for process: {processName}");

            // Immediate synchronous attempt: if SCP:SL is already running with modules loaded, attach in 0ms
            try
            {
                Attach();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"[DMA] Initial attach attempt deferred: {ex.Message}");
            }

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (ProcessPID == 0)
                        {
                            Attach();
                        }
                        else
                        {
                            uint[] pids = _hVMM.PidGetAllFromName(_processName);
                            bool stillAlive = pids != null && pids.Contains(ProcessPID);
                            if (!stillAlive)
                            {
                                Log.WriteLine($"[DMA] Target process exited (was PID {ProcessPID}). Resetting state...");
                                ProcessPID = 0;
                                UnityBase = 0;
                                GameAssemblyBase = 0;
                                MonoBase = 0;
                                OnRoundEnded();
                                DmaBase.DMA.Features.IFeature.DispatchRoundEnd();
                                OnGameStopped();
                                DmaBase.DMA.Features.IFeature.DispatchGameStop();
                                GameReader.TryRestoreInstantAds();
                                GameReader.TryRestoreTpv();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"[DMA] Error during process monitor: {ex.Message}");
                    }

                    // Poll every 100ms while waiting to attach for near-instant detection; 1000ms once attached
                    Thread.Sleep(ProcessPID == 0 ? 100 : 1000);
                }
            });
        }

        private bool Attach()
        {
            uint[] candidatePids = _hVMM.PidGetAllFromName(_processName);
            if (candidatePids == null || candidatePids.Length == 0)
            {
                if (_hVMM.PidGetFromName(_processName, out uint singlePid) && singlePid != 0)
                    candidatePids = new uint[] { singlePid };
                else
                    return false;
            }

            uint selectedPid = 0;
            ulong unityBase = 0;
            ulong gaBase = 0;

            foreach (uint pid in candidatePids)
            {
                if (pid == 0) continue;
                ulong uBase = _hVMM.ProcessGetModuleBase(pid, "UnityPlayer.dll");
                if (uBase == 0) continue;
                ulong gBase = _hVMM.ProcessGetModuleBase(pid, "GameAssembly.dll");
                if (gBase == 0) continue;

                // Found the client!
                selectedPid = pid;
                unityBase = uBase;
                gaBase = gBase;
                break;
            }

            if (selectedPid == 0)
            {
                return false;
            }

            ProcessPID = selectedPid;
            UnityBase = unityBase;
            GameAssemblyBase = gaBase;
            MonoBase = gaBase;

            Log.WriteLine($"[DMA] Attached! PID: {selectedPid}  UnityPlayer: 0x{UnityBase:X}  GameAssembly: 0x{GameAssemblyBase:X}");
            var sw = Stopwatch.StartNew();
            try
            {
                UnityInput.Initialize(UnityBase);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"[DMA] UnityInput init failed: {ex.Message}");
            }
            Log.WriteLine($"[DMA] UnityInput.Initialize took: {sw.ElapsedMilliseconds}ms");

            OnGameStarted();
            DmaBase.DMA.Features.IFeature.DispatchGameStart();
            return true;
        }

        public override bool Ready => ProcessPID != 0 && UnityBase != 0 && GameAssemblyBase != 0;
        public override bool InRound => GameReader.ScpslOverlay.LatestRoundStarted;
        public int RoundTime => GameReader.ScpslOverlay.LatestRoundTime;

        /// <summary>
        /// Rejects the complete destination range if it overlaps .text, .rdata,
        /// or any executable section in any loaded PE image. Classification is
        /// refreshed for every write and failures block the write.
        /// </summary>
        internal void EnsureWriteAllowed(ulong address, ulong byteLength)
        {
            if (address == 0)
                throw new ArgumentOutOfRangeException(nameof(address));
            if (byteLength == 0)
                return;

            ulong writeEnd = GetEndExclusive(address, byteLength, "write range");

            lock (_writeGuardLock)
            {
                if (ProcessPID == 0)
                    throw new InvalidOperationException(
                        "Memory write blocked: no target process is attached.");

                var modules = _hVMM.Map_GetModule(ProcessPID, false);
                if (modules == null || modules.Length == 0)
                    throw new InvalidOperationException(
                        "Memory write blocked: unable to enumerate loaded PE modules.");

                foreach (var module in modules)
                {
                    if (module.vaBase == 0 || module.cbImageSize == 0)
                        continue;

                    ulong moduleEnd = GetEndExclusive(
                        module.vaBase,
                        module.cbImageSize,
                        $"module '{module.sText}'");

                    if (!RangesOverlap(address, writeEnd, module.vaBase, moduleEnd))
                        continue;

                    if (!module.fValid)
                        throw new InvalidOperationException(
                            "Memory write blocked: the destination overlaps an invalid PE module entry.");

                    if (string.IsNullOrWhiteSpace(module.sText))
                        throw new InvalidOperationException(
                            "Memory write blocked: the destination overlaps an unnamed PE image.");

                    var sections = _hVMM.ProcessGetSections(ProcessPID, module.sText);
                    if (sections == null || sections.Length == 0)
                        throw new InvalidOperationException(
                            $"Memory write blocked: unable to classify sections for '{module.sText}'.");

                    foreach (var section in sections)
                    {
                        string sectionName = (section.Name ?? string.Empty).TrimEnd('\0', ' ');
                        bool protectedSection =
                            sectionName.StartsWith(".text", StringComparison.OrdinalIgnoreCase) ||
                            sectionName.StartsWith(".rdata", StringComparison.OrdinalIgnoreCase) ||
                            (section.Characteristics & ImageScnMemExecute) != 0;

                        if (!protectedSection)
                            continue;

                        ulong sectionLength = Math.Max(
                            (ulong)section.MiscPhysicalAddressOrVirtualSize,
                            section.SizeOfRawData);
                        if (sectionLength == 0)
                            continue;

                        ulong rawSectionStart = checked(module.vaBase + section.VA);
                        ulong rawSectionEnd = GetEndExclusive(
                            rawSectionStart,
                            sectionLength,
                            $"{module.sText}!{sectionName}");

                        // Conservatively protect every mapped page occupied by
                        // the section, including its PE alignment padding.
                        ulong sectionStart = rawSectionStart & ~0xFFFUL;
                        ulong sectionEnd = checked((rawSectionEnd + 0xFFFUL) & ~0xFFFUL);

                        if (RangesOverlap(address, writeEnd, sectionStart, sectionEnd))
                        {
                            throw new InvalidOperationException(
                                $"Memory write blocked: 0x{address:X}-0x{writeEnd - 1:X} overlaps " +
                                $"{module.sText}!{sectionName}.");
                        }
                    }
                }
            }
        }

        private static ulong GetEndExclusive(ulong start, ulong length, string label)
        {
            try
            {
                return checked(start + length);
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException(
                    $"Memory write blocked: invalid {label} address range.", ex);
            }
        }

        private static bool RangesOverlap(
            ulong firstStart,
            ulong firstEnd,
            ulong secondStart,
            ulong secondEnd) =>
            firstStart < secondEnd && secondStart < firstEnd;
    }

    // ═══════════════════════════════════════════════════════════
    //  ENUMS
    // ═══════════════════════════════════════════════════════════
    public enum RoleTypeId : sbyte
    {
        None = -1, Scp173 = 0, ClassD = 1, Spectator = 2, Scp106 = 3,
        NtfSpecialist = 4, Scp049 = 5, Scientist = 6, Scp079 = 7,
        ChaosConscript = 8, Scp096 = 9, Scp0492 = 10, NtfSergeant = 11,
        NtfCaptain = 12, NtfPrivate = 13, Tutorial = 14, FacilityGuard = 15,
        Scp939 = 16, CustomRole = 17, ChaosRifleman = 18, ChaosMarauder = 19,
        ChaosRepressor = 20, Overwatch = 21, Filmmaker = 22, Scp3114 = 23,
        Destroyed = 24, Flamingo = 25, AlphaFlamingo = 26, ZombieFlamingo = 27,
        NtfFlamingo = 28, ChaosFlamingo = 29,
    }

    public enum FacilityZone : byte
    {
        None = 0,
        LightContainment = 1,
        HeavyContainment = 2,
        Entrance = 3,
        Surface = 4,
        Other = 5
    }

    public enum RoomName
    {
        Unnamed = 0,
        LczClassDSpawn = 1,
        LczComputerRoom = 2,
        LczCheckpointA = 3,
        LczCheckpointB = 4,
        LczToilets = 5,
        LczArmory = 6,
        Lcz173 = 7,
        LczGlassroom = 8,
        Lcz330 = 9,
        Lcz914 = 10,
        LczGreenhouse = 11,
        LczAirlock = 12,
        HczCheckpointToEntranceZone = 13,
        HczCheckpointA = 14,
        HczCheckpointB = 15,
        HczWarhead = 16,
        Hcz049 = 17,
        Hcz079 = 18,
        Hcz096 = 19,
        Hcz106 = 20,
        Hcz939 = 21,
        HczMicroHID = 22,
        HczArmory = 23,
        HczServers = 24,
        HczTesla = 25,
        EzCollapsedTunnel = 26,
        EzGateA = 27,
        EzGateB = 28,
        EzRedroom = 29,
        EzEvacShelter = 30,
        EzIntercom = 31,
        EzOfficeStoried = 32,
        EzOfficeLarge = 33,
        EzOfficeSmall = 34,
        Outside = 35,
        Pocket = 36,
        HczTestroom = 37,
        Hcz127 = 38,
        HczAcroamaticAbatement = 39,
        HczWaysideIncinerator = 40,
        HczRampTunnel = 41
    }

    public enum Team : byte
    {
        SCPs = 0, FoundationForces = 1, ChaosInsurgency = 2, Scientists = 3,
        ClassD = 4, Dead = 5, OtherAlive = 6, Flamingos = 7,
    }

    

    // ═══════════════════════════════════════════════════════════
    //  SKELETON BONES CONFIGURATION
    // ═══════════════════════════════════════════════════════════
    public static class SkeletonData
    {
        // SCP:SL CharacterModel._hitboxesCache (15 HitboxIdentity items):
        // 0 = Pelvis, 1 = Spine1, 2 = Spine2, 3 = Chest, 4 = Head
        // 5 = L Upperarm, 6 = L Forearm
        // 7 = R Upperarm, 8 = R Forearm
        // 9 = L Thigh, 10 = L Calf, 11 = L Foot
        // 12 = R Thigh, 13 = R Calf, 14 = R Foot
        public const int HeadHitboxIndex = 4;
        public const int ChestHitboxIndex = 3;
        public const int PelvisHitboxIndex = 0;
        public const int LeftHandIndex = 15;
        public const int RightHandIndex = 16;
        public const int TotalBones = 17;

        public static readonly (int From, int To)[] Connections =
        {
            // Spine / Torso
            (0, 1), // Pelvis -> Spine1
            (1, 2), // Spine1 -> Spine2
            (2, 3), // Spine2 -> Chest
            (3, 4), // Chest -> Head

            // Left Arm (Collar -> Upper Arm -> Forearm)
            (3, 5),  // Chest -> L Upperarm
            (5, 6),  // L Upperarm -> L Forearm
            (6, 15), // L Forearm -> L Hand

            // Right Arm (Collar -> Upper Arm -> Forearm)
            (3, 7),  // Chest -> R Upperarm
            (7, 8),  // R Upperarm -> R Forearm
            (8, 16), // R Forearm -> R Hand

            // Left Leg
            (0, 9),   // Pelvis -> L Thigh
            (9, 10),  // L Thigh -> L Calf
            (10, 11), // L Calf -> L Foot

            // Right Leg
            (0, 12),  // Pelvis -> R Thigh
            (12, 13), // R Thigh -> R Calf
            (13, 14)  // R Calf -> R Foot
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  DATA STRUCTS
    // ═══════════════════════════════════════════════════════════
    public class PlayerInfo
    {
        public ulong Hub;
        public int PlayerId;
        public string Name = "Player";
        public RoleTypeId Role;
        public Team Team;
        public Vector3 Position;
        public float Health;
        public float MaxHealth;
        public bool IsLocal;
        public bool HasPosition;
        public bool Alive;
        public ulong DeathRolePointer;

        // Base Pointers for multi-threaded fetching
        public ulong RoleManager;
        public ulong PlayerStats;
        public ulong NicknameSync;
        
        // Secondary Pointers
        public ulong CurRole;
        public ulong StatModules;
        public ulong DisplayName;
        public ulong MyNickSync;
        public ulong HealthModule;
        public ulong HumeShieldModule;
        public ulong AhpModule;
        public float HumeShield;
        public float MaxHumeShield;
        public float Ahp;
        public ulong FpcModule;

        // Skeleton / Bone Tracking
        public UnityTransform? BoneTransform;
        public ulong HierarchyAddr;
        public ulong VerticesAddr;
        public int Capacity;
        public int[]? Indices;
        public int[]? HitboxIndices;
        public Vector3[]? BonePositions;
        public bool HasBones;
        public long LastBoneAttemptTicks;

        // UI Text & Metric Caching
        public int CachedDistanceInt = -1;
        public string CachedDistanceLabel = string.Empty;
        public Vector2 CachedLabelSize = Vector2.Zero;
    }

    public struct CameraInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Fov;
        public bool Valid;
    }

    public readonly struct CameraFrame
    {
        public readonly Vector3 CamPos;
        public readonly Vector3 Right;
        public readonly Vector3 Up;
        public readonly Vector3 Forward;
        public readonly float HalfW;
        public readonly float HalfH;
        public readonly float InvHalfW;
        public readonly float InvHalfH;
        public readonly float HalfScreenW;
        public readonly float HalfScreenH;
        public readonly float ScreenW;
        public readonly float ScreenH;
        public readonly bool Valid;

        public CameraFrame(in CameraInfo cam, float screenW, float screenH)
        {
            ScreenW = screenW;
            ScreenH = screenH;
            Valid = cam.Valid;
            CamPos = cam.Position;

            if (!Valid || screenW <= 0f || screenH <= 0f)
            {
                Right = Up = Forward = Vector3.Zero;
                HalfW = HalfH = InvHalfW = InvHalfH = HalfScreenW = HalfScreenH = 0f;
                return;
            }

            var camRot = cam.Rotation;
            float xx = camRot.X * camRot.X, yy = camRot.Y * camRot.Y, zz = camRot.Z * camRot.Z;
            float xy = camRot.X * camRot.Y, xz = camRot.X * camRot.Z, yz = camRot.Y * camRot.Z;
            float wx = camRot.W * camRot.X, wy = camRot.W * camRot.Y, wz = camRot.W * camRot.Z;

            Right = new Vector3(1f - 2f * (yy + zz), 2f * (xy + wz), 2f * (xz - wy));
            Up = new Vector3(2f * (xy - wz), 1f - 2f * (xx + zz), 2f * (yz + wx));
            Forward = new Vector3(2f * (xz + wy), 2f * (yz - wx), 1f - 2f * (xx + yy));

            float fovRad = cam.Fov * 0.0174532925f;
            HalfH = MathF.Tan(fovRad * 0.5f);
            HalfW = HalfH * (screenW / screenH);
            InvHalfW = HalfW > 0.0001f ? (1.0f / HalfW) : 0f;
            InvHalfH = HalfH > 0.0001f ? (1.0f / HalfH) : 0f;
            HalfScreenW = 0.5f * screenW;
            HalfScreenH = 0.5f * screenH;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Project(in Vector3 world, out float sx, out float sy)
        {
            sx = sy = 0;
            if (!Valid) return false;

            Vector3 delta = world - CamPos;
            float z = Vector3.Dot(delta, Forward);
            if (z <= 0.05f) return false;

            float invZ = 1.0f / z;
            float x = Vector3.Dot(delta, Right);
            float y = Vector3.Dot(delta, Up);

            sx = (x * invZ * InvHalfW + 1f) * HalfScreenW;
            sy = (1f - (y * invZ * InvHalfH + 1f) * 0.5f) * ScreenH;
            return true;
        }
    }

    public struct SyncInfo
    {
        public byte ScenarioId;
        public byte ScenarioType;
        public double StartTime;
    }

    public enum WarheadState
    {
        Off,
        Ready,
        Active,
        Cooldown,
        Detonated
    }

    public struct PanelWarheadInfo
    {
        public WarheadState State;
        public float TimeRemainingSeconds;
        public int StatusType;  // 3 is Ready, 4 is off, 5 is deadmans, maybe detonating
        public int RawTime;
    }

    // Place under DATA STRUCTS
    public class RoomInfo
    {
        public ulong Address;
        public RoomName Name;
        public FacilityZone Zone;
        public Vector3 Position;

        // UI Text & Metric Caching
        public int CachedDistanceInt = -1;
        public string CachedDistanceLabel = string.Empty;
        public Vector2 CachedLabelSize = Vector2.Zero;
    }

    public class GeneratorInfo
    {
        public ulong Address;
        public RoomName Room;
        public Vector3 Position;
        public byte Flags;
        public short SyncTime;
        public float TotalActivationTime;
        public ulong ParentRoom;

        public bool IsEngaged => (Flags & 16) != 0;
        public bool IsActivating => (Flags & 8) != 0;
        public bool IsUnlocked => (Flags & 2) != 0 || (Flags & 4) != 0;

        // UI Text & Metric Caching
        public int CachedDistanceInt = -1;
        public byte CachedFlags = 0xFF;
        public short CachedSyncTime = -1;
        public string CachedDistanceLabel = string.Empty;
        public Vector2 CachedLabelSize = Vector2.Zero;
    }

    public class ItemPickupInfo
    {
        public ulong Address;
        public ItemType ItemType;
        public ItemCategory Category;
        public string DisplayName = string.Empty;
        public Vector3 Position;
        public uint Color;

        // UI Text & Metric Caching
        public int CachedDistanceInt = -1;
        public string CachedDistanceLabel = string.Empty;
        public Vector2 CachedLabelSize = Vector2.Zero;
    }

    public sealed class AliveSpectatableInfo
    {
        public bool IsAvailable { get; init; }
        public HashSet<ulong> AliveHubs { get; init; } = new();
        public Dictionary<ulong, RoleTypeId> HubRoles { get; init; } = new();
    }

    // ═══════════════════════════════════════════════════════════
    //  DIAGNOSTICS (thread-safe timing stats)
    // ═══════════════════════════════════════════════════════════
    public static class Diag
    {
        public static double LastTotalMs;
        public static double LastResolveHubsMs;
        public static double LastHashSetMs;
        public static double LastPlayersMs;
        public static double LastCameraMs;
        public static int LastPlayerCount;
        public static int LastDmaReads;
        public static double AvgTotalMs;
        private static double _totalAccum;
        private static int _totalSamples;

        // Per-sub-operation totals (summed across all players in one frame)
        public static double LastNameMs;
        public static double LastRoleMs;
        public static double LastPosMs;
        public static double LastBoneMs;
        public static double LastHealthMs;

        private static int _dmaReadCounter;
        public static void CountRead() => Interlocked.Increment(ref _dmaReadCounter);
        public static int ResetReadCounter() => Interlocked.Exchange(ref _dmaReadCounter, 0);

        public static void Update(double totalMs)
        {
            LastTotalMs = totalMs;
            _totalAccum += totalMs;
            _totalSamples++;
            if (_totalSamples > 60) { _totalAccum = totalMs; _totalSamples = 1; }
            AvgTotalMs = _totalAccum / _totalSamples;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  OFFSETS (from Il2CppDumper / script.json)
    // ═══════════════════════════════════════════════════════════
    public static class Offsets
    {
        public const ulong ReferenceHub_TypeInfo = 0x3F29E18;
        public const ulong MainCameraController_TypeInfo = 0x3F28EE0;
        public const ulong HealthStat_TypeInfo = 0x3F304A8;
        public const ulong FpcStandardRoleBase_TypeInfo = 0x3F462E0;
        public const ulong RoomIdentifier_TypeInfo = 0x3F30058;
        public const ulong AlphaWarheadOutsitePanel_TypeInfo = 0x3F29508;
        public const ulong AlphaWarheadController_TypeInfo = 0x3F29510;
        public const ulong CameraShakeController_TypeInfo = 0x3F19198;
        public const ulong Camera_TypeInfo = 0x3F2A090;
        public const ulong RagdollManager_TypeInfo = 0x3F41C78;
        public const ulong Scp079Role_TypeInfo = 0x3F023A8;
        public const ulong RoundSummary_TypeInfo = 0x3F382F8;
        public const ulong RoundSummary_singleton = 0x8;
        public const ulong RoundSummary_singletonSet = 0x10;
        public const ulong RoundSummary_roundTime = 0x38;
        public const ulong RoundSummary_isRoundEnded = 0x89;

        public const ulong RoundStart_TypeInfo = 0x3F29BF0;
        public const ulong RoundStart_static_singleton = 0x0;
        public const ulong RoundStart_static_RoundStartTimer = 0x10;
        public const ulong RoundStart_NetworkTimer = 0xBA;
        public const ulong Stopwatch_elapsed = 0x10;
        public const ulong Stopwatch_started = 0x18;
        public const ulong Stopwatch_isRunning = 0x20;

        public const ulong NetworkClient_TypeInfo = 0x3F296B0;
        public const ulong NC_static_spawned = 0x10;
        public const ulong ItemPickupBase_TypeInfo = 0x3F43670;
        public const ulong ItemPickupBase_Info = 0x88;
        public const ulong ItemPickupBase_transform = 0xF8;
        public const ulong PickupSyncInfo_ItemId = 0x00;
        public const ulong PickupSyncInfo_Serial = 0x04;
        public const ulong Dictionary_entries = 0x18;
        public const ulong Dictionary_count = 0x20;
        public const ulong RH_inventory = 0xA0;
        public const ulong Inventory_UserInventory = 0x70;
        public const ulong InventoryInfo_Items = 0x10;
        public const ulong InventoryInfo_ReserveAmmo = 0x18;
        public const ulong ItemBase_ItemTypeId = 0x20;
        public const ulong ItemBase_Category = 0x24;

        public const ulong Il2CppClass_name = 0x10;
        public const ulong Il2CppClass_parent = 0x58;
        public const ulong Il2CppClass_fields = 0x80;
        public const ulong Il2CppClass_static_fields = 0xB8;

        public const ulong FieldInfo_name = 0x0;
        public const ulong FieldInfo_parent = 0x10;
        public const ulong FieldInfo_offset = 0x18;
        public const ulong FieldInfo_size = 0x20;

        public const ulong String_length = 0x10;
        public const ulong String_chars = 0x14;

        public const ulong Array_max_length = 0x18;
        public const ulong Array_length = 0x18;
        public const ulong Array_items = 0x20;

        public const ulong HashSet_slots = 0x18;
        public const ulong HashSet_lastIndex = 0x24;
        public const ulong HashSetSlot_size = 0x10;
        public const ulong HashSetSlot_hashCode = 0x0;
        public const ulong HashSetSlot_value = 0x8;

        public const ulong RH_static_AllHubs = 0x18;
        public const ulong RH_static_localHubSet = 0x30;
        public const ulong RH_static_hostHubSet = 0x31;
        public const ulong RH_static_localHub = 0x38;
        public const ulong RH_static_hostHub = 0x40;

        public const ulong RH_playerId = 0x70;
        public const ulong RH_PlayerCameraReference = 0x78;
        public const ulong RH_characterClassManager = 0x88;
        public const ulong RH_roleManager = 0x90;
        public const ulong RH_playerStats = 0x98;
        public const ulong RH_nicknameSync = 0xC0;

        public const ulong CCM_RoundStarted = 0x8A;

        public const ulong RagdollManager_static_AllRagdolls = 0x18;
        public const ulong Ragdoll_Info_RoleType = 0x70;
        public const ulong Ragdoll_Info_CreationTime = 0xB0;
        public const ulong Ragdoll_Info_OwnerHub = 0xB8;

        public const ulong NS_myNickSync = 0xA0;
        public const ulong NS_displayName = 0xB0;

        public const ulong PS_statModules = 0x98;
        public const ulong Stat_lastValue = 0x24;
        public const ulong HealthStat_maxValue = 0x38;

        public const ulong PRM_curRole = 0x90;
        public const ulong Fpc_FpcModule = 0x80;
        public const ulong Fpc_lastPos = 0xB0;
        public const ulong Role_roleTypeId = 0xD8;
        public const ulong Fpm_cachedPosition = 0xE4;

        // FirstPersonControl / MouseLook
        public const ulong FpcMouseLook_TypeInfo = 0x3F0E270;
        public const ulong FpcMouseLook_static_localInstance = 0x00;
        public const ulong Fpm_MouseLook = 0x88;
        public const ulong FpcMouseLook_curHorizontal = 0x20;
        public const ulong FpcMouseLook_curVertical = 0x24;
        public const ulong FpcMouseLook_inputHorizontal = 0x30;
        public const ulong FpcMouseLook_inputVertical = 0x34;

        public const ulong MCC_InstanceActive = 0x10;
        public const ulong MCC_LastPosition = 0x14;
        public const ulong MCC_LastRotation = 0x20;

        // Status Effects & PlayerEffectsController
        public const ulong RH_playerEffectsController = 0xD0;
        public const ulong PEC_AllEffects = 0x70;
        public const ulong PEC_EffectsLength = 0x78;

        public const ulong SEB_intensity = 0x30; // byte
        public const ulong SEB_duration = 0x34;  // float
        public const ulong SEB_timeLeft = 0x38;  // float

        public const ulong Flashed_processVolume = 0x40;    // Volume
        public const ulong Flashed_lightenEffect = 0x48;    // Lighten
        public const ulong Flashed_darkenEffect = 0x50;     // Darken
        public const ulong Flashed_remainingWeight = 0x58; // float

        // Unity HDRP Volume & Components
        public const ulong Volume_priority = 0x24; // float
        public const ulong Volume_weight = 0x2C; // float
        public const ulong VolumeManager_TypeInfo = 0x3EDA200;
        public const ulong VM_static_s_Instance = 0x18;
        public const ulong Lazy_value = 0x20;
        public const ulong VM_stack = 0x10;
        public const ulong VM_globalDefaultProfile = 0x20;
        public const ulong VM_qualityDefaultProfile = 0x28;
        public const ulong VM_volumeCollection = 0x38;
        public const ulong VM_componentsDefaultState = 0x40;
        public const ulong VolumeStack_components = 0x10;
        public const ulong VolumeCollection_volumes = 0x18;
        public const ulong Volume_sharedProfile = 0x30;
        public const ulong Volume_internalProfile = 0x48;
        public const ulong VolumeProfile_components = 0x18;

        public const ulong LiftGammaGain_lift = 0x38;            // Vector4Parameter
        public const ulong LiftGammaGain_gamma = 0x40;           // Vector4Parameter
        public const ulong LiftGammaGain_gain = 0x48;            // Vector4Parameter
        public const ulong Vector4Parameter_value = 0x18;        // Vector4

        public const ulong Exposure_fixedExposure = 0x50;        // FloatParameter
        public const ulong Exposure_compensation = 0x58;         // FloatParameter

        public const ulong ColorAdjustments_postExposure = 0x38; // FloatParameter
        public const ulong Fog_enabled = 0x40;                  // BoolParameter
        public const ulong Fog_meanFreePath = 0x90;             // MinFloatParameter

        public const ulong VolumeParameter_overrideState = 0x10; // bool
        public const ulong BoolParameter_value = 0x18;           // bool
        public const ulong FloatParameter_value = 0x18;          // float

        // FogController & FogSetting & CustomFog
        public const ulong FogController_TypeInfo = 0x3F29378;
        public const ulong FogController_static_Singleton = 0x8;
        public const ulong FogController_fogSettings = 0x28;     // FogSetting[]
        public const ulong FogController_fogEffect = 0x20;       // CustomFog
        public const ulong FogSetting_weight = 0x20;             // float
        public const ulong FogSetting_fogType = 0x24;            // int
        public const ulong FogSetting_startDistance = 0x2C;      // float
        public const ulong FogSetting_endDistance = 0x30;        // float
        public const ulong FogSetting_isEnabled = 0x49;          // bool
        public const ulong CustomFog_fadeIntensity = 0xF0;       // float
        public const ulong CustomFog_endDistance = 0xF4;         // float
        public const ulong CustomFog_startDistance = 0xF8;       // float

        // SCP-244 Deployable Pickup
        public const ulong Scp244DeployablePickup_TypeInfo = 0x3F293D8;
        public const ulong Scp244_static_Instances = 0x0;
        public const ulong Scp244_MaxDiameter = 0x128;        // float
        public const ulong Scp244_CurrentSizePercent = 0x144; // float

        public const float DefaultVerticalFOV = 70.0f;
        public const ulong RI_static_AllRoomIdentifiers = 0x0;   // HashSet<RoomIdentifier> at static fields + 0x0
        public const ulong RI_Name = 0x34;                      // RoomName enum (int)
        public const ulong RI_Zone = 0x38;                      // FacilityZone enum (int)
        public const ulong RI_WorldspaceBounds = 0x54;          // Bounds.center Vector3 starts at offset 0x54
        // AlphaWarheadController
        public const ulong AWC_static_Singleton = 0x0;
        public const ulong AWC_IsLocked = 0x98;            // bool
        
        // AlphaWarheadSyncInfo embedded at 0xA0
        public const ulong AWC_Info_ScenarioId = 0xA0;     // byte
        public const ulong AWC_Info_ScenarioType = 0xA1;   // byte
        public const ulong AWC_Info_StartTime = 0xA8;      // double

        public const ulong AWC_CooldownEndTime = 0xB0;     // double
        public const ulong AWC_IsAutomatic = 0xC0;         // bool
        public const ulong AWC_AutoDetonateTime = 0xC4;    // float
        public const ulong AWC_TriggeringPlayer = 0xD0;    // Footprint struct
        public const ulong AWOP_static_lastTextType = 0x0;
        public const ulong AWOP_static_lastDisplayedTime = 0x4;

        // SCP-079 Generators & Spawnable Structures
        public const ulong Scp079Recontainer_TypeInfo = 0x3F09920;
        public const ulong SR_static_AllGenerators = 0x0;
        public const ulong SpawnableStructure_TypeInfo = 0x3F17048;
        public const ulong SS_static_AllInstances = 0x10;
        public const ulong SS_StructureType = 0x70;
        public const ulong SS_ParentRoom = 0x78;
        public const ulong GEN_ParentRoom = 0x78;
        public const ulong GEN_TotalActivationTime = 0xF4;
        public const ulong GEN_Flags = 0x148;
        public const ulong GEN_SyncTime = 0x14A;

        // Character Model & Bones (Unity 2021/2022 IL2CPP)
        public const ulong Fpm_CharacterModelInstance = 0xA0;
        public const ulong CM_ownerTr = 0x70;
        public const ulong CM_cachedTransform = 0xE8;
        public const ulong CM_hitboxesCache = 0x98;
        public const ulong CM_hardpoints = 0xB0;
        public const ulong MRP_transformCache = 0x20;
        public const ulong MRP_type = 0x28;
        public const ulong Fpc_hubTransform = 0xC0;
        public const ulong HB_transformCache = 0x20;
        public const ulong HB_dmgMultiplier = 0x40;
        public const ulong Tr_nativeTransform = 0x10;
        public const ulong NativeTr_hierarchy = 0x28;
        public const ulong NativeTr_index = 0x30;
        public const ulong Hier_vertices = 0x18;
        public const ulong Hier_indices = 0x20;
        public const ulong Hier_capacity = 0x10;

        // SpectatableModuleBase (authoritative alive player tracking)
        public const ulong SpectatableModuleBase_TypeInfo = 0x3F45E08;
        public const ulong SpectatableModule_static_AllInstances = 0x10; // HashSet<SpectatableModuleBase>
        public const ulong SpectatableModule_cachedRole = 0x28;          // PlayerRoleBase
        public const ulong RoleBase_lastOwner = 0x48;                   // ReferenceHub
    }

    // ═══════════════════════════════════════════════════════════
    //  SAFE READ HELPER (counts reads for diagnostics)
    // ═══════════════════════════════════════════════════════════
    public static class Mem
    {
        public static ulong Ptr(ulong addr, bool useCache = true)
        {
            if (addr == 0) return 0;
            try
            {
                Diag.CountRead();
                ulong v = DmaMemory.ReadValue<ulong>(addr, useCache);
                return v.IsValidVirtualAddress() ? v : 0;
            }
            catch { return 0; }
        }

        public static T Val<T>(ulong addr, bool useCache = true) where T : unmanaged
        {
            if (addr == 0) return default;
            try
            {
                Diag.CountRead();
                return DmaMemory.ReadValue<T>(addr, useCache);
            }
            catch { return default; }
        }

        public static string Str(ulong addr, int len, bool useCache = true)
        {
            if (addr == 0 || len <= 0) return string.Empty;
            try
            {
                Diag.CountRead();
                return DmaMemory.ReadString(addr, len, useCache);
            }
            catch { return string.Empty; }
        }

        public static string StrUnicode(ulong addr, int charLen, bool useCache = true)
        {
            if (addr == 0 || charLen <= 0 || charLen > 1024) return string.Empty;
            try
            {
                Diag.CountRead();
                int byteLen = charLen * 2;
                byte[] buf = new byte[byteLen];
                DmaMemory.ReadBuffer<byte>(addr, buf.AsSpan(), useCache);
                return System.Text.Encoding.Unicode.GetString(buf).Trim('\0').Trim();
            }
            catch { return string.Empty; }
        }

        public static void Write<T>(ulong addr, T value) where T : unmanaged
        {
            if (addr == 0) return;
            try
            {
                if (SharedProgram.Config?.MemWritesEnabled != true && !GameReader.IsRestoring && !GameReader.HasPendingRestores) return;
                var memory = ScpslMemory.Instance ??
                    throw new InvalidOperationException("DMA memory is not initialized.");
                memory.EnsureWriteAllowed(addr, (ulong)Unsafe.SizeOf<T>());
                DmaMemory.WriteValue<T>(addr, value);
            }
            catch { }
        }

        public static bool TryWriteValue<T>(ulong addr, T value) where T : unmanaged
        {
            if (addr == 0) return false;

            try
            {
                if (SharedProgram.Config?.MemWritesEnabled != true && !GameReader.IsRestoring && !GameReader.HasPendingRestores) return false;
                var memory = ScpslMemory.Instance ??
                    throw new InvalidOperationException("DMA memory is not initialized.");
                memory.EnsureWriteAllowed(addr, (ulong)Unsafe.SizeOf<T>());
                DmaMemory.WriteValue<T>(addr, value);
                return true;
            }
            catch { return false; }
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  GAME READER (with aggressive caching)
    // ═══════════════════════════════════════════════════════════
    public class GameReader
    {
        // ── Caches that survive across frames ──
        // TypeInfo RVA → Il2CppClass* (never changes while GA is loaded)
        private static readonly ConcurrentDictionary<ulong, ulong> _tiCache = new();
        // TypeInfo RVA → Il2CppClass.static_fields pointer (static fields table is immutable once loaded)
        private static readonly ConcurrentDictionary<ulong, ulong> _staticFieldsCache = new();
        // Il2CppClass* → is-subclass-of-FpcStandardRoleBase (class hierarchy is static)
        private static readonly ConcurrentDictionary<ulong, bool> _subclassCache = new();
        // Il2CppClass* → class name string (immutable)
        private static readonly ConcurrentDictionary<ulong, string> _classNameCache = new();
        // hub address → cached name string (rarely changes, refresh every N frames)
        private static readonly ConcurrentDictionary<ulong, string> _playerNameCache = new();
        // hub → health stat module pointer (survives until role change)
        private static readonly ConcurrentDictionary<ulong, ulong> _healthModCache = new();
        // CharacterModel / role pointer → UnityTransform (with bone hierarchy)
        private static readonly ConcurrentDictionary<ulong, UnityTransform> _cachedBoneTransforms = new();
        // Il2CppClass* + field name → instance field offset.
        private static readonly ConcurrentDictionary<(ulong Klass, string FieldName), ulong> _fieldOffsetCache = new();
        private const int MaxPlayers = 40;
        private static readonly ulong[] _rM = new ulong[MaxPlayers];
        private static readonly ulong[] _pS = new ulong[MaxPlayers];
        private static readonly ulong[] _nS = new ulong[MaxPlayers];
        private static readonly int[] _pId = new int[MaxPlayers];
        private static readonly PrefetchedData[] _prefetches = new PrefetchedData[MaxPlayers];
        private static readonly Vector3?[] _positions = new Vector3?[MaxPlayers];
        private static readonly float?[] _hps = new float?[MaxPlayers];
        private static readonly float[] _maxHps = new float[MaxPlayers];

        private static int _nameRefreshCounter = 0;
        private const int NameRefreshInterval = 3000; // re-read names every 3000 frames

        // Restoration & safety tracking
        private static int _restoringCounter = 0;
        public static bool IsRestoring => _restoringCounter > 0;

        public static bool HasPendingRestores =>
            _activeWorldOriginalCaptured ||
            _activeViewmodelOriginalCaptured ||
            _activeSwayOriginalCaptured ||
            _activeRecoilOriginalCaptured ||
            _activeJumpOriginalCaptured ||
            _origFogSettings.Count > 0 ||
            _customFogCaptured ||
            _origHdrpFogDict.Count > 0 ||
            _origScp244Dict.Count > 0 ||
            _origLiftGammaDict.Count > 0 ||
            _origExposureDict.Count > 0 ||
            _origColorAdjDict.Count > 0 ||
            _activeFlashlightOriginalCaptured ||
            _adsOriginalCaptured ||
            _tpvWasApplied;

        // Viewmodel FOV is changed through the active viewmodel's backing data.
        // No function body, vtable, executable page, or read-only PE data is modified.
        public static bool ViewmodelFovChangerEnabled = false;
        public static float CustomViewmodelFov = 70.0f;
        public static string ViewmodelFovStatus { get; private set; } = "Off";

        private static readonly object _viewmodelFovLock = new();
        private static ulong _activeViewmodelObject;
        private static ulong _activeViewmodelClass;
        private static ulong _activeViewmodelFovAddress;
        private static float _activeViewmodelOriginalFov;
        private static bool _activeViewmodelOriginalCaptured;
        private static int _viewmodelFovRestoreFailures = 0;

        // World Camera FOV & Hold-to-Zoom Changer.
        // Pure data-only write to native Unity::Camera and Unity::Behaviour on the heap.
        // No function body, vtable, executable page, or read-only PE data is modified.
        public static bool MasterMemWritesEnabled = false;
        public static bool WorldFovChangerEnabled = false;
        public static float CustomWorldFov = 90.0f;
        public static bool ZoomFovEnabled = false;
        public static float ZoomFov = 35.0f;
        public static UnityKeyCode ZoomKey = UnityKeyCode.Mouse1;
        public static string WorldFovStatus { get; private set; } = "Off";

        private static readonly object _worldFovLock = new();
        private static ulong _activeWorldSingleton;
        private static ulong _activeWorldCameraObj;
        private static ulong _activeWorldAltCameraObj;
        private static ulong _activeWorldFovAddress;
        private static ulong _activeWorldDirtyAddress;
        private static float _activeWorldOriginalFov = 70.0f;
        private static bool _activeWorldOriginalCaptured;
        private static float _activeWorldFovValue = 70.0f;
        private static int _worldFovRestoreFailures = 0;

        public static float ActiveWorldFov
        {
            get
            {
                if (WorldFovChangerEnabled && _activeWorldFovValue >= 10.0f && _activeWorldFovValue <= 170.0f)
                    return _activeWorldFovValue;
                return Offsets.DefaultVerticalFOV;
            }
        }

        // No Sway / Weapon Bobbing
        public static bool NoSwayEnabled = false;
        public static string NoSwayStatus { get; private set; } = "Off";

        private static readonly object _noSwayLock = new();
        private static ulong _activeNoSwayViewmodelObject;
        private static ulong _activeNoSwayViewmodelClass;
        private struct SwayTarget
        {
            public ulong Address;
            public float OriginalValue;
            public string Name;
        }
        private static readonly List<SwayTarget> _activeSwayTargets = new();
        private static bool _activeSwayOriginalCaptured;
        private static ulong _lastCheckedSwayItem;
        private static bool _lastSwayItemHadTargets;
        private static bool _swayApplied;
        private static long _lastSwayVerifyTicks;

        // Weapon Recoil Control (No Recoil)
        public static bool NoRecoilEnabled = false;
        public static float RecoilIntensity = 0.0f; // 0% (No Recoil) to 100% (Stock Recoil)
        public static string NoRecoilStatus { get; private set; } = "Off";

        private static readonly object _noRecoilLock = new();
        private static ulong _activeRecoilItemObject;
        private static ulong _activeRecoilItemClass;
        private struct RecoilTarget
        {
            public ulong Address;
            public float OriginalValue;
            public string Name;
            public bool IsFovKick;
        }
        private static readonly List<RecoilTarget> _activeRecoilTargets = new();
        private static bool _activeRecoilOriginalCaptured;
        private static ulong _lastCheckedRecoilItem;
        private static bool _lastRecoilItemHadTargets;
        private static bool _recoilApplied;
        private static float _lastRecoilIntensity = -1f;
        private static long _lastRecoilVerifyTicks;

        // Auto-Bunnyhop
        public static bool AutoBunnyhopEnabled = false;
        public static string AutoBunnyhopStatus { get; private set; } = "Off";

        // Viewangle Aim (Memory Aimbot)
        public static bool ViewangleAimEnabled = false;
        public static float ViewangleAimFov = 25.0f;
        public static float ViewangleAimSmoothness = 0.20f;
        public static int ViewangleAimTargetBone = 0; // 0 = Head, 1 = Chest
        public static float ViewangleAimHeadOffset = 0.0f;
        public static float ViewangleAimChestOffset = 0.0f;
        public static bool ViewangleAimTeamCheck = true;
        public static bool ViewangleAimDrawFov = true;
        public static UnityKeyCode ViewangleAimKey = UnityKeyCode.Mouse1;
        public static bool ViewangleAimRequireKey = true;
        public static int ViewangleAimKeyMode = 0; // 0 = Hold, 1 = Toggle
        public static string ViewangleAimStatus { get; private set; } = "Off";
        private static bool _aimbotToggleActive = false;
        private static readonly object _aimbotLock = new();

        private static readonly object _bunnyhopLock = new();
        private static ulong _activeJumpControllerObject;
        private static ulong _activeJumpControllerClass;
        private static ulong _activeRequestedJumpAddress;
        private static bool _activeRequestedJumpOriginal;
        private static bool _activeJumpOriginalCaptured;

        // No Flash
        public static bool NoFlashEnabled = false;
        public static string NoFlashStatus { get; private set; } = "Off";
        private static readonly object _noFlashLock = new();
        private static ulong _cachedLocalHubForEffects;
        private static ulong _cachedFlashedEffect;
        private static ulong _cachedFogControlEffect;
        private static ulong _cachedConcussedEffect;

        // No Fog (formerly Less Fog / Smoke)
        public static bool LessFogEnabled = false;
        public static bool FullNoFogEnabled = false;
        public static float LessFogClearness = 3.0f;
        public static bool LessFogScp244Enabled = false;
        public static string LessFogStatus { get; private set; } = "Off";
        public static bool NoFogEnabled { get => LessFogEnabled; set => LessFogEnabled = value; }
        public static bool NoFogAtmosphericEnabled { get => FullNoFogEnabled; set => FullNoFogEnabled = value; }
        public static bool NoFogScp244Enabled { get => LessFogScp244Enabled; set => LessFogScp244Enabled = value; }
        public static string NoFogStatus => LessFogStatus;
        public static bool NoSmokeEnabled { get => LessFogEnabled; set => LessFogEnabled = value; }
        public static bool NoSmokeScp244Enabled { get => LessFogScp244Enabled; set => LessFogScp244Enabled = value; }
        public static string NoSmokeStatus => LessFogStatus;
        private static readonly object _lessFogLock = new();

        // FogController & FogSettings restoration tracking
        private static readonly Dictionary<ulong, (float start, float end, float weight, bool isEnabled)> _origFogSettings = new();
        private static bool _customFogCaptured;
        private static float _origCustomFogFade;
        private static float _origCustomFogStart;
        private static float _origCustomFogEnd;
        private static readonly Dictionary<ulong, (float size, float diameter)> _origScp244Dict = new();

        // Brightness / Fullbright (Multi-Volume LiftGammaGain, Exposure, ColorAdjustments)
        public static bool BrightnessEnabled = false;
        public static float BrightnessEv = 1.5f;
        public static string BrightnessStatus { get; private set; } = "Off";
        private static readonly object _brightnessLock = new();

        // Gun Flashlight Enhancement
        public static bool GunFlashlightEnabled = false;
        public static float GunFlashlightSpotAngle = 80.0f;
        public static float GunFlashlightIntensityMult = 1.0f;
        public static float GunFlashlightRange = 60.0f;
        public static string GunFlashlightStatus { get; private set; } = "Off";

        private static readonly object _gunFlashlightLock = new();
        private static ulong _activeFlashlightViewmodelObject;
        private static ulong _activeFlashlightLightSource;
        private static ulong _activeFlashlightHdLightData;
        private static ulong _activeFlashlightNativeLight;
        private static ulong _lastCheckedFlashlightItem;
        private static bool _lastFlashlightItemHadFlashlight;
        private static string? _lastFlashlightFailure;
        private static bool _flashlightApplied;
        private static long _lastFlashlightVerifyTicks;
        private static long _lastFlashlightRetryTicks;

        // Targets for memory writes
        private static ulong _addrFadeDistance;
        private static ulong _addrLightDimmer;
        private static ulong _addrIntensity;
        private static ulong _addrInnerSpotPercent;
        private static ulong _addrVolumetricDimmer;
        private static ulong _addrVolumetricFadeDistance;
        private static ulong _addrNativeIntensity;
        private static ulong _addrNativeRange;
        private static ulong _addrNativeSpotAngle;
        private static ulong _addrNativeInnerSpotAngle;

        // Captured original values
        private static float _origFadeDistance;
        private static float _origLightDimmer;
        private static float _origIntensity;
        private static float _origInnerSpotPercent;
        private static float _origVolumetricDimmer;
        private static float _origVolumetricFadeDistance;
        private static float _origNativeIntensity;
        private static float _origNativeRange;
        private static float _origNativeSpotAngle;
        private static float _origNativeInnerSpotAngle;
        private static bool _activeFlashlightOriginalCaptured;

        // Instant ADS (Aim-Down-Sights)
        public static bool InstantAdsEnabled = false;
        public static string InstantAdsStatus { get; private set; } = "Off";
        private static readonly object _instantAdsLock = new();
        private static ulong _activeAdsExtension;
        private static ulong _activeAdsItem;
        private static ulong _addrAdsInSpeed;
        private static ulong _addrAdsOutSpeed;
        private static float _origAdsInSpeed;
        private static float _origAdsOutSpeed;
        private static bool _adsOriginalCaptured;
        private static bool _adsApplied;
        private static long _lastAdsVerifyTicks;

        // Third-Person View (TPV) / Corner Peek Mode
        public static bool TpvEnabled = false;
        public static float TpvDistance = 2.2f;
        public static float TpvHeight = 0.4f;
        public static float TpvShoulderOffset = 0.45f;
        public static UnityKeyCode TpvKey = UnityKeyCode.F4;
        public static bool TpvRequireKey = true;
        public static int TpvKeyMode = 0; // 0 = Toggle, 1 = Hold
        public static string TpvStatus { get; private set; } = "Off";
        public static bool TpvActive { get; private set; } = false;
        private static readonly object _tpvLock = new();
        private static bool _tpvToggleState = false;
        private static bool _tpvPrevKeyDown = false;
        private static bool _tpvWasApplied = false;
        private static ulong _tpvActiveTranslationAddr = 0;
        private static Vector3 _origPcrTranslation = Vector3.Zero;
        private static bool _tpvOrigCaptured = false;
        private static ulong _tpvActiveHub = 0;


        // HDRP Volume Scanning & Restoration Structures
        private struct HdrpLiftGammaGainTarget
        {
            public ulong GammaOverrideAddr;
            public ulong GammaValueAddr;
            public ulong GainOverrideAddr;
            public ulong GainValueAddr;
        }

        private struct HdrpFloatParamTarget
        {
            public ulong OverrideAddr;
            public ulong ValueAddr;
        }

        private struct HdrpFogTarget
        {
            public ulong EnabledOverrideAddr;
            public ulong EnabledValueAddr;
            public ulong MeanFreeOverrideAddr;
            public ulong MeanFreePathValueAddr;
        }

        private struct OrigLiftGammaGain
        {
            public bool OrigGammaOverride;
            public Vector4 OrigGammaValue;
            public bool OrigGainOverride;
            public Vector4 OrigGainValue;
        }

        private struct OrigFloatParam
        {
            public bool OrigOverride;
            public float OrigValue;
        }

        private struct OrigFogState
        {
            public bool OrigEnabledOverride;
            public bool OrigEnabledValue;
            public bool OrigMeanFreeOverride;
            public float OrigMeanFreePath;
        }

        private static readonly Dictionary<ulong, OrigLiftGammaGain> _origLiftGammaDict = new();
        private static readonly Dictionary<ulong, OrigFloatParam> _origExposureDict = new();
        private static readonly Dictionary<ulong, OrigFloatParam> _origColorAdjDict = new();
        private static readonly Dictionary<ulong, OrigFogState> _origHdrpFogDict = new();

        private static readonly List<HdrpLiftGammaGainTarget> _activeLiftGammaTargets = new();
        private static readonly List<HdrpFloatParamTarget> _activeExposureTargets = new();
        private static readonly List<HdrpFloatParamTarget> _activeColorAdjTargets = new();
        private static readonly List<HdrpFogTarget> _activeHdrpFogTargets = new();
        private static DateTime _lastHdrpScanTime = DateTime.MinValue;
        private static long _lastHdrpScanTicks = 0;
        private static long _lastLessFogTicks = 0;
        private static bool _lastAppliedFullNoFog = false;
        private static bool _fogSettingsApplied = false;
        private static bool _hdrpFogApplied = false;

        private static long _lastBrightnessTicks = 0;
        private static float _lastAppliedBrightnessEv = -1f;
        private static bool _brightnessApplied = false;

        public static void InvalidateCaches()
        {
            _tiCache.Clear();
            _staticFieldsCache.Clear();
            _subclassCache.Clear();
            _classNameCache.Clear();
            _playerNameCache.Clear();
            _healthModCache.Clear();
            _fieldOffsetCache.Clear();
            _cachedBoneTransforms.Clear();
            _cachedLocalHubForEffects = 0;
            _cachedFlashedEffect = 0;
            _cachedFogControlEffect = 0;
            _cachedConcussedEffect = 0;
            _lastHdrpScanTime = DateTime.MinValue;
            _lastHdrpScanTicks = 0;
            _lastLessFogTicks = 0;
            _lastAppliedFullNoFog = false;
            _fogSettingsApplied = false;
            _hdrpFogApplied = false;
            _lastBrightnessTicks = 0;
            _lastAppliedBrightnessEv = -1f;
            _brightnessApplied = false;
            _activeLiftGammaTargets.Clear();
            _activeExposureTargets.Clear();
            _activeColorAdjTargets.Clear();
            _activeHdrpFogTargets.Clear();
            _origScp244Dict.Clear();
            ClearActiveFlashlightState();
            _lastCheckedFlashlightItem = 0;
            _lastFlashlightItemHadFlashlight = false;
        }

        // ── Cached TypeInfo read ──
        private static ulong TypeinfoClass(ulong typeinfoRva)
        {
            if (_tiCache.TryGetValue(typeinfoRva, out ulong cached) && cached != 0)
                return cached;
            ulong ptr = Mem.Ptr(DmaMemory.GameAssemblyBase + typeinfoRva);
            if (ptr != 0) _tiCache[typeinfoRva] = ptr;
            return ptr;
        }

        private static ulong StaticFields(ulong typeinfoRva)
        {
            if (_staticFieldsCache.TryGetValue(typeinfoRva, out ulong cached) && cached != 0)
                return cached;
            ulong klass = TypeinfoClass(typeinfoRva);
            if (klass == 0) return 0;
            ulong sf = Mem.Ptr(klass + Offsets.Il2CppClass_static_fields);
            if (sf != 0) _staticFieldsCache[typeinfoRva] = sf;
            return sf;
        }

        // ── Cached class name ──
        private static string ClassName(ulong obj)
        {
            ulong klass = Mem.Ptr(obj);
            if (klass == 0) return string.Empty;

            if (_classNameCache.TryGetValue(klass, out string? cached))
                return cached;

            ulong namePtr = Mem.Ptr(klass + Offsets.Il2CppClass_name);
            if (namePtr == 0) return string.Empty;
            string name = Mem.Str(namePtr, 96) ?? string.Empty;
            _classNameCache[klass] = name;
            return name;
        }

        // ── Cached subclass check ──
        private static bool IsFpcSubclass(ulong roleKlass)
        {
            if (roleKlass == 0) return false;
            if (_subclassCache.TryGetValue(roleKlass, out bool cached))
                return cached;

            ulong fpcTi = TypeinfoClass(Offsets.FpcStandardRoleBase_TypeInfo);
            if (fpcTi == 0) { _subclassCache[roleKlass] = false; return false; }

            ulong cur = roleKlass;
            bool result = false;
            for (int i = 0; i < 32 && cur != 0; ++i)
            {
                if (cur == fpcTi) { result = true; break; }
                cur = Mem.Ptr(cur + Offsets.Il2CppClass_parent);
            }
            _subclassCache[roleKlass] = result;
            return result;
        }

        // ── Resolve hub list (2 DMA reads) ──
        public static ulong ResolveAllHubs()
        {
            ulong sf = StaticFields(Offsets.ReferenceHub_TypeInfo);
            if (sf == 0) return 0;
            ulong ptr = Mem.Ptr(sf + Offsets.RH_static_AllHubs);
            return ptr;
        }

        // ── Read HashSet as bulk byte buffer instead of per-slot (Zero-Allocation) ──
        public static List<ulong> HashSetElements(ulong hashset)
        {
            var outList = new List<ulong>();
            if (hashset == 0) return outList;

            // Packed read: slotsArr (+0x18, 8 bytes) and lastIndex (+0x24, 4 bytes) in single 16-byte DMA read
            Span<byte> header = stackalloc byte[16];
            DmaMemory.ReadBuffer<byte>(hashset + Offsets.HashSet_slots, header, false);
            ulong slotsArr = System.Runtime.InteropServices.MemoryMarshal.Read<ulong>(header.Slice(0, 8));
            int lastIndex = System.Runtime.InteropServices.MemoryMarshal.Read<int>(header.Slice(12, 4));
            if (slotsArr == 0 || lastIndex <= 0 || lastIndex > 8192) return outList;

            // Bulk read: each slot is 16 bytes (hash:4 + next:4 + value:8)
            // Items start at Array_items offset
            int slotCount = lastIndex;
            int bytesNeeded = slotCount * 16;
            byte[]? rented = null;
            Span<byte> buf = bytesNeeded <= 1024
                ? stackalloc byte[bytesNeeded]
                : (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(bytesNeeded)).AsSpan(0, bytesNeeded);
            try
            {
                Diag.CountRead();
                DmaMemory.ReadBuffer<byte>(slotsArr + Offsets.Array_items, buf, true);

                for (int i = 0; i < slotCount; i++)
                {
                    int offset = i * 16;
                    int hash = System.Runtime.InteropServices.MemoryMarshal.Read<int>(buf.Slice(offset, 4));
                    if (hash < 0) continue;
                    ulong value = System.Runtime.InteropServices.MemoryMarshal.Read<ulong>(buf.Slice(offset + 8, 8));
                    if (value.IsValidVirtualAddress())
                        outList.Add(value);
                }
            }
            catch { return outList; }
            finally
            {
                if (rented != null)
                    System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }
            return outList;
        }

        private static ulong _cachedLocalHub = 0;
        private static long _lastLocalHubTicks = 0;
        private static ulong _cachedLocalHubForInv = 0;
        private static ulong _cachedInventoryPtr = 0;

        public static ulong LocalHub()
        {
            long now = Stopwatch.GetTimestamp();
            if (_cachedLocalHub != 0 && (now - _lastLocalHubTicks) < (Stopwatch.Frequency / 5)) // 200ms cache TTL
            {
                return _cachedLocalHub;
            }

            ulong sf = StaticFields(Offsets.ReferenceHub_TypeInfo);
            if (sf == 0) return 0;

            // Packed read: set (+0x30, 1 byte) and localHub (+0x38, 8 bytes) in single 16-byte DMA read
            Span<byte> buf = stackalloc byte[16];
            DmaMemory.ReadBuffer<byte>(sf + Offsets.RH_static_localHubSet, buf, false);
            byte set = buf[0];
            if (set == 0)
            {
                _cachedLocalHub = 0;
                _lastLocalHubTicks = now;
                return 0;
            }

            ulong hub = System.Runtime.InteropServices.MemoryMarshal.Read<ulong>(buf.Slice(8, 8));
            _cachedLocalHub = hub;
            _lastLocalHubTicks = now;
            return hub;
        }

        public static ulong GetEquippedItem(ulong localHub)
        {
            if (localHub == 0) return 0;
            if (_cachedLocalHubForInv != localHub || _cachedInventoryPtr == 0)
            {
                if (!TryReadObjectField(localHub, "inventory", out _cachedInventoryPtr))
                {
                    _cachedInventoryPtr = 0;
                    return 0;
                }
                _cachedLocalHubForInv = localHub;
            }

            if (_cachedInventoryPtr != 0 && TryReadObjectField(_cachedInventoryPtr, "_curInstance", out ulong equipped))
            {
                return equipped;
            }
            return 0;
        }

        public static ulong HostHub()
        {
            ulong sf = StaticFields(Offsets.ReferenceHub_TypeInfo);
            if (sf == 0) return 0;
            byte set = Mem.Val<byte>(sf + Offsets.RH_static_hostHubSet);
            if (set == 0) return 0;
            return Mem.Ptr(sf + Offsets.RH_static_hostHub);
        }

        public static bool IsLocalPlayerAlive()
        {
            ulong localHub = LocalHub();
            if (!localHub.IsValidVirtualAddress()) return false;
            ulong rm = Mem.Ptr(localHub + Offsets.RH_roleManager);
            if (!rm.IsValidVirtualAddress()) return false;
            ulong curRole = Mem.Ptr(rm + Offsets.PRM_curRole);
            if (!curRole.IsValidVirtualAddress()) return false;
            RoleTypeId role = ReadRole(curRole);
            return RoleIsAlive(role);
        }

        public static bool HasLivingPlayers(IReadOnlyList<PlayerInfo>? players)
        {
            if (players == null || players.Count == 0) return false;
            for (int i = 0; i < players.Count; i++)
            {
                if (RoleIsAlive(players[i].Role))
                    return true;
            }
            return false;
        }

        public static bool IsRoundStartTimerRunning()
        {
            ulong sf = StaticFields(Offsets.RoundStart_TypeInfo);
            if (sf == 0) return false;
            ulong swPtr = Mem.Ptr(sf + Offsets.RoundStart_static_RoundStartTimer);
            if (!swPtr.IsValidVirtualAddress()) return false;
            return Mem.Val<byte>(swPtr + Offsets.Stopwatch_isRunning) != 0;
        }

        public static bool IsRoundStarted(IReadOnlyList<PlayerInfo>? players = null)
        {
            // 1. Authoritative: Local player is alive (Class-D, Scientist, Guard, MTF, Chaos, SCP)
            if (IsLocalPlayerAlive()) return true;

            // 2. Authoritative: Any snapshot player has a confirmed living role
            if (HasLivingPlayers(players)) return true;

            // 3. Authoritative: GameCore.RoundStart stopwatch is running
            if (IsRoundStartTimerRunning()) return true;

            // 4. Check RoundSummary synchronized elapsed timer (>0 means round in progress)
            int roundTime = ReadRoundTime();
            if (roundTime > 0) return true;

            // 5. Check LocalHub CharacterClassManager
            ulong localHub = LocalHub();
            if (localHub.IsValidVirtualAddress())
            {
                ulong ccm = Mem.Ptr(localHub + Offsets.RH_characterClassManager);
                if (ccm.IsValidVirtualAddress() && Mem.Val<byte>(ccm + Offsets.CCM_RoundStarted) != 0)
                {
                    return true;
                }
            }

            // 6. Check HostHub CharacterClassManager (only as positive confirmation, never as false blocker)
            ulong hostHub = HostHub();
            if (hostHub.IsValidVirtualAddress())
            {
                ulong ccm = Mem.Ptr(hostHub + Offsets.RH_characterClassManager);
                if (ccm.IsValidVirtualAddress() && Mem.Val<byte>(ccm + Offsets.CCM_RoundStarted) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static int ReadRoundTime()
        {
            // 1. Check GameCore.RoundStart.RoundStartTimer (synced client stopwatch)
            ulong rsSf = StaticFields(Offsets.RoundStart_TypeInfo);
            if (rsSf != 0)
            {
                ulong swPtr = Mem.Ptr(rsSf + Offsets.RoundStart_static_RoundStartTimer);
                if (swPtr.IsValidVirtualAddress())
                {
                    bool isRunning = Mem.Val<byte>(swPtr + Offsets.Stopwatch_isRunning) != 0;
                    if (isRunning)
                    {
                        long elapsed = Mem.Val<long>(swPtr + Offsets.Stopwatch_elapsed);
                        long started = Mem.Val<long>(swPtr + Offsets.Stopwatch_started);
                        long now = Stopwatch.GetTimestamp();
                        long totalTicks = elapsed + (now - started);
                        if (totalTicks > 0 && Stopwatch.Frequency > 0)
                        {
                            int sec = (int)(totalTicks / Stopwatch.Frequency);
                            if (sec >= 0 && sec < 86400) return sec;
                        }
                    }
                }
            }

            // 2. Check RoundSummary synchronized elapsed timer (server/host)
            ulong sf = StaticFields(Offsets.RoundSummary_TypeInfo);
            if (sf != 0)
            {
                int rTime = Mem.Val<int>(sf + Offsets.RoundSummary_roundTime);
                if (rTime > 0 && rTime < 86400) return rTime;
            }

            return 0;
        }

        public static bool IsRoundEnded(IReadOnlyList<PlayerInfo>? players = null)
        {
            // Guard: If local player or any snapshot player is actively alive, the round CANNOT have ended!
            if (IsLocalPlayerAlive() || HasLivingPlayers(players))
                return false;

            ulong sf = StaticFields(Offsets.RoundSummary_TypeInfo);
            if (sf == 0) return false;
            byte set = Mem.Val<byte>(sf + Offsets.RoundSummary_singletonSet);
            if (set == 0) return false;
            ulong singleton = Mem.Ptr(sf + Offsets.RoundSummary_singleton);
            if (!singleton.IsValidVirtualAddress()) return false;
            return Mem.Val<byte>(singleton + Offsets.RoundSummary_isRoundEnded) != 0;
        }

        public static HashSet<ulong> ReadDeadPlayerHubs()
        {
            var deadHubs = new HashSet<ulong>();
            ulong sf = StaticFields(Offsets.RagdollManager_TypeInfo);
            if (sf == 0) return deadHubs;

            ulong allRagdollsAddr = Mem.Ptr(sf + Offsets.RagdollManager_static_AllRagdolls);
            if (allRagdollsAddr == 0) return deadHubs;

            List<ulong> ragdollPtrs = HashSetElements(allRagdollsAddr);
            if (ragdollPtrs.Count == 0) return deadHubs;

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < ragdollPtrs.Count; i++)
                {
                    round[i].AddEntry<ulong>(0, ragdollPtrs[i] + Offsets.Ragdoll_Info_OwnerHub);
                }
                map.Execute();

                for (int i = 0; i < ragdollPtrs.Count; i++)
                {
                    if (round[i].TryGetResult(0, out ulong ownerHub) && ownerHub.IsValidVirtualAddress())
                    {
                        deadHubs.Add(ownerHub);
                    }
                }
            }

            return deadHubs;
        }

        public static AliveSpectatableInfo ReadAliveSpectatableHubs()
        {
            ulong sf = StaticFields(Offsets.SpectatableModuleBase_TypeInfo);
            if (sf == 0) return new AliveSpectatableInfo { IsAvailable = false };

            ulong allInstancesAddr = Mem.Ptr(sf + Offsets.SpectatableModule_static_AllInstances);
            if (allInstancesAddr == 0) return new AliveSpectatableInfo { IsAvailable = false };

            List<ulong> modules = HashSetElements(allInstancesAddr);
            if (modules.Count == 0)
            {
                return new AliveSpectatableInfo { IsAvailable = true };
            }

            var aliveHubs = new HashSet<ulong>();
            var hubRoles = new Dictionary<ulong, RoleTypeId>();

            using (var map = ScatterReadMap.Get())
            {
                var round0 = map.AddRound(useCache: false);
                for (int i = 0; i < modules.Count; i++)
                {
                    round0[i].AddEntry<ulong>(0, modules[i] + Offsets.SpectatableModule_cachedRole);
                }
                map.Execute();

                var round1 = map.AddRound(useCache: false);
                var validIndices = new List<(int index, ulong cachedRole)>();
                for (int i = 0; i < modules.Count; i++)
                {
                    if (round0[i].TryGetResult(0, out ulong cachedRole) && cachedRole.IsValidVirtualAddress())
                    {
                        validIndices.Add((i, cachedRole));
                        round1[i].AddEntry<ulong>(0, cachedRole + Offsets.RoleBase_lastOwner);
                        round1[i].AddEntry<sbyte>(1, cachedRole + Offsets.Role_roleTypeId);
                    }
                }
                map.Execute();

                for (int j = 0; j < validIndices.Count; j++)
                {
                    int i = validIndices[j].index;
                    if (round1[i].TryGetResult(0, out ulong ownerHub) && ownerHub.IsValidVirtualAddress())
                    {
                        aliveHubs.Add(ownerHub);
                        if (round1[i].TryGetResult(1, out sbyte roleId))
                        {
                            hubRoles[ownerHub] = (RoleTypeId)roleId;
                        }
                    }
                }
            }

            return new AliveSpectatableInfo
            {
                IsAvailable = true,
                AliveHubs = aliveHubs,
                HubRoles = hubRoles
            };
        }

        // ── Position: 4-6 DMA reads (was 7-10+) ──
        public static Vector3? ReadPlayerPosition(ulong curRole)
        {
            if (curRole == 0) return null;

            ulong roleKlass = Mem.Ptr(curRole); // klass can be cached
            if (IsFpcSubclass(roleKlass))
            {
                ulong fpcModule = Mem.Ptr(curRole + Offsets.Fpc_FpcModule);
                if (fpcModule != 0)
                {
                    var pos = Mem.Val<Vector3>(fpcModule + Offsets.Fpm_cachedPosition, false);
                    if (Math.Abs(pos.X) < 20000f && Math.Abs(pos.Y) < 20000f && Math.Abs(pos.Z) < 20000f)
                        return pos;
                }
                var last = Mem.Val<Vector3>(curRole + Offsets.Fpc_lastPos, false);
                if (Math.Abs(last.X) < 20000f && Math.Abs(last.Z) < 20000f)
                    return last;
            }
            return null;
        }

        private static bool IsValidFov(float fov)
        {
            return !float.IsNaN(fov) &&
                   !float.IsInfinity(fov) &&
                   fov >= 1.0f &&
                   fov < 179.0f;
        }

        /// <summary>
        /// Resolves an instance-field offset from IL2CPP metadata. This only reads
        /// metadata; the resolved destination is always validated again by the
        /// protected memory-write layer before any value is changed.
        /// </summary>
        private static bool TryGetFieldOffset(ulong klass, string fieldName, out ulong offset)
        {
            offset = 0;
            if (klass == 0 || string.IsNullOrEmpty(fieldName))
                return false;

            ulong rootKlass = klass;
            if (_fieldOffsetCache.TryGetValue((rootKlass, fieldName), out offset))
                return offset != 0;

            for (int depth = 0; depth < 32 && klass != 0; depth++)
            {
                if (_fieldOffsetCache.TryGetValue((klass, fieldName), out offset))
                {
                    _fieldOffsetCache[(rootKlass, fieldName)] = offset;
                    return offset != 0;
                }

                ulong fields = Mem.Ptr(klass + Offsets.Il2CppClass_fields, false);
                if (fields != 0)
                {
                    // FieldInfo entries are contiguous. Requiring each entry's
                    // parent pointer to match the class prevents walking into the
                    // next metadata array if a malformed count is encountered.
                    for (int i = 0; i < 256; i++)
                    {
                        ulong fieldInfo = checked(
                            fields + (ulong)i * Offsets.FieldInfo_size);
                        ulong parent = Mem.Ptr(
                            fieldInfo + Offsets.FieldInfo_parent,
                            false);
                        if (parent != klass)
                            break;

                        ulong namePointer = Mem.Ptr(
                            fieldInfo + Offsets.FieldInfo_name,
                            false);
                        if (namePointer == 0)
                            continue;

                        string name = Mem.Str(namePointer, 128, false);
                        if (!name.Equals(fieldName, StringComparison.Ordinal))
                            continue;

                        int rawOffset = Mem.Val<int>(
                            fieldInfo + Offsets.FieldInfo_offset,
                            false);
                        if (rawOffset < 0x10 || rawOffset > 0x10000)
                        {
                            _fieldOffsetCache[(klass, fieldName)] = 0;
                            _fieldOffsetCache[(rootKlass, fieldName)] = 0;
                            offset = 0;
                            return false;
                        }

                        offset = (ulong)rawOffset;
                        _fieldOffsetCache[(klass, fieldName)] = offset;
                        _fieldOffsetCache[(rootKlass, fieldName)] = offset;
                        return true;
                    }
                }

                klass = Mem.Ptr(klass + Offsets.Il2CppClass_parent, false);
            }

            // Negative caching: record 0 so subsequent requests for this missing field return false immediately with 0 DMA reads
            _fieldOffsetCache[(rootKlass, fieldName)] = 0;
            offset = 0;
            return false;
        }

        private static bool TryReadObjectField(
            ulong instance,
            string fieldName,
            out ulong value)
        {
            value = 0;
            if (instance == 0)
                return false;

            try
            {
                ulong klass = Mem.Ptr(instance, false);
                if (!TryGetFieldOffset(klass, fieldName, out ulong fieldOffset))
                    return false;

                value = Mem.Ptr(checked(instance + fieldOffset), false);
                return value != 0;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private static bool TryResolveActiveViewmodelFov(
            out ulong viewmodel,
            out ulong fovAddress,
            out float dynamicFovOffset,
            out string failure)
        {
            viewmodel = 0;
            fovAddress = 0;
            dynamicFovOffset = 0f;
            failure = "Viewmodel unavailable";

            ulong localHub = LocalHub();
            if (localHub == 0)
            {
                failure = "Waiting for local player";
                return false;
            }

            if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
            {
                failure = "Inventory unavailable";
                return false;
            }

            if (!TryReadObjectField(inventory, "_curInstance", out ulong item))
            {
                failure = "No equipped item";
                return false;
            }

            if (!TryReadObjectField(item, "ViewModel", out viewmodel))
            {
                failure = "Equipped item has no viewmodel";
                return false;
            }

            ulong viewmodelKlass = Mem.Ptr(viewmodel, false);
            if (!TryGetFieldOffset(viewmodelKlass, "_fov", out ulong fovFieldOffset))
            {
                failure = "This viewmodel has no writable FOV data field";
                return false;
            }

            try
            {
                fovAddress = checked(viewmodel + fovFieldOffset);

                // Firearm viewmodels subtract their live sight/attachment offset
                // from _fov. Compensating here keeps the requested final FOV while
                // still writing only the backing object data.
                if (TryGetFieldOffset(
                    viewmodelKlass,
                    "<FovOffset>k__BackingField",
                    out ulong offsetFieldOffset))
                {
                    float candidate = Mem.Val<float>(
                        checked(viewmodel + offsetFieldOffset),
                        false);
                    if (!float.IsNaN(candidate) &&
                        !float.IsInfinity(candidate) &&
                        Math.Abs(candidate) <= 90f)
                    {
                        dynamicFovOffset = candidate;
                    }
                }
            }
            catch (OverflowException)
            {
                failure = "Invalid viewmodel address";
                return false;
            }

            return true;
        }

        private static void ClearActiveViewmodelFovState()
        {
            _activeViewmodelFovAddress = 0;
            _activeViewmodelObject = 0;
            _activeViewmodelClass = 0;
            _activeViewmodelOriginalFov = 0f;
            _activeViewmodelOriginalCaptured = false;
        }

        private static bool TryRestoreActiveViewmodelFov()
        {
            if (!_activeViewmodelOriginalCaptured ||
                _activeViewmodelFovAddress == 0)
            {
                ClearActiveViewmodelFovState();
                _viewmodelFovRestoreFailures = 0;
                return true;
            }

            // If the managed object was destroyed, there is no live destination
            // left to restore and writing its stale address would be unsafe.
            if (_activeViewmodelObject == 0 ||
                Mem.Ptr(_activeViewmodelObject, false) != _activeViewmodelClass)
            {
                ClearActiveViewmodelFovState();
                _viewmodelFovRestoreFailures = 0;
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                if (!Mem.TryWriteValue(
                    _activeViewmodelFovAddress,
                    _activeViewmodelOriginalFov))
                {
                    _viewmodelFovRestoreFailures++;
                    if (_viewmodelFovRestoreFailures >= 3)
                    {
                        ClearActiveViewmodelFovState();
                        _viewmodelFovRestoreFailures = 0;
                        return true;
                    }
                    return false;
                }

                float restored = Mem.Val<float>(
                    _activeViewmodelFovAddress,
                    false);
                if (Math.Abs(restored - _activeViewmodelOriginalFov) > 0.05f)
                {
                    _viewmodelFovRestoreFailures++;
                    if (_viewmodelFovRestoreFailures >= 3)
                    {
                        ClearActiveViewmodelFovState();
                        _viewmodelFovRestoreFailures = 0;
                        return true;
                    }
                    return false;
                }

                ClearActiveViewmodelFovState();
                _viewmodelFovRestoreFailures = 0;
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void PollViewmodelFov()
        {
            lock (_viewmodelFovLock)
            {
                if (!MasterMemWritesEnabled || !ViewmodelFovChangerEnabled)
                {
                    ViewmodelFovStatus = TryRestoreActiveViewmodelFov()
                        ? "Off"
                        : "Restore blocked";
                    return;
                }

                float target = Math.Clamp(CustomViewmodelFov, 30.0f, 140.0f);
                CustomViewmodelFov = target;

                if (!TryResolveActiveViewmodelFov(
                    out ulong viewmodel,
                    out ulong fovAddress,
                    out float dynamicFovOffset,
                    out string failure))
                {
                    if (!TryRestoreActiveViewmodelFov())
                    {
                        ViewmodelFovStatus = "Restore blocked";
                        return;
                    }

                    ViewmodelFovStatus = failure;
                    return;
                }

                if (_activeViewmodelFovAddress != fovAddress ||
                    _activeViewmodelObject != viewmodel)
                {
                    if (!TryRestoreActiveViewmodelFov())
                    {
                        ViewmodelFovStatus = "Restore blocked";
                        return;
                    }

                    float original = Mem.Val<float>(fovAddress, false);
                    if (!IsValidFov(original))
                    {
                        ViewmodelFovStatus = "Invalid viewmodel FOV data";
                        return;
                    }

                    _activeViewmodelObject = viewmodel;
                    _activeViewmodelClass = Mem.Ptr(viewmodel, false);
                    _activeViewmodelFovAddress = fovAddress;
                    _activeViewmodelOriginalFov = original;
                    _activeViewmodelOriginalCaptured = true;
                }

                float desiredBackingValue = target + dynamicFovOffset;
                if (!IsValidFov(desiredBackingValue))
                {
                    ViewmodelFovStatus = "Requested FOV is outside the safe range";
                    return;
                }

                float current = Mem.Val<float>(fovAddress, false);
                if (Math.Abs(current - desiredBackingValue) > 0.001f)
                {
                    if (!Mem.TryWriteValue(fovAddress, desiredBackingValue))
                    {
                        ViewmodelFovStatus = "Write blocked";
                        return;
                    }

                    float verified = Mem.Val<float>(fovAddress, false);
                    ViewmodelFovStatus =
                        Math.Abs(verified - desiredBackingValue) <= 0.001f
                            ? "Applied (data only)"
                            : "Write verification failed";
                }
                else
                {
                    ViewmodelFovStatus = "Applied (data only)";
                }
            }
        }

        public static void RestoreViewmodelFov()
        {
            lock (_viewmodelFovLock)
            {
                TryRestoreActiveViewmodelFov();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  WORLD CAMERA FOV & HOLD-TO-ZOOM
        // ═══════════════════════════════════════════════════════════
        private static ulong ScanForAlternateCamera(ulong realCameraObj, ulong realCameraKlass)
        {
            if (realCameraObj < 0x10000000 || realCameraKlass == 0) return 0;
            // Scan up to 16MB before realCameraObj in 2MB chunks
            ulong searchStart = realCameraObj > 16 * 1024 * 1024 ? realCameraObj - 16 * 1024 * 1024 : 0x10000000;
            ulong searchEnd = realCameraObj + 4 * 1024 * 1024;
            ulong chunkSize = 2 * 1024 * 1024;
            byte[] klassBytes = BitConverter.GetBytes(realCameraKlass);

            for (ulong chunkAddr = searchStart; chunkAddr < searchEnd; chunkAddr += chunkSize)
            {
                int readSize = (int)Math.Min(chunkSize, searchEnd - chunkAddr);
                byte[] buffer = new byte[readSize];
                try
                {
                    DmaMemory.ReadBuffer<byte>(chunkAddr, buffer.AsSpan(), false);
                }
                catch
                {
                    continue;
                }

                Span<byte> span = buffer.AsSpan();
                int idx = 0;
                while (idx <= readSize - 24)
                {
                    int match = span.Slice(idx).IndexOf(klassBytes);
                    if (match == -1) break;
                    int candidateOffset = idx + match;
                    if ((candidateOffset % 8) == 0)
                    {
                        ulong candidateObj = chunkAddr + (ulong)candidateOffset;
                        if (candidateObj != realCameraObj)
                        {
                            ulong nativePtr = Mem.Ptr(candidateObj + 0x10, false);
                            if (nativePtr != 0 && nativePtr.IsValidVirtualAddress())
                            {
                                float fov = Mem.Val<float>(nativePtr + UnityOffsets.Camera.FOV, false);
                                if (IsValidFov(fov))
                                {
                                    return candidateObj;
                                }
                            }
                        }
                    }
                    idx = candidateOffset + 8;
                }
            }
            return 0;
        }

        private static bool TryResolveWorldCamera(
            out ulong singleton,
            out ulong realCameraObj,
            out ulong altCameraObj,
            out ulong fovAddress,
            out ulong dirtyAddress,
            out string failure)
        {
            singleton = 0;
            realCameraObj = 0;
            altCameraObj = 0;
            fovAddress = 0;
            dirtyAddress = 0;
            failure = "Camera unavailable";

            ulong ti = Offsets.CameraShakeController_TypeInfo;
            ulong sf = StaticFields(ti);
            if (sf == 0)
            {
                failure = "Waiting for CameraShakeController";
                return false;
            }

            singleton = Mem.Ptr(sf + 0x0, false);
            if (singleton == 0)
            {
                failure = "Waiting for player camera";
                return false;
            }

            // Real player camera object is stored in singleton._camera (+0x20).
            // If already redirected to altCamera, active state holds the real camera object.
            if (_activeWorldOriginalCaptured && _activeWorldCameraObj != 0 && _activeWorldSingleton == singleton)
            {
                realCameraObj = _activeWorldCameraObj;
            }
            else
            {
                realCameraObj = Mem.Ptr(singleton + 0x20, false);
            }

            if (realCameraObj == 0)
            {
                failure = "Camera component null";
                return false;
            }

            ulong realCameraKlass = Mem.Ptr(realCameraObj, false);
            if (realCameraKlass == 0)
            {
                failure = "Camera class pointer null";
                return false;
            }

            ulong nativeCamera = Mem.Ptr(realCameraObj + 0x10, false);
            if (nativeCamera == 0)
            {
                failure = "Native camera pointer null";
                return false;
            }

            // Resolve an alternate Camera object to redirect CameraShakeController._camera into
            // Priority 1: CameraShakeController static fields at sf + 0x10
            ulong candidate = Mem.Ptr(sf + 0x10, false);
            if (candidate != 0 && candidate != realCameraObj && Mem.Ptr(candidate, false) == realCameraKlass && Mem.Ptr(candidate + 0x10, false) != 0)
            {
                altCameraObj = candidate;
            }

            // Priority 2: Scan memory chunk around realCameraObj
            if (altCameraObj == 0)
            {
                altCameraObj = ScanForAlternateCamera(realCameraObj, realCameraKlass);
            }

            if (altCameraObj == 0)
            {
                failure = "Alternate camera target unavailable";
                return false;
            }

            try
            {
                fovAddress = checked(nativeCamera + UnityOffsets.Camera.FOV);
                dirtyAddress = checked(nativeCamera + UnityOffsets.Camera.DirtyFlags);
            }
            catch (OverflowException)
            {
                failure = "Invalid camera memory address";
                return false;
            }

            return true;
        }

        private static void ClearActiveWorldFovState()
        {
            _activeWorldSingleton = 0;
            _activeWorldCameraObj = 0;
            _activeWorldAltCameraObj = 0;
            _activeWorldFovAddress = 0;
            _activeWorldDirtyAddress = 0;
            _activeWorldOriginalFov = Offsets.DefaultVerticalFOV;
            _activeWorldOriginalCaptured = false;
            _activeWorldFovValue = Offsets.DefaultVerticalFOV;
        }

        private static bool TryRestoreActiveWorldFov()
        {
            _activeWorldFovValue = Offsets.DefaultVerticalFOV;

            if (!_activeWorldOriginalCaptured || _activeWorldFovAddress == 0)
            {
                ClearActiveWorldFovState();
                _worldFovRestoreFailures = 0;
                return true;
            }

            // If the managed objects or native camera are destroyed/invalid, clear and finish
            if (_activeWorldSingleton == 0 || _activeWorldCameraObj == 0 ||
                Mem.Ptr(_activeWorldSingleton, false) == 0 || Mem.Ptr(_activeWorldCameraObj, false) == 0)
            {
                ClearActiveWorldFovState();
                _worldFovRestoreFailures = 0;
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                bool allRestored = true;

                // 1. Restore native Camera FOV
                if (!Mem.TryWriteValue<float>(_activeWorldFovAddress, _activeWorldOriginalFov))
                {
                    allRestored = false;
                }
                else
                {
                    float restored = Mem.Val<float>(_activeWorldFovAddress, false);
                    if (Math.Abs(restored - _activeWorldOriginalFov) > 0.05f)
                        allRestored = false;
                }

                // 2. Set projection matrix dirty flag (0x101) so Unity recalculates projection
                if (_activeWorldDirtyAddress != 0)
                {
                    Mem.TryWriteValue<ushort>(_activeWorldDirtyAddress, 0x101);
                }

                // 3. Restore CameraShakeController._camera (+0x20) back to real camera
                if (!Mem.TryWriteValue<ulong>(_activeWorldSingleton + 0x20, _activeWorldCameraObj))
                {
                    allRestored = false;
                }
                else
                {
                    ulong restoredCam = Mem.Ptr(_activeWorldSingleton + 0x20, false);
                    if (restoredCam != _activeWorldCameraObj)
                        allRestored = false;
                }

                if (!allRestored)
                {
                    _worldFovRestoreFailures++;
                    if (_worldFovRestoreFailures >= 3)
                    {
                        ClearActiveWorldFovState();
                        _worldFovRestoreFailures = 0;
                        return true;
                    }
                    return false;
                }

                ClearActiveWorldFovState();
                _worldFovRestoreFailures = 0;
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void PollWorldFov()
        {
            lock (_worldFovLock)
            {
                if (!MasterMemWritesEnabled || !WorldFovChangerEnabled)
                {
                    WorldFovStatus = TryRestoreActiveWorldFov()
                        ? "Off"
                        : "Restore blocked";
                    return;
                }

                bool isZooming = ZoomFovEnabled && UnityInput.IsKeyDown(ZoomKey);
                float target = isZooming
                    ? Math.Clamp(ZoomFov, 15.0f, 60.0f)
                    : Math.Clamp(CustomWorldFov, 60.0f, 120.0f);

                _activeWorldFovValue = target;

                ulong singleton = _activeWorldSingleton;
                ulong realCameraObj = _activeWorldCameraObj;
                ulong altCameraObj = _activeWorldAltCameraObj;
                ulong fovAddress = _activeWorldFovAddress;
                ulong dirtyAddress = _activeWorldDirtyAddress;

                bool needResolve = singleton == 0 || realCameraObj == 0 || fovAddress == 0 ||
                                   Mem.Ptr(realCameraObj, false) == 0;

                if (needResolve)
                {
                    if (!TryResolveWorldCamera(
                        out singleton,
                        out realCameraObj,
                        out altCameraObj,
                        out fovAddress,
                        out dirtyAddress,
                        out string failure))
                    {
                        if (!TryRestoreActiveWorldFov())
                        {
                            WorldFovStatus = "Restore blocked";
                            return;
                        }

                        WorldFovStatus = failure;
                        return;
                    }
                }

                if (_activeWorldFovAddress != fovAddress ||
                    _activeWorldSingleton != singleton ||
                    _activeWorldCameraObj != realCameraObj)
                {
                    if (!TryRestoreActiveWorldFov())
                    {
                        WorldFovStatus = "Restore blocked";
                        return;
                    }

                    float originalFov = Mem.Val<float>(fovAddress, false);
                    if (!IsValidFov(originalFov))
                    {
                        WorldFovStatus = "Invalid camera FOV data";
                        return;
                    }

                    _activeWorldSingleton = singleton;
                    _activeWorldCameraObj = realCameraObj;
                    _activeWorldAltCameraObj = altCameraObj;
                    _activeWorldFovAddress = fovAddress;
                    _activeWorldDirtyAddress = dirtyAddress;
                    _activeWorldOriginalFov = originalFov;
                    _activeWorldOriginalCaptured = true;
                }

                // Redirect CameraShakeController._camera (+0x20) to altCameraObj to eliminate
                // LateUpdate from overwriting 70.0f to the real player camera every frame
                ulong currentCamRef = Mem.Ptr(singleton + 0x20, false);
                if (currentCamRef != altCameraObj && !Mem.TryWriteValue<ulong>(singleton + 0x20, altCameraObj))
                {
                    WorldFovStatus = "Write blocked (Redirection)";
                    return;
                }

                // Write custom FOV to real native Camera structure
                float currentFov = Mem.Val<float>(fovAddress, false);
                if (Math.Abs(currentFov - target) > 0.001f)
                {
                    if (!Mem.TryWriteValue<float>(fovAddress, target))
                    {
                        WorldFovStatus = "Write blocked (FOV)";
                        return;
                    }
                    if (dirtyAddress != 0)
                    {
                        Mem.TryWriteValue<ushort>(dirtyAddress, 0x101);
                    }

                    float verifiedFov = Mem.Val<float>(fovAddress, false);
                    WorldFovStatus = Math.Abs(verifiedFov - target) <= 0.001f
                        ? (isZooming ? "Zooming" : "Applied")
                        : "Write verification failed";
                }
                else
                {
                    WorldFovStatus = isZooming ? "Zooming" : "Applied";
                }
            }
        }

        public static void RestoreWorldFov()
        {
            lock (_worldFovLock)
            {
                TryRestoreActiveWorldFov();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  NO WEAPON SWAY & BOBBING
        // ═══════════════════════════════════════════════════════════
        private static bool IsValidSwayMultiplier(float val)
        {
            return !float.IsNaN(val) &&
                   !float.IsInfinity(val) &&
                   val >= 0.0f &&
                   val <= 100.0f;
        }

        private static bool TryResolveNoSwayTargets(
            out ulong viewmodel,
            out List<SwayTarget> targets,
            out string failure)
        {
            viewmodel = 0;
            targets = new List<SwayTarget>();
            failure = "Viewmodel unavailable";

            var resolvedTargets = new List<SwayTarget>();

            ulong localHub = LocalHub();
            if (localHub == 0)
            {
                failure = "Waiting for local player";
                return false;
            }

            if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
            {
                failure = "Inventory unavailable";
                return false;
            }

            if (!TryReadObjectField(inventory, "_curInstance", out ulong item))
            {
                failure = "No equipped item";
                return false;
            }

            if (!TryReadObjectField(item, "ViewModel", out viewmodel))
            {
                failure = "Equipped item has no viewmodel";
                return false;
            }

            ulong viewmodelKlass = Mem.Ptr(viewmodel, false);
            if (viewmodelKlass == 0)
            {
                failure = "Invalid viewmodel class";
                return false;
            }

            void AddTarget(ulong addr, string name)
            {
                if (addr == 0) return;
                float current = Mem.Val<float>(addr, false);
                if (IsValidSwayMultiplier(current))
                {
                    if (!resolvedTargets.Exists(t => t.Address == addr))
                    {
                        resolvedTargets.Add(new SwayTarget { Address = addr, OriginalValue = current, Name = name });
                    }
                }
            }

            try
            {
                // 1. Hip sway settings on AnimatedFirearmViewmodel (struct GoopSwaySettings at offset 0x80)
                if (TryGetFieldOffset(viewmodelKlass, "_hipSwaySettings", out ulong hipOffset))
                {
                    ulong hipAddr = checked(viewmodel + hipOffset);
                    AddTarget(checked(hipAddr + 0x8), "Hip Sway");
                    AddTarget(checked(hipAddr + 0xC), "Hip Translation");
                    AddTarget(checked(hipAddr + 0x1C), "Hip Bob");
                }

                // 2. Sway controller on StandardAnimatedViemodel (_swayController at offset 0x78)
                if (TryGetFieldOffset(viewmodelKlass, "_swayController", out ulong swayOffset))
                {
                    ulong swayController = Mem.Ptr(checked(viewmodel + swayOffset), false);
                    if (swayController != 0)
                    {
                        ulong swayKlass = Mem.Ptr(swayController, false);
                        if (swayKlass != 0)
                        {
                            // Internal GoopSway._settings struct (offset 0x18)
                            if (TryGetFieldOffset(swayKlass, "_settings", out ulong settingsOffset))
                            {
                                ulong settingsAddr = checked(swayController + settingsOffset);
                                AddTarget(checked(settingsAddr + 0x8), "Settings Sway");
                                AddTarget(checked(settingsAddr + 0xC), "Settings Translation");
                                AddTarget(checked(settingsAddr + 0x1C), "Settings Bob");
                            }

                            // WalkSway multipliers
                            if (TryGetFieldOffset(swayKlass, "_walkSwayWeightMultiplier", out ulong walkOffset))
                            {
                                AddTarget(checked(swayController + walkOffset), "Walk Sway Multiplier");
                            }
                            if (TryGetFieldOffset(swayKlass, "_jumpSwayWeightMultiplier", out ulong jumpOffset))
                            {
                                AddTarget(checked(swayController + jumpOffset), "Jump Sway Multiplier");
                            }

                            // FirearmSway scales
                            if (TryGetFieldOffset(swayKlass, "_bobScale", out ulong bobScaleOffset))
                            {
                                AddTarget(checked(swayController + bobScaleOffset), "Bob Scale");
                            }
                            if (TryGetFieldOffset(swayKlass, "_jumpScale", out ulong jumpScaleOffset))
                            {
                                AddTarget(checked(swayController + jumpScaleOffset), "Jump Scale");
                            }
                            if (TryGetFieldOffset(swayKlass, "_walkScale", out ulong walkScaleOffset))
                            {
                                AddTarget(checked(swayController + walkScaleOffset), "Walk Scale");
                            }

                            // ADS Extension on FirearmSway (_adsExtension at offset 0xA0)
                            if (TryGetFieldOffset(swayKlass, "_adsExtension", out ulong adsExtOffset))
                            {
                                ulong adsExt = Mem.Ptr(checked(swayController + adsExtOffset), false);
                                if (adsExt != 0)
                                {
                                    ulong adsExtKlass = Mem.Ptr(adsExt, false);
                                    if (adsExtKlass != 0 && TryGetFieldOffset(adsExtKlass, "<AdsSway>k__BackingField", out ulong adsSwayOffset))
                                    {
                                        ulong adsSwayAddr = checked(adsExt + adsSwayOffset);
                                        AddTarget(checked(adsSwayAddr + 0x8), "ADS Sway");
                                        AddTarget(checked(adsSwayAddr + 0xC), "ADS Translation");
                                        AddTarget(checked(adsSwayAddr + 0x1C), "ADS Bob");
                                    }
                                }
                            }
                        }
                    }
                }

                // 3. Fallback: check <Extensions>k__BackingField on AnimatedFirearmViewmodel
                if (!resolvedTargets.Exists(t => t.Name == "ADS Sway") &&
                    TryGetFieldOffset(viewmodelKlass, "<Extensions>k__BackingField", out ulong extsOffset))
                {
                    ulong extsArr = Mem.Ptr(checked(viewmodel + extsOffset), false);
                    if (extsArr != 0)
                    {
                        int len = Mem.Val<int>(extsArr + Offsets.Array_max_length, false);
                        if (len > 0 && len <= 32)
                        {
                            for (int i = 0; i < len; i++)
                            {
                                ulong comp = Mem.Ptr(checked(extsArr + Offsets.Array_items + (ulong)i * 8), false);
                                if (comp == 0) continue;
                                ulong compKlass = Mem.Ptr(comp, false);
                                if (compKlass == 0) continue;
                                string name = ClassName(comp);
                                if (name == "ViewmodelAdsExtension")
                                {
                                    if (TryGetFieldOffset(compKlass, "<AdsSway>k__BackingField", out ulong adsSwayOffset))
                                    {
                                        ulong adsSwayAddr = checked(comp + adsSwayOffset);
                                        AddTarget(checked(adsSwayAddr + 0x8), "ADS Sway");
                                        AddTarget(checked(adsSwayAddr + 0xC), "ADS Translation");
                                        AddTarget(checked(adsSwayAddr + 0x1C), "ADS Bob");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                failure = "Invalid sway address arithmetic";
                return false;
            }

            if (resolvedTargets.Count == 0)
            {
                failure = "No sway controller fields found";
                return false;
            }

            targets = resolvedTargets;
            return true;
        }

        private static void ClearActiveNoSwayState()
        {
            _activeSwayTargets.Clear();
            _activeNoSwayViewmodelObject = 0;
            _activeNoSwayViewmodelClass = 0;
            _activeSwayOriginalCaptured = false;
            _swayApplied = false;
        }

        private static bool TryRestoreNoSway()
        {
            if (!_activeSwayOriginalCaptured || _activeSwayTargets.Count == 0)
            {
                ClearActiveNoSwayState();
                return true;
            }

            // If the managed object was destroyed, there is no live destination
            // left to restore and writing its stale address would be unsafe.
            if (_activeNoSwayViewmodelObject == 0 ||
                Mem.Ptr(_activeNoSwayViewmodelObject, false) != _activeNoSwayViewmodelClass)
            {
                ClearActiveNoSwayState();
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                bool allRestored = true;
                foreach (var target in _activeSwayTargets)
                {
                    if (target.Address == 0) continue;
                    if (!Mem.TryWriteValue(target.Address, target.OriginalValue))
                    {
                        allRestored = false;
                        continue;
                    }

                    float restored = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(restored - target.OriginalValue) > 0.05f)
                        allRestored = false;
                }

                if (!allRestored)
                    return false;

                ClearActiveNoSwayState();
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void PollNoSway(ulong localHub = 0, ulong item = 0)
        {
            lock (_noSwayLock)
            {
                if (!MasterMemWritesEnabled || !NoSwayEnabled)
                {
                    _lastCheckedSwayItem = 0;
                    _lastSwayItemHadTargets = false;
                    _swayApplied = false;
                    NoSwayStatus = TryRestoreNoSway() ? "Off" : "Restore blocked";
                    return;
                }

                if (localHub == 0)
                    localHub = LocalHub();
                if (localHub == 0)
                {
                    _lastCheckedSwayItem = 0;
                    _lastSwayItemHadTargets = false;
                    _swayApplied = false;
                    if (!TryRestoreNoSway())
                    {
                        NoSwayStatus = "Restore blocked";
                        return;
                    }
                    NoSwayStatus = "Waiting for local player";
                    return;
                }

                if (item == 0)
                {
                    if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
                    {
                        _lastCheckedSwayItem = 0;
                        _lastSwayItemHadTargets = false;
                        _swayApplied = false;
                        if (!TryRestoreNoSway())
                        {
                            NoSwayStatus = "Restore blocked";
                            return;
                        }
                        NoSwayStatus = "Inventory unavailable";
                        return;
                    }
                    TryReadObjectField(inventory, "_curInstance", out item);
                }

                // If equipped item has not changed and previously had no sway targets, skip resolving to avoid DMA bus choking
                if (item == _lastCheckedSwayItem && !_lastSwayItemHadTargets && _activeNoSwayViewmodelObject == 0)
                {
                    NoSwayStatus = item == 0 ? "No equipped item" : "Item has no sway";
                    return;
                }

                // If already applied and item unchanged, throttle verification reads to 10 Hz
                long now = Stopwatch.GetTimestamp();
                if (_swayApplied && item == _lastCheckedSwayItem && _activeNoSwayViewmodelObject != 0)
                {
                    if (now - _lastSwayVerifyTicks < (Stopwatch.Frequency / 10))
                        return;
                }

                if (!TryResolveNoSwayTargets(
                    out ulong viewmodel,
                    out List<SwayTarget> targets,
                    out string failure))
                {
                    _lastCheckedSwayItem = item;
                    _lastSwayItemHadTargets = false;
                    _swayApplied = false;
                    if (!TryRestoreNoSway())
                    {
                        NoSwayStatus = "Restore blocked";
                        return;
                    }

                    NoSwayStatus = failure;
                    return;
                }

                _lastCheckedSwayItem = item;
                _lastSwayItemHadTargets = true;

                if (_activeNoSwayViewmodelObject != viewmodel)
                {
                    if (!TryRestoreNoSway())
                    {
                        NoSwayStatus = "Restore blocked";
                        return;
                    }

                    _activeNoSwayViewmodelObject = viewmodel;
                    _activeNoSwayViewmodelClass = Mem.Ptr(viewmodel, false);
                    _activeSwayTargets.Clear();
                    _activeSwayTargets.AddRange(targets);
                    _activeSwayOriginalCaptured = true;
                    _swayApplied = false;
                }

                bool allWritten = true;
                foreach (var target in _activeSwayTargets)
                {
                    float current = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(current - 0.0f) > 0.001f &&
                        !Mem.TryWriteValue(target.Address, 0.0f))
                    {
                        allWritten = false;
                        break;
                    }

                    float verified = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(verified - 0.0f) > 0.001f)
                    {
                        allWritten = false;
                        break;
                    }
                }

                _swayApplied = allWritten;
                _lastSwayVerifyTicks = now;
                NoSwayStatus = allWritten ? "Applied (data only)" : "Write verification failed";
            }
        }

        public static void RestoreNoSway()
        {
            lock (_noSwayLock)
            {
                _lastCheckedSwayItem = 0;
                _lastSwayItemHadTargets = false;
                TryRestoreNoSway();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  WEAPON RECOIL CONTROL (NO RECOIL)
        // ═══════════════════════════════════════════════════════════
        private static bool IsValidRecoilValue(float val, bool isFovKick = false)
        {
            if (float.IsNaN(val) || float.IsInfinity(val))
                return false;

            if (isFovKick)
                return val >= 0.0f && val <= 10.0f;

            return val >= 0.0f && val <= 1000.0f;
        }

        private static bool TryResolveNoRecoilTargets(
            out ulong firearmItem,
            out List<RecoilTarget> targets,
            out string failure)
        {
            firearmItem = 0;
            targets = new List<RecoilTarget>();
            failure = "Weapon unavailable";

            var resolvedTargets = new List<RecoilTarget>();

            ulong localHub = LocalHub();
            if (localHub == 0)
            {
                failure = "Waiting for local player";
                return false;
            }

            if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
            {
                failure = "Inventory unavailable";
                return false;
            }

            if (!TryReadObjectField(inventory, "_curInstance", out firearmItem))
            {
                failure = "No equipped item";
                return false;
            }

            ulong itemKlass = Mem.Ptr(firearmItem, false);
            if (itemKlass == 0)
            {
                failure = "Invalid item class";
                return false;
            }

            void AddTarget(ulong addr, string name, bool isFovKick = false)
            {
                if (addr == 0) return;
                float current = Mem.Val<float>(addr, false);
                if (IsValidRecoilValue(current, isFovKick))
                {
                    if (!resolvedTargets.Exists(t => t.Address == addr))
                    {
                        resolvedTargets.Add(new RecoilTarget
                        {
                            Address = addr,
                            OriginalValue = current,
                            Name = name,
                            IsFovKick = isFovKick
                        });
                    }
                }
            }

            try
            {
                // 1. Walk _modules on the active Firearm
                if (TryGetFieldOffset(itemKlass, "_modules", out ulong modulesOffset))
                {
                    ulong modulesArr = Mem.Ptr(checked(firearmItem + modulesOffset), false);
                    if (modulesArr != 0)
                    {
                        int len = Mem.Val<int>(modulesArr + Offsets.Array_max_length, false);
                        if (len > 0 && len <= 64)
                        {
                            for (int i = 0; i < len; i++)
                            {
                                ulong modulePtr = Mem.Ptr(checked(modulesArr + Offsets.Array_items + (ulong)i * 8), false);
                                if (modulePtr == 0) continue;
                                ulong moduleKlass = Mem.Ptr(modulePtr, false);
                                if (moduleKlass == 0) continue;

                                string moduleName = ClassName(modulePtr);
                                if (moduleName == "RecoilPatternModule")
                                {
                                    if (TryGetFieldOffset(moduleKlass, "<BaseRecoil>k__BackingField", out ulong baseRecoilOffset))
                                    {
                                        ulong baseRecoilAddr = checked(modulePtr + baseRecoilOffset);
                                        // struct RecoilSettings:
                                        // +0x00: AnimationTime (do NOT touch, game divides by it!)
                                        // +0x04: ZAxis
                                        // +0x08: FovKick
                                        // +0x0C: UpKick
                                        // +0x10: SideKick
                                        AddTarget(checked(baseRecoilAddr + 0x4), "Base Recoil Z-Axis");
                                        AddTarget(checked(baseRecoilAddr + 0x8), "Base Recoil FOV Kick", isFovKick: true);
                                        AddTarget(checked(baseRecoilAddr + 0xC), "Base Recoil Up-Kick");
                                        AddTarget(checked(baseRecoilAddr + 0x10), "Base Recoil Side-Kick");
                                    }

                                    if (TryGetFieldOffset(moduleKlass, "<AdsRecoilScale>k__BackingField", out ulong adsScaleOffset))
                                    {
                                        AddTarget(checked(modulePtr + adsScaleOffset), "ADS Recoil Scale");
                                    }
                                }
                                else if (moduleName == "DisruptorModeSelector")
                                {
                                    if (TryGetFieldOffset(moduleKlass, "_singleRecoilScale", out ulong singleRecoilOffset))
                                    {
                                        AddTarget(checked(modulePtr + singleRecoilOffset), "Disruptor Single Recoil Scale");
                                    }
                                }
                            }
                        }
                    }
                }

                // 2. Walk ViewModel Extensions for ViewmodelFullAutoSwayExtension
                if (TryReadObjectField(firearmItem, "ViewModel", out ulong viewmodel) && viewmodel != 0)
                {
                    ulong viewmodelKlass = Mem.Ptr(viewmodel, false);
                    if (viewmodelKlass != 0 && TryGetFieldOffset(viewmodelKlass, "<Extensions>k__BackingField", out ulong extsOffset))
                    {
                        ulong extsArr = Mem.Ptr(checked(viewmodel + extsOffset), false);
                        if (extsArr != 0)
                        {
                            int len = Mem.Val<int>(extsArr + Offsets.Array_max_length, false);
                            if (len > 0 && len <= 32)
                            {
                                for (int i = 0; i < len; i++)
                                {
                                    ulong comp = Mem.Ptr(checked(extsArr + Offsets.Array_items + (ulong)i * 8), false);
                                    if (comp == 0) continue;
                                    ulong compKlass = Mem.Ptr(comp, false);
                                    if (compKlass == 0) continue;

                                    string name = ClassName(comp);
                                    if (name == "ViewmodelFullAutoSwayExtension")
                                    {
                                        if (TryGetFieldOffset(compKlass, "_adsMultiplier", out ulong adsMultOffset))
                                        {
                                            AddTarget(checked(comp + adsMultOffset), "Full-Auto ADS Sway Multiplier");
                                        }
                                        if (TryGetFieldOffset(compKlass, "_hipMultiplier", out ulong hipMultOffset))
                                        {
                                            AddTarget(checked(comp + hipMultOffset), "Full-Auto Hip Sway Multiplier");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                failure = "Invalid recoil address arithmetic";
                return false;
            }

            if (resolvedTargets.Count == 0)
            {
                failure = "No recoil module fields found";
                return false;
            }

            targets = resolvedTargets;
            return true;
        }

        private static void ClearActiveNoRecoilState()
        {
            _activeRecoilTargets.Clear();
            _activeRecoilItemObject = 0;
            _activeRecoilItemClass = 0;
            _activeRecoilOriginalCaptured = false;
            _recoilApplied = false;
        }

        private static bool TryRestoreNoRecoil()
        {
            if (!_activeRecoilOriginalCaptured || _activeRecoilTargets.Count == 0)
            {
                ClearActiveNoRecoilState();
                return true;
            }

            // If the managed firearm item object was destroyed, there is no live destination
            // left to restore and writing its stale address would be unsafe.
            if (_activeRecoilItemObject == 0 ||
                Mem.Ptr(_activeRecoilItemObject, false) != _activeRecoilItemClass)
            {
                ClearActiveNoRecoilState();
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                bool allRestored = true;
                foreach (var target in _activeRecoilTargets)
                {
                    if (target.Address == 0) continue;
                    if (!Mem.TryWriteValue(target.Address, target.OriginalValue))
                    {
                        allRestored = false;
                        continue;
                    }

                    float restored = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(restored - target.OriginalValue) > 0.05f)
                        allRestored = false;
                }

                if (!allRestored)
                    return false;

                ClearActiveNoRecoilState();
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void PollNoRecoil(ulong localHub = 0, ulong item = 0)
        {
            lock (_noRecoilLock)
            {
                if (!MasterMemWritesEnabled || !NoRecoilEnabled)
                {
                    _lastCheckedRecoilItem = 0;
                    _lastRecoilItemHadTargets = false;
                    _recoilApplied = false;
                    NoRecoilStatus = TryRestoreNoRecoil() ? "Off" : "Restore blocked";
                    return;
                }

                if (localHub == 0)
                    localHub = LocalHub();
                if (localHub == 0)
                {
                    _lastCheckedRecoilItem = 0;
                    _lastRecoilItemHadTargets = false;
                    _recoilApplied = false;
                    if (!TryRestoreNoRecoil())
                    {
                        NoRecoilStatus = "Restore blocked";
                        return;
                    }
                    NoRecoilStatus = "Waiting for local player";
                    return;
                }

                if (item == 0)
                {
                    if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
                    {
                        _lastCheckedRecoilItem = 0;
                        _lastRecoilItemHadTargets = false;
                        _recoilApplied = false;
                        if (!TryRestoreNoRecoil())
                        {
                            NoRecoilStatus = "Restore blocked";
                            return;
                        }
                        NoRecoilStatus = "Inventory unavailable";
                        return;
                    }
                    TryReadObjectField(inventory, "_curInstance", out item);
                }

                // If equipped item has not changed and previously had no recoil targets, skip resolving to avoid DMA bus choking
                if (item == _lastCheckedRecoilItem && !_lastRecoilItemHadTargets && _activeRecoilItemObject == 0)
                {
                    NoRecoilStatus = item == 0 ? "No equipped item" : "Item is not a firearm";
                    return;
                }

                float intensityFactor = Math.Clamp(RecoilIntensity / 100.0f, 0.0f, 1.0f);
                long now = Stopwatch.GetTimestamp();

                // If already applied, item unchanged, and intensity unchanged, throttle verification reads to 10 Hz
                if (_recoilApplied && item == _lastCheckedRecoilItem && _activeRecoilItemObject != 0 && Math.Abs(intensityFactor - _lastRecoilIntensity) <= 0.001f)
                {
                    if (now - _lastRecoilVerifyTicks < (Stopwatch.Frequency / 10))
                        return;
                }

                if (!TryResolveNoRecoilTargets(
                    out ulong firearmItem,
                    out List<RecoilTarget> targets,
                    out string failure))
                {
                    _lastCheckedRecoilItem = item;
                    _lastRecoilItemHadTargets = false;
                    _recoilApplied = false;
                    if (!TryRestoreNoRecoil())
                    {
                        NoRecoilStatus = "Restore blocked";
                        return;
                    }

                    NoRecoilStatus = failure;
                    return;
                }

                _lastCheckedRecoilItem = item;
                _lastRecoilItemHadTargets = true;

                if (_activeRecoilItemObject != firearmItem)
                {
                    if (!TryRestoreNoRecoil())
                    {
                        NoRecoilStatus = "Restore blocked";
                        return;
                    }

                    _activeRecoilItemObject = firearmItem;
                    _activeRecoilItemClass = Mem.Ptr(firearmItem, false);
                    _activeRecoilTargets.Clear();
                    _activeRecoilTargets.AddRange(targets);
                    _activeRecoilOriginalCaptured = true;
                    _recoilApplied = false;
                }

                bool allWritten = true;
                foreach (var target in _activeRecoilTargets)
                {
                    float targetVal;
                    if (target.IsFovKick)
                    {
                        // FovKick baseline is 1.0f (neutral). When scaling down to 0%, target is 1.0f (no kick).
                        targetVal = 1.0f + (target.OriginalValue - 1.0f) * intensityFactor;
                    }
                    else
                    {
                        targetVal = target.OriginalValue * intensityFactor;
                    }

                    float current = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(current - targetVal) > 0.001f &&
                        !Mem.TryWriteValue(target.Address, targetVal))
                    {
                        allWritten = false;
                        break;
                    }

                    float verified = Mem.Val<float>(target.Address, false);
                    if (Math.Abs(verified - targetVal) > 0.001f)
                    {
                        allWritten = false;
                        break;
                    }
                }

                _recoilApplied = allWritten;
                _lastRecoilIntensity = intensityFactor;
                _lastRecoilVerifyTicks = now;
                NoRecoilStatus = allWritten
                    ? (intensityFactor <= 0.001f ? "Applied (0% Recoil)" : $"Applied ({RecoilIntensity:0}% Recoil)")
                    : "Write verification failed";
            }
        }

        public static void RestoreNoRecoil()
        {
            lock (_noRecoilLock)
            {
                _lastCheckedRecoilItem = 0;
                _lastRecoilItemHadTargets = false;
                TryRestoreNoRecoil();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  AUTO-BUNNYHOP / INSTANT JUMP TIMING
        // ═══════════════════════════════════════════════════════════
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_SPACE = 0x20;

        private static bool IsGameWindowFocused()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return false;
                GetWindowThreadProcessId(hwnd, out uint pid);
                var mem = ScpslMemory.Instance;
                return mem != null && mem.ProcessPID != 0 && pid == mem.ProcessPID;
            }
            catch { return false; }
        }

        private static bool TryResolveJumpController(
            out ulong jumpController,
            out ulong requestedJumpAddr,
            out bool isGrounded,
            out string failure)
        {
            jumpController = 0;
            requestedJumpAddr = 0;
            isGrounded = false;
            failure = "Waiting for local player";

            ulong local = LocalHub();
            if (local == 0) return false;

            if (!TryReadObjectField(local, "roleManager", out ulong roleManager))
            {
                roleManager = Mem.Ptr(local + Offsets.RH_roleManager, false);
                if (roleManager == 0)
                {
                    failure = "RoleManager unavailable";
                    return false;
                }
            }

            if (!TryReadObjectField(roleManager, "_curRole", out ulong curRole))
            {
                curRole = Mem.Ptr(roleManager + Offsets.PRM_curRole, false);
                if (curRole == 0)
                {
                    failure = "Current role unavailable";
                    return false;
                }
            }

            ulong curRoleKlass = Mem.Ptr(curRole, false);
            if (!IsFpcSubclass(curRoleKlass))
            {
                failure = "Role does not support jumping";
                return false;
            }

            if (!TryGetFieldOffset(curRoleKlass, "<FpcModule>k__BackingField", out ulong fpcOffset))
                fpcOffset = Offsets.Fpc_FpcModule;

            ulong fpcModule = Mem.Ptr(curRole + fpcOffset, false);
            if (fpcModule == 0)
            {
                failure = "Movement module unavailable";
                return false;
            }

            ulong fpmKlass = Mem.Ptr(fpcModule, false);
            if (!TryGetFieldOffset(fpmKlass, "<Motor>k__BackingField", out ulong motorOffset))
            {
                failure = "Motor field not found";
                return false;
            }

            ulong motor = Mem.Ptr(fpcModule + motorOffset, false);
            if (motor == 0)
            {
                failure = "Motor unavailable";
                return false;
            }

            ulong motorKlass = Mem.Ptr(motor, false);
            if (!TryGetFieldOffset(motorKlass, "JumpController", out ulong jcOffset))
            {
                failure = "JumpController field not found";
                return false;
            }

            jumpController = Mem.Ptr(motor + jcOffset, false);
            if (jumpController == 0)
            {
                failure = "JumpController unavailable";
                return false;
            }

            ulong jcKlass = Mem.Ptr(jumpController, false);
            if (!TryGetFieldOffset(jcKlass, "_requestedJump", out ulong reqJumpOffset))
            {
                failure = "_requestedJump field not found";
                return false;
            }

            try
            {
                requestedJumpAddr = checked(jumpController + reqJumpOffset);
            }
            catch (OverflowException)
            {
                failure = "Invalid jump address arithmetic";
                return false;
            }

            bool hasSyncGrounded = TryGetFieldOffset(fpmKlass, "_syncGrounded", out ulong syncGroundedOffset);
            bool syncGrounded = hasSyncGrounded && (Mem.Val<byte>(fpcModule + syncGroundedOffset, false) != 0);

            bool hasIsJumping = TryGetFieldOffset(jcKlass, "<IsJumping>k__BackingField", out ulong isJumpingOffset);
            bool isJumping = hasIsJumping && (Mem.Val<byte>(jumpController + isJumpingOffset, false) != 0);

            isGrounded = hasSyncGrounded ? (syncGrounded && !isJumping) : !isJumping;
            return true;
        }

        private static void ClearActiveBunnyhopState()
        {
            _activeJumpControllerObject = 0;
            _activeJumpControllerClass = 0;
            _activeRequestedJumpAddress = 0;
            _activeRequestedJumpOriginal = false;
            _activeJumpOriginalCaptured = false;
        }

        private static bool TryRestoreBunnyhop()
        {
            if (!_activeJumpOriginalCaptured || _activeRequestedJumpAddress == 0)
            {
                ClearActiveBunnyhopState();
                return true;
            }

            if (_activeJumpControllerObject == 0 ||
                Mem.Ptr(_activeJumpControllerObject, false) != _activeJumpControllerClass)
            {
                ClearActiveBunnyhopState();
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                byte valToRestore = (byte)(_activeRequestedJumpOriginal ? 1 : 0);
                if (!Mem.TryWriteValue<byte>(_activeRequestedJumpAddress, valToRestore))
                {
                    return false;
                }

                byte restored = Mem.Val<byte>(_activeRequestedJumpAddress, false);
                if (restored != valToRestore)
                    return false;

                ClearActiveBunnyhopState();
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void PollBunnyhop()
        {
            lock (_bunnyhopLock)
            {
                if (!MasterMemWritesEnabled || !AutoBunnyhopEnabled)
                {
                    AutoBunnyhopStatus = TryRestoreBunnyhop() ? "Off" : "Restore blocked";
                    return;
                }

                if (!TryResolveJumpController(
                    out ulong jumpController,
                    out ulong requestedJumpAddr,
                    out bool isGrounded,
                    out string failure))
                {
                    if (!TryRestoreBunnyhop())
                    {
                        AutoBunnyhopStatus = "Restore blocked";
                        return;
                    }

                    AutoBunnyhopStatus = failure;
                    return;
                }

                if (_activeJumpControllerObject != jumpController ||
                    _activeRequestedJumpAddress != requestedJumpAddr)
                {
                    if (!TryRestoreBunnyhop())
                    {
                        AutoBunnyhopStatus = "Restore blocked";
                        return;
                    }

                    byte orig = Mem.Val<byte>(requestedJumpAddr, false);
                    _activeJumpControllerObject = jumpController;
                    _activeJumpControllerClass = Mem.Ptr(jumpController, false);
                    _activeRequestedJumpAddress = requestedJumpAddr;
                    _activeRequestedJumpOriginal = orig != 0;
                    _activeJumpOriginalCaptured = true;
                }

                // Prefer read-only Unity InputManager over DMA (avoids window focus issues & works on 2-PC)
                if (!UnityInput.IsConnected && DmaMemory.UnityBase != 0)
                {
                    try { UnityInput.Initialize(DmaMemory.UnityBase); } catch { }
                }

                bool isSpaceHeld;
                if (UnityInput.IsConnected)
                {
                    isSpaceHeld = UnityInput.IsKeyDown(UnityKeyCode.Space);
                }
                else
                {
                    if (!IsGameWindowFocused())
                    {
                        AutoBunnyhopStatus = "Ready (game not focused)";
                        return;
                    }
                    isSpaceHeld = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;
                }

                if (!isSpaceHeld)
                {
                    byte current = Mem.Val<byte>(requestedJumpAddr, false);
                    if (current != 0)
                    {
                        Mem.TryWriteValue<byte>(requestedJumpAddr, 0);
                    }
                    AutoBunnyhopStatus = UnityInput.IsConnected ? "Ready (hold space - Unity)" : "Ready (hold space)";
                    return;
                }

                if (isGrounded)
                {
                    byte current = Mem.Val<byte>(requestedJumpAddr, false);
                    if (current == 0)
                    {
                        if (!Mem.TryWriteValue<byte>(requestedJumpAddr, 1))
                        {
                            AutoBunnyhopStatus = "Write blocked";
                            return;
                        }

                        byte verified = Mem.Val<byte>(requestedJumpAddr, false);
                        if (verified == 0)
                        {
                            AutoBunnyhopStatus = "Write verification failed";
                            return;
                        }
                    }

                    AutoBunnyhopStatus = "Hopping (jump queued)";
                }
                else
                {
                    AutoBunnyhopStatus = "Airborne (holding space)";
                }
            }
        }

        public static void RestoreBunnyhop()
        {
            lock (_bunnyhopLock)
            {
                TryRestoreBunnyhop();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  VIEWANGLE AIM (MEMORY AIMBOT)
        // ═══════════════════════════════════════════════════════════
        public static bool IsAimbotEnemy(RoleTypeId localRole, RoleTypeId targetRole)
        {
            Team localTeam = TeamFromRole(localRole);
            Team targetTeam = TeamFromRole(targetRole);

            if (targetTeam == Team.Dead) return false;
            if (localTeam == Team.Dead || localTeam == Team.OtherAlive) return true;

            // Same team is friendly
            if (localTeam == targetTeam) return false;

            // SCPs friendly to other SCPs/Flamingos
            if (localTeam == Team.SCPs || localTeam == Team.Flamingos)
                return targetTeam != Team.SCPs && targetTeam != Team.Flamingos;

            // Non-SCPs: SCPs/Flamingos are always enemies
            if (targetTeam == Team.SCPs || targetTeam == Team.Flamingos)
                return true;

            // Foundation (MTF/Guards) & Scientists are allies
            if ((localTeam == Team.FoundationForces && targetTeam == Team.Scientists) ||
                (localTeam == Team.Scientists && targetTeam == Team.FoundationForces))
                return false;

            // Chaos Insurgency & Class-D are allies
            if ((localTeam == Team.ChaosInsurgency && targetTeam == Team.ClassD) ||
                (localTeam == Team.ClassD && targetTeam == Team.ChaosInsurgency))
                return false;

            return true;
        }

        public static bool TryResolveMouseLook(out ulong mouseLook)
        {
            mouseLook = 0;

            // 1. Direct path: static field _localInstance in FpcMouseLook_TypeInfo
            ulong sf = StaticFields(Offsets.FpcMouseLook_TypeInfo);
            if (sf.IsValidVirtualAddress())
            {
                ulong inst = Mem.Ptr(sf + Offsets.FpcMouseLook_static_localInstance, false);
                if (inst.IsValidVirtualAddress())
                {
                    mouseLook = inst;
                    return true;
                }
            }

            // 2. Fallback path: LocalHub -> RoleManager -> curRole -> FpcModule -> MouseLook (+0x88)
            ulong localHub = LocalHub();
            if (localHub.IsValidVirtualAddress())
            {
                ulong rm = Mem.Ptr(localHub + Offsets.RH_roleManager, false);
                if (rm.IsValidVirtualAddress())
                {
                    ulong curRole = Mem.Ptr(rm + Offsets.PRM_curRole, false);
                    if (curRole.IsValidVirtualAddress())
                    {
                        ulong fpcMod = Mem.Ptr(curRole + Offsets.Fpc_FpcModule, false);
                        if (fpcMod.IsValidVirtualAddress())
                        {
                            ulong ml = Mem.Ptr(fpcMod + Offsets.Fpm_MouseLook, false);
                            if (ml.IsValidVirtualAddress())
                            {
                                mouseLook = ml;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static void PollViewangleAim(in CameraInfo cam, List<PlayerInfo> currentPlayers)
        {
            lock (_aimbotLock)
            {
                if (!MasterMemWritesEnabled || !ViewangleAimEnabled)
                {
                    ViewangleAimStatus = "Off";
                    _aimbotToggleActive = false;
                    return;
                }

                if (!cam.Valid || currentPlayers == null || currentPlayers.Count == 0)
                {
                    ViewangleAimStatus = "Waiting for game/camera";
                    return;
                }

                if (!UnityInput.IsConnected && DmaMemory.UnityBase != 0)
                {
                    try { UnityInput.Initialize(DmaMemory.UnityBase); } catch { }
                }

                bool isAimActive;
                if (ViewangleAimRequireKey)
                {
                    if (ViewangleAimKeyMode == 1) // Toggle
                    {
                        if (UnityInput.GetKeyDown(ViewangleAimKey))
                        {
                            _aimbotToggleActive = !_aimbotToggleActive;
                        }
                        isAimActive = _aimbotToggleActive;
                    }
                    else // Hold
                    {
                        isAimActive = UnityInput.IsKeyDown(ViewangleAimKey);
                    }
                }
                else
                {
                    isAimActive = true;
                }

                if (!TryResolveMouseLook(out ulong mouseLook))
                {
                    ViewangleAimStatus = "Waiting for first-person player";
                    return;
                }

                if (!isAimActive)
                {
                    ViewangleAimStatus = ViewangleAimRequireKey
                        ? (ViewangleAimKeyMode == 1 ? "Ready (toggle off)" : $"Ready (hold {ScpslOverlay.FormatKeyName(ViewangleAimKey)})")
                        : "Ready";
                    return;
                }

                var r = cam.Rotation;
                float xz = r.X * r.Z, wy = r.W * r.Y;
                float yz = r.Y * r.Z, wx = r.W * r.X;
                float xx = r.X * r.X, yy = r.Y * r.Y;
                Vector3 camFwd = new Vector3(2f * (xz + wy), 2f * (yz - wx), 1f - 2f * (xx + yy));

                RoleTypeId localRole = RoleTypeId.None;
                ulong localHub = LocalHub();
                if (localHub.IsValidVirtualAddress())
                {
                    ulong rm = Mem.Ptr(localHub + Offsets.RH_roleManager, false);
                    if (rm.IsValidVirtualAddress())
                    {
                        ulong curRole = Mem.Ptr(rm + Offsets.PRM_curRole, false);
                        if (curRole.IsValidVirtualAddress())
                        {
                            localRole = ReadRole(curRole);
                        }
                    }
                }

                PlayerInfo? bestTarget = null;
                Vector3 bestTargetPos = Vector3.Zero;
                float bestAngle = float.MaxValue;

                for (int i = 0; i < currentPlayers.Count; i++)
                {
                    var p = currentPlayers[i];
                    if (p == null || p.IsLocal || !p.Alive || !p.HasPosition) continue;
                    if (p.Role == RoleTypeId.None || p.Role == RoleTypeId.Spectator || p.Role == RoleTypeId.Overwatch || p.Role == RoleTypeId.Destroyed || p.Role == RoleTypeId.Scp079) continue;

                    if (ViewangleAimTeamCheck && !IsAimbotEnemy(localRole, p.Role))
                        continue;

                    Vector3 targetPos;
                    if (ViewangleAimTargetBone == 0) // Head
                    {
                        if (p.BonePositions != null && p.BonePositions[SkeletonData.HeadHitboxIndex] != Vector3.Zero)
                        {
                            targetPos = p.BonePositions[SkeletonData.HeadHitboxIndex] + new Vector3(0, ViewangleAimHeadOffset, 0);
                        }
                        else
                        {
                            targetPos = p.Position + new Vector3(0, 1.60f + ViewangleAimHeadOffset, 0);
                        }
                    }
                    else // Chest
                    {
                        if (p.BonePositions != null && p.BonePositions[SkeletonData.ChestHitboxIndex] != Vector3.Zero)
                        {
                            targetPos = p.BonePositions[SkeletonData.ChestHitboxIndex] + new Vector3(0, ViewangleAimChestOffset, 0);
                        }
                        else
                        {
                            targetPos = p.Position + new Vector3(0, 1.20f + ViewangleAimChestOffset, 0);
                        }
                    }

                    Vector3 toTarget = targetPos - cam.Position;
                    float dist = toTarget.Length();
                    if (dist < 0.2f || dist > 250.0f) continue;

                    Vector3 targetDir = toTarget / dist;
                    float dot = Vector3.Dot(camFwd, targetDir);
                    dot = Math.Clamp(dot, -1.0f, 1.0f);
                    float angleDeg = MathF.Acos(dot) * (180.0f / MathF.PI);

                    if (angleDeg <= ViewangleAimFov && angleDeg < bestAngle)
                    {
                        bestAngle = angleDeg;
                        bestTarget = p;
                        bestTargetPos = targetPos;
                    }
                }

                if (bestTarget == null)
                {
                    ViewangleAimStatus = "Searching (no target in FOV)";
                    return;
                }

                Vector3 aimDelta = bestTargetPos - cam.Position;
                float hyp = MathF.Sqrt(aimDelta.X * aimDelta.X + aimDelta.Z * aimDelta.Z);
                if (hyp < 0.001f) return;

                float targetYaw = MathF.Atan2(aimDelta.X, aimDelta.Z) * (180.0f / MathF.PI);
                if (targetYaw < 0f) targetYaw += 360.0f;
                targetYaw = (targetYaw % 360.0f + 360.0f) % 360.0f;

                float pitchAngle = MathF.Atan2(aimDelta.Y, hyp) * (180.0f / MathF.PI);
                float targetPitch = Math.Clamp(-pitchAngle, -88.0f, 88.0f);

                float curYaw = Mem.Val<float>(mouseLook + Offsets.FpcMouseLook_curHorizontal, false);
                float curPitch = Mem.Val<float>(mouseLook + Offsets.FpcMouseLook_curVertical, false);

                float yawDelta = targetYaw - curYaw;
                while (yawDelta > 180.0f) yawDelta -= 360.0f;
                while (yawDelta < -180.0f) yawDelta += 360.0f;

                float pitchDelta = targetPitch - curPitch;

                float smooth = Math.Clamp(ViewangleAimSmoothness, 0.05f, 1.0f);
                float newYaw;
                float newPitch;

                if (smooth >= 0.99f)
                {
                    newYaw = targetYaw;
                    newPitch = targetPitch;
                }
                else
                {
                    newYaw = (curYaw + yawDelta * smooth) % 360.0f;
                    if (newYaw < 0f) newYaw += 360.0f;
                    newPitch = Math.Clamp(curPitch + pitchDelta * smooth, -88.0f, 88.0f);
                }

                Mem.TryWriteValue<float>(mouseLook + Offsets.FpcMouseLook_curHorizontal, newYaw);
                Mem.TryWriteValue<float>(mouseLook + Offsets.FpcMouseLook_curVertical, newPitch);
                Mem.TryWriteValue<float>(mouseLook + Offsets.FpcMouseLook_inputHorizontal, newYaw);
                Mem.TryWriteValue<float>(mouseLook + Offsets.FpcMouseLook_inputVertical, newPitch);

                ViewangleAimStatus = $"Locked: {RoleName(bestTarget.Role)} ({bestAngle:0.0}°)";
            }
        }

        public static void RestoreViewangleAim()
        {
            lock (_aimbotLock)
            {
                _aimbotToggleActive = false;
                ViewangleAimStatus = "Off";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  NO FLASH (Anti-Flashbang & Anti-Blindness)
        // ═══════════════════════════════════════════════════════════
        private static void TryResolveLocalPlayerEffects(ulong localHub)
        {
            if (localHub == 0 || !localHub.IsValidVirtualAddress())
            {
                _cachedLocalHubForEffects = 0;
                _cachedFlashedEffect = 0;
                _cachedFogControlEffect = 0;
                _cachedConcussedEffect = 0;
                return;
            }

            if (_cachedLocalHubForEffects != localHub)
            {
                _cachedLocalHubForEffects = localHub;
                _cachedFlashedEffect = 0;
                _cachedFogControlEffect = 0;
                _cachedConcussedEffect = 0;
            }

            if (_cachedFlashedEffect != 0)
                return;

            ulong pec = Mem.Ptr(localHub + Offsets.RH_playerEffectsController, false);
            if (!pec.IsValidVirtualAddress()) return;

            ulong allEffects = Mem.Ptr(pec + Offsets.PEC_AllEffects, false);
            int len = Mem.Val<int>(pec + Offsets.PEC_EffectsLength, false);
            if (!allEffects.IsValidVirtualAddress() || len <= 0 || len > 128) return;

            for (int i = 0; i < len; i++)
            {
                ulong effect = Mem.Ptr(allEffects + Offsets.Array_items + (ulong)i * 8, false);
                if (!effect.IsValidVirtualAddress()) continue;

                string name = ClassName(effect);
                if (name == "Flashed") _cachedFlashedEffect = effect;
                else if (name == "FogControl") _cachedFogControlEffect = effect;
                else if (name == "Concussed") _cachedConcussedEffect = effect;
            }
        }

        public static void PollNoFlash(ulong localHub = 0)
        {
            lock (_noFlashLock)
            {
                if (!MasterMemWritesEnabled || !NoFlashEnabled)
                {
                    NoFlashStatus = "Off";
                    return;
                }

                if (localHub == 0) localHub = LocalHub();
                if (localHub == 0)
                {
                    NoFlashStatus = "Waiting for player";
                    return;
                }

                TryResolveLocalPlayerEffects(localHub);

                if (_cachedFlashedEffect == 0)
                {
                    NoFlashStatus = "Locating effects";
                    return;
                }

                byte intensity = Mem.Val<byte>(_cachedFlashedEffect + Offsets.SEB_intensity, false);
                float remWeight = Mem.Val<float>(_cachedFlashedEffect + Offsets.Flashed_remainingWeight, false);

                if (intensity > 0 || remWeight > 0.001f)
                {
                    Mem.TryWriteValue<float>(_cachedFlashedEffect + Offsets.Flashed_remainingWeight, 0.0f);
                    Mem.TryWriteValue<byte>(_cachedFlashedEffect + Offsets.SEB_intensity, 0);
                    Mem.TryWriteValue<float>(_cachedFlashedEffect + Offsets.SEB_timeLeft, 0.0f);

                    ulong procVol = Mem.Ptr(_cachedFlashedEffect + Offsets.Flashed_processVolume, false);
                    if (procVol.IsValidVirtualAddress())
                    {
                        Mem.TryWriteValue<float>(procVol + Offsets.Volume_weight, 0.0f);
                    }

                    NoFlashStatus = "Neutralizing flash!";
                }
                else
                {
                    NoFlashStatus = "Active (Protected)";
                }

                if (_cachedConcussedEffect != 0)
                {
                    byte concIntensity = Mem.Val<byte>(_cachedConcussedEffect + Offsets.SEB_intensity, false);
                    if (concIntensity > 0)
                    {
                        Mem.TryWriteValue<byte>(_cachedConcussedEffect + Offsets.SEB_intensity, 0);
                        Mem.TryWriteValue<float>(_cachedConcussedEffect + Offsets.SEB_timeLeft, 0.0f);
                    }
                }
            }
        }

        public static void RestoreNoFlash()
        {
            lock (_noFlashLock)
            {
                _cachedFlashedEffect = 0;
                _cachedLocalHubForEffects = 0;
                NoFlashStatus = "Off";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  HDRP VOLUME COMPONENT DISCOVERY & SCANNING
        // ═══════════════════════════════════════════════════════════
        private static void ScanHdrpVolumeComponents()
        {
            long now = Stopwatch.GetTimestamp();
            bool hasTargets = (_activeLiftGammaTargets.Count > 0 || _activeExposureTargets.Count > 0 || _activeColorAdjTargets.Count > 0 || _activeHdrpFogTargets.Count > 0);
            double elapsed = (double)(now - _lastHdrpScanTicks) / Stopwatch.Frequency;
            if (hasTargets ? elapsed < 3.0 : elapsed < 1.0)
            {
                return;
            }
            _lastHdrpScanTicks = now;
            _lastHdrpScanTime = DateTime.UtcNow;

            ulong sf = StaticFields(Offsets.VolumeManager_TypeInfo);
            if (!sf.IsValidVirtualAddress()) return;

            ulong lazyInst = Mem.Ptr(sf + Offsets.VM_static_s_Instance, false);
            if (!lazyInst.IsValidVirtualAddress()) return;

            ulong vm = Mem.Ptr(lazyInst + Offsets.Lazy_value, false);
            if (!vm.IsValidVirtualAddress()) return;

            var foundProfiles = new HashSet<ulong>();
            var foundComponents = new HashSet<ulong>();

            // 1. Scene Volumes from VolumeCollection.m_Volumes
            ulong volColl = Mem.Ptr(vm + Offsets.VM_volumeCollection, false);
            if (volColl.IsValidVirtualAddress())
            {
                ulong volList = Mem.Ptr(volColl + Offsets.VolumeCollection_volumes, false);
                if (volList.IsValidVirtualAddress())
                {
                    ulong items = Mem.Ptr(volList + 0x10, false);
                    int count = Mem.Val<int>(volList + 0x18, false);
                    if (items.IsValidVirtualAddress() && count > 0 && count < 128)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            ulong vol = Mem.Ptr(items + Offsets.Array_items + (ulong)i * 8, false);
                            if (!vol.IsValidVirtualAddress()) continue;

                            ulong sharedProf = Mem.Ptr(vol + Offsets.Volume_sharedProfile, false);
                            ulong internalProf = Mem.Ptr(vol + Offsets.Volume_internalProfile, false);
                            ulong prof = internalProf.IsValidVirtualAddress() ? internalProf : sharedProf;
                            if (prof.IsValidVirtualAddress())
                            {
                                foundProfiles.Add(prof);
                            }
                        }
                    }
                }
            }

            // 2. Global, Quality Default Profiles
            ulong globalProf = Mem.Ptr(vm + Offsets.VM_globalDefaultProfile, false);
            if (globalProf.IsValidVirtualAddress()) foundProfiles.Add(globalProf);

            ulong qualityProf = Mem.Ptr(vm + Offsets.VM_qualityDefaultProfile, false);
            if (qualityProf.IsValidVirtualAddress()) foundProfiles.Add(qualityProf);

            // 3. Components from VolumeManager.m_ComponentsDefaultState (VolumeComponent[] array)
            ulong defaultCompsArr = Mem.Ptr(vm + Offsets.VM_componentsDefaultState, false);
            if (defaultCompsArr.IsValidVirtualAddress())
            {
                int count = Mem.Val<int>(defaultCompsArr + Offsets.Array_length, false);
                if (count > 0 && count < 128)
                {
                    for (int i = 0; i < count; i++)
                    {
                        ulong comp = Mem.Ptr(defaultCompsArr + Offsets.Array_items + (ulong)i * 8, false);
                        if (comp.IsValidVirtualAddress()) foundComponents.Add(comp);
                    }
                }
            }

            // 4. VolumeManager stack components dictionary
            ulong stack = Mem.Ptr(vm + Offsets.VM_stack, false);
            if (stack.IsValidVirtualAddress())
            {
                ulong compDict = Mem.Ptr(stack + Offsets.VolumeStack_components, false);
                if (compDict.IsValidVirtualAddress())
                {
                    ulong entries = Mem.Ptr(compDict + 0x18, false);
                    int count = Mem.Val<int>(compDict + 0x20, false);
                    if (entries.IsValidVirtualAddress() && count > 0 && count < 256)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            ulong entryAddr = entries + Offsets.Array_items + (ulong)i * 24;
                            ulong comp = Mem.Ptr(entryAddr + 16, false);
                            if (comp.IsValidVirtualAddress()) foundComponents.Add(comp);
                        }
                    }
                }
            }

            // Expand all foundProfiles into foundComponents
            foreach (ulong prof in foundProfiles)
            {
                ulong compList = Mem.Ptr(prof + Offsets.VolumeProfile_components, false);
                if (!compList.IsValidVirtualAddress()) continue;

                ulong items = Mem.Ptr(compList + 0x10, false);
                int count = Mem.Val<int>(compList + 0x18, false);
                if (items.IsValidVirtualAddress() && count > 0 && count < 128)
                {
                    for (int j = 0; j < count; j++)
                    {
                        ulong comp = Mem.Ptr(items + Offsets.Array_items + (ulong)j * 8, false);
                        if (comp.IsValidVirtualAddress()) foundComponents.Add(comp);
                    }
                }
            }

            _activeLiftGammaTargets.Clear();
            _activeExposureTargets.Clear();
            _activeColorAdjTargets.Clear();
            _activeHdrpFogTargets.Clear();

            foreach (ulong comp in foundComponents)
            {
                string name = ClassName(comp);
                if (name == "LiftGammaGain")
                {
                    ulong gammaParam = Mem.Ptr(comp + Offsets.LiftGammaGain_gamma, false);
                    ulong gainParam = Mem.Ptr(comp + Offsets.LiftGammaGain_gain, false);
                    if (gammaParam.IsValidVirtualAddress() && gainParam.IsValidVirtualAddress())
                    {
                        if (!_origLiftGammaDict.ContainsKey(comp))
                        {
                            bool origGammaOver = Mem.Val<bool>(gammaParam + Offsets.VolumeParameter_overrideState, false);
                            Vector4 origGammaVal = Mem.Val<Vector4>(gammaParam + Offsets.Vector4Parameter_value, false);
                            bool origGainOver = Mem.Val<bool>(gainParam + Offsets.VolumeParameter_overrideState, false);
                            Vector4 origGainVal = Mem.Val<Vector4>(gainParam + Offsets.Vector4Parameter_value, false);

                            if (BrightnessEnabled)
                            {
                                origGammaOver = false;
                                origGammaVal = new Vector4(1f, 1f, 1f, 0f);
                                origGainOver = false;
                                origGainVal = new Vector4(1f, 1f, 1f, 0f);
                            }

                            _origLiftGammaDict[comp] = new OrigLiftGammaGain
                            {
                                OrigGammaOverride = origGammaOver,
                                OrigGammaValue = origGammaVal,
                                OrigGainOverride = origGainOver,
                                OrigGainValue = origGainVal
                            };
                        }

                        _activeLiftGammaTargets.Add(new HdrpLiftGammaGainTarget
                        {
                            GammaOverrideAddr = gammaParam + Offsets.VolumeParameter_overrideState,
                            GammaValueAddr = gammaParam + Offsets.Vector4Parameter_value,
                            GainOverrideAddr = gainParam + Offsets.VolumeParameter_overrideState,
                            GainValueAddr = gainParam + Offsets.Vector4Parameter_value
                        });
                    }
                }
                else if (name == "Exposure")
                {
                    ulong compParam = Mem.Ptr(comp + Offsets.Exposure_compensation, false);
                    if (compParam.IsValidVirtualAddress())
                    {
                        if (!_origExposureDict.ContainsKey(comp))
                        {
                            bool origOver = Mem.Val<bool>(compParam + Offsets.VolumeParameter_overrideState, false);
                            float origVal = Mem.Val<float>(compParam + Offsets.FloatParameter_value, false);

                            if (BrightnessEnabled || Math.Abs(origVal - BrightnessEv) < 0.05f)
                            {
                                origOver = false;
                                origVal = 0.0f;
                            }

                            _origExposureDict[comp] = new OrigFloatParam
                            {
                                OrigOverride = origOver,
                                OrigValue = origVal
                            };
                        }

                        _activeExposureTargets.Add(new HdrpFloatParamTarget
                        {
                            OverrideAddr = compParam + Offsets.VolumeParameter_overrideState,
                            ValueAddr = compParam + Offsets.FloatParameter_value
                        });
                    }
                }
                else if (name == "ColorAdjustments")
                {
                    ulong postExp = Mem.Ptr(comp + Offsets.ColorAdjustments_postExposure, false);
                    if (postExp.IsValidVirtualAddress())
                    {
                        if (!_origColorAdjDict.ContainsKey(comp))
                        {
                            bool origOver = Mem.Val<bool>(postExp + Offsets.VolumeParameter_overrideState, false);
                            float origVal = Mem.Val<float>(postExp + Offsets.FloatParameter_value, false);

                            if (BrightnessEnabled || Math.Abs(origVal - BrightnessEv) < 0.05f)
                            {
                                origOver = false;
                                origVal = 0.0f;
                            }

                            _origColorAdjDict[comp] = new OrigFloatParam
                            {
                                OrigOverride = origOver,
                                OrigValue = origVal
                            };
                        }

                        _activeColorAdjTargets.Add(new HdrpFloatParamTarget
                        {
                            OverrideAddr = postExp + Offsets.VolumeParameter_overrideState,
                            ValueAddr = postExp + Offsets.FloatParameter_value
                        });
                    }
                }
                else if (name == "Fog")
                {
                    ulong enabledParam = Mem.Ptr(comp + Offsets.Fog_enabled, false);
                    ulong meanFreeParam = Mem.Ptr(comp + Offsets.Fog_meanFreePath, false);
                    if (enabledParam.IsValidVirtualAddress() && meanFreeParam.IsValidVirtualAddress())
                    {
                        if (!_origHdrpFogDict.ContainsKey(comp))
                        {
                            _origHdrpFogDict[comp] = new OrigFogState
                            {
                                OrigEnabledOverride = Mem.Val<bool>(enabledParam + Offsets.VolumeParameter_overrideState, false),
                                OrigEnabledValue = Mem.Val<bool>(enabledParam + Offsets.BoolParameter_value, false),
                                OrigMeanFreeOverride = Mem.Val<bool>(meanFreeParam + Offsets.VolumeParameter_overrideState, false),
                                OrigMeanFreePath = Mem.Val<float>(meanFreeParam + Offsets.FloatParameter_value, false)
                            };
                        }

                        _activeHdrpFogTargets.Add(new HdrpFogTarget
                        {
                            EnabledOverrideAddr = enabledParam + Offsets.VolumeParameter_overrideState,
                            EnabledValueAddr = enabledParam + Offsets.BoolParameter_value,
                            MeanFreeOverrideAddr = meanFreeParam + Offsets.VolumeParameter_overrideState,
                            MeanFreePathValueAddr = meanFreeParam + Offsets.FloatParameter_value
                        });
                    }
                }
            }

            _brightnessApplied = false;
            _hdrpFogApplied = false;
        }

        // ═══════════════════════════════════════════════════════════
        //  LESS FOG / SMOKE & NO FOG (CustomRendering.FogController & HDRP)
        // ═══════════════════════════════════════════════════════════
        public static void PollLessFog()
        {
            lock (_lessFogLock)
            {
                if (!MasterMemWritesEnabled || !LessFogEnabled)
                {
                    TryRestoreLessFog();
                    LessFogStatus = "Off";
                    return;
                }

                // Throttle fog checks and writes to ~10 Hz (every 100ms) to eliminate PCIe bus saturation
                long now = Stopwatch.GetTimestamp();
                if ((double)(now - _lastLessFogTicks) / Stopwatch.Frequency < 0.1)
                {
                    return;
                }
                _lastLessFogTicks = now;

                // 1. FogControl player status effect (prevent decontamination blindness when atmospheric fog eliminated)
                if (FullNoFogEnabled && _cachedFogControlEffect != 0)
                {
                    byte fogInt = Mem.Val<byte>(_cachedFogControlEffect + Offsets.SEB_intensity, false);
                    if (fogInt > 0)
                    {
                        Mem.TryWriteValue<byte>(_cachedFogControlEffect + Offsets.SEB_intensity, 0);
                        Mem.TryWriteValue<float>(_cachedFogControlEffect + Offsets.SEB_timeLeft, 0.0f);
                    }
                }

                // 2. Atmospheric Fog Elimination (CustomRendering.FogController & FogSetting)
                if (FullNoFogEnabled)
                {
                    if (!_fogSettingsApplied || !_lastAppliedFullNoFog)
                    {
                        ulong sfFog = StaticFields(Offsets.FogController_TypeInfo);
                        if (sfFog.IsValidVirtualAddress())
                        {
                            ulong singleton = Mem.Ptr(sfFog + Offsets.FogController_static_Singleton, false);
                            if (singleton.IsValidVirtualAddress())
                            {
                                ulong fogSettingsArr = Mem.Ptr(singleton + Offsets.FogController_fogSettings, false);
                                if (fogSettingsArr.IsValidVirtualAddress())
                                {
                                    int count = Mem.Val<int>(fogSettingsArr + Offsets.Array_length, false);
                                    if (count > 0 && count < 32)
                                    {
                                        for (int i = 0; i < count; i++)
                                        {
                                            ulong setting = Mem.Ptr(fogSettingsArr + Offsets.Array_items + (ulong)i * 8, false);
                                            if (!setting.IsValidVirtualAddress()) continue;

                                            if (!_origFogSettings.ContainsKey(setting))
                                            {
                                                float sStart = Mem.Val<float>(setting + Offsets.FogSetting_startDistance, false);
                                                float sEnd = Mem.Val<float>(setting + Offsets.FogSetting_endDistance, false);
                                                float sWeight = Mem.Val<float>(setting + Offsets.FogSetting_weight, false);
                                                bool sEnabled = Mem.Val<bool>(setting + Offsets.FogSetting_isEnabled, false);
                                                _origFogSettings[setting] = (sStart, sEnd, sWeight, sEnabled);
                                            }

                                            Mem.TryWriteValue<float>(setting + Offsets.FogSetting_startDistance, 10000.0f);
                                            Mem.TryWriteValue<float>(setting + Offsets.FogSetting_endDistance, 20000.0f);
                                            Mem.TryWriteValue<float>(setting + Offsets.FogSetting_weight, 0.0f);
                                            Mem.TryWriteValue<bool>(setting + Offsets.FogSetting_isEnabled, false);
                                        }
                                    }
                                }

                                ulong customFog = Mem.Ptr(singleton + Offsets.FogController_fogEffect, false);
                                if (customFog.IsValidVirtualAddress())
                                {
                                    if (!_customFogCaptured)
                                    {
                                        _origCustomFogFade = Mem.Val<float>(customFog + Offsets.CustomFog_fadeIntensity, false);
                                        _origCustomFogStart = Mem.Val<float>(customFog + Offsets.CustomFog_startDistance, false);
                                        _origCustomFogEnd = Mem.Val<float>(customFog + Offsets.CustomFog_endDistance, false);
                                        _customFogCaptured = true;
                                    }

                                    Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_fadeIntensity, 0.0f);
                                    Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_startDistance, 10000.0f);
                                    Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_endDistance, 20000.0f);
                                }
                            }
                        }
                        _fogSettingsApplied = true;
                    }

                    // 3. HDRP Volumetric Fog
                    if (_activeHdrpFogTargets.Count == 0)
                    {
                        ScanHdrpVolumeComponents();
                    }

                    if (!_hdrpFogApplied || !_lastAppliedFullNoFog)
                    {
                        for (int i = 0; i < _activeHdrpFogTargets.Count; i++)
                        {
                            var t = _activeHdrpFogTargets[i];
                            Mem.TryWriteValue<bool>(t.EnabledOverrideAddr, true);
                            Mem.TryWriteValue<bool>(t.EnabledValueAddr, false);
                            Mem.TryWriteValue<bool>(t.MeanFreeOverrideAddr, true);
                            Mem.TryWriteValue<float>(t.MeanFreePathValueAddr, 100000.0f);
                        }
                        _hdrpFogApplied = true;
                    }
                }
                else if (_lastAppliedFullNoFog)
                {
                    Interlocked.Increment(ref _restoringCounter);
                    try
                    {
                        TryRestoreAtmosphericFog();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _restoringCounter);
                    }
                }

                _lastAppliedFullNoFog = FullNoFogEnabled;

                // 4. SCP-244 Ice Mist Suppression & Tracking (write only on drift)
                if (LessFogScp244Enabled)
                {
                    ulong sf244 = StaticFields(Offsets.Scp244DeployablePickup_TypeInfo);
                    if (sf244.IsValidVirtualAddress())
                    {
                        ulong instances = Mem.Ptr(sf244 + Offsets.Scp244_static_Instances, false);
                        if (instances.IsValidVirtualAddress())
                        {
                            ulong slots = Mem.Ptr(instances + Offsets.HashSet_slots, false);
                            int lastIdx = Mem.Val<int>(instances + Offsets.HashSet_lastIndex, false);
                            if (slots.IsValidVirtualAddress() && lastIdx > 0 && lastIdx < 64)
                            {
                                for (int i = 0; i < lastIdx; i++)
                                {
                                    ulong slotAddr = slots + Offsets.Array_items + (ulong)i * Offsets.HashSetSlot_size;
                                    int hash = Mem.Val<int>(slotAddr + Offsets.HashSetSlot_hashCode, false);
                                    if (hash < 0) continue;
                                    ulong pickup = Mem.Ptr(slotAddr + Offsets.HashSetSlot_value, false);
                                    if (!pickup.IsValidVirtualAddress()) continue;

                                    if (!_origScp244Dict.ContainsKey(pickup))
                                    {
                                        float origSize = Mem.Val<float>(pickup + Offsets.Scp244_CurrentSizePercent, false);
                                        float origDiam = Mem.Val<float>(pickup + Offsets.Scp244_MaxDiameter, false);
                                        _origScp244Dict[pickup] = (origSize, origDiam);
                                    }

                                    float targetSize = 0.20f;
                                    float targetDiam = 3.5f;
                                    float curSize = Mem.Val<float>(pickup + Offsets.Scp244_CurrentSizePercent, false);
                                    if (Math.Abs(curSize - targetSize) > 0.02f)
                                    {
                                        Mem.TryWriteValue<float>(pickup + Offsets.Scp244_CurrentSizePercent, targetSize);
                                        Mem.TryWriteValue<float>(pickup + Offsets.Scp244_MaxDiameter, targetDiam);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_origScp244Dict.Count > 0)
                {
                    Interlocked.Increment(ref _restoringCounter);
                    try
                    {
                        TryRestoreScp244();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _restoringCounter);
                    }
                }

                if (FullNoFogEnabled && LessFogScp244Enabled)
                    LessFogStatus = "Active (Atmospheric & SCP-244 Mist Eliminated)";
                else if (FullNoFogEnabled)
                    LessFogStatus = "Active (Atmospheric Fog Eliminated)";
                else if (LessFogScp244Enabled)
                    LessFogStatus = "Active (SCP-244 Mist Thinned)";
                else
                    LessFogStatus = "Enabled (Select options below)";
            }
        }

        private static void TryRestoreAtmosphericFog()
        {
            foreach (var kvp in _origFogSettings)
            {
                ulong setting = kvp.Key;
                var orig = kvp.Value;
                if (setting.IsValidVirtualAddress())
                {
                    Mem.TryWriteValue<float>(setting + Offsets.FogSetting_startDistance, orig.start);
                    Mem.TryWriteValue<float>(setting + Offsets.FogSetting_endDistance, orig.end);
                    Mem.TryWriteValue<float>(setting + Offsets.FogSetting_weight, orig.weight);
                    Mem.TryWriteValue<bool>(setting + Offsets.FogSetting_isEnabled, orig.isEnabled);
                }
            }
            _origFogSettings.Clear();

            if (_customFogCaptured)
            {
                ulong sfFog = StaticFields(Offsets.FogController_TypeInfo);
                if (sfFog.IsValidVirtualAddress())
                {
                    ulong singleton = Mem.Ptr(sfFog + Offsets.FogController_static_Singleton, false);
                    if (singleton.IsValidVirtualAddress())
                    {
                        ulong customFog = Mem.Ptr(singleton + Offsets.FogController_fogEffect, false);
                        if (customFog.IsValidVirtualAddress())
                        {
                            Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_fadeIntensity, _origCustomFogFade);
                            Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_startDistance, _origCustomFogStart);
                            Mem.TryWriteValue<float>(customFog + Offsets.CustomFog_endDistance, _origCustomFogEnd);
                        }
                    }
                }
                _customFogCaptured = false;
            }

            foreach (var kvp in _origHdrpFogDict)
            {
                ulong comp = kvp.Key;
                var orig = kvp.Value;
                ulong enabledParam = Mem.Ptr(comp + Offsets.Fog_enabled, false);
                ulong meanFreeParam = Mem.Ptr(comp + Offsets.Fog_meanFreePath, false);
                if (enabledParam.IsValidVirtualAddress())
                {
                    Mem.TryWriteValue<bool>(enabledParam + Offsets.VolumeParameter_overrideState, orig.OrigEnabledOverride);
                    Mem.TryWriteValue<bool>(enabledParam + Offsets.BoolParameter_value, orig.OrigEnabledValue);
                }
                if (meanFreeParam.IsValidVirtualAddress())
                {
                    Mem.TryWriteValue<bool>(meanFreeParam + Offsets.VolumeParameter_overrideState, orig.OrigMeanFreeOverride);
                    Mem.TryWriteValue<float>(meanFreeParam + Offsets.FloatParameter_value, orig.OrigMeanFreePath);
                }
            }
            _origHdrpFogDict.Clear();
            _activeHdrpFogTargets.Clear();

            _fogSettingsApplied = false;
            _hdrpFogApplied = false;
            _lastAppliedFullNoFog = false;
        }

        private static void TryRestoreScp244()
        {
            foreach (var kvp in _origScp244Dict)
            {
                ulong pickup = kvp.Key;
                var orig = kvp.Value;
                if (pickup.IsValidVirtualAddress())
                {
                    Mem.TryWriteValue<float>(pickup + Offsets.Scp244_CurrentSizePercent, orig.size);
                    Mem.TryWriteValue<float>(pickup + Offsets.Scp244_MaxDiameter, orig.diameter);
                }
            }
            _origScp244Dict.Clear();
        }

        private static void TryRestoreLessFog()
        {
            Interlocked.Increment(ref _restoringCounter);
            try
            {
                TryRestoreAtmosphericFog();
                TryRestoreScp244();
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void RestoreLessFog()
        {
            lock (_lessFogLock)
            {
                TryRestoreLessFog();
                LessFogStatus = "Off";
            }
        }

        public static void PollNoSmoke() => PollLessFog();
        public static void RestoreNoSmoke() => RestoreLessFog();

        // ═══════════════════════════════════════════════════════════
        //  GUN FLASHLIGHT ENHANCEMENT
        // ═══════════════════════════════════════════════════════════
        private static void ClearActiveFlashlightState()
        {
            _activeFlashlightViewmodelObject = 0;
            _activeFlashlightLightSource = 0;
            _activeFlashlightHdLightData = 0;
            _activeFlashlightNativeLight = 0;
            _addrFadeDistance = 0;
            _addrLightDimmer = 0;
            _addrIntensity = 0;
            _addrInnerSpotPercent = 0;
            _addrVolumetricDimmer = 0;
            _addrVolumetricFadeDistance = 0;
            _addrNativeIntensity = 0;
            _addrNativeRange = 0;
            _addrNativeSpotAngle = 0;
            _addrNativeInnerSpotAngle = 0;
            _origFadeDistance = 0;
            _origLightDimmer = 0;
            _origIntensity = 0;
            _origInnerSpotPercent = 0;
            _origVolumetricDimmer = 0;
            _origVolumetricFadeDistance = 0;
            _origNativeIntensity = 0;
            _origNativeRange = 0;
            _origNativeSpotAngle = 0;
            _origNativeInnerSpotAngle = 0;
            _activeFlashlightOriginalCaptured = false;
            _flashlightApplied = false;
        }

        public static bool TryRestoreGunFlashlight()
        {
            if (!_activeFlashlightOriginalCaptured)
            {
                ClearActiveFlashlightState();
                return true;
            }

            Interlocked.Increment(ref _restoringCounter);
            try
            {
                if (_addrLightDimmer != 0)
                    Mem.TryWriteValue<float>(_addrLightDimmer, _origLightDimmer);
                if (_addrFadeDistance != 0)
                    Mem.TryWriteValue<float>(_addrFadeDistance, _origFadeDistance);
                if (_addrIntensity != 0 && _origIntensity > 0f)
                    Mem.TryWriteValue<float>(_addrIntensity, _origIntensity);
                if (_addrInnerSpotPercent != 0)
                    Mem.TryWriteValue<float>(_addrInnerSpotPercent, _origInnerSpotPercent);
                if (_addrVolumetricDimmer != 0)
                    Mem.TryWriteValue<float>(_addrVolumetricDimmer, _origVolumetricDimmer);
                if (_addrVolumetricFadeDistance != 0)
                    Mem.TryWriteValue<float>(_addrVolumetricFadeDistance, _origVolumetricFadeDistance);
                if (_addrNativeIntensity != 0 && _origNativeIntensity > 0f)
                    Mem.TryWriteValue<float>(_addrNativeIntensity, _origNativeIntensity);
                if (_addrNativeRange != 0 && _origNativeRange > 0f)
                    Mem.TryWriteValue<float>(_addrNativeRange, _origNativeRange);
                if (_addrNativeSpotAngle != 0 && _origNativeSpotAngle > 0f)
                    Mem.TryWriteValue<float>(_addrNativeSpotAngle, _origNativeSpotAngle);
                if (_addrNativeInnerSpotAngle != 0 && _origNativeInnerSpotAngle > 0f)
                    Mem.TryWriteValue<float>(_addrNativeInnerSpotAngle, _origNativeInnerSpotAngle);

                ClearActiveFlashlightState();
                GunFlashlightStatus = "Off";
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void RestoreGunFlashlight()
        {
            lock (_gunFlashlightLock)
            {
                TryRestoreGunFlashlight();
            }
        }

        private static bool TryGetManagedComponent(ulong compPtr, out ulong managedObj, out string className)
        {
            managedObj = 0;
            className = string.Empty;
            if (compPtr == 0 || !compPtr.IsValidVirtualAddress()) return false;

            ulong[] candidateOffsets = [0x28, 0x20, 0x30, 0x10];
            for (int i = 0; i < candidateOffsets.Length; i++)
            {
                ulong candidate = Mem.Ptr(compPtr + candidateOffsets[i], false);
                if (candidate == 0 || !candidate.IsValidVirtualAddress()) continue;

                string name = ClassName(candidate);
                if (!string.IsNullOrEmpty(name))
                {
                    managedObj = candidate;
                    className = name;
                    return true;
                }
            }
            return false;
        }

        private static void TryScanGameObjectForLight(
            ulong nativeGo,
            ref ulong flashlightExt,
            ref ulong lightSource,
            ref ulong hdLightData,
            ref ulong nativeLight)
        {
            if (nativeGo == 0 || !nativeGo.IsValidVirtualAddress()) return;

            ulong compArrayBase = Mem.Ptr(nativeGo + 0x58, false);
            ulong compSize = Mem.Val<ulong>(nativeGo + 0x68, false);
            if (compSize == 0 || compSize > 64 || !compArrayBase.IsValidVirtualAddress()) return;

            for (int c = 0; c < (int)compSize; c++)
            {
                ulong compPtr = Mem.Ptr(compArrayBase + (ulong)c * 16 + 8, false);
                if (compPtr == 0 || !compPtr.IsValidVirtualAddress()) continue;

                if (TryGetManagedComponent(compPtr, out ulong managedComp, out string compName))
                {
                    if (flashlightExt == 0 && (compName.IndexOf("FlashlightExtension", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (compName.IndexOf("Flashlight", StringComparison.OrdinalIgnoreCase) >= 0 && compName.IndexOf("Attachment", StringComparison.OrdinalIgnoreCase) < 0)))
                    {
                        flashlightExt = managedComp;
                        ulong ls = Mem.Ptr(managedComp + 0x40, false);
                        if (ls == 0 || !ls.IsValidVirtualAddress())
                        {
                            TryReadObjectField(managedComp, "_lightSource", out ls);
                        }
                        if (ls != 0 && ls.IsValidVirtualAddress())
                        {
                            lightSource = ls;
                            nativeLight = Mem.Ptr(ls + 0x10, false);
                        }
                    }
                    else if (hdLightData == 0 && compName.IndexOf("HDAdditionalLightData", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hdLightData = managedComp;
                    }
                    else if (compName.Equals("Light", StringComparison.OrdinalIgnoreCase))
                    {
                        if (lightSource == 0) lightSource = managedComp;
                        if (nativeLight == 0) nativeLight = compPtr;
                    }
                }
            }
        }

        private static ulong FindHDLightDataFromNative(ulong nativeLight, ref ulong lightSource)
        {
            if (nativeLight == 0 || !nativeLight.IsValidVirtualAddress()) return 0;

            // In Unity, nativeLight + 0x18 points directly to an array of managed component pointers on this GameObject
            ulong compArr = Mem.Ptr(nativeLight + 0x18, false);
            if (compArr != 0 && compArr.IsValidVirtualAddress())
            {
                Span<ulong> compPtrs = stackalloc ulong[64];
                DmaMemory.ReadBuffer<ulong>(compArr, compPtrs, false);
                ulong foundHd = 0;
                for (int i = 0; i < compPtrs.Length; i++)
                {
                    ulong comp = compPtrs[i];
                    if (comp == 0 || !comp.IsValidVirtualAddress()) continue;
                    string cName = ClassName(comp);
                    if (foundHd == 0 && cName.IndexOf("HDAdditionalLightData", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundHd = comp;
                    }
                    if (lightSource == 0 && cName.Equals("Light", StringComparison.OrdinalIgnoreCase))
                    {
                        lightSource = comp;
                    }
                    if (foundHd != 0 && lightSource != 0)
                        return foundHd;
                }
                if (foundHd != 0) return foundHd;
            }

            // Inspect candidate pointers on nativeLight: 0x20 (native GameObject), 0x28, 0x30, 0x58
            ulong[] candOffsets = [0x20, 0x28, 0x30, 0x58];
            Span<ulong> subPtrs = stackalloc ulong[32];
            for (int o = 0; o < candOffsets.Length; o++)
            {
                ulong cand = Mem.Ptr(nativeLight + candOffsets[o], false);
                if (cand == 0 || !cand.IsValidVirtualAddress()) continue;

                string candName = ClassName(cand);
                if (candName.IndexOf("HDAdditionalLightData", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return cand;
                }

                // Check if candidate points to a component array at +0x18 or +0x58
                ulong[] subOffsets = [0x18, 0x58];
                for (int s = 0; s < subOffsets.Length; s++)
                {
                    ulong subArr = Mem.Ptr(cand + subOffsets[s], false);
                    if (subArr == 0 || !subArr.IsValidVirtualAddress()) continue;

                    DmaMemory.ReadBuffer<ulong>(subArr, subPtrs, false);
                    for (int i = 0; i < subPtrs.Length; i++)
                    {
                        ulong subComp = subPtrs[i];
                        if (subComp == 0 || !subComp.IsValidVirtualAddress()) continue;
                        string subName = ClassName(subComp);
                        if (subName.IndexOf("HDAdditionalLightData", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return subComp;
                        }
                    }
                }
            }

            return 0;
        }

        public static bool TryResolveGunFlashlight(
            ulong localHub,
            ulong equippedItem,
            out ulong viewmodel,
            out ulong lightSource,
            out ulong hdLightData,
            out ulong nativeLight,
            out string failure)
        {
            viewmodel = 0;
            lightSource = 0;
            hdLightData = 0;
            nativeLight = 0;
            failure = "No flashlight found";

            if (localHub == 0 || !localHub.IsValidVirtualAddress())
            {
                failure = "Waiting for player";
                return false;
            }

            if (equippedItem == 0 || !equippedItem.IsValidVirtualAddress())
            {
                failure = "No equipped weapon";
                return false;
            }

            ulong flashlightExt = 0;

            // Check if equippedItem is a standalone handheld flashlight (FlashlightItem)
            string itemKlass = ClassName(equippedItem);
            if (itemKlass.IndexOf("FlashlightItem", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (TryReadObjectField(equippedItem, "_lightSource", out lightSource) ||
                    (lightSource = Mem.Ptr(equippedItem + 0x98, false)) != 0)
                {
                    if (lightSource.IsValidVirtualAddress())
                    {
                        nativeLight = Mem.Ptr(lightSource + 0x10, false);
                    }
                }
            }

            // Resolve Viewmodel
            if (!TryReadObjectField(equippedItem, "ViewModel", out viewmodel) || viewmodel == 0 || !viewmodel.IsValidVirtualAddress())
            {
                viewmodel = Mem.Ptr(equippedItem + 0x30, false);
            }

            if (viewmodel != 0 && viewmodel.IsValidVirtualAddress())
            {
                ulong viewmodelKlass = Mem.Ptr(viewmodel, false);
                string vmKlassName = ClassName(viewmodel);

                // Standalone Flashlight Viewmodel check
                if (vmKlassName.IndexOf("FlashlightViewmodel", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (lightSource == 0)
                    {
                        if (TryReadObjectField(viewmodel, "_light", out lightSource) ||
                            (lightSource = Mem.Ptr(viewmodel + 0x80, false)) != 0)
                        {
                            if (lightSource.IsValidVirtualAddress())
                            {
                                nativeLight = Mem.Ptr(lightSource + 0x10, false);
                            }
                        }
                    }
                }

                // Tier 1: Scan Viewmodel Attachment GameObject Groups (<Attachments>k__BackingField at 0xC0)
                ulong attGroupsOffset = 0;
                if (!TryGetFieldOffset(viewmodelKlass, "<Attachments>k__BackingField", out attGroupsOffset) &&
                    !TryGetFieldOffset(viewmodelKlass, "_attachments", out attGroupsOffset))
                {
                    attGroupsOffset = 0xC0;
                }

                ulong attGroupsArr = Mem.Ptr(viewmodel + attGroupsOffset, false);
                if (attGroupsArr == 0 && attGroupsOffset != 0xC0)
                {
                    attGroupsArr = Mem.Ptr(viewmodel + 0xC0, false);
                }

                if (attGroupsArr != 0 && attGroupsArr.IsValidVirtualAddress())
                {
                    int groupCount = Mem.Val<int>(attGroupsArr + Offsets.Array_max_length, false);
                    if (groupCount > 0 && groupCount <= 64)
                    {
                        for (int g = 0; g < groupCount; g++)
                        {
                            // AttachmentGameObjectGroup struct at offset Array_items + g * 8
                            // Field 0x0 is GameObject[] Group
                            ulong groupArr = Mem.Ptr(attGroupsArr + Offsets.Array_items + (ulong)g * 8, false);
                            if (groupArr == 0 || !groupArr.IsValidVirtualAddress()) continue;

                            int goCount = Mem.Val<int>(groupArr + Offsets.Array_max_length, false);
                            if (goCount <= 0 || goCount > 64) continue;

                            for (int j = 0; j < goCount; j++)
                            {
                                ulong managedGo = Mem.Ptr(groupArr + Offsets.Array_items + (ulong)j * 8, false);
                                if (managedGo == 0 || !managedGo.IsValidVirtualAddress()) continue;

                                ulong nativeGo = Mem.Ptr(managedGo + 0x10, false);
                                if (nativeGo == 0 || !nativeGo.IsValidVirtualAddress()) continue;

                                TryScanGameObjectForLight(nativeGo, ref flashlightExt, ref lightSource, ref hdLightData, ref nativeLight);

                                if (flashlightExt != 0 && hdLightData != 0 && nativeLight != 0)
                                    break;
                            }

                            if (flashlightExt != 0 && hdLightData != 0 && nativeLight != 0)
                                break;
                        }
                    }
                }

                // Tier 2: Scan Viewmodel Extensions (<Extensions>k__BackingField at 0xB8)
                if (flashlightExt == 0 || lightSource == 0)
                {
                    ulong extsOffset = 0;
                    if (!TryGetFieldOffset(viewmodelKlass, "<Extensions>k__BackingField", out extsOffset) &&
                        !TryGetFieldOffset(viewmodelKlass, "_extensions", out extsOffset))
                    {
                        extsOffset = 0xB8;
                    }

                    ulong extsArr = Mem.Ptr(viewmodel + extsOffset, false);
                    if (extsArr == 0 && extsOffset != 0xB8)
                    {
                        extsArr = Mem.Ptr(viewmodel + 0xB8, false);
                    }

                    if (extsArr != 0 && extsArr.IsValidVirtualAddress())
                    {
                        int len = Mem.Val<int>(extsArr + Offsets.Array_max_length, false);
                        if (len > 0 && len <= 64)
                        {
                            for (int i = 0; i < len; i++)
                            {
                                ulong ext = Mem.Ptr(extsArr + Offsets.Array_items + (ulong)i * 8, false);
                                if (ext == 0 || !ext.IsValidVirtualAddress()) continue;

                                string name = ClassName(ext);
                                if (name.IndexOf("Flashlight", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    flashlightExt = ext;
                                    ulong ls = Mem.Ptr(ext + 0x40, false);
                                    if (ls != 0 && ls.IsValidVirtualAddress())
                                    {
                                        lightSource = ls;
                                        nativeLight = Mem.Ptr(ls + 0x10, false);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Tier 3: Scan Firearm Item Attachments (_attachments at 0x118)
            if (flashlightExt == 0 || lightSource == 0)
            {
                ulong attOffset = 0x118;
                ulong attArr = Mem.Ptr(equippedItem + attOffset, false);
                if (attArr != 0 && attArr.IsValidVirtualAddress())
                {
                    int attLen = Mem.Val<int>(attArr + Offsets.Array_max_length, false);
                    if (attLen > 0 && attLen <= 32)
                    {
                        for (int a = 0; a < attLen; a++)
                        {
                            ulong att = Mem.Ptr(attArr + Offsets.Array_items + (ulong)a * 8, false);
                            if (att == 0 || !att.IsValidVirtualAddress()) continue;

                            string attClass = ClassName(att);
                            if (attClass.IndexOf("Flashlight", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                ulong nativeAtt = Mem.Ptr(att + 0x10, false);
                                if (nativeAtt.IsValidVirtualAddress())
                                {
                                    ulong[] goOffsets = [0x58, 0x30, 0x28, 0x18];
                                    for (int o = 0; o < goOffsets.Length; o++)
                                    {
                                        ulong nativeGo = Mem.Ptr(nativeAtt + goOffsets[o], false);
                                        if (nativeGo.IsValidVirtualAddress())
                                        {
                                            TryScanGameObjectForLight(nativeGo, ref flashlightExt, ref lightSource, ref hdLightData, ref nativeLight);
                                            if (flashlightExt != 0 || lightSource != 0) break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // If we found flashlightExt but lightSource is still 0, try reading _lightSource field directly
            if (flashlightExt != 0 && (lightSource == 0 || !lightSource.IsValidVirtualAddress()))
            {
                ulong ls = Mem.Ptr(flashlightExt + 0x40, false);
                if (ls == 0 || !ls.IsValidVirtualAddress())
                {
                    TryReadObjectField(flashlightExt, "_lightSource", out ls);
                }
                if (ls != 0 && ls.IsValidVirtualAddress())
                {
                    lightSource = ls;
                    nativeLight = Mem.Ptr(ls + 0x10, false);
                }
            }

            // If we have lightSource, resolve nativeLight
            if (lightSource != 0 && lightSource.IsValidVirtualAddress() && (nativeLight == 0 || !nativeLight.IsValidVirtualAddress()))
            {
                nativeLight = Mem.Ptr(lightSource + 0x10, false);
            }

            // If we have nativeLight, resolve hdLightData directly from the native component array (nativeLight + 0x18)
            if (hdLightData == 0 && nativeLight != 0 && nativeLight.IsValidVirtualAddress())
            {
                hdLightData = FindHDLightDataFromNative(nativeLight, ref lightSource);
            }

            // Secondary search if hdLightData is still 0
            if (hdLightData == 0)
            {
                if (nativeLight != 0 && nativeLight.IsValidVirtualAddress())
                {
                    ulong[] goOffsets = [0x20, 0x58, 0x30, 0x28, 0x18];
                    for (int o = 0; o < goOffsets.Length; o++)
                    {
                        ulong nativeGo = Mem.Ptr(nativeLight + goOffsets[o], false);
                        if (nativeGo.IsValidVirtualAddress())
                        {
                            TryScanGameObjectForLight(nativeGo, ref flashlightExt, ref lightSource, ref hdLightData, ref nativeLight);
                            if (hdLightData != 0) break;
                        }
                    }
                }

                if (hdLightData == 0 && flashlightExt != 0)
                {
                    if (TryReadObjectField(flashlightExt, "_hdLightData", out ulong hd) ||
                        TryReadObjectField(flashlightExt, "_additionalData", out hd))
                    {
                        if (hd.IsValidVirtualAddress()) hdLightData = hd;
                    }
                }
            }

            if (hdLightData == 0 && nativeLight == 0)
            {
                failure = (flashlightExt == 0) ? "Weapon has no flashlight attachment" : "Light component unresolvable";
                return false;
            }

            return true;
        }

        public static void PollGunFlashlight(ulong localHub = 0, ulong item = 0)
        {
            lock (_gunFlashlightLock)
            {
                if (!MasterMemWritesEnabled || !GunFlashlightEnabled)
                {
                    _lastCheckedFlashlightItem = 0;
                    _lastFlashlightItemHadFlashlight = false;
                    _lastFlashlightFailure = null;
                    _flashlightApplied = false;
                    GunFlashlightStatus = TryRestoreGunFlashlight() ? "Off" : "Restore blocked";
                    return;
                }

                if (localHub == 0) localHub = LocalHub();
                if (localHub == 0)
                {
                    _lastCheckedFlashlightItem = 0;
                    _lastFlashlightItemHadFlashlight = false;
                    _lastFlashlightFailure = null;
                    _flashlightApplied = false;
                    if (!TryRestoreGunFlashlight())
                    {
                        GunFlashlightStatus = "Restore blocked";
                        return;
                    }
                    GunFlashlightStatus = "Waiting for player";
                    return;
                }

                if (item == 0)
                {
                    if (!TryReadObjectField(localHub, "inventory", out ulong inventory))
                    {
                        _lastCheckedFlashlightItem = 0;
                        _lastFlashlightItemHadFlashlight = false;
                        _lastFlashlightFailure = null;
                        _flashlightApplied = false;
                        if (!TryRestoreGunFlashlight())
                        {
                            GunFlashlightStatus = "Restore blocked";
                            return;
                        }
                        GunFlashlightStatus = "Inventory unavailable";
                        return;
                    }
                    TryReadObjectField(inventory, "_curInstance", out item);
                }

                long now = Stopwatch.GetTimestamp();

                // If resolution previously failed on this item, retry every 500ms instead of permanently locking out
                if (item == _lastCheckedFlashlightItem && !_lastFlashlightItemHadFlashlight && _activeFlashlightViewmodelObject == 0)
                {
                    if ((double)(now - _lastFlashlightRetryTicks) / Stopwatch.Frequency < 0.5)
                    {
                        GunFlashlightStatus = item == 0 ? "No equipped item" : (_lastFlashlightFailure ?? "Weapon has no flashlight attachment");
                        return;
                    }
                    _lastFlashlightRetryTicks = now;
                }

                if (_flashlightApplied && item == _lastCheckedFlashlightItem && _activeFlashlightViewmodelObject != 0)
                {
                    if ((double)(now - _lastFlashlightVerifyTicks) / Stopwatch.Frequency < 0.1)
                        return;
                }

                if (!TryResolveGunFlashlight(
                    localHub,
                    item,
                    out ulong viewmodel,
                    out ulong lightSource,
                    out ulong hdLightData,
                    out ulong nativeLight,
                    out string failure))
                {
                    _lastCheckedFlashlightItem = item;
                    _lastFlashlightItemHadFlashlight = false;
                    _lastFlashlightFailure = failure;
                    _flashlightApplied = false;
                    if (!TryRestoreGunFlashlight())
                    {
                        GunFlashlightStatus = "Restore blocked";
                        return;
                    }
                    GunFlashlightStatus = failure;
                    return;
                }

                _lastCheckedFlashlightItem = item;
                _lastFlashlightItemHadFlashlight = true;
                _lastFlashlightFailure = null;

                if (_activeFlashlightViewmodelObject != viewmodel ||
                    _activeFlashlightLightSource != lightSource ||
                    _activeFlashlightHdLightData != hdLightData ||
                    _activeFlashlightNativeLight != nativeLight)
                {
                    if (!TryRestoreGunFlashlight())
                    {
                        GunFlashlightStatus = "Restore blocked";
                        return;
                    }

                    _activeFlashlightViewmodelObject = viewmodel;
                    _activeFlashlightLightSource = lightSource;
                    _activeFlashlightHdLightData = hdLightData;
                    _activeFlashlightNativeLight = nativeLight;

                    if (hdLightData != 0)
                    {
                        ulong hdKlass = Mem.Ptr(hdLightData, false);
                        ulong dimmerOffset = 0x48;
                        if (TryGetFieldOffset(hdKlass, "m_LightDimmer", out ulong dOff) ||
                            TryGetFieldOffset(hdKlass, "lightDimmer", out dOff))
                            dimmerOffset = dOff;

                        ulong fadeDistOffset = 0x50;
                        if (TryGetFieldOffset(hdKlass, "m_FadeDistance", out ulong fOff) ||
                            TryGetFieldOffset(hdKlass, "fadeDistance", out fOff))
                            fadeDistOffset = fOff;

                        ulong intensityOffset = 0x38;
                        if (TryGetFieldOffset(hdKlass, "m_Intensity", out ulong iOff) ||
                            TryGetFieldOffset(hdKlass, "intensity", out iOff) ||
                            TryGetFieldOffset(hdKlass, "displayLightIntensity", out iOff))
                            intensityOffset = iOff;

                        ulong innerSpotOffset = 0x40;
                        if (TryGetFieldOffset(hdKlass, "m_InnerSpotPercent", out ulong inOff) ||
                            TryGetFieldOffset(hdKlass, "innerSpotPercent", out inOff))
                            innerSpotOffset = inOff;

                        ulong volDimmerOffset = 0x4C;
                        if (TryGetFieldOffset(hdKlass, "m_VolumetricDimmer", out ulong vOff) ||
                            TryGetFieldOffset(hdKlass, "volumetricDimmer", out vOff))
                            volDimmerOffset = vOff;

                        ulong volFadeOffset = 0x54;
                        if (TryGetFieldOffset(hdKlass, "m_VolumetricFadeDistance", out ulong vfOff) ||
                            TryGetFieldOffset(hdKlass, "volumetricFadeDistance", out vfOff))
                            volFadeOffset = vfOff;

                        _addrLightDimmer = hdLightData + dimmerOffset;
                        _addrFadeDistance = hdLightData + fadeDistOffset;
                        _addrIntensity = hdLightData + intensityOffset;
                        _addrInnerSpotPercent = hdLightData + innerSpotOffset;
                        _addrVolumetricDimmer = hdLightData + volDimmerOffset;
                        _addrVolumetricFadeDistance = hdLightData + volFadeOffset;

                        _origLightDimmer = Mem.Val<float>(_addrLightDimmer, false);
                        _origFadeDistance = Mem.Val<float>(_addrFadeDistance, false);
                        _origIntensity = Mem.Val<float>(_addrIntensity, false);
                        _origInnerSpotPercent = Mem.Val<float>(_addrInnerSpotPercent, false);
                        _origVolumetricDimmer = Mem.Val<float>(_addrVolumetricDimmer, false);
                        _origVolumetricFadeDistance = Mem.Val<float>(_addrVolumetricFadeDistance, false);

                        if (_origLightDimmer <= 0.001f || _origLightDimmer > 32.0f) _origLightDimmer = 1.0f;
                        if (_origFadeDistance <= 1.0f || _origFadeDistance > 100000.0f) _origFadeDistance = 10000.0f;
                        if (_origIntensity <= 0.001f || _origIntensity > 100000.0f) _origIntensity = 50.0f;
                        if (_origVolumetricDimmer <= 0.001f || _origVolumetricDimmer > 32.0f) _origVolumetricDimmer = 1.0f;
                        if (_origVolumetricFadeDistance <= 1.0f || _origVolumetricFadeDistance > 100000.0f) _origVolumetricFadeDistance = 10000.0f;
                    }

                    if (nativeLight != 0)
                    {
                        ulong lightData = Mem.Ptr(nativeLight + 0x40, false);
                        if (lightData == 0 || !lightData.IsValidVirtualAddress())
                            lightData = nativeLight + 0x90;

                        _addrNativeIntensity = lightData + 0x38;
                        _addrNativeRange = lightData + 0x3C;
                        _addrNativeSpotAngle = lightData + 0x48;
                        _addrNativeInnerSpotAngle = lightData + 0x4C;

                        _origNativeIntensity = Mem.Val<float>(_addrNativeIntensity, false);
                        _origNativeRange = Mem.Val<float>(_addrNativeRange, false);
                        _origNativeSpotAngle = Mem.Val<float>(_addrNativeSpotAngle, false);
                        _origNativeInnerSpotAngle = Mem.Val<float>(_addrNativeInnerSpotAngle, false);

                        if (_origNativeIntensity <= 0.01f || _origNativeIntensity > 500.0f) _origNativeIntensity = 27.211f;
                        if (_origNativeRange <= 1.0f || _origNativeRange > 500.0f) _origNativeRange = 60.0f;
                        if (_origNativeSpotAngle <= 5.0f || _origNativeSpotAngle > 175.0f) _origNativeSpotAngle = 80.0f;
                        if (_origNativeInnerSpotAngle < 0.1f || _origNativeInnerSpotAngle > 175.0f) _origNativeInnerSpotAngle = 8.0f;
                    }

                    _activeFlashlightOriginalCaptured = true;
                }

                _lastFlashlightVerifyTicks = now;

                if (_addrLightDimmer != 0)
                {
                    float targetDimmer = Math.Clamp(_origLightDimmer * GunFlashlightIntensityMult, 0.1f, 32.0f);
                    Mem.TryWriteValue<float>(_addrLightDimmer, targetDimmer);
                }
                if (_addrIntensity != 0 && _origIntensity > 0f)
                {
                    float targetIntensity = _origIntensity * GunFlashlightIntensityMult;
                    Mem.TryWriteValue<float>(_addrIntensity, targetIntensity);
                }
                if (_addrFadeDistance != 0)
                {
                    float targetFade = Math.Max(_origFadeDistance, GunFlashlightRange * 2f);
                    Mem.TryWriteValue<float>(_addrFadeDistance, targetFade);
                }
                if (_addrInnerSpotPercent != 0)
                {
                    float targetInner = Math.Clamp((GunFlashlightSpotAngle / 120.0f) * 100.0f, 0f, 95f);
                    Mem.TryWriteValue<float>(_addrInnerSpotPercent, targetInner);
                }
                if (_addrVolumetricDimmer != 0)
                {
                    float targetVol = Math.Clamp(_origVolumetricDimmer * GunFlashlightIntensityMult, 0.1f, 32.0f);
                    Mem.TryWriteValue<float>(_addrVolumetricDimmer, targetVol);
                }
                if (_addrVolumetricFadeDistance != 0)
                {
                    float targetVolFade = Math.Max(_origVolumetricFadeDistance, GunFlashlightRange * 2f);
                    Mem.TryWriteValue<float>(_addrVolumetricFadeDistance, targetVolFade);
                }
                if (_addrNativeIntensity != 0 && _origNativeIntensity > 0f)
                {
                    Mem.TryWriteValue<float>(_addrNativeIntensity, _origNativeIntensity * GunFlashlightIntensityMult);
                }
                if (_addrNativeRange != 0)
                {
                    Mem.TryWriteValue<float>(_addrNativeRange, GunFlashlightRange);
                }
                if (_addrNativeSpotAngle != 0)
                {
                    Mem.TryWriteValue<float>(_addrNativeSpotAngle, GunFlashlightSpotAngle);
                }
                if (_addrNativeInnerSpotAngle != 0)
                {
                    float targetInnerAngle = Math.Clamp(GunFlashlightSpotAngle * 0.15f, 1.0f, Math.Max(1.0f, GunFlashlightSpotAngle - 1.0f));
                    Mem.TryWriteValue<float>(_addrNativeInnerSpotAngle, targetInnerAngle);
                }

                _flashlightApplied = true;
                GunFlashlightStatus = $"Active ({GunFlashlightSpotAngle:0}°, {GunFlashlightIntensityMult:0.0}x, {GunFlashlightRange:0}m)";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  INSTANT ADS (AIM-DOWN-SIGHTS)
        // ═══════════════════════════════════════════════════════════
        public static bool TryResolveInstantAds(
            ulong localHub,
            ulong equippedItem,
            out ulong adsExtension,
            out ulong inSpeedAddr,
            out ulong outSpeedAddr,
            out string failure)
        {
            adsExtension = 0;
            inSpeedAddr = 0;
            outSpeedAddr = 0;
            failure = "No firearm equipped";

            if (localHub == 0 || !localHub.IsValidVirtualAddress())
            {
                failure = "Waiting for player";
                return false;
            }

            if (equippedItem == 0 || !equippedItem.IsValidVirtualAddress())
            {
                failure = "No equipped item";
                return false;
            }

            // Resolve Viewmodel
            if (!TryReadObjectField(equippedItem, "ViewModel", out ulong viewmodel) || viewmodel == 0 || !viewmodel.IsValidVirtualAddress())
            {
                viewmodel = Mem.Ptr(equippedItem + 0x30, false);
            }

            if (viewmodel == 0 || !viewmodel.IsValidVirtualAddress())
            {
                failure = "Equipped item has no viewmodel";
                return false;
            }

            ulong vmKlass = Mem.Ptr(viewmodel, false);
            ulong extsOffset = 0;
            if (!TryGetFieldOffset(vmKlass, "<Extensions>k__BackingField", out extsOffset) &&
                !TryGetFieldOffset(vmKlass, "_extensions", out extsOffset))
            {
                extsOffset = 0xB8;
            }

            ulong extsArr = Mem.Ptr(viewmodel + extsOffset, false);
            if (extsArr == 0 && extsOffset != 0xB8)
            {
                extsArr = Mem.Ptr(viewmodel + 0xB8, false);
            }

            if (extsArr != 0 && extsArr.IsValidVirtualAddress())
            {
                int len = Mem.Val<int>(extsArr + Offsets.Array_max_length, false);
                if (len > 0 && len <= 32)
                {
                    for (int i = 0; i < len; i++)
                    {
                        ulong ext = Mem.Ptr(extsArr + Offsets.Array_items + (ulong)i * 8, false);
                        if (ext == 0 || !ext.IsValidVirtualAddress()) continue;

                        string name = ClassName(ext);
                        if (name.IndexOf("AdsExtension", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("ViewmodelAdsExtension", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            adsExtension = ext;
                            ulong extKlass = Mem.Ptr(ext, false);
                            ulong inOff = 0x88;
                            ulong outOff = 0x8C;
                            if (TryGetFieldOffset(extKlass, "_adsInAnimationSpeed", out ulong fo1)) inOff = fo1;
                            if (TryGetFieldOffset(extKlass, "_adsOutAnimationSpeed", out ulong fo2)) outOff = fo2;
                            inSpeedAddr = ext + inOff;
                            outSpeedAddr = ext + outOff;
                            failure = string.Empty;
                            return true;
                        }
                    }
                }
            }

            failure = "Weapon has no ADS extension";
            return false;
        }

        public static bool TryRestoreInstantAds()
        {
            lock (_instantAdsLock)
            {
                if (!_adsOriginalCaptured || _addrAdsInSpeed == 0 || _addrAdsOutSpeed == 0)
                {
                    _activeAdsExtension = 0;
                    _activeAdsItem = 0;
                    _addrAdsInSpeed = 0;
                    _addrAdsOutSpeed = 0;
                    _adsOriginalCaptured = false;
                    _adsApplied = false;
                    return true;
                }

                Interlocked.Increment(ref _restoringCounter);
                try
                {
                    bool ok1 = Mem.TryWriteValue<float>(_addrAdsInSpeed, _origAdsInSpeed);
                    bool ok2 = Mem.TryWriteValue<float>(_addrAdsOutSpeed, _origAdsOutSpeed);

                    _activeAdsExtension = 0;
                    _activeAdsItem = 0;
                    _addrAdsInSpeed = 0;
                    _addrAdsOutSpeed = 0;
                    _adsOriginalCaptured = false;
                    _adsApplied = false;
                    return ok1 && ok2;
                }
                finally
                {
                    Interlocked.Decrement(ref _restoringCounter);
                }
            }
        }

        public static void PollInstantAds(ulong localHub = 0, ulong item = 0)
        {
            lock (_instantAdsLock)
            {
                if (!MasterMemWritesEnabled || !InstantAdsEnabled)
                {
                    InstantAdsStatus = TryRestoreInstantAds() ? "Off" : "Restore blocked";
                    return;
                }

                if (localHub == 0) localHub = LocalHub();
                if (localHub == 0)
                {
                    TryRestoreInstantAds();
                    InstantAdsStatus = "Waiting for player";
                    return;
                }

                if (item == 0)
                {
                    if (!TryReadObjectField(localHub, "inventory", out ulong inv))
                    {
                        TryRestoreInstantAds();
                        InstantAdsStatus = "Inventory unavailable";
                        return;
                    }
                    TryReadObjectField(inv, "_curInstance", out item);
                }

                if (item == 0)
                {
                    TryRestoreInstantAds();
                    InstantAdsStatus = "No equipped item";
                    return;
                }

                long now = Stopwatch.GetTimestamp();

                if (_adsApplied && item == _activeAdsItem && _activeAdsExtension != 0)
                {
                    if (now - _lastAdsVerifyTicks < (Stopwatch.Frequency / 10))
                        return;
                }

                if (item != _activeAdsItem)
                {
                    TryRestoreInstantAds();
                }

                if (!TryResolveInstantAds(localHub, item, out ulong adsExt, out ulong inAddr, out ulong outAddr, out string fail))
                {
                    TryRestoreInstantAds();
                    InstantAdsStatus = fail;
                    return;
                }

                if (_activeAdsExtension != adsExt)
                {
                    TryRestoreInstantAds();

                    float origIn = Mem.Val<float>(inAddr, false);
                    float origOut = Mem.Val<float>(outAddr, false);

                    if (origIn > 0.01f && origIn < 25.0f && origOut > 0.01f && origOut < 25.0f)
                    {
                        _origAdsInSpeed = origIn;
                        _origAdsOutSpeed = origOut;
                        _adsOriginalCaptured = true;
                    }
                    else
                    {
                        _origAdsInSpeed = 1.0f;
                        _origAdsOutSpeed = 1.0f;
                        _adsOriginalCaptured = true;
                    }

                    _activeAdsExtension = adsExt;
                    _activeAdsItem = item;
                    _addrAdsInSpeed = inAddr;
                    _addrAdsOutSpeed = outAddr;
                }

                const float targetSpeed = 50.0f;
                Mem.TryWriteValue<float>(_addrAdsInSpeed, targetSpeed);
                Mem.TryWriteValue<float>(_addrAdsOutSpeed, targetSpeed);
                _adsApplied = true;
                _lastAdsVerifyTicks = now;
                InstantAdsStatus = "Active (Instant ADS)";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  THIRD-PERSON VIEW (TPV) / CORNER PEEK MODE
        // ═══════════════════════════════════════════════════════════
        public static bool TryResolvePcrTranslation(ulong localHub, out ulong trsAddr)
        {
            trsAddr = 0;
            if (localHub == 0 || !localHub.IsValidVirtualAddress()) return false;

            ulong pcr = Mem.Ptr(localHub + Offsets.RH_PlayerCameraReference, false);
            if (pcr == 0 || !pcr.IsValidVirtualAddress()) return false;

            ulong pcrNat = Mem.Ptr(pcr + 0x10, false);
            if (pcrNat == 0 || !pcrNat.IsValidVirtualAddress()) return false;

            byte[] pcrBytes = new byte[0x40];
            DmaMemory.ReadBuffer<byte>(pcrNat, pcrBytes.AsSpan(), false);
            ulong pcrHier = BitConverter.ToUInt64(pcrBytes, 0x28);
            int pcrIdx = BitConverter.ToInt32(pcrBytes, 0x30);

            if (!pcrHier.IsValidVirtualAddress() || pcrIdx < 0) return false;

            ulong pcrVerts = Mem.Ptr(pcrHier + 0x18, false);
            if (!pcrVerts.IsValidVirtualAddress()) return false;

            trsAddr = pcrVerts + (ulong)pcrIdx * 48;
            return true;
        }

        public static bool TryRestoreTpv()
        {
            lock (_tpvLock)
            {
                if (_tpvActiveTranslationAddr != 0 && _tpvOrigCaptured)
                {
                    Interlocked.Increment(ref _restoringCounter);
                    try
                    {
                        // In SCP:SL, first-person PlayerCameraReference translation is always Vector3.Zero.
                        // If captured original had an active TPV offset (e.g. from an overlay restart while active), fallback to Vector3.Zero.
                        Vector3 target = (_origPcrTranslation.Z < -0.5f || Math.Abs(_origPcrTranslation.X) > 0.2f) ? Vector3.Zero : _origPcrTranslation;
                        Mem.TryWriteValue<Vector3>(_tpvActiveTranslationAddr, target);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _restoringCounter);
                    }
                }
                _tpvActiveTranslationAddr = 0;
                _tpvOrigCaptured = false;
                _tpvWasApplied = false;
                _tpvToggleState = false;
                TpvActive = false;
                TpvStatus = "Off";
                return true;
            }
        }

        public static void PollTpv(ref CameraInfo cam, ulong localHub = 0)
        {
            lock (_tpvLock)
            {
                if (!MasterMemWritesEnabled || !TpvEnabled)
                {
                    if (_tpvWasApplied)
                    {
                        TryRestoreTpv();
                    }
                    TpvActive = false;
                    TpvStatus = "Off";
                    return;
                }

                if (!cam.Valid)
                {
                    if (_tpvWasApplied) TryRestoreTpv();
                    TpvActive = false;
                    TpvStatus = "Waiting for camera";
                    return;
                }

                if (localHub == 0) localHub = LocalHub();
                if (localHub == 0)
                {
                    if (_tpvWasApplied) TryRestoreTpv();
                    TpvActive = false;
                    TpvStatus = "Waiting for player";
                    return;
                }

                bool active = false;
                if (TpvRequireKey)
                {
                    bool keyDown = UnityInput.IsKeyDown(TpvKey);
                    if (TpvKeyMode == 0) // Toggle
                    {
                        if (keyDown && !_tpvPrevKeyDown)
                        {
                            _tpvToggleState = !_tpvToggleState;
                        }
                        _tpvPrevKeyDown = keyDown;
                        active = _tpvToggleState;
                    }
                    else // Hold
                    {
                        active = keyDown;
                    }
                }
                else
                {
                    active = true;
                }

                TpvActive = active;

                if (!active)
                {
                    if (_tpvWasApplied)
                    {
                        TryRestoreTpv();
                    }
                    TpvStatus = TpvRequireKey ? $"Standby ({TpvKey})" : "Off";
                    return;
                }

                // Resolve native PlayerCameraReference translation
                if (localHub != _tpvActiveHub || _tpvActiveTranslationAddr == 0)
                {
                    if (_tpvWasApplied) TryRestoreTpv();

                    if (!TryResolvePcrTranslation(localHub, out ulong trsAddr))
                    {
                        TpvStatus = "Resolving camera transform...";
                        return;
                    }

                    _origPcrTranslation = Mem.Val<Vector3>(trsAddr, false);
                    _tpvActiveTranslationAddr = trsAddr;
                    _tpvActiveHub = localHub;
                    _tpvOrigCaptured = true;
                }

                // Local translation offset: X = Shoulder, Y = Height, -Z = Distance behind
                Vector3 localOffset = new Vector3(TpvShoulderOffset, TpvHeight, -TpvDistance);
                Mem.TryWriteValue<Vector3>(_tpvActiveTranslationAddr, localOffset);

                // Note: Unity's native transform hierarchy automatically propagates PlayerCameraReference's
                // local translation down to Camera.main, and MainCameraController updates LastPosition with
                // the camera's true world position. Therefore, cam.Position read from ReadCamera() already
                // contains the third-person offset and does not need to be adjusted manually.

                _tpvWasApplied = true;
                TpvStatus = $"Active ({TpvDistance:0.1}m)";
            }
        }


        // ═══════════════════════════════════════════════════════════
        //  BRIGHTNESS / FULLBRIGHT (LiftGammaGain, Exposure, ColorAdjustments)
        // ═══════════════════════════════════════════════════════════
        public static void PollBrightness(ulong localHub = 0)
        {
            lock (_brightnessLock)
            {
                if (!MasterMemWritesEnabled || !BrightnessEnabled)
                {
                    Interlocked.Increment(ref _restoringCounter);
                    try
                    {
                        TryRestoreBrightness();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _restoringCounter);
                    }
                    _lastAppliedBrightnessEv = -1f;
                    _brightnessApplied = false;
                    BrightnessStatus = "Off";
                    return;
                }

                long now = Stopwatch.GetTimestamp();

                // Throttle HDRP Volume modifications to ~10 Hz (every 100ms)
                if ((double)(now - _lastBrightnessTicks) / Stopwatch.Frequency < 0.1)
                {
                    return;
                }
                _lastBrightnessTicks = now;

                // Discover / refresh HDRP Volume components periodically
                if (_activeLiftGammaTargets.Count == 0 && _activeExposureTargets.Count == 0 && _activeColorAdjTargets.Count == 0)
                {
                    ScanHdrpVolumeComponents();
                }

                float targetEv = Math.Clamp(BrightnessEv, 0.0f, 4.0f);
                bool evChanged = (Math.Abs(targetEv - _lastAppliedBrightnessEv) > 0.02f) || !_brightnessApplied;

                if (evChanged)
                {
                    // Apply LiftGammaGain (gamma & gain vector boost)
                    var gammaVal = new Vector4(1.0f + 0.25f * targetEv, 1.0f + 0.25f * targetEv, 1.0f + 0.25f * targetEv, 0.35f * targetEv);
                    var gainVal = new Vector4(1.0f + 0.15f * targetEv, 1.0f + 0.15f * targetEv, 1.0f + 0.15f * targetEv, 0.20f * targetEv);
                    for (int i = 0; i < _activeLiftGammaTargets.Count; i++)
                    {
                        var t = _activeLiftGammaTargets[i];
                        Mem.TryWriteValue<bool>(t.GammaOverrideAddr, true);
                        Mem.TryWriteValue<Vector4>(t.GammaValueAddr, gammaVal);
                        Mem.TryWriteValue<bool>(t.GainOverrideAddr, true);
                        Mem.TryWriteValue<Vector4>(t.GainValueAddr, gainVal);
                    }

                    // Apply Exposure compensation
                    for (int i = 0; i < _activeExposureTargets.Count; i++)
                    {
                        var t = _activeExposureTargets[i];
                        Mem.TryWriteValue<bool>(t.OverrideAddr, true);
                        Mem.TryWriteValue<float>(t.ValueAddr, targetEv);
                    }

                    // Apply ColorAdjustments postExposure
                    for (int i = 0; i < _activeColorAdjTargets.Count; i++)
                    {
                        var t = _activeColorAdjTargets[i];
                        Mem.TryWriteValue<bool>(t.OverrideAddr, true);
                        Mem.TryWriteValue<float>(t.ValueAddr, targetEv);
                    }

                    _lastAppliedBrightnessEv = targetEv;
                    _brightnessApplied = true;
                }

                int totalBoosted = _activeLiftGammaTargets.Count + _activeExposureTargets.Count + _activeColorAdjTargets.Count;
                BrightnessStatus = totalBoosted > 0 ? $"Active (+{targetEv:F1} EV, {totalBoosted} comps)" : "Scanning Volumes...";
            }
        }

        private static void TryRestoreBrightness()
        {
            Interlocked.Increment(ref _restoringCounter);
            try
            {
                // 1. Direct restore of all active LiftGammaGain targets to neutral
                for (int i = 0; i < _activeLiftGammaTargets.Count; i++)
                {
                    var t = _activeLiftGammaTargets[i];
                    Mem.TryWriteValue<bool>(t.GammaOverrideAddr, false);
                    Mem.TryWriteValue<Vector4>(t.GammaValueAddr, new Vector4(1f, 1f, 1f, 0f));
                    Mem.TryWriteValue<bool>(t.GainOverrideAddr, false);
                    Mem.TryWriteValue<Vector4>(t.GainValueAddr, new Vector4(1f, 1f, 1f, 0f));
                }

                // Restore custom overrides if original captured
                foreach (var kvp in _origLiftGammaDict)
                {
                    ulong comp = kvp.Key;
                    var orig = kvp.Value;
                    if (orig.OrigGammaOverride || orig.OrigGainOverride)
                    {
                        ulong gammaParam = Mem.Ptr(comp + Offsets.LiftGammaGain_gamma, false);
                        ulong gainParam = Mem.Ptr(comp + Offsets.LiftGammaGain_gain, false);
                        if (gammaParam.IsValidVirtualAddress() && orig.OrigGammaOverride)
                        {
                            Mem.TryWriteValue<bool>(gammaParam + Offsets.VolumeParameter_overrideState, true);
                            Mem.TryWriteValue<Vector4>(gammaParam + Offsets.Vector4Parameter_value, orig.OrigGammaValue);
                        }
                        if (gainParam.IsValidVirtualAddress() && orig.OrigGainOverride)
                        {
                            Mem.TryWriteValue<bool>(gainParam + Offsets.VolumeParameter_overrideState, true);
                            Mem.TryWriteValue<Vector4>(gainParam + Offsets.Vector4Parameter_value, orig.OrigGainValue);
                        }
                    }
                }

                // 2. Direct restore of all active Exposure targets to neutral
                for (int i = 0; i < _activeExposureTargets.Count; i++)
                {
                    var t = _activeExposureTargets[i];
                    Mem.TryWriteValue<bool>(t.OverrideAddr, false);
                    Mem.TryWriteValue<float>(t.ValueAddr, 0.0f);
                }
                foreach (var kvp in _origExposureDict)
                {
                    ulong comp = kvp.Key;
                    var orig = kvp.Value;
                    if (orig.OrigOverride)
                    {
                        ulong compParam = Mem.Ptr(comp + Offsets.Exposure_compensation, false);
                        if (compParam.IsValidVirtualAddress())
                        {
                            Mem.TryWriteValue<bool>(compParam + Offsets.VolumeParameter_overrideState, true);
                            Mem.TryWriteValue<float>(compParam + Offsets.FloatParameter_value, orig.OrigValue);
                        }
                    }
                }

                // 3. Direct restore of all active ColorAdjustments targets to neutral
                for (int i = 0; i < _activeColorAdjTargets.Count; i++)
                {
                    var t = _activeColorAdjTargets[i];
                    Mem.TryWriteValue<bool>(t.OverrideAddr, false);
                    Mem.TryWriteValue<float>(t.ValueAddr, 0.0f);
                }
                foreach (var kvp in _origColorAdjDict)
                {
                    ulong comp = kvp.Key;
                    var orig = kvp.Value;
                    if (orig.OrigOverride)
                    {
                        ulong postExp = Mem.Ptr(comp + Offsets.ColorAdjustments_postExposure, false);
                        if (postExp.IsValidVirtualAddress())
                        {
                            Mem.TryWriteValue<bool>(postExp + Offsets.VolumeParameter_overrideState, true);
                            Mem.TryWriteValue<float>(postExp + Offsets.FloatParameter_value, orig.OrigValue);
                        }
                    }
                }

                _lastAppliedBrightnessEv = -1f;
                _brightnessApplied = false;
            }
            finally
            {
                Interlocked.Decrement(ref _restoringCounter);
            }
        }

        public static void RestoreBrightness()
        {
            lock (_brightnessLock)
            {
                TryRestoreBrightness();
                BrightnessStatus = "Off";
            }
        }

        // ── Role: map class name or read _roleId / _roleTypeId for FPC roles ──
        private static RoleTypeId ClassNameToRole(string className) => className switch
        {
            "Scp173Role" => RoleTypeId.Scp173,
            "Scp049Role" => RoleTypeId.Scp049,
            "Scp079Role" => RoleTypeId.Scp079,
            "Scp106Role" => RoleTypeId.Scp106,
            "Scp096Role" => RoleTypeId.Scp096,
            "Scp0492Role" or "ZombieRole" => RoleTypeId.Scp0492,
            "Scp939Role" => RoleTypeId.Scp939,
            "Scp3114Role" => RoleTypeId.Scp3114,
            "Scp1507Role" => RoleTypeId.Flamingo,
            "ClassDRole" => RoleTypeId.ClassD,
            "ScientistRole" => RoleTypeId.Scientist,
            "SpectatorRole" => RoleTypeId.Spectator,
            "OverwatchRole" => RoleTypeId.Overwatch,
            "FilmmakerRole" => RoleTypeId.Filmmaker,
            "NoneRole" => RoleTypeId.None,
            "DestroyedRole" => RoleTypeId.Destroyed,
            _ => RoleTypeId.None
        };

        public static RoleTypeId ReadRole(ulong curRole)
        {
            if (curRole == 0) return RoleTypeId.None;

            // Check cached class name FIRST. Spectator, None, Overwatch, and Scp079
            // are only 0xC0 bytes and do NOT have an offset 0xD8 field (reading 0xD8 hits uninitialized heap).
            string className = ClassName(curRole);
            if (!string.IsNullOrEmpty(className))
            {
                switch (className)
                {
                    case "SpectatorRole": return RoleTypeId.Spectator;
                    case "OverwatchRole": return RoleTypeId.Overwatch;
                    case "FilmmakerRole": return RoleTypeId.Filmmaker;
                    case "NoneRole": return RoleTypeId.None;
                    case "DestroyedRole": return RoleTypeId.Destroyed;
                    case "Scp079Role": return RoleTypeId.Scp079;
                }
            }

            // For HumanRole, FpcStandardScp, and subclasses, roleId is at 0xD8
            sbyte roleId = Mem.Val<sbyte>(curRole + Offsets.Role_roleTypeId, false);
            if (roleId >= 0 && roleId <= 29)
            {
                return (RoleTypeId)roleId;
            }

            if (!string.IsNullOrEmpty(className))
            {
                RoleTypeId mapped = ClassNameToRole(className);
                if (mapped != RoleTypeId.None) return mapped;
            }

            return RoleTypeId.None;
        }

        // ── Health: cache the health stat module pointer ──
        public static float? ReadHealth(ulong hub, ulong statModules, out float maxHealth)
        {
            maxHealth = 100.0f;

            // Try cached health module pointer first
            if (_healthModCache.TryGetValue(hub, out ulong cachedMod) && cachedMod != 0)
            {
                float cur = Mem.Val<float>(cachedMod + Offsets.Stat_lastValue, false);
                float mx = Mem.Val<float>(cachedMod + Offsets.HealthStat_maxValue, false);
                maxHealth = mx;
                return cur;
            }

            if (statModules == 0) return null;

            ulong maxLength = Mem.Val<ulong>(statModules + Offsets.Array_max_length);
            if (maxLength == 0 || maxLength > 32) return null;

            // Just take index 0 (HealthStat is always first)
            ulong mod = Mem.Ptr(statModules + Offsets.Array_items);
            if (mod == 0) return null;

            // Cache it
            _healthModCache[hub] = mod;

            float cur2 = Mem.Val<float>(mod + Offsets.Stat_lastValue, false);
            float mx2 = Mem.Val<float>(mod + Offsets.HealthStat_maxValue, false);
            maxHealth = mx2;
            return cur2;
        }

        // ── Name: cached, refreshed every N frames ──
        public static string ReadPlayerName(ulong hub, ulong displayName, ulong myNick)
        {
            if (_playerNameCache.TryGetValue(hub, out string? cached) && _nameRefreshCounter > 0)
                return cached;
            if (displayName != 0)
            {
                int len = Mem.Val<int>(displayName + Offsets.String_length);
                if (len > 0 && len <= 256)
                {
                    string s = Mem.StrUnicode(displayName + Offsets.String_chars, len);
                    if (!string.IsNullOrEmpty(s)) { _playerNameCache[hub] = s; return s; }
                }
            }

            if (myNick != 0)
            {
                int len = Mem.Val<int>(myNick + Offsets.String_length);
                if (len > 0 && len <= 256)
                {
                    string s = Mem.StrUnicode(myNick + Offsets.String_chars, len);
                    if (!string.IsNullOrEmpty(s)) { _playerNameCache[hub] = s; return s; }
                }
            }

            _playerNameCache[hub] = "Player";
            return "Player";
        }

        public struct PrefetchedData
        {
            public int playerId;
            public ulong curRole;
            public ulong statModules;
            public ulong displayName;
            public ulong myNickSync;
        }

        public static PlayerInfo ReadPlayer(ulong hub, ulong localHub, ref PrefetchedData data, Vector3? pos, float? hp, float maxHp, ref double nameMs, ref double roleMs)
        {
            var sw = Stopwatch.StartNew();
            var p = new PlayerInfo();
            p.Hub = hub;
            p.IsLocal = (localHub != 0 && hub == localHub);
            p.PlayerId = data.playerId;

            double t0 = sw.Elapsed.TotalMilliseconds;
            p.Name = ReadPlayerName(hub, data.displayName, data.myNickSync);
            nameMs += sw.Elapsed.TotalMilliseconds - t0;

            t0 = sw.Elapsed.TotalMilliseconds;
            p.Role = ReadRole(data.curRole);
            p.Team = TeamFromRole(p.Role);
            p.Alive = RoleIsAlive(p.Role);
            roleMs += sw.Elapsed.TotalMilliseconds - t0;

            if (pos.HasValue) { p.Position = pos.Value; p.HasPosition = true; }
            if (hp.HasValue) { p.Health = hp.Value; p.MaxHealth = maxHp > 0f ? maxHp : 100f; }

            return p;
        }

        // ── SKELETON / BONE HIERARCHY RESOLUTION (Unity 2021/2022 IL2CPP) ──
        [ThreadStatic]
        private static UnityTransform.TrsX[]? _boneVertsBuffer;

        public static bool SetupPlayerBones(PlayerInfo p)
        {
            if (p == null || p.IsLocal) return false;

            // Ensure CurRole is populated
            if (p.CurRole == 0 && p.RoleManager != 0)
            {
                p.CurRole = Mem.Ptr(p.RoleManager + Offsets.PRM_curRole);
            }
            if (p.CurRole == 0) return false;

            // Ensure FpcModule is populated
            if (p.FpcModule == 0)
            {
                ulong roleKlass = Mem.Ptr(p.CurRole);
                if (IsFpcSubclass(roleKlass))
                {
                    p.FpcModule = Mem.Ptr(p.CurRole + Offsets.Fpc_FpcModule);
                }
            }
            if (p.FpcModule == 0) return false;

            // Read CharacterModel instance
            ulong cm = Mem.Ptr(p.FpcModule + Offsets.Fpm_CharacterModelInstance);
            if (cm == 0) return false;

            // Read hitboxes array (fall back to Hardpoints if _hitboxesCache is null/unpopulated or has fewer than 10 hitboxes)
            ulong hitboxesArr = Mem.Ptr(cm + Offsets.CM_hitboxesCache);
            bool isHardpoints = false;
            int hbLen = (hitboxesArr != 0) ? Mem.Val<int>(hitboxesArr + Offsets.Array_max_length) : 0;
            if (hitboxesArr == 0 || hbLen < 10)
            {
                hitboxesArr = Mem.Ptr(cm + Offsets.CM_hardpoints);
                isHardpoints = true;
                hbLen = (hitboxesArr != 0) ? Mem.Val<int>(hitboxesArr + Offsets.Array_max_length) : 0;
            }
            if (hitboxesArr == 0 || hbLen < 10 || hbLen > 64) return false;

            // Bulk read all hitbox pointers in 1 DMA buffer read
            int count = Math.Min(hbLen, 32);
            Span<ulong> rawPtrs = stackalloc ulong[count];
            DmaMemory.ReadBuffer<ulong>(hitboxesArr + Offsets.Array_items, rawPtrs, false);

            // Find hierarchy address: first check CharacterModel / FpcModule root transforms
            ulong hier = 0;
            ulong cmTr = Mem.Ptr(cm + Offsets.CM_cachedTransform);
            if (cmTr == 0) cmTr = Mem.Ptr(cm + Offsets.CM_ownerTr);
            if (cmTr == 0) cmTr = Mem.Ptr(p.FpcModule + 0xD8);
            if (cmTr != 0)
            {
                ulong nTr = Mem.Ptr(cmTr + Offsets.Tr_nativeTransform);
                if (nTr != 0)
                {
                    ulong candHier = Mem.Ptr(nTr + Offsets.NativeTr_hierarchy);
                    if (candHier.IsValidVirtualAddress()) hier = candHier;
                }
            }

            // Scatter Batch: Resolve hitbox transforms and indices
            // Round 1: read _transformCache (0x20), _dmgMultiplier (0x40), and native component (0x10)
            // If isHardpoints, also read Type (0x28) to filter out non-hitbox reference points
            Span<ulong> trPtrs = stackalloc ulong[count];
            Span<ulong> ncPtrs = stackalloc ulong[count];
            Span<int> dmgMultipliers = stackalloc int[count];
            Span<int> refTypes = stackalloc int[count];
            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int h = 0; h < count; h++)
                {
                    if (rawPtrs[h] != 0)
                    {
                        var entry = round[h];
                        entry.AddEntry<ulong>(0, rawPtrs[h] + Offsets.HB_transformCache);
                        entry.AddEntry<int>(1, rawPtrs[h] + Offsets.HB_dmgMultiplier);
                        entry.AddEntry<ulong>(2, rawPtrs[h] + 0x10); // Native component
                        if (isHardpoints)
                        {
                            entry.AddEntry<int>(3, rawPtrs[h] + Offsets.MRP_type);
                        }
                    }
                }
                map.Execute();

                for (int h = 0; h < count; h++)
                {
                    if (rawPtrs[h] != 0)
                    {
                        var entry = round[h];
                        if (entry.TryGetResult(0, out ulong tr)) trPtrs[h] = tr;
                        if (entry.TryGetResult(1, out int dmg)) dmgMultipliers[h] = dmg;
                        if (entry.TryGetResult(2, out ulong nc)) ncPtrs[h] = nc;
                        if (isHardpoints && entry.TryGetResult(3, out int t)) refTypes[h] = t;
                    }
                }
            }

            // Compact valid hitboxes into hbPtrs, validTrPtrs, validNcPtrs, and validDmgMultipliers
            Span<ulong> hbPtrs = stackalloc ulong[count];
            Span<ulong> validTrPtrs = stackalloc ulong[count];
            Span<ulong> validNcPtrs = stackalloc ulong[count];
            Span<int> validDmgMultipliers = stackalloc int[count];
            int validCount = 0;
            for (int h = 0; h < count; h++)
            {
                if (rawPtrs[h] != 0 && (trPtrs[h] != 0 || ncPtrs[h] != 0))
                {
                    // If from Hardpoints, skip pure non-hitbox reference points (e.g. LineOfSightPoint = 1)
                    if (isHardpoints && refTypes[h] == 1) continue;

                    hbPtrs[validCount] = rawPtrs[h];
                    validTrPtrs[validCount] = trPtrs[h];
                    validNcPtrs[validCount] = ncPtrs[h];
                    validDmgMultipliers[validCount] = dmgMultipliers[h];
                    validCount++;
                }
            }
            if (validCount < 10) return false;

            // Round 2: read nativeTransform (offset 0x10) from validTrPtrs,
            // OR if _transformCache was null (0), read nativeGameObject (0x20) from validNcPtrs
            Span<ulong> nativeTrPtrs = stackalloc ulong[validCount];
            Span<ulong> nativeGoPtrs = stackalloc ulong[validCount];
            bool needGoResolution = false;
            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int h = 0; h < validCount; h++)
                {
                    var entry = round[h];
                    if (validTrPtrs[h] != 0)
                    {
                        entry.AddEntry<ulong>(0, validTrPtrs[h] + Offsets.Tr_nativeTransform);
                    }
                    else if (validNcPtrs[h] != 0)
                    {
                        entry.AddEntry<ulong>(1, validNcPtrs[h] + 0x20); // native GameObject
                        needGoResolution = true;
                    }
                }
                map.Execute();

                for (int h = 0; h < validCount; h++)
                {
                    var entry = round[h];
                    if (entry.TryGetResult(0, out ulong nTr)) nativeTrPtrs[h] = nTr;
                    if (entry.TryGetResult(1, out ulong go)) nativeGoPtrs[h] = go;
                }
            }

            // If any hitboxes had uninitialized _transformCache, resolve nativeTransform via GameObject -> ComponentArray -> Component[0]
            if (needGoResolution)
            {
                Span<ulong> compArrPtrs = stackalloc ulong[validCount];
                using (var map = ScatterReadMap.Get())
                {
                    var round = map.AddRound(useCache: false);
                    for (int h = 0; h < validCount; h++)
                    {
                        if (nativeTrPtrs[h] == 0 && nativeGoPtrs[h] != 0)
                        {
                            round[h].AddEntry<ulong>(0, nativeGoPtrs[h] + 0x20); // ComponentArray
                        }
                    }
                    map.Execute();

                    for (int h = 0; h < validCount; h++)
                    {
                        if (round[h].TryGetResult(0, out ulong ca)) compArrPtrs[h] = ca;
                    }
                }

                using (var map = ScatterReadMap.Get())
                {
                    var round = map.AddRound(useCache: false);
                    for (int h = 0; h < validCount; h++)
                    {
                        if (nativeTrPtrs[h] == 0 && compArrPtrs[h] != 0)
                        {
                            round[h].AddEntry<ulong>(0, compArrPtrs[h] + 0x8); // Component[0] = native Transform
                        }
                    }
                    map.Execute();

                    for (int h = 0; h < validCount; h++)
                    {
                        if (round[h].TryGetResult(0, out ulong nTr)) nativeTrPtrs[h] = nTr;
                    }
                }
            }

            // Round 3: read NativeTr_hierarchy (0x28) and NativeTr_index (0x30) from nativeTrPtrs
            Span<int> nativeIndices = stackalloc int[validCount];
            nativeIndices.Fill(-1);
            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int h = 0; h < validCount; h++)
                {
                    if (nativeTrPtrs[h] != 0)
                    {
                        var entry = round[h];
                        if (hier == 0) entry.AddEntry<ulong>(0, nativeTrPtrs[h] + Offsets.NativeTr_hierarchy);
                        entry.AddEntry<int>(1, nativeTrPtrs[h] + Offsets.NativeTr_index);
                    }
                }
                map.Execute();

                for (int h = 0; h < validCount; h++)
                {
                    if (nativeTrPtrs[h] != 0)
                    {
                        var entry = round[h];
                        if (hier == 0 && entry.TryGetResult(0, out ulong candHier) && candHier.IsValidVirtualAddress())
                            hier = candHier;
                        if (entry.TryGetResult(1, out int idx))
                            nativeIndices[h] = idx;
                    }
                }
            }

            if (hier == 0) return false;

            ulong vertsAddr = Mem.Ptr(hier + Offsets.Hier_vertices);
            ulong indicesAddr = Mem.Ptr(hier + Offsets.Hier_indices);
            int cap = Mem.Val<int>(hier + Offsets.Hier_capacity);
            if (!vertsAddr.IsValidVirtualAddress() || !indicesAddr.IsValidVirtualAddress() || cap <= 0 || cap > 65536)
                return false;

            p.HitboxIndices = new int[SkeletonData.TotalBones];
            Array.Fill(p.HitboxIndices, -1);

            if (validCount >= 15)
            {
                for (int h = 0; h < 15 && h < nativeIndices.Length; h++)
                {
                    p.HitboxIndices[h] = nativeIndices[h];
                }
            }
            else if (validCount == 11) // SCP-106 (Larry)
            {
                int[] scp106Map = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 13 };
                for (int h = 0; h < 11 && h < nativeIndices.Length; h++)
                {
                    int targetSlot = scp106Map[h];
                    if (targetSlot < 15) p.HitboxIndices[targetSlot] = nativeIndices[h];
                }
            }
            else if (validCount == 10) // SCP-049-2 (Zombie)
            {
                int[] zombMap = { 0, 1, 2, 3, 5, 6, 7, 4, 9, 12 };
                for (int h = 0; h < 10 && h < nativeIndices.Length; h++)
                {
                    int targetSlot = zombMap[h];
                    if (targetSlot < 15) p.HitboxIndices[targetSlot] = nativeIndices[h];
                }
            }
            else
            {
                for (int h = 0; h < validCount && h < 15 && h < nativeIndices.Length; h++)
                {
                    p.HitboxIndices[h] = nativeIndices[h];
                }
            }

            // Guarantee Head bone index: if slot 4 is missing, check if any hitbox had Headshot dmg multiplier (2)
            if (p.HitboxIndices[SkeletonData.HeadHitboxIndex] < 0)
            {
                for (int h = 0; h < validCount; h++)
                {
                    if (validDmgMultipliers[h] == 2 && nativeIndices[h] >= 0)
                    {
                        p.HitboxIndices[SkeletonData.HeadHitboxIndex] = nativeIndices[h];
                        break;
                    }
                }
            }

            // Cache parent indices ONCE per player model for hierarchy traversal and hand bone resolution
            p.Indices = new int[cap];
            DmaMemory.ReadBuffer<int>(indicesAddr, p.Indices.AsSpan(), false);

            // Resolve Left Hand (slot 15) and Right Hand (slot 16) from Forearms (slots 6 and 8)
            int lForearm = p.HitboxIndices[6];
            int rForearm = p.HitboxIndices[8];

            if (lForearm >= 0 && lForearm < cap)
            {
                int lHand = -1;
                for (int k = 0; k < cap; k++)
                {
                    if (p.Indices[k] == lForearm)
                    {
                        if (k > lHand) lHand = k;
                    }
                }
                if (lHand < 0 && lForearm + 3 < cap && p.Indices[lForearm + 3] == lForearm)
                    lHand = lForearm + 3;

                p.HitboxIndices[SkeletonData.LeftHandIndex] = lHand;
            }

            if (rForearm >= 0 && rForearm < cap)
            {
                int rHand = -1;
                for (int k = 0; k < cap; k++)
                {
                    if (p.Indices[k] == rForearm)
                    {
                        if (k > rHand) rHand = k;
                    }
                }
                if (rHand < 0 && rForearm + 3 < cap && p.Indices[rForearm + 3] == rForearm)
                    rHand = rForearm + 3;

                p.HitboxIndices[SkeletonData.RightHandIndex] = rHand;
            }

            bool anyBoneFound = false;
            for (int h = 0; h < SkeletonData.TotalBones; h++)
            {
                if (p.HitboxIndices[h] >= 0) { anyBoneFound = true; break; }
            }
            if (!anyBoneFound) return false;

            p.HierarchyAddr = hier;
            p.VerticesAddr = vertsAddr;
            p.Capacity = cap;

            p.BonePositions ??= new Vector3[SkeletonData.TotalBones];
            p.HasBones = true;
            return true;
        }

        public static void PollBones(List<PlayerInfo> players, Vector3 camPos, float maxDistance, bool headOnly = false)
        {
            PollBones(players, camPos, Vector3.Zero, maxDistance, headOnly);
        }

        public static void PollBones(List<PlayerInfo> players, Vector3 camPos, Vector3 camForward, float maxDistance, bool headOnly = false)
        {
            if (players == null || players.Count == 0) return;
            var sw = Stopwatch.StartNew();

            float effectiveMaxDist = Math.Min(maxDistance, 120f);
            float maxDistSq = effectiveMaxDist * effectiveMaxDist;
            bool filterFrustum = camForward != Vector3.Zero;
            ulong lastVerticesAddr = 0;
            int lastVerticesCap = 0;

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p.IsLocal || !p.Alive || !p.HasPosition || !p.HasBones || p.Indices == null || p.BonePositions == null || p.HitboxIndices == null)
                    continue;

                Vector3 delta = p.Position - camPos;
                float distSq = delta.LengthSquared();
                if (distSq > maxDistSq)
                    continue;

                // Skip bone calculation for players further than 15m who are behind the camera view
                if (filterFrustum && distSq > 225f && Vector3.Dot(delta, camForward) < 0f)
                    continue;

                int cap = p.Capacity;
                if (cap <= 0 || cap > 65536 || !p.VerticesAddr.IsValidVirtualAddress())
                    continue;

                try
                {
                    if (p.VerticesAddr != lastVerticesAddr || cap > lastVerticesCap)
                    {
                        if (_boneVertsBuffer == null || _boneVertsBuffer.Length < cap)
                            _boneVertsBuffer = new UnityTransform.TrsX[Math.Max(cap + 64, 512)];

                        DmaMemory.ReadBuffer<UnityTransform.TrsX>(p.VerticesAddr, _boneVertsBuffer.AsSpan(0, cap), false);
                        lastVerticesAddr = p.VerticesAddr;
                        lastVerticesCap = cap;
                    }

                    var indices = p.Indices;
                    var hbIndices = p.HitboxIndices;

                    if (headOnly)
                    {
                        int idx = hbIndices[SkeletonData.HeadHitboxIndex];
                        if (idx >= 0 && idx < cap)
                        {
                            Vector3 pos = _boneVertsBuffer[idx].t;
                            int parent = indices[idx];
                            int iter = 0;
                            while (parent >= 0 && parent < cap && iter++ < 100)
                            {
                                ref readonly var pt = ref _boneVertsBuffer[parent];
                                pos = pt.q.Multiply(pos) * pt.s + pt.t;
                                parent = indices[parent];
                            }
                            p.BonePositions[SkeletonData.HeadHitboxIndex] = pos;
                        }

                        if (MasterMemWritesEnabled && ViewangleAimEnabled && ViewangleAimTargetBone == 1 && _boneVertsBuffer != null)
                        {
                            int chestIdx = hbIndices[SkeletonData.ChestHitboxIndex];
                            if (chestIdx >= 0 && chestIdx < cap)
                            {
                                Vector3 cPos = _boneVertsBuffer[chestIdx].t;
                                int cParent = indices[chestIdx];
                                int cIter = 0;
                                while (cParent >= 0 && cParent < cap && cIter++ < 100)
                                {
                                    ref readonly var pt = ref _boneVertsBuffer[cParent];
                                    cPos = pt.q.Multiply(cPos) * pt.s + pt.t;
                                    cParent = indices[cParent];
                                }
                                p.BonePositions[SkeletonData.ChestHitboxIndex] = cPos;
                            }
                        }
                    }
                    else
                    {
                        for (int h = 0; h < SkeletonData.TotalBones; h++)
                        {
                            int idx = hbIndices[h];
                            if (idx < 0 || idx >= cap)
                            {
                                p.BonePositions[h] = Vector3.Zero;
                                continue;
                            }

                            Vector3 pos = _boneVertsBuffer[idx].t;
                            int parent = indices[idx];
                            int iter = 0;
                            while (parent >= 0 && parent < cap && iter++ < 100)
                            {
                                ref readonly var pt = ref _boneVertsBuffer[parent];
                                pos = pt.q.Multiply(pos) * pt.s + pt.t;
                                parent = indices[parent];
                            }
                            p.BonePositions[h] = pos;
                        }

                        // Invalidation check: if all bone positions evaluated to zero on an alive player,
                        // mark HasBones = false so SetupPlayerBones will re-acquire fresh transforms immediately
                        bool anyNonZero = false;
                        for (int h = 0; h < SkeletonData.TotalBones; h++)
                        {
                            if (p.BonePositions[h] != Vector3.Zero)
                            {
                                anyNonZero = true;
                                break;
                            }
                        }
                        if (!anyNonZero)
                        {
                            p.HasBones = false;
                        }
                    }
                }
                catch
                {
                    // If memory read fails, ignore smoothly
                }
            }

            sw.Stop();
            Diag.LastBoneMs = sw.Elapsed.TotalMilliseconds;
        }

        public static void ResolveStatModules(PlayerInfo p)
        {
            if (p.StatModules == 0 || !p.StatModules.IsValidVirtualAddress()) return;
            int len = Mem.Val<int>(p.StatModules + Offsets.Array_max_length, false);
            if (len <= 0 || len > 16) return;

            for (int m = 0; m < len; m++)
            {
                ulong mod = Mem.Ptr(p.StatModules + Offsets.Array_items + (ulong)m * 8, false);
                if (mod == 0 || !mod.IsValidVirtualAddress()) continue;

                string cls = ClassName(mod);
                if (cls.IndexOf("HealthStat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    p.HealthModule = mod;
                }
                else if (cls.IndexOf("HumeShieldStat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    p.HumeShieldModule = mod;
                }
                else if (cls.IndexOf("AhpStat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    p.AhpModule = mod;
                }
            }

            if (p.HealthModule == 0 && len > 0)
            {
                p.HealthModule = Mem.Ptr(p.StatModules + Offsets.Array_items, false);
            }
        }

        // ── 1. PLAYER LIST & BASE POINTER DISCOVERY (Runs every 2000ms) ──
        public static List<PlayerInfo> PollPlayerList(List<PlayerInfo>? existingPlayers = null)
        {
            ulong all = ResolveAllHubs();
            if (all == 0) return new List<PlayerInfo>();

            var hubs = HashSetElements(all);
            ulong local = LocalHub();
            var players = new List<PlayerInfo>(hubs.Count);

            if (hubs.Count == 0) return players;

            Dictionary<ulong, PlayerInfo>? existingMap = null;
            if (existingPlayers != null && existingPlayers.Count > 0)
            {
                existingMap = new Dictionary<ulong, PlayerInfo>(existingPlayers.Count);
                for (int e = 0; e < existingPlayers.Count; e++)
                {
                    var ep = existingPlayers[e];
                    if (ep != null && ep.Hub != 0)
                        existingMap[ep.Hub] = ep;
                }
            }

            ulong[] rM = new ulong[hubs.Count];
            ulong[] pS = new ulong[hubs.Count];
            ulong[] nS = new ulong[hubs.Count];
            int[] pId = new int[hubs.Count];

            // Scatter Batch 1: Base components
            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < hubs.Count; i++)
                {
                    var idx = round[i];
                    idx.AddEntry<ulong>(0, hubs[i] + Offsets.RH_roleManager);
                    idx.AddEntry<ulong>(1, hubs[i] + Offsets.RH_playerStats);
                    idx.AddEntry<ulong>(2, hubs[i] + Offsets.RH_nicknameSync);
                    idx.AddEntry<int>(3, hubs[i] + Offsets.RH_playerId);
                }
                map.Execute();

                for (int i = 0; i < hubs.Count; i++)
                {
                    var idx = round[i];
                    if (idx.TryGetResult(0, out ulong rm)) rM[i] = rm;
                    if (idx.TryGetResult(1, out ulong ps)) pS[i] = ps;
                    if (idx.TryGetResult(2, out ulong ns)) nS[i] = ns;
                    if (idx.TryGetResult(3, out int pid)) pId[i] = pid;
                }
            }

            // Scatter Batch 2: Dependent modules
            PrefetchedData[] prefetches = new PrefetchedData[hubs.Count];
            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < hubs.Count; i++)
                {
                    var idx = round[i];
                    if (rM[i] != 0) idx.AddEntry<ulong>(0, rM[i] + Offsets.PRM_curRole);
                    if (pS[i] != 0) idx.AddEntry<ulong>(1, pS[i] + Offsets.PS_statModules);
                    if (nS[i] != 0)
                    {
                        idx.AddEntry<ulong>(2, nS[i] + Offsets.NS_displayName);
                        idx.AddEntry<ulong>(3, nS[i] + Offsets.NS_myNickSync);
                    }
                }
                map.Execute();

                for (int i = 0; i < hubs.Count; i++)
                {
                    var idx = round[i];
                    prefetches[i].playerId = pId[i];
                    if (idx.TryGetResult(0, out ulong cr)) prefetches[i].curRole = cr;
                    if (idx.TryGetResult(1, out ulong sm)) prefetches[i].statModules = sm;
                    if (idx.TryGetResult(2, out ulong dn)) prefetches[i].displayName = dn;
                    if (idx.TryGetResult(3, out ulong mn)) prefetches[i].myNickSync = mn;
                }
            }

            // Build Player Objects
            for (int i = 0; i < hubs.Count; i++)
            {
                ulong hub = hubs[i];
                if (hub == 0) continue;

                PlayerInfo? existing = null;
                existingMap?.TryGetValue(hub, out existing);

                var p = new PlayerInfo
                {
                    Hub = hub,
                    IsLocal = (local != 0 && hub == local),
                    PlayerId = prefetches[i].playerId,
                    RoleManager = rM[i],
                    PlayerStats = pS[i],
                    NicknameSync = nS[i],
                    CurRole = prefetches[i].curRole,
                    StatModules = prefetches[i].statModules,
                    DisplayName = prefetches[i].displayName,
                    MyNickSync = prefetches[i].myNickSync
                };

                // Read and cache name once upon discovery (or keep existing)
                if (existing != null && !string.IsNullOrEmpty(existing.Name) && existing.Name != "Player")
                    p.Name = existing.Name;
                else
                    p.Name = ReadPlayerName(hub, p.DisplayName, p.MyNickSync);

                // Initial Role Read
                p.Role = ReadRole(p.CurRole);
                p.Team = TeamFromRole(p.Role);
                p.Alive = RoleIsAlive(p.Role);

                // Cache FpcModule Pointer
                if (p.CurRole != 0)
                {
                    ulong roleKlass = Mem.Ptr(p.CurRole);
                    if (IsFpcSubclass(roleKlass))
                    {
                        p.FpcModule = Mem.Ptr(p.CurRole + Offsets.Fpc_FpcModule);
                    }
                }

                // Cache Stat Modules (HealthStat, HumeShieldStat, AhpStat)
                if (p.StatModules != 0)
                {
                    if (existing != null && existing.StatModules == p.StatModules && existing.HealthModule != 0)
                    {
                        p.HealthModule = existing.HealthModule;
                        p.HumeShieldModule = existing.HumeShieldModule;
                        p.AhpModule = existing.AhpModule;
                    }
                    else
                    {
                        ResolveStatModules(p);
                    }
                }

                // Preserve existing position & health state
                if (existing != null)
                {
                    p.Position = existing.Position;
                    p.HasPosition = existing.HasPosition;
                    p.Health = existing.Health;
                    p.MaxHealth = existing.MaxHealth;
                    p.HumeShield = existing.HumeShield;
                    p.MaxHumeShield = existing.MaxHumeShield;
                    p.Ahp = existing.Ahp;
                }

                // Preserve existing bone setup if role and model haven't changed
                if (existing != null && existing.HasBones && existing.CurRole == p.CurRole && p.Alive)
                {
                    p.BoneTransform = existing.BoneTransform;
                    p.HierarchyAddr = existing.HierarchyAddr;
                    p.VerticesAddr = existing.VerticesAddr;
                    p.Capacity = existing.Capacity;
                    p.Indices = existing.Indices;
                    p.HitboxIndices = existing.HitboxIndices;
                    p.BonePositions = existing.BonePositions;
                    p.HasBones = true;
                    if (p.FpcModule == 0) p.FpcModule = existing.FpcModule;
                }
                else if (p.Alive && !p.IsLocal && p.Role != RoleTypeId.Scp173 && p.Role != RoleTypeId.Scp079)
                {
                    p.LastBoneAttemptTicks = Stopwatch.GetTimestamp();
                    SetupPlayerBones(p);
                }

                players.Add(p);
            }

            Diag.LastPlayerCount = players.Count;
            return players;
        }

        // ── 2. ROLE CACHE REFRESH (Runs every 5000ms) ──
        public static void PollRoles(List<PlayerInfo> players)
        {
            if (players == null || players.Count == 0) return;

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].RoleManager != 0)
                        round[i].AddEntry<ulong>(0, players[i].RoleManager + Offsets.PRM_curRole);
                    else if (players[i].CurRole != 0)
                        round[i].AddEntry<sbyte>(1, players[i].CurRole + Offsets.Role_roleTypeId);
                }
                map.Execute();

                for (int i = 0; i < players.Count; i++)
                {
                    var idx = round[i];
                    ulong curRole = players[i].CurRole;
                    if (idx.TryGetResult(0, out ulong newCurRole) && newCurRole != 0)
                    {
                        if (newCurRole != players[i].CurRole)
                        {
                            players[i].CurRole = newCurRole;
                            ulong roleKlass = Mem.Ptr(newCurRole);
                            if (IsFpcSubclass(roleKlass))
                            {
                                players[i].FpcModule = Mem.Ptr(newCurRole + Offsets.Fpc_FpcModule);
                            }
                            else
                            {
                                players[i].FpcModule = 0;
                            }
                            players[i].HasBones = false;
                        }
                        curRole = newCurRole;
                    }

                    if (curRole != 0)
                    {
                        RoleTypeId role = ReadRole(curRole);
                        players[i].Role = role;
                        players[i].Team = TeamFromRole(role);
                        players[i].Alive = RoleIsAlive(role);

                        if (players[i].Alive && !players[i].IsLocal && !players[i].HasBones &&
                            role != RoleTypeId.Scp173 && role != RoleTypeId.Scp079)
                        {
                            // Re-read FpcModule if missing
                            if (players[i].FpcModule == 0)
                            {
                                ulong roleKlass = Mem.Ptr(curRole);
                                if (IsFpcSubclass(roleKlass))
                                {
                                    players[i].FpcModule = Mem.Ptr(curRole + Offsets.Fpc_FpcModule);
                                }
                            }

                            long now = Stopwatch.GetTimestamp();
                            // Rate-limit retry attempts to once every 50ms to avoid bus saturation
                            if (now - players[i].LastBoneAttemptTicks > (Stopwatch.Frequency / 20))
                            {
                                players[i].LastBoneAttemptTicks = now;
                                SetupPlayerBones(players[i]);
                            }
                        }
                        else if (!players[i].Alive)
                        {
                            players[i].HasBones = false;
                        }
                    }
                }
            }
        }

        // ── 3. HEALTH POLLING (Runs every 50ms) ──
        public static void PollHealth(List<PlayerInfo> players)
        {
            if (players == null || players.Count == 0) return;

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    var idx = round[i];
                    if (p.HealthModule != 0)
                    {
                        idx.AddEntry<float>(0, p.HealthModule + Offsets.Stat_lastValue);
                        idx.AddEntry<float>(1, p.HealthModule + Offsets.HealthStat_maxValue);
                    }
                    if (p.HumeShieldModule != 0)
                    {
                        idx.AddEntry<float>(2, p.HumeShieldModule + Offsets.Stat_lastValue);
                    }
                    if (p.AhpModule != 0)
                    {
                        idx.AddEntry<float>(3, p.AhpModule + Offsets.Stat_lastValue);
                    }
                }
                map.Execute();

                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    var idx = round[i];

                    bool gotHp = idx.TryGetResult(0, out float rHp);
                    bool gotMax = idx.TryGetResult(1, out float rMax);
                    bool gotHume = idx.TryGetResult(2, out float rHume);
                    bool gotAhp = idx.TryGetResult(3, out float rAhp);

                    if (!gotHp && p.HealthModule == 0) continue;

                    float hp = gotHp ? rHp : p.Health;
                    float maxHp = gotMax && rMax > 0f ? rMax : p.MaxHealth;
                    float hume = gotHume ? Math.Max(0f, rHume) : 0f;
                    float ahp = gotAhp ? Math.Max(0f, rAhp) : 0f;

                    p.Health = hp;
                    p.MaxHealth = maxHp > 0f ? maxHp : 100f;
                    p.HumeShield = hume;
                    p.Ahp = ahp;
                }
            }
        }

        // ── 4. FAST POSITION POLLING (Runs every 5-7ms) ──
        public static void PollPositions(List<PlayerInfo> players)
        {
            if (players == null || players.Count == 0) return;

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p.FpcModule != 0)
                        round[i].AddEntry<Vector3>(0, p.FpcModule + Offsets.Fpm_cachedPosition);
                    else if (p.CurRole != 0)
                        round[i].AddEntry<Vector3>(1, p.CurRole + Offsets.Fpc_lastPos);
                }
                map.Execute();

                for (int i = 0; i < players.Count; i++)
                {
                    var idx = round[i];
                    if (idx.TryGetResult(0, out Vector3 pos) && Math.Abs(pos.X) < 20000f)
                    {
                        players[i].Position = pos;
                        players[i].HasPosition = true;
                    }
                    else if (idx.TryGetResult(1, out Vector3 lastPos) && Math.Abs(lastPos.X) < 20000f)
                    {
                        players[i].Position = lastPos;
                        players[i].HasPosition = true;
                    }
                }
            }
        }

        private static ulong _cachedNativeCameraFovAddr = 0;
        private static long _lastCameraFovResolveTicks = 0;

        public static ulong GetCameraFovAddress()
        {
            long now = Stopwatch.GetTimestamp();
            if (_cachedNativeCameraFovAddr != 0 && (now - _lastCameraFovResolveTicks) < (Stopwatch.Frequency / 2))
            {
                return _cachedNativeCameraFovAddr;
            }

            _lastCameraFovResolveTicks = now;

            // 1. Resolve via CameraShakeController singleton -> _camera (+0x20) -> natCam (+0x10) -> +0x170 (FOV)
            ulong cscSf = StaticFields(Offsets.CameraShakeController_TypeInfo);
            if (cscSf != 0)
            {
                ulong cscInst = Mem.Ptr(cscSf + 0x0, false);
                if (cscInst != 0 && cscInst.IsValidVirtualAddress())
                {
                    ulong camObj = Mem.Ptr(cscInst + 0x20, false);
                    if (camObj != 0 && camObj.IsValidVirtualAddress())
                    {
                        ulong natCam = Mem.Ptr(camObj + 0x10, false);
                        if (natCam != 0 && natCam.IsValidVirtualAddress())
                        {
                            _cachedNativeCameraFovAddr = natCam + UnityOffsets.Camera.FOV;
                            return _cachedNativeCameraFovAddr;
                        }
                    }
                }
            }

            // 2. Fallback to active world camera if resolved by WorldFovChanger
            if (_activeWorldCameraObj != 0 && _activeWorldCameraObj.IsValidVirtualAddress())
            {
                ulong natCam = Mem.Ptr(_activeWorldCameraObj + 0x10, false);
                if (natCam != 0 && natCam.IsValidVirtualAddress())
                {
                    _cachedNativeCameraFovAddr = natCam + UnityOffsets.Camera.FOV;
                    return _cachedNativeCameraFovAddr;
                }
            }

            _cachedNativeCameraFovAddr = 0;
            return 0;
        }

        // ── 4. UNIFIED SINGLE-ROUND SCATTER READ (Positions + Camera in ONE PCIe cycle) ──
        public static CameraInfo PollPositionsAndCamera(List<PlayerInfo> players)
        {
            var cam = new CameraInfo { Fov = ActiveWorldFov };
            if (players == null || players.Count == 0)
            {
                return ReadCamera();
            }

            ulong sf = StaticFields(Offsets.MainCameraController_TypeInfo);
            int camIndex = players.Count;
            ulong fovAddr = GetCameraFovAddress();

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p.FpcModule != 0)
                        round[i].AddEntry<Vector3>(0, p.FpcModule + Offsets.Fpm_cachedPosition);
                    else if (p.CurRole != 0)
                        round[i].AddEntry<Vector3>(1, p.CurRole + Offsets.Fpc_lastPos);
                }

                if (sf != 0)
                {
                    round[camIndex].AddEntry<Vector3>(0, sf + Offsets.MCC_LastPosition);
                    round[camIndex].AddEntry<Quaternion>(1, sf + Offsets.MCC_LastRotation);
                    if (fovAddr != 0)
                    {
                        round[camIndex].AddEntry<float>(2, fovAddr);
                    }
                }

                map.Execute();

                for (int i = 0; i < players.Count; i++)
                {
                    var idx = round[i];
                    if (idx.TryGetResult(0, out Vector3 pos) && Math.Abs(pos.X) < 20000f)
                    {
                        players[i].Position = pos;
                        players[i].HasPosition = true;
                    }
                    else if (idx.TryGetResult(1, out Vector3 lastPos) && Math.Abs(lastPos.X) < 20000f)
                    {
                        players[i].Position = lastPos;
                        players[i].HasPosition = true;
                    }
                }

                if (sf != 0)
                {
                    var idx = round[camIndex];
                    if (idx.TryGetResult(0, out Vector3 cPos) && idx.TryGetResult(1, out Quaternion cRot))
                    {
                        bool looksDefault = Math.Abs(cPos.X) < 1f && Math.Abs(cPos.Z) < 1f && Math.Abs(cPos.Y - 3500f) < 50f;
                        float qn = cRot.X * cRot.X + cRot.Y * cRot.Y + cRot.Z * cRot.Z + cRot.W * cRot.W;
                        bool rotOk = qn > 0.5f && qn < 1.5f;

                        if (!looksDefault && rotOk)
                        {
                            cam.Position = cPos;
                            cam.Rotation = cRot;
                            if (fovAddr != 0 && idx.TryGetResult(2, out float dynFov) && dynFov >= 10.0f && dynFov <= 170.0f)
                            {
                                cam.Fov = dynFov;
                            }
                            cam.Valid = true;
                            return cam;
                        }
                    }
                }
            }

            return ReadCamera();
        }

        public static SyncInfo ReadSyncInfo(ulong controllerInstance)
        {
            ulong infoAddr = controllerInstance + Offsets.AWC_Info_ScenarioId;

            return new SyncInfo
            {
                ScenarioId = Mem.Val<byte>(infoAddr),
                ScenarioType = Mem.Val<byte>(infoAddr + 0x01),
                StartTime = Mem.Val<double>(controllerInstance + Offsets.AWC_Info_StartTime)
            };
        }

        public static PanelWarheadInfo ReadPanelWarheadTime()
        {
            var info = new PanelWarheadInfo 
            { 
                State = WarheadState.Off, 
                TimeRemainingSeconds = 0,
                StatusType = -1,
                RawTime = 0
            };

            ulong sf = StaticFields(Offsets.AlphaWarheadOutsitePanel_TypeInfo);
            if (sf == 0) return info;

            int statusType = Mem.Val<int>(sf + Offsets.AWOP_static_lastTextType);
            int rawTime = Mem.Val<int>(sf + Offsets.AWOP_static_lastDisplayedTime);

            info.StatusType = statusType;
            info.RawTime = rawTime;

            // AlphaWarheadOutsitePanel.UpdateType enum:
            // 0: Empty (detonated/blank)
            // 1: Zero ("00:00.00", detonated)
            // 2: Wait ("WAIT", cooldown)
            // 3: Ready ("READY", lever armed)
            // 4: Disabled ("DISABLED", lever disarmed/off)
            // 5: Time (countdown active, digits updating, rawTime in centiseconds)
            // 6: Skip (countdown active, digits unchanged)
            switch (statusType)
            {
                case 5: // Time (active countdown)
                case 6: // Skip (active countdown, digits unchanged)
                    info.State = WarheadState.Active;
                    info.TimeRemainingSeconds = rawTime > 0 ? rawTime / 100f : 0f;
                    break;

                case 3: // Ready ("READY", lever armed)
                    info.State = WarheadState.Ready;
                    info.TimeRemainingSeconds = 0;
                    break;

                case 2: // Wait ("WAIT", cooldown)
                    info.State = WarheadState.Cooldown;
                    info.TimeRemainingSeconds = 0;
                    break;

                case 0: // Empty
                case 1: // Zero ("00:00.00")
                    info.State = WarheadState.Detonated;
                    info.TimeRemainingSeconds = 0;
                    break;

                case 4: // Disabled ("DISABLED", lever disarmed/off)
                default:
                    info.State = WarheadState.Off;
                    info.TimeRemainingSeconds = 0;
                    break;
            }

            // Cross-validate with AlphaWarheadController singleton
            ulong ctrlSf = StaticFields(Offsets.AlphaWarheadController_TypeInfo);
            if (ctrlSf != 0)
            {
                ulong singleton = Mem.Ptr(ctrlSf + Offsets.AWC_static_Singleton);
                if (singleton != 0)
                {
                    double startTime = Mem.Val<double>(singleton + Offsets.AWC_Info_StartTime, false);
                    bool alreadyDetonated = Mem.Val<bool>(singleton + 0x70, false);

                    if (alreadyDetonated)
                    {
                        info.State = WarheadState.Detonated;
                    }
                    else if (startTime <= 0 && info.State == WarheadState.Active)
                    {
                        // Controller says warhead is NOT running, override false active state
                        info.State = WarheadState.Off;
                        info.TimeRemainingSeconds = 0;
                    }
                    else if (startTime > 0 && info.State != WarheadState.Active)
                    {
                        // Controller says warhead IS running
                        info.State = WarheadState.Active;
                    }
                }
            }

            return info;
        }

        // ── ROOM DMA READING ──
        private static List<RoomInfo> _cachedRoomsList = new();
        private static DateTime _lastRoomFetch = DateTime.MinValue;

        public static void ResetRoundCaches()
        {
            _cachedLocalHub = 0;
            _cachedLocalHubForInv = 0;
            _cachedInventoryPtr = 0;
            _cachedRoomsList.Clear();
            _cachedGenerators.Clear();
            _cachedNativeCameraFovAddr = 0;
            _lastRoomFetch = DateTime.MinValue;
            _lastGenFetch = DateTime.MinValue;
        }

        public static List<RoomInfo> ReadRooms()
        {
            if (_cachedRoomsList.Count > 0)
                return _cachedRoomsList;

            ulong sf = StaticFields(Offsets.RoomIdentifier_TypeInfo);
            if (sf == 0) return _cachedRoomsList;

            ulong hashSetAddr = Mem.Ptr(sf + Offsets.RI_static_AllRoomIdentifiers);
            if (hashSetAddr == 0) return _cachedRoomsList;

            List<ulong> roomPtrs = HashSetElements(hashSetAddr);
            if (roomPtrs.Count == 0) return _cachedRoomsList;

            var rooms = new List<RoomInfo>();

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < roomPtrs.Count; i++)
                {
                    var idx = round[i];
                    idx.AddEntry<int>(0, roomPtrs[i] + Offsets.RI_Name);
                    idx.AddEntry<int>(1, roomPtrs[i] + Offsets.RI_Zone);
                    idx.AddEntry<Vector3>(2, roomPtrs[i] + Offsets.RI_WorldspaceBounds);
                }
                map.Execute();

                for (int i = 0; i < roomPtrs.Count; i++)
                {
                    var idx = round[i];
                    if (idx.TryGetResult(0, out int nameVal) && idx.TryGetResult(2, out Vector3 pos))
                    {
                        idx.TryGetResult(1, out int zoneVal);
                        var roomName = (RoomName)nameVal;
                        var roomZone = (FacilityZone)zoneVal;
                        if (roomZone <= FacilityZone.None || roomZone > FacilityZone.Other)
                        {
                            roomZone = RoomConfig.GetZone(roomName);
                        }

                        rooms.Add(new RoomInfo
                        {
                            Address = roomPtrs[i],
                            Name = roomName,
                            Zone = roomZone,
                            Position = pos
                        });
                    }
                }
            }
            _cachedRoomsList = rooms;
            _lastRoomFetch = DateTime.UtcNow;
            return rooms;
        }

        // ── GENERATOR DMA READING ──
        private static List<GeneratorInfo> _cachedGenerators = new();
        private static DateTime _lastGenFetch = DateTime.MinValue;

        private static Vector3 ReadNativeTransformPosition(ulong nativeTr)
        {
            if (nativeTr == 0 || !nativeTr.IsValidVirtualAddress()) return Vector3.Zero;
            try
            {
                ulong hier = Mem.Ptr(nativeTr + Offsets.NativeTr_hierarchy);
                int index = Mem.Val<int>(nativeTr + Offsets.NativeTr_index);
                if (hier == 0 || !hier.IsValidVirtualAddress() || index < 0 || index > 65536) return Vector3.Zero;

                ulong vertsAddr = Mem.Ptr(hier + Offsets.Hier_vertices);
                ulong indicesAddr = Mem.Ptr(hier + Offsets.Hier_indices);
                int cap = Mem.Val<int>(hier + Offsets.Hier_capacity);
                if (!vertsAddr.IsValidVirtualAddress() || !indicesAddr.IsValidVirtualAddress() || cap <= 0 || cap > 65536 || index >= cap)
                    return Vector3.Zero;

                // Read vertex at index
                var vert = Mem.Val<UnityTransform.TrsX>(vertsAddr + (ulong)(index * Unsafe.SizeOf<UnityTransform.TrsX>()), false);
                Vector3 pos = vert.t;

                int parent = Mem.Val<int>(indicesAddr + (ulong)(index * 4), false);
                int iter = 0;
                while (parent >= 0 && parent < cap && iter++ < 100)
                {
                    var pt = Mem.Val<UnityTransform.TrsX>(vertsAddr + (ulong)(parent * Unsafe.SizeOf<UnityTransform.TrsX>()), false);
                    pos = pt.q.Multiply(pos) * pt.s + pt.t;
                    parent = Mem.Val<int>(indicesAddr + (ulong)(parent * 4), false);
                }

                if (Math.Abs(pos.X) < 20000f && Math.Abs(pos.Y) < 20000f && Math.Abs(pos.Z) < 20000f)
                    return pos;
            }
            catch { }
            return Vector3.Zero;
        }

        private static Vector3 ResolveNativeTransformPosition(ulong nativeTr)
        {
            if (nativeTr == 0 || !nativeTr.IsValidVirtualAddress()) return Vector3.Zero;
            Vector3 directPos = ReadNativeTransformPosition(nativeTr);
            if (directPos != Vector3.Zero) return directPos;

            try
            {
                var ut = new UnityTransform(nativeTr, false);
                ut.UpdatePosition();
                if (ut.HasValidPosition && Math.Abs(ut.Position.X) < 20000f)
                    return ut.Position;
            }
            catch { }

            return Vector3.Zero;
        }

        private static Vector3 ResolveTransformFromComponent(ulong comp)
        {
            if (comp == 0 || !comp.IsValidVirtualAddress()) return Vector3.Zero;

            try
            {
                ulong nativeComp = Mem.Ptr(comp + 0x10);
                if (nativeComp == 0 || !nativeComp.IsValidVirtualAddress()) return Vector3.Zero;

                // If nativeComp itself is already a native Transform
                Vector3 direct = ResolveNativeTransformPosition(nativeComp);
                if (direct != Vector3.Zero) return direct;

                // Candidate offsets for GameObject pointer on native Component: 0x20, 0x30, 0x58
                ulong[] goOffsets = [0x20, 0x30, 0x58];
                for (int g = 0; g < goOffsets.Length; g++)
                {
                    ulong nativeGo = Mem.Ptr(nativeComp + goOffsets[g]);
                    if (nativeGo == 0 || !nativeGo.IsValidVirtualAddress()) continue;

                    // Candidate offsets for ComponentArray on native GameObject: 0x20, 0x30, 0x58
                    ulong[] compArrayOffsets = [0x20, 0x30, 0x58];
                    for (int a = 0; a < compArrayOffsets.Length; a++)
                    {
                        ulong compArray = Mem.Ptr(nativeGo + compArrayOffsets[a]);
                        if (compArray == 0 || !compArray.IsValidVirtualAddress()) continue;

                        // Component[0] at compArray + 0x8 is ALWAYS the native Transform
                        ulong nativeTr = Mem.Ptr(compArray + 0x8);
                        if (nativeTr != 0 && nativeTr.IsValidVirtualAddress())
                        {
                            Vector3 p = ResolveNativeTransformPosition(nativeTr);
                            if (p != Vector3.Zero) return p;
                        }
                    }
                }
            }
            catch { }

            return Vector3.Zero;
        }

        public static Vector3 GetGeneratorPosition(ulong gen, ulong parentRoom)
        {
            if (gen != 0 && gen.IsValidVirtualAddress())
            {
                // 1. Try directly from the Generator Component -> GameObject -> Transform
                Vector3 pos = ResolveTransformFromComponent(gen);
                if (pos != Vector3.Zero) return pos;

                // 2. Try _localGauge (0x100) -> _gauge Transform (0x10) -> nativeTr (0x10)
                ulong localGauge = Mem.Ptr(gen + 0x100);
                if (localGauge != 0 && localGauge.IsValidVirtualAddress())
                {
                    ulong gaugeTr = Mem.Ptr(localGauge + 0x10);
                    if (gaugeTr != 0 && gaugeTr.IsValidVirtualAddress())
                    {
                        ulong nativeTr = Mem.Ptr(gaugeTr + 0x10);
                        pos = ResolveNativeTransformPosition(nativeTr);
                        if (pos != Vector3.Zero) return pos;
                    }
                }

                // 3. Try _totalGauge (0x108) -> _gauge Transform (0x10) -> nativeTr (0x10)
                ulong totalGauge = Mem.Ptr(gen + 0x108);
                if (totalGauge != 0 && totalGauge.IsValidVirtualAddress())
                {
                    ulong gaugeTr = Mem.Ptr(totalGauge + 0x10);
                    if (gaugeTr != 0 && gaugeTr.IsValidVirtualAddress())
                    {
                        ulong nativeTr = Mem.Ptr(gaugeTr + 0x10);
                        pos = ResolveNativeTransformPosition(nativeTr);
                        if (pos != Vector3.Zero) return pos;
                    }
                }

                // 4. Try sub-components: _doorAnimator (0x90), _leverAnimator (0x98), _audioSource (0xA0), _identityCache (0x60)
                ulong[] subOffsets = [0x90, 0x98, 0xA0, 0x60];
                for (int s = 0; s < subOffsets.Length; s++)
                {
                    ulong subComp = Mem.Ptr(gen + subOffsets[s]);
                    if (subComp != 0 && subComp.IsValidVirtualAddress())
                    {
                        pos = ResolveTransformFromComponent(subComp);
                        if (pos != Vector3.Zero) return pos;
                    }
                }
            }

            // Fallback: parent room bounds center if generator transform cannot be resolved
            if (parentRoom != 0 && parentRoom.IsValidVirtualAddress())
            {
                var pos = Mem.Val<Vector3>(parentRoom + Offsets.RI_WorldspaceBounds, false);
                if (Math.Abs(pos.X) < 20000f)
                    return pos;
            }

            return Vector3.Zero;
        }

        public static List<GeneratorInfo> ReadGenerators()
        {
            if (_cachedGenerators.Count >= 3 || ((DateTime.UtcNow - _lastGenFetch).TotalSeconds < 15.0 && _cachedGenerators.Count > 0))
                return _cachedGenerators;

            List<ulong>? genPtrs = null;

            // 1. Try Scp079Recontainer.AllGenerators
            ulong sf = StaticFields(Offsets.Scp079Recontainer_TypeInfo);
            if (sf != 0)
            {
                ulong hashSetAddr = Mem.Ptr(sf + Offsets.SR_static_AllGenerators);
                if (hashSetAddr != 0)
                {
                    var list = HashSetElements(hashSetAddr);
                    if (list.Count > 0) genPtrs = list;
                }
            }

            // 2. Fallback: SpawnableStructure.AllInstances (filter StructureType == 3)
            if (genPtrs == null || genPtrs.Count == 0)
            {
                ulong spawnSf = StaticFields(Offsets.SpawnableStructure_TypeInfo);
                if (spawnSf != 0)
                {
                    ulong allInstAddr = Mem.Ptr(spawnSf + Offsets.SS_static_AllInstances);
                    if (allInstAddr != 0)
                    {
                        var allInst = HashSetElements(allInstAddr);
                        var found = new List<ulong>();
                        for (int i = 0; i < allInst.Count; i++)
                        {
                            ulong inst = allInst[i];
                            int sType = Mem.Val<int>(inst + Offsets.SS_StructureType, false);
                            if (sType == 3) // StructureType.Scp079Generator
                            {
                                found.Add(inst);
                            }
                        }
                        if (found.Count > 0) genPtrs = found;
                    }
                }
            }

            if (genPtrs == null || genPtrs.Count == 0) return _cachedGenerators;

            var generators = new List<GeneratorInfo>();
            for (int i = 0; i < genPtrs.Count; i++)
            {
                ulong genAddr = genPtrs[i];
                if (genAddr == 0) continue;

                ulong parentRoom = Mem.Ptr(genAddr + Offsets.GEN_ParentRoom);
                RoomName rName = RoomName.Unnamed;
                if (parentRoom != 0)
                {
                    int nameVal = Mem.Val<int>(parentRoom + Offsets.RI_Name);
                    rName = (RoomName)nameVal;
                }

                Vector3 pos = GetGeneratorPosition(genAddr, parentRoom);
                byte flags = Mem.Val<byte>(genAddr + Offsets.GEN_Flags, false);
                short syncTime = Mem.Val<short>(genAddr + Offsets.GEN_SyncTime, false);
                float totalActivationTime = Mem.Val<float>(genAddr + Offsets.GEN_TotalActivationTime, false);

                generators.Add(new GeneratorInfo
                {
                    Address = genAddr,
                    Room = rName,
                    Position = pos,
                    Flags = flags,
                    SyncTime = syncTime,
                    TotalActivationTime = totalActivationTime,
                    ParentRoom = parentRoom
                });
            }

            _cachedGenerators = generators;
            _lastGenFetch = DateTime.UtcNow;
            return generators;
        }

        public static void PollGeneratorState(List<GeneratorInfo> generators)
        {
            if (generators == null || generators.Count == 0) return;

            using (var map = ScatterReadMap.Get())
            {
                var round = map.AddRound(useCache: false);
                for (int i = 0; i < generators.Count; i++)
                {
                    if (generators[i].Address != 0)
                    {
                        round[i].AddEntry<byte>(0, generators[i].Address + Offsets.GEN_Flags);
                        round[i].AddEntry<short>(1, generators[i].Address + Offsets.GEN_SyncTime);
                    }
                }
                map.Execute();

                for (int i = 0; i < generators.Count; i++)
                {
                    var idx = round[i];
                    if (idx.TryGetResult(0, out byte flags)) generators[i].Flags = flags;
                    if (idx.TryGetResult(1, out short st)) generators[i].SyncTime = st;
                }
            }
        }

        // ── ITEM PICKUP DMA READING ──
        public static int LastSpawnedCount { get; private set; }
        public static ulong LastSpawnedDictAddr { get; private set; }
        public static int RawPickupCount { get; private set; }

        private static readonly ConcurrentDictionary<ulong, bool> _pickupSubclassCache = new();
        private static readonly Dictionary<ulong, ItemPickupInfo> _cachedPickups = new();
        private static DateTime _lastPickupFetch = DateTime.MinValue;

        private static bool IsItemPickupSubclass(ulong klass)
        {
            if (klass == 0) return false;
            if (_pickupSubclassCache.TryGetValue(klass, out bool cached))
                return cached;

            ulong pickupTi = TypeinfoClass(Offsets.ItemPickupBase_TypeInfo);

            ulong cur = klass;
            bool result = false;
            for (int i = 0; i < 32 && cur != 0; ++i)
            {
                if (pickupTi != 0 && cur == pickupTi) { result = true; break; }
                ulong namePtr = Mem.Ptr(cur + Offsets.Il2CppClass_name);
                if (namePtr != 0)
                {
                    string name = Mem.Str(namePtr, 64) ?? string.Empty;
                    if (name.EndsWith("Pickup") || name.EndsWith("PickupBase") || name == "ThrownProjectile" || name.Contains("Pickup"))
                    {
                        result = true;
                        break;
                    }
                }
                cur = Mem.Ptr(cur + Offsets.Il2CppClass_parent);
            }
            _pickupSubclassCache[klass] = result;
            return result;
        }

        public static Vector3 GetPickupPosition(ulong pickupAddr, ulong identityAddr = 0)
        {
            if (pickupAddr == 0 && identityAddr == 0) return Vector3.Zero;

            // 1. Try managed Transform cached field at pickupAddr + 0xF8
            if (pickupAddr != 0)
            {
                ulong managedTransform = Mem.Ptr(pickupAddr + Offsets.ItemPickupBase_transform);
                if (managedTransform != 0)
                {
                    ulong nativeTr = Mem.Ptr(managedTransform + 0x10);
                    Vector3 directPos = ResolveNativeTransformPosition(nativeTr);
                    if (directPos != Vector3.Zero) return directPos;
                }
            }

            // 2. Fallback: Component -> GameObject -> ComponentArray -> Component[0] (native Transform)
            ulong[] components = [pickupAddr, identityAddr];
            for (int c = 0; c < components.Length; c++)
            {
                ulong comp = components[c];
                if (comp == 0) continue;

                Vector3 pos = ResolveTransformFromComponent(comp);
                if (pos != Vector3.Zero) return pos;
            }

            return Vector3.Zero;
        }

        public static List<ItemPickupInfo> ReadPickups()
        {
            if ((DateTime.UtcNow - _lastPickupFetch).TotalMilliseconds < 500.0 && _cachedPickups.Count > 0)
            {
                var list = new List<ItemPickupInfo>(_cachedPickups.Count);
                foreach (var v in _cachedPickups.Values) list.Add(v);
                return list;
            }

            ulong sf = StaticFields(Offsets.NetworkClient_TypeInfo);
            if (sf == 0)
            {
                var list = new List<ItemPickupInfo>(_cachedPickups.Count);
                foreach (var v in _cachedPickups.Values) list.Add(v);
                return list;
            }

            ulong spawnedDict = Mem.Ptr(sf + Offsets.NC_static_spawned);
            if (spawnedDict == 0)
            {
                var list = new List<ItemPickupInfo>(_cachedPickups.Count);
                foreach (var v in _cachedPickups.Values) list.Add(v);
                return list;
            }

            int count = Mem.Val<int>(spawnedDict + Offsets.Dictionary_count);
            ulong entriesArr = Mem.Ptr(spawnedDict + Offsets.Dictionary_entries);
            LastSpawnedDictAddr = spawnedDict;
            LastSpawnedCount = count;

            if (count <= 0 || count > 4096 || entriesArr == 0)
            {
                var list = new List<ItemPickupInfo>(_cachedPickups.Count);
                foreach (var v in _cachedPickups.Values) list.Add(v);
                return list;
            }

            // Each dictionary entry is 24 bytes (hashCode: 4, next: 4, key: 4, padding: 4, value: 8)
            int bytesNeeded = count * 24;
            byte[]? rented = null;
            Span<byte> buf = bytesNeeded <= 2048
                ? stackalloc byte[bytesNeeded]
                : (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(bytesNeeded)).AsSpan(0, bytesNeeded);

            var seenIdentities = new HashSet<ulong>();
            try
            {
                Diag.CountRead();
                DmaMemory.ReadBuffer<byte>(entriesArr + Offsets.Array_items, buf, true);

                for (int i = 0; i < count; i++)
                {
                    int offset = i * 24;
                    int hashCode = MemoryMarshal.Read<int>(buf.Slice(offset, 4));
                    if (hashCode < 0) continue;

                    ulong identityPtr = MemoryMarshal.Read<ulong>(buf.Slice(offset + 16, 8));
                    if (!identityPtr.IsValidVirtualAddress()) continue;

                    seenIdentities.Add(identityPtr);

                    // If already known and tracked, keep existing ItemPickupInfo
                    if (_cachedPickups.ContainsKey(identityPtr))
                        continue;

                    // Inspect NetworkBehaviours
                    ulong behavioursArr = Mem.Ptr(identityPtr + 0x58);
                    if (behavioursArr == 0) continue;

                    int bCount = Mem.Val<int>(behavioursArr + Offsets.Array_max_length);
                    if (bCount <= 0 || bCount > 64) continue;

                    ulong pickupBehaviour = 0;
                    for (int b = 0; b < Math.Min(bCount, 16); b++)
                    {
                        ulong bPtr = Mem.Ptr(behavioursArr + Offsets.Array_items + (ulong)(b * 8));
                        if (bPtr == 0) continue;
                        ulong klass = Mem.Ptr(bPtr + 0x0);
                        if (IsItemPickupSubclass(klass))
                        {
                            pickupBehaviour = bPtr;
                            break;
                        }
                    }
                    if (pickupBehaviour == 0) continue;

                    // Read ItemType
                    int itemTypeId = Mem.Val<int>(pickupBehaviour + Offsets.ItemPickupBase_Info + Offsets.PickupSyncInfo_ItemId);
                    if (itemTypeId < 0 || itemTypeId > 69) continue;

                    ItemType itemType = (ItemType)itemTypeId;
                    Vector3 pos = GetPickupPosition(pickupBehaviour, identityPtr);
                    if (pos == Vector3.Zero || Math.Abs(pos.X) >= 20000f) continue;

                    var info = new ItemPickupInfo
                    {
                        Address = pickupBehaviour,
                        ItemType = itemType,
                        Category = ItemConfig.GetCategory(itemType),
                        DisplayName = ItemConfig.GetDisplayName(itemType),
                        Position = pos,
                        Color = ItemConfig.GetColor(itemType)
                    };

                    _cachedPickups[identityPtr] = info;
                }

                // Remove stale pickups that despawned
                if (_cachedPickups.Count > 0)
                {
                    var toRemove = new List<ulong>();
                    foreach (var kvp in _cachedPickups)
                    {
                        if (!seenIdentities.Contains(kvp.Key))
                            toRemove.Add(kvp.Key);
                    }
                    for (int r = 0; r < toRemove.Count; r++)
                    {
                        _cachedPickups.Remove(toRemove[r]);
                    }
                }
            }
            catch { }
            finally
            {
                if (rented != null)
                    System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }

            _lastPickupFetch = DateTime.UtcNow;
            RawPickupCount = _cachedPickups.Count;
            var resultList = new List<ItemPickupInfo>(_cachedPickups.Count);
            foreach (var v in _cachedPickups.Values) resultList.Add(v);
            return resultList;
        }

        public static LocalInventoryState PollLocalInventory()
        {
            var state = new LocalInventoryState();
            try
            {
                ulong localHub = LocalHub();
                if (localHub == 0 || !localHub.IsValidVirtualAddress())
                    return state;

                ulong inv = Mem.Ptr(localHub + Offsets.RH_inventory);
                if (inv == 0 || !inv.IsValidVirtualAddress())
                    return state;

                ulong userInv = Mem.Ptr(inv + Offsets.Inventory_UserInventory);
                if (userInv == 0 || !userInv.IsValidVirtualAddress())
                    return state;

                // 1. Read Held Items (Dictionary<ushort, ItemBase>)
                ulong itemsDict = Mem.Ptr(userInv + Offsets.InventoryInfo_Items);
                if (itemsDict != 0 && itemsDict.IsValidVirtualAddress())
                {
                    int count = Mem.Val<int>(itemsDict + Offsets.Dictionary_count);
                    ulong entriesArr = Mem.Ptr(itemsDict + Offsets.Dictionary_entries);
                    if (count > 0 && count <= 32 && entriesArr != 0 && entriesArr.IsValidVirtualAddress())
                    {
                        int bytesNeeded = count * 24;
                        Span<byte> buf = stackalloc byte[bytesNeeded];
                        DmaMemory.ReadBuffer<byte>(entriesArr + Offsets.Array_items, buf, false);

                        for (int i = 0; i < count; i++)
                        {
                            int offset = i * 24;
                            int hashCode = MemoryMarshal.Read<int>(buf.Slice(offset, 4));
                            if (hashCode < 0) continue;

                            ulong itemBasePtr = MemoryMarshal.Read<ulong>(buf.Slice(offset + 16, 8));
                            if (itemBasePtr == 0 || !itemBasePtr.IsValidVirtualAddress()) continue;

                            int itemTypeId = Mem.Val<int>(itemBasePtr + Offsets.ItemBase_ItemTypeId, false);
                            var itemType = (ItemType)itemTypeId;
                            if (itemType != ItemType.None)
                            {
                                state.HeldItems.Add(itemType);
                                state.HeldItemSet.Add(itemType);

                                // Keycard permissions & tier
                                var perms = ItemConfig.GetKeycardPermissions(itemType);
                                if (perms != DoorPermissionFlags.None)
                                {
                                    state.CombinedKeycardPermissions |= perms;
                                    int tier = ItemConfig.GetKeycardTier(itemType);
                                    if (tier > state.HighestKeycardTier) state.HighestKeycardTier = tier;
                                }

                                // Armor tier
                                var armorTier = ItemConfig.GetArmorTier(itemType);
                                if (armorTier > state.HighestArmorTier) state.HighestArmorTier = armorTier;

                                // Weapon caliber
                                var caliber = ItemConfig.GetWeaponCaliber(itemType);
                                if (caliber != AmmoCaliber.None) state.CarriedWeaponCalibers.Add(caliber);
                            }
                        }
                    }
                }

                // 2. Read Reserve Ammo (Dictionary<ItemType, ushort>)
                ulong ammoDict = Mem.Ptr(userInv + Offsets.InventoryInfo_ReserveAmmo);
                if (ammoDict != 0 && ammoDict.IsValidVirtualAddress())
                {
                    int ammoCount = Mem.Val<int>(ammoDict + Offsets.Dictionary_count);
                    ulong ammoEntriesArr = Mem.Ptr(ammoDict + Offsets.Dictionary_entries);
                    if (ammoCount > 0 && ammoCount <= 16 && ammoEntriesArr != 0 && ammoEntriesArr.IsValidVirtualAddress())
                    {
                        int bytes = ammoCount * 16;
                        Span<byte> ammoBuf = stackalloc byte[bytes];
                        DmaMemory.ReadBuffer<byte>(ammoEntriesArr + Offsets.Array_items, ammoBuf, false);

                        for (int i = 0; i < ammoCount; i++)
                        {
                            int offset = i * 16;
                            int hashCode = MemoryMarshal.Read<int>(ammoBuf.Slice(offset, 4));
                            if (hashCode < 0) continue;

                            int ammoTypeId = MemoryMarshal.Read<int>(ammoBuf.Slice(offset + 8, 4));
                            ushort countVal = MemoryMarshal.Read<ushort>(ammoBuf.Slice(offset + 12, 2));
                            var ammoCaliber = ItemConfig.GetAmmoCaliber((ItemType)ammoTypeId);
                            if (ammoCaliber != AmmoCaliber.None)
                            {
                                state.AmmoCounts[ammoCaliber] = countVal;
                            }
                        }
                    }
                }
            }
            catch { }

            return state;
        }

        public static CameraInfo ReadCamera()
        {
            var cam = new CameraInfo
            {
                Fov = ActiveWorldFov
            };

            ulong fovAddr = GetCameraFovAddress();
            if (fovAddr != 0)
            {
                float dynFov = Mem.Val<float>(fovAddr, false);
                if (dynFov >= 10.0f && dynFov <= 170.0f)
                {
                    cam.Fov = dynFov;
                }
            }

            ulong sf = StaticFields(Offsets.MainCameraController_TypeInfo);
            if (sf != 0)
            {
                var pos = Mem.Val<Vector3>(sf + Offsets.MCC_LastPosition, false);
                var rot = Mem.Val<Quaternion>(sf + Offsets.MCC_LastRotation, false);

                // Validate: skip default menu camera ~(0, 3500, 0)
                bool looksDefault = Math.Abs(pos.X) < 1f && Math.Abs(pos.Z) < 1f && Math.Abs(pos.Y - 3500f) < 50f;
                float qn = rot.X * rot.X + rot.Y * rot.Y + rot.Z * rot.Z + rot.W * rot.W;
                bool rotOk = qn > 0.5f && qn < 1.5f;

                if (!looksDefault && rotOk)
                {
                    cam.Position = pos;
                    cam.Rotation = rot;
                    cam.Valid = true;
                    return cam;
                }
            }

            ulong local = LocalHub();
            if (local != 0)
            {
                ulong roleManager = Mem.Ptr(local + Offsets.RH_roleManager);
                ulong curRole = roleManager != 0 ? Mem.Ptr(roleManager + Offsets.PRM_curRole) : 0;
                var p = ReadPlayerPosition(curRole);
                if (p.HasValue)
                {
                    cam.Position = p.Value;
                    cam.Position.Y += 0.85f;
                    if (sf != 0)
                    {
                        var rot = Mem.Val<Quaternion>(sf + Offsets.MCC_LastRotation, false);
                        float qn = rot.X * rot.X + rot.Y * rot.Y + rot.Z * rot.Z + rot.W * rot.W;
                        if (qn > 0.5f && qn < 1.5f) cam.Rotation = rot;
                    }
                    cam.Valid = true;
                }
            }

            return cam;
        }

        public static string RoleName(RoleTypeId r) => r switch
        {
            RoleTypeId.None => "None", RoleTypeId.Scp173 => "Peanut", RoleTypeId.ClassD => "Class-D",
            RoleTypeId.Spectator => "Spectator", RoleTypeId.Scp106 => "Larry",
            RoleTypeId.NtfSpecialist => "NTF", RoleTypeId.Scp049 => "Doctor",
            RoleTypeId.Scientist => "Scientist", RoleTypeId.Scp079 => "Computer",
            RoleTypeId.ChaosConscript => "Chaos", RoleTypeId.Scp096 => "ShyGuy",
            RoleTypeId.Scp0492 => "Zombie", RoleTypeId.NtfSergeant => "NTF",
            RoleTypeId.NtfCaptain => "NTF", RoleTypeId.NtfPrivate => "NTF",
            RoleTypeId.Tutorial => "Tutorial", RoleTypeId.FacilityGuard => "Guard",
            RoleTypeId.Scp939 => "Dog", RoleTypeId.CustomRole => "Custom",
            RoleTypeId.ChaosRifleman => "Chaos", RoleTypeId.ChaosMarauder => "Chaos",
            RoleTypeId.ChaosRepressor => "Chaos", RoleTypeId.Overwatch => "Overwatch",
            RoleTypeId.Filmmaker => "Filmmaker", RoleTypeId.Scp3114 => "SCP-3114",
            RoleTypeId.Destroyed => "Destroyed", RoleTypeId.Flamingo => "Flamingo",
            RoleTypeId.AlphaFlamingo => "Alpha Flamingo", RoleTypeId.ZombieFlamingo => "Zombie Flamingo",
            RoleTypeId.NtfFlamingo => "NTF Flamingo", RoleTypeId.ChaosFlamingo => "Chaos Flamingo",
            _ => "Unknown"
        };

        public static Team TeamFromRole(RoleTypeId r) => r switch
        {
            RoleTypeId.Scp173 or RoleTypeId.Scp106 or RoleTypeId.Scp049 or RoleTypeId.Scp079
            or RoleTypeId.Scp096 or RoleTypeId.Scp0492 or RoleTypeId.Scp939 or RoleTypeId.Scp3114 => Team.SCPs,
            RoleTypeId.NtfSpecialist or RoleTypeId.NtfSergeant or RoleTypeId.NtfCaptain
            or RoleTypeId.NtfPrivate or RoleTypeId.FacilityGuard or RoleTypeId.NtfFlamingo => Team.FoundationForces,
            RoleTypeId.ChaosConscript or RoleTypeId.ChaosRifleman or RoleTypeId.ChaosMarauder
            or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosFlamingo => Team.ChaosInsurgency,
            RoleTypeId.Scientist => Team.Scientists,
            RoleTypeId.ClassD => Team.ClassD,
            RoleTypeId.Spectator or RoleTypeId.Overwatch or RoleTypeId.Filmmaker
            or RoleTypeId.Destroyed or RoleTypeId.None => Team.Dead,
            RoleTypeId.Flamingo or RoleTypeId.AlphaFlamingo or RoleTypeId.ZombieFlamingo => Team.Flamingos,
            _ => Team.OtherAlive
        };

        public static bool RoleIsAlive(RoleTypeId r) => TeamFromRole(r) != Team.Dead;

        // ═══════════════════════════════════════════════════════════
        //  WARHEAD INTERPOLATOR
        // ═══════════════════════════════════════════════════════════
        public static class WarheadTracker
        {
            private static float _lastReadTime = 0f;
            private static readonly Stopwatch _sw = Stopwatch.StartNew();
            private static double _lastSyncSeconds = 0;

            public static float GetSmoothTime(PanelWarheadInfo info)
            {
                if (info.State != WarheadState.Active)
                    return info.TimeRemainingSeconds;

                // Resync baseline whenever DMA reads a new value from memory
                if (Math.Abs(info.TimeRemainingSeconds - _lastReadTime) > 0.001f)
                {
                    _lastReadTime = info.TimeRemainingSeconds;
                    _lastSyncSeconds = _sw.Elapsed.TotalSeconds;
                }

                double elapsedSinceSync = _sw.Elapsed.TotalSeconds - _lastSyncSeconds;
                float currentRemaining = (float)(_lastReadTime - elapsedSinceSync);

                return Math.Max(0f, currentRemaining);
            }
        }


        // ═══════════════════════════════════════════════════════════
        //  OVERLAY FORM (optimised rendering)
        // ═══════════════════════════════════════════════════════════
        public class ScpslOverlay : Overlay
        {
            private sealed class GameSnapshot
            {
                public readonly List<PlayerInfo> Players;
                public readonly List<RoomInfo> Rooms;
                public readonly CameraInfo Camera;
                public readonly PanelWarheadInfo Warhead;
                public readonly List<GeneratorInfo> Generators;
                public readonly List<ItemPickupInfo> Pickups;
                public readonly LocalInventoryState LocalInventory;

                public GameSnapshot(List<PlayerInfo> players, List<RoomInfo> rooms, CameraInfo camera, PanelWarheadInfo warhead, List<GeneratorInfo> generators, List<ItemPickupInfo> pickups, LocalInventoryState localInventory)
                {
                    Players = players;
                    Rooms = rooms;
                    Camera = camera;
                    Warhead = warhead;
                    Generators = generators;
                    Pickups = pickups;
                    LocalInventory = localInventory;
                }
            }

            private static volatile GameSnapshot _currentSnapshot = new(new(), new(), default, default, new(), new(), new());
            private static volatile LocalInventoryState _latestInventory = new();
            private static long _lastInventoryPollTicks;
            private static PanelWarheadInfo _latestWarhead;
            private static volatile bool _latestRoundStarted;
            private static volatile int _latestRoundTime;
            private static readonly Stopwatch _localRoundStopwatch = new();
            private static volatile AliveSpectatableInfo _latestAliveInfo = new();
            private static volatile HashSet<ulong> _latestDeadHubs = new();
            private static bool _dmaRunning;
            private static long _lastBonePollTicks;
            private static long _lastSpectatorPollTicks;
            private static int _roundEndConsecutiveTicks;
            private static int _roundStartConsecutiveTicks;

            // Lock-free double-buffered camera for zero-allocation FastLoop
            private static CameraInfo _camBuf0;
            private static CameraInfo _camBuf1;
            private static volatile int _camReadIdx;

            private static void UpdateCamera(in CameraInfo cam)
            {
                int nextWrite = 1 - _camReadIdx;
                if (nextWrite == 0) _camBuf0 = cam; else _camBuf1 = cam;
                _camReadIdx = nextWrite;
            }

            public static CameraInfo GetLatestCamera()
            {
                return _camReadIdx == 0 ? _camBuf0 : _camBuf1;
            }

            public static bool LatestRoundStarted => _latestRoundStarted;
            public static int LatestRoundTime => _latestRoundTime;
            public static IReadOnlyList<PlayerInfo> CurrentPlayers => _currentSnapshot.Players;
            public static Stopwatch LocalRoundStopwatch => _localRoundStopwatch;

            private int _overlayWidth;
            private int _overlayHeight;
            private int _targetFps;

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            public void UpdateBounds(int x, int y, int width, int height, int refreshRate = 0)
            {
                _overlayWidth = width;
                _overlayHeight = height;
                if (refreshRate > 0)
                {
                    _targetFps = refreshRate;
                }
                Position = new System.Drawing.Point(x, y);
                Size = new System.Drawing.Size(width, height);
                IntPtr hwnd = GetWindowHandle();
                if (hwnd != IntPtr.Zero)
                {
                    SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, 0x0014); // SWP_NOZORDER | SWP_NOACTIVATE
                }
            }

            private static readonly string[] CachedRoleNames = new string[35];
            private static readonly Vector2[] CachedRoleTextSizes = new Vector2[35];
            private static readonly string[] MenuTabs = { "Esp", "MemWrites", "Settings" };

            static ScpslOverlay()
            {
                for (int i = 0; i < CachedRoleNames.Length; i++)
                {
                    CachedRoleNames[i] = GameReader.RoleName((RoleTypeId)i);
                }
            }

            private static ScpslOverlay? _instance;
            private bool _showDiag = false;
            private int _hudX = 10;
            private int _hudY = 10;
            private bool _isDraggingEnabled;
            private bool _isDraggingHud;

            // Pure C# bit-shifting color packer (avoids calling native ImGui before context initialization)
            private static uint PackColor(float r, float g, float b, float a) =>
                ((uint)(a * 255f) << 24) |
                ((uint)(b * 255f) << 16) |
                ((uint)(g * 255f) << 8)  |
                (uint)(r * 255f);

            // Direct3D / ImGui Color Helpers (ABGR Format)
            private static readonly uint ColBg = PackColor(0.05f, 0.05f, 0.07f, 0.7f);
            private static readonly uint ColDiagBg = PackColor(0.03f, 0.03f, 0.05f, 0.8f);
            private static readonly uint ColWhite = PackColor(1f, 1f, 1f, 1f);
            private static readonly uint ColGreen = PackColor(0f, 1f, 0f, 1f);
            private static readonly uint ColRed = PackColor(1f, 0f, 0f, 1f);
            private static readonly uint ColGray = PackColor(0.6f, 0.6f, 0.6f, 1f);
            private static readonly uint ColCyan = PackColor(0f, 1f, 1f, 1f);

            private static readonly uint ColScp = PackColor(0.86f, 0.2f, 0.2f, 1f);
            private static readonly uint ColNtf = PackColor(0.15f, 0.55f, 1f, 1f);
            private static readonly uint ColChaos = PackColor(0.2f, 0.78f, 0.27f, 1f);
            private static readonly uint ColSci = PackColor(0.94f, 0.86f, 0.2f, 1f);
            private static readonly uint ColClassD = PackColor(0.94f, 0.58f, 0.12f, 1f);
            private static readonly uint ColFlamingo = PackColor(1f, 0.39f, 0.7f, 1f);
            private static readonly uint ColOther = PackColor(0.63f, 0.63f, 0.63f, 1f);

            public ScpslOverlay(int width, int height, int maxFps = 144) : base("SCPSL", true)
            {
                _instance = this;
                _overlayWidth = width;
                _overlayHeight = height;
                _targetFps = maxFps;
            }

            protected override Task PostInitialized()
            {
                unsafe
                {
                    ImGui.GetIO().NativePtr->IniFilename = null;
                }
                CleanStaleImguiIni();

                VSync = true;
                Position = new System.Drawing.Point(0, 0);
                Size = new System.Drawing.Size(_overlayWidth, _overlayHeight);
                GetWindowHandle();

                MonitorChooser.OnPostInitialized(this);

                bool keepConsole = File.Exists(ConsoleCfgPath);
                _showConsoleOnStart = keepConsole;
                _showConsole = keepConsole;
                if (!keepConsole)
                {
                    SetConsoleVisible(false);
                }

                string fontPath = @"C:\Windows\Fonts\arial.ttf";
                if (System.IO.File.Exists(fontPath))
                {
                    ReplaceFont(fontPath, 16, FontGlyphRangeType.English);
                }

                // Apply SCPSLDMA Dark/Purple Theme
                var style = ImGui.GetStyle();
                style.WindowRounding = 14.0f;
                style.ChildRounding = 10.0f;
                style.FrameRounding = 8.0f;
                style.PopupRounding = 8.0f;
                style.ScrollbarRounding = 10.0f;
                style.GrabRounding = 6.0f;
                style.WindowPadding = new Vector2(14, 14);
                style.FramePadding = new Vector2(10, 6);
                style.ItemSpacing = new Vector2(10, 10);
                style.ItemInnerSpacing = new Vector2(8, 6);

                var colors = style.Colors;
                colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.05f, 0.08f, 0.98f);
                colors[(int)ImGuiCol.ChildBg] = new Vector4(0.09f, 0.08f, 0.12f, 0.95f);
                colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.07f, 0.10f, 0.98f);
                colors[(int)ImGuiCol.Border] = new Vector4(0.18f, 0.16f, 0.24f, 0.70f);
                colors[(int)ImGuiCol.FrameBg] = new Vector4(0.12f, 0.11f, 0.16f, 1.00f);
                colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.18f, 0.16f, 0.24f, 1.00f);
                colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.24f, 0.20f, 0.32f, 1.00f);
                colors[(int)ImGuiCol.TitleBg] = new Vector4(0.06f, 0.05f, 0.08f, 1.00f);
                colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.05f, 0.08f, 1.00f);
                colors[(int)ImGuiCol.CheckMark] = new Vector4(0.68f, 0.45f, 1.00f, 1.00f);
                colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.68f, 0.45f, 1.00f, 1.00f);
                colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.78f, 0.58f, 1.00f, 1.00f);
                colors[(int)ImGuiCol.Button] = new Vector4(0.13f, 0.12f, 0.18f, 1.00f);
                colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.22f, 0.19f, 0.30f, 1.00f);
                colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.58f, 0.35f, 0.95f, 0.85f);
                colors[(int)ImGuiCol.Header] = new Vector4(0.58f, 0.35f, 0.95f, 0.35f);
                colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.58f, 0.35f, 0.95f, 0.55f);
                colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.58f, 0.35f, 0.95f, 0.80f);
                colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.89f, 0.94f, 1.00f);
                colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.44f, 0.42f, 0.54f, 1.00f);

                LoadConfig();

                return Task.CompletedTask;
            }

            public static void StartDMALoop(ScpslOverlay overlay)
            {
                if (_dmaRunning) return;
                _dmaRunning = true;

                var thread1 = new Thread(FastLoop) { Name = "DMA-Positions-Camera", IsBackground = true, Priority = ThreadPriority.AboveNormal };
                var thread2 = new Thread(HealthLoop) { Name = "DMA-Health", IsBackground = true };
                var thread3 = new Thread(DiscoveryLoop) { Name = "DMA-Discovery", IsBackground = true };
                var thread4 = new Thread(RoleLoop) { Name = "DMA-Roles", IsBackground = true };

                thread1.Start();
                thread2.Start();
                thread3.Start();
                thread4.Start();
            }

            private static void FastLoop()
            {
                var sw = new Stopwatch();
                using var waitTimer = new WaitTimer();
                while (_dmaRunning)
                {
                    if (ScpslMemory.Instance == null || !ScpslMemory.Instance.Ready)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    sw.Restart();
                    var currentPlayers = _currentSnapshot.Players;
                    CameraInfo cam;

                    if (currentPlayers.Count > 0)
                    {
                        cam = GameReader.PollPositionsAndCamera(currentPlayers);
                        if (_skeletonEsp || _headCircleEsp || (GameReader.MasterMemWritesEnabled && GameReader.ViewangleAimEnabled))
                        {
                            long now = Stopwatch.GetTimestamp();

                            // Instant bones: check if any alive player in range lacks bones and resolve immediately
                            for (int pIdx = 0; pIdx < currentPlayers.Count; pIdx++)
                            {
                                var p = currentPlayers[pIdx];
                                if (p != null && p.Alive && !p.IsLocal && !p.HasBones && p.HasPosition &&
                                    p.Role != RoleTypeId.Scp173 && p.Role != RoleTypeId.Scp079)
                                {
                                    if (now - p.LastBoneAttemptTicks > (Stopwatch.Frequency / 20)) // 50ms rate limit
                                    {
                                        p.LastBoneAttemptTicks = now;
                                        GameReader.SetupPlayerBones(p);
                                    }
                                }
                            }

                            // Throttle bone buffer polling to ~60 Hz (every 16.6ms) to prevent DMA bus saturation
                            if ((now - _lastBonePollTicks) >= (Stopwatch.Frequency / 60))
                            {
                                _lastBonePollTicks = now;
                                Vector3 camFwd = Vector3.Zero;
                                if (cam.Valid)
                                {
                                    var r = cam.Rotation;
                                    float xz = r.X * r.Z, wy = r.W * r.Y;
                                    float yz = r.Y * r.Z, wx = r.W * r.X;
                                    float xx = r.X * r.X, yy = r.Y * r.Y;
                                    camFwd = new Vector3(2f * (xz + wy), 2f * (yz - wx), 1f - 2f * (xx + yy));
                                }
                                GameReader.PollBones(currentPlayers, cam.Position, camFwd, _maxDistance, !_skeletonEsp);
                            }
                        }
                    }
                    else
                    {
                        cam = GameReader.ReadCamera();
                    }

                    ulong localHub = 0;
                    ulong equippedItem = 0;
                    if (GameReader.NoSwayEnabled || GameReader.NoRecoilEnabled || GameReader.NoFlashEnabled || GameReader.GunFlashlightEnabled || GameReader.InstantAdsEnabled || GameReader.HasPendingRestores)
                    {
                        localHub = GameReader.LocalHub();
                        if (localHub != 0 && (GameReader.NoSwayEnabled || GameReader.NoRecoilEnabled || GameReader.GunFlashlightEnabled || GameReader.InstantAdsEnabled || GameReader.HasPendingRestores))
                        {
                            equippedItem = GameReader.GetEquippedItem(localHub);
                        }
                    }

                    GameReader.PollWorldFov();
                    GameReader.PollViewmodelFov();
                    GameReader.PollNoSway(localHub, equippedItem);
                    GameReader.PollNoRecoil(localHub, equippedItem);
                    GameReader.PollInstantAds(localHub, equippedItem);
                    GameReader.PollGunFlashlight(localHub, equippedItem);
                    GameReader.PollBunnyhop();
                    GameReader.PollViewangleAim(cam, currentPlayers);
                    GameReader.PollNoFlash(localHub);
                    GameReader.PollTpv(ref cam, localHub);
                    UpdateCamera(cam);

                    // Atomically publish lock-free snapshot only when player list reference changes
                    var curSnap = _currentSnapshot;
                    if (curSnap.Players != currentPlayers)
                    {
                        _currentSnapshot = new GameSnapshot(currentPlayers, curSnap.Rooms, cam, _latestWarhead, curSnap.Generators, curSnap.Pickups, _latestInventory);
                    }

                    sw.Stop();
                    double elapsed = sw.Elapsed.TotalMilliseconds;
                    Diag.LastPosMs = elapsed;
                    Diag.LastCameraMs = elapsed;
                    Diag.Update(elapsed);

                    int waitMs = (int)Math.Max(1, 5 - elapsed);
                    waitTimer.AutoWait(TimeSpan.FromMilliseconds(waitMs));
                }
            }

            private static void HealthLoop()
            {
                var sw = new Stopwatch();
                using var waitTimer = new WaitTimer();
                while (_dmaRunning)
                {
                    if (ScpslMemory.Instance == null || !ScpslMemory.Instance.Ready)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    sw.Restart();
                    var currentPlayers = _currentSnapshot.Players;

                    if (currentPlayers.Count > 0)
                    {
                        GameReader.PollHealth(currentPlayers);
                    }

                    var currentGenerators = _currentSnapshot.Generators;
                    if (currentGenerators != null && currentGenerators.Count > 0)
                    {
                        GameReader.PollGeneratorState(currentGenerators);
                    }

                    // Visual Enhancements (Fog, Brightness) at 20Hz (~50ms cadence)
                    if (GameReader.MasterMemWritesEnabled)
                    {
                        ulong localHub = GameReader.LocalHub();
                        if (GameReader.LessFogEnabled || GameReader.HasPendingRestores)
                        {
                            GameReader.PollLessFog();
                        }
                        if (GameReader.BrightnessEnabled || GameReader.HasPendingRestores)
                        {
                            GameReader.PollBrightness(localHub);
                        }
                    }

                    // Poll warhead and round progress at 20Hz (~50ms)
                    _latestWarhead = GameReader.ReadPanelWarheadTime();
                    bool localAlive = GameReader.IsLocalPlayerAlive();
                    bool hasLivingPlayers = localAlive || GameReader.HasLivingPlayers(currentPlayers);
                    bool roundStartedRaw = hasLivingPlayers || GameReader.IsRoundStarted(currentPlayers);
                    _latestRoundTime = GameReader.ReadRoundTime();
                    bool isEnded = !hasLivingPlayers && GameReader.IsRoundEnded(currentPlayers);

                    // Throttle spectatable alive hubs and dead ragdolls to 2 Hz (500ms) to save PCIe scatter traffic
                    long now = Stopwatch.GetTimestamp();
                    if ((now - _lastSpectatorPollTicks) >= (Stopwatch.Frequency / 2))
                    {
                        _lastSpectatorPollTicks = now;
                        _latestAliveInfo = GameReader.ReadAliveSpectatableHubs();
                        _latestDeadHubs = GameReader.ReadDeadPlayerHubs();
                    }

                    // Debounced round edge detection (20Hz / ~50ms cadence)
                    if (_latestRoundStarted)
                    {
                        // Currently IN ROUND. Check if the round has ended.
                        bool appearsEnded = !hasLivingPlayers && (isEnded || (!roundStartedRaw && _latestRoundTime == 0));

                        if (appearsEnded)
                        {
                            _roundEndConsecutiveTicks++;
                            // If explicit isRoundEnded summary screen from game, confirm in 10 ticks (500ms).
                            // If inferred from lobby reset (no living players & roundTime=0), require 40 ticks (2.0s).
                            int requiredTicks = isEnded ? 10 : 40;
                            if (_roundEndConsecutiveTicks >= requiredTicks)
                            {
                                _latestRoundStarted = false;
                                _localRoundStopwatch.Reset();
                                _roundEndConsecutiveTicks = 0;
                                _roundStartConsecutiveTicks = 0;
                                GameReader.ResetRoundCaches();
                                MemDMABase.OnRoundEnded();
                                DmaBase.DMA.Features.IFeature.DispatchRoundEnd();
                                GameReader.RestoreViewangleAim();
                                GameReader.RestoreNoFlash();
                                GameReader.RestoreNoSmoke();
                                GameReader.RestoreBrightness();
                                GameReader.TryRestoreInstantAds();
                                GameReader.TryRestoreTpv();
                                Log.WriteLine("[DMA] SCP:SL Round Ended / Waiting for Players.");
                            }
                        }
                        else
                        {
                            _roundEndConsecutiveTicks = 0;
                        }
                    }
                    else
                    {
                        // Currently NOT IN ROUND (Waiting for players / Lobby). Check if round has started.
                        if (hasLivingPlayers || (roundStartedRaw && !isEnded))
                        {
                            _roundStartConsecutiveTicks++;
                            // Fast startup: 2 consecutive positive ticks (100ms) to filter out single-tick memory noise
                            if (_roundStartConsecutiveTicks >= 2)
                            {
                                _latestRoundStarted = true;
                                _localRoundStopwatch.Restart();
                                _roundStartConsecutiveTicks = 0;
                                _roundEndConsecutiveTicks = 0;
                                GameReader.ResetRoundCaches();
                                MemDMABase.OnRoundStarted();
                                DmaBase.DMA.Features.IFeature.DispatchRoundStart();
                                Log.WriteLine("[DMA] SCP:SL Round Started!");
                            }
                        }
                        else
                        {
                            _roundStartConsecutiveTicks = 0;
                        }
                    }

                    sw.Stop();
                    Diag.LastHealthMs = sw.Elapsed.TotalMilliseconds;

                    waitTimer.AutoWait(TimeSpan.FromMilliseconds(50));
                }
            }

            private static void DiscoveryLoop()
            {
                using var waitTimer = new WaitTimer();
                while (_dmaRunning)
                {
                    if (ScpslMemory.Instance == null || !ScpslMemory.Instance.Ready)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    try
                    {
                        var players = GameReader.PollPlayerList(_currentSnapshot.Players);
                        var rooms = GameReader.ReadRooms(); // Static locations
                        var warhead = GameReader.ReadPanelWarheadTime();
                        _latestWarhead = warhead;
                        var generators = GameReader.ReadGenerators();
                        var pickups = GameReader.ReadPickups();
                        var inv = GameReader.PollLocalInventory();
                        _latestInventory = inv;

                        _currentSnapshot = new GameSnapshot(players, rooms, _currentSnapshot.Camera, warhead, generators, pickups, inv);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"[DMA Discovery Thread Error] {ex.Message}");
                    }

                    waitTimer.AutoWait(TimeSpan.FromMilliseconds(2000));
                }
            }

            private static void RoleLoop()
            {
                var sw = new Stopwatch();
                using var waitTimer = new WaitTimer();
                while (_dmaRunning)
                {
                    if (ScpslMemory.Instance == null || !ScpslMemory.Instance.Ready)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    sw.Restart();
                    var currentPlayers = _currentSnapshot.Players;

                    if (currentPlayers.Count > 0)
                    {
                        GameReader.PollRoles(currentPlayers);
                    }

                    // Responsive inventory polling (~250ms cadence)
                    long nowTicks = Stopwatch.GetTimestamp();
                    if ((double)(nowTicks - _lastInventoryPollTicks) / Stopwatch.Frequency >= 0.25)
                    {
                        _lastInventoryPollTicks = nowTicks;
                        var inv = GameReader.PollLocalInventory();
                        _latestInventory = inv;
                    }

                    sw.Stop();
                    Diag.LastRoleMs = sw.Elapsed.TotalMilliseconds;

                    waitTimer.AutoWait(TimeSpan.FromMilliseconds(50));
                }
            }

            private uint GetRoleColor(RoleTypeId role) => role switch
            {
                RoleTypeId.Scp173 or RoleTypeId.Scp106 or RoleTypeId.Scp049 or RoleTypeId.Scp096 
                or RoleTypeId.Scp939 or RoleTypeId.Scp0492 or RoleTypeId.Scp3114 or RoleTypeId.Scp079 => ColRed,
                RoleTypeId.Scientist => ColSci,
                RoleTypeId.ClassD => ColClassD,
                RoleTypeId.NtfSpecialist or RoleTypeId.NtfSergeant or RoleTypeId.NtfCaptain or RoleTypeId.NtfPrivate => ColNtf,
                RoleTypeId.ChaosConscript or RoleTypeId.ChaosRifleman or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor => ColChaos,
                RoleTypeId.Spectator => ColGray,
                _ => ColWhite
            };

            private uint GetTeamColor(Team team) => team switch
            {
                Team.SCPs => ColScp,
                Team.FoundationForces => ColNtf,
                Team.ChaosInsurgency => ColChaos,
                Team.Scientists => ColSci,
                Team.ClassD => ColClassD,
                Team.Flamingos => ColFlamingo,
                _ => ColOther
            };

            public static bool WorldToScreen(Vector3 world, Vector3 camPos, Quaternion camRot,
                float fovYDeg, float screenW, float screenH, out float sx, out float sy)
            {
                var cam = new CameraInfo { Position = camPos, Rotation = camRot, Fov = fovYDeg, Valid = true };
                var frame = new CameraFrame(in cam, screenW, screenH);
                return frame.Project(in world, out sx, out sy);
            }


            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(int vKey);

            private bool _f6WasDown;
            private bool _f7WasDown;
            private bool _endWasDown;

            private const int VK_END = 0x23;
            private const int VK_F6 = 0x75;
            private const int VK_F7 = 0x76;
            private bool _showMenu = true;
            private bool _prevShowMenu = true;
            private bool _insertWasDown;
            private bool _menuKeyWasDown;
            private bool _menuUnityWasDown;
            private const int VK_INSERT = 0x2D;
            public static UnityKeyCode MenuKey = UnityKeyCode.F1;
            private static bool _isSelectingMenuKey = false;
            private static int _selectingMenuKeyCooldown = 0;

            // Menu navigation state
            // --- DELETE/REPLACE EXISTING MENU STATE VARIABLES WITH THIS: ---
            private int _selectedTab = 0; // Default to Esp tab

            // Distance Filter
            private static float _maxDistance = 250f;

            // Visuals Settings
            private bool _espEnabled = true;
            private bool _boundingBox = true;
            private bool _nameTags = true;
            private bool _healthBar = true;
            private static bool _skeletonEsp = true;
            private static bool _headCircleEsp = true;
            private static float _headCircleRadius = 5.0f;
            private bool _showRoomEsp = true;
            private static bool _roomCurrentZoneOnly = true;
            private bool _showGeneratorEsp = true;
            private static bool _genCurrentZoneOnly = true;
            private bool _itemEspEnabled = true;
            private static float _itemMaxDistance = 30f;

            // Item Display Styles
            private static bool _showItemIcons = true;
            private static bool _showItemText = true;
            private static float _itemIconSize = 24.0f;

            // SCP Display Styles
            private static bool _showScpIcons = true;
            private static float _scpIconSize = 28.0f;

            // Category Visibility Filters (All 7 independent toggles)
            private static bool _showKeycards = true;
            private static bool _showWeapons = true;
            private static bool _showAmmo = true;
            private static bool _showArmor = true;
            private static bool _showMedical = true;
            private static bool _showScpItems = true;
            private static bool _showUtility = true;

            // Smart Inventory-Aware Culling Filters
            private static bool _filterMaxAmmo = true;              // If ammo full for caliber, cull ammo
            private static bool _filterSmartAmmo = false;           // If no gun carried for caliber, cull ammo
            private static bool _filterBestArmor = true;            // If wearing Heavy Armor, cull all armors
            private static bool _filterInferiorArmor = true;        // If wearing Combat Armor, cull Light Armor
            private static bool _filterInferiorKeycards = true;     // If held keycard grants all perms, cull floor card
            private static bool _filterDuplicateKeycards = true;    // Cull identical keycards
            private static bool _filterDuplicateUtility = false;    // Cull duplicate utility items (Radio, Light)
            private static int _filterMinKeycardTier = 0;           // 0=All, 1=Scientist+, 2=Guard+, 3=Operative+, 4=Captain+, 5=O5 Only
            private static int _filterMinArmorTier = 0;             // 0=All, 1=Light+, 2=Combat+, 3=Heavy Only

            private static bool _isSelectingTpvKey = false;
            private static int _selectingTpvKeyCooldown = 0;

            // Icon Resource Caching
            private static readonly Dictionary<ItemType, IntPtr> _iconPointers = new();
            private static readonly Dictionary<ItemType, Vector2> _iconOriginalSizes = new();
            private static readonly HashSet<ItemType> _missingIcons = new();

            private IntPtr GetOrLoadItemIcon(ItemType itemType, out Vector2 origSize)
            {
                if (_iconPointers.TryGetValue(itemType, out var cachedPtr))
                {
                    origSize = _iconOriginalSizes[itemType];
                    return cachedPtr;
                }

                if (_missingIcons.Contains(itemType))
                {
                    origSize = Vector2.Zero;
                    return IntPtr.Zero;
                }

                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string name = $"{itemType}.png";
                    string iconPath = Path.Combine(baseDir, "Icons", name);
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(baseDir, "src", "Unity", "Icons", name);
                    }
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(baseDir, "..", "src", "Unity", "Icons", name);
                    }

                    if (File.Exists(iconPath))
                    {
                        AddOrGetImagePointer(iconPath, false, out IntPtr handle, out uint w, out uint h);
                        if (handle != IntPtr.Zero)
                        {
                            _iconPointers[itemType] = handle;
                            origSize = new Vector2(w, h);
                            _iconOriginalSizes[itemType] = origSize;
                            return handle;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"[Icon Load Warning] Failed to load icon for {itemType}: {ex.Message}");
                }

                _missingIcons.Add(itemType);
                origSize = Vector2.Zero;
                return IntPtr.Zero;
            }

            // SCP Icon Resource Caching
            private static readonly Dictionary<RoleTypeId, IntPtr> _scpIconPointers = new();
            private static readonly Dictionary<RoleTypeId, Vector2> _scpIconOriginalSizes = new();
            private static readonly HashSet<RoleTypeId> _missingScpIcons = new();

            private IntPtr GetOrLoadScpIcon(RoleTypeId role, out Vector2 origSize)
            {
                if (_scpIconPointers.TryGetValue(role, out var cachedPtr))
                {
                    origSize = _scpIconOriginalSizes[role];
                    return cachedPtr;
                }

                if (_missingScpIcons.Contains(role))
                {
                    origSize = Vector2.Zero;
                    return IntPtr.Zero;
                }

                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string name = $"{role}.png";
                    string iconPath = Path.Combine(baseDir, "Icons", name);
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(baseDir, "src", "Unity", "Icons", name);
                    }
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(baseDir, "..", "src", "Unity", "Icons", name);
                    }

                    if (File.Exists(iconPath))
                    {
                        AddOrGetImagePointer(iconPath, false, out IntPtr handle, out uint w, out uint h);
                        if (handle != IntPtr.Zero)
                        {
                            _scpIconPointers[role] = handle;
                            origSize = new Vector2(w, h);
                            _scpIconOriginalSizes[role] = origSize;
                            return handle;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"[Icon Load Warning] Failed to load icon for SCP {role}: {ex.Message}");
                }

                _missingScpIcons.Add(role);
                origSize = Vector2.Zero;
                return IntPtr.Zero;
            }

            private static bool ShouldRenderPickup(ItemPickupInfo item, LocalInventoryState inv)
            {
                // 1. Category Visibility Filters
                switch (item.Category)
                {
                    case ItemCategory.Keycard:
                        if (!_showKeycards) return false;
                        break;
                    case ItemCategory.Weapon:
                        if (!_showWeapons) return false;
                        break;
                    case ItemCategory.Ammo:
                        if (!_showAmmo) return false;
                        break;
                    case ItemCategory.Armor:
                        if (!_showArmor) return false;
                        break;
                    case ItemCategory.Medical:
                        if (!_showMedical) return false;
                        break;
                    case ItemCategory.ScpItem:
                        if (!_showScpItems) return false;
                        break;
                    case ItemCategory.Utility:
                    default:
                        if (!_showUtility) return false;
                        break;
                }

                // 2. Armor Culling
                if (item.Category == ItemCategory.Armor)
                {
                    var itemArmorTier = ItemConfig.GetArmorTier(item.ItemType);

                    // If player has best armor (Heavy Armor), don't show any armor
                    if (_filterBestArmor && inv.HasBestArmor)
                        return false;

                    // If wearing better armor, hide inferior armor
                    if (_filterInferiorArmor && inv.HighestArmorTier > ArmorTier.None && itemArmorTier <= inv.HighestArmorTier)
                        return false;

                    // Minimum Armor Tier
                    if (_filterMinArmorTier > 0 && (int)itemArmorTier < _filterMinArmorTier)
                        return false;
                }

                // 3. Ammo Culling
                if (item.Category == ItemCategory.Ammo)
                {
                    var caliber = ItemConfig.GetAmmoCaliber(item.ItemType);
                    if (caliber != AmmoCaliber.None)
                    {
                        // Max Ammo Culling: if player already has max ammo for this caliber, don't show ammo
                        if (_filterMaxAmmo && inv.IsAmmoFull(caliber))
                            return false;

                        // Smart Ammo: if player has no gun compatible with this caliber, don't show ammo
                        if (_filterSmartAmmo && !inv.CarriedWeaponCalibers.Contains(caliber))
                            return false;
                    }
                }

                // 4. Keycard Hierarchy & Culling
                if (item.Category == ItemCategory.Keycard)
                {
                    var groundPerms = ItemConfig.GetKeycardPermissions(item.ItemType);
                    int groundTier = ItemConfig.GetKeycardTier(item.ItemType);

                    // Min Keycard Tier
                    if (_filterMinKeycardTier > 0 && groundTier < _filterMinKeycardTier)
                        return false;

                    // Hide Duplicate Keycards (already holding this exact card)
                    if (_filterDuplicateKeycards && inv.HeldItemSet.Contains(item.ItemType))
                        return false;

                    // Hide Inferior Keycards (held cards already grant all permissions of this card)
                    if (_filterInferiorKeycards && inv.CombinedKeycardPermissions != DoorPermissionFlags.None && groundPerms != DoorPermissionFlags.None)
                    {
                        if ((groundPerms & ~inv.CombinedKeycardPermissions) == 0)
                            return false;
                    }
                }

                // 5. Utility / General Duplicate Culling
                if (_filterDuplicateUtility && item.Category == ItemCategory.Utility)
                {
                    if (inv.HeldItemSet.Contains(item.ItemType))
                        return false;
                }

                return true;
            }
            private bool _showHud = false;
            private bool _hudShowTitle = true;
            private bool _hudShowAlive = true;
            private bool _hudShowDead = true;
            private bool _hudShowWarhead = true;
            private bool _hudShowGenerators = true;
            private bool _hudShowRound = true;

            // Settings UI State Variables
            private bool _showWatermark = false;
            private bool _autoSave = true;
            private static bool _suppressAutoSave = false;
            public static bool AutoSaveEnabled => (_instance?._autoSave ?? true) && !_suppressAutoSave;
            private bool _debugInput = false;
            private readonly List<string> _recentInputEvents = new();
            private readonly HashSet<UnityKeyCode> _previouslyHeldKeys = new();
            private static bool _isSelectingZoomKey = false;
            private static int _selectingZoomKeyCooldown = 0;
            private static bool _isSelectingAimKey = false;
            private static int _selectingAimKeyCooldown = 0;

            public static string FormatKeyName(UnityKeyCode key)
            {
                return key switch
                {
                    UnityKeyCode.Mouse0 => "Left Click (Mouse0)",
                    UnityKeyCode.Mouse1 => "Right Click (Mouse1)",
                    UnityKeyCode.Mouse2 => "Middle Click (Mouse2)",
                    UnityKeyCode.Mouse3 => "Mouse 4 (XButton1)",
                    UnityKeyCode.Mouse4 => "Mouse 5 (XButton2)",
                    UnityKeyCode.LeftAlt => "Left Alt",
                    UnityKeyCode.RightAlt => "Right Alt",
                    UnityKeyCode.LeftShift => "Left Shift",
                    UnityKeyCode.RightShift => "Right Shift",
                    UnityKeyCode.LeftControl => "Left Ctrl",
                    UnityKeyCode.RightControl => "Right Ctrl",
                    UnityKeyCode.Space => "Space",
                    UnityKeyCode.None => "None",
                    _ => key.ToString()
                };
            }

            private static bool IsKeyPressedGlobal(int vKey, ref bool wasDown)
            {
                bool isDown = (GetAsyncKeyState(vKey) & 0x8000) != 0;
                bool pressed = isDown && !wasDown;
                wasDown = isDown;
                return pressed;
            }

            [DllImport("user32.dll")]
            private static extern bool GetCursorPos(out POINT lpPoint);

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int X;
                public int Y;
            }

            private int _dragOffsetX;
            private int _dragOffsetY;
            private const int VK_LBUTTON = 0x01;

            // Streamer / Capture Bypass (SetWindowDisplayAffinity)
            private static bool _streamerMode = false;
            private static IntPtr _overlayHwnd = IntPtr.Zero;

            private const uint WDA_NONE = 0x00000000;
            private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

            [DllImport("user32.dll")]
            private static extern bool IsWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

            [DllImport("user32.dll")]
            private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

            private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            private const int SW_HIDE = 0;
            private const int SW_SHOW = 5;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FreeConsole();

            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            private static bool _showConsole = false;
            private static bool _showConsoleOnStart = false;
            private static readonly string ConsoleCfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "showconsole.cfg");

            private static IntPtr GetWindowHandle()
            {
                if (_overlayHwnd != IntPtr.Zero && IsWindow(_overlayHwnd))
                    return _overlayHwnd;

                // 1. Check reflection on Overlay fields/properties
                try
                {
                    if (_instance != null)
                    {
                        var fields = typeof(Overlay).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                        foreach (var f in fields)
                        {
                            if (f.FieldType == typeof(IntPtr))
                            {
                                IntPtr val = (IntPtr)f.GetValue(_instance)!;
                                if (val != IntPtr.Zero && IsWindow(val))
                                {
                                    _overlayHwnd = val;
                                    return val;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 2. FindWindow by title
                IntPtr hwnd = FindWindow(null, "SCPSL");
                if (hwnd != IntPtr.Zero && IsWindow(hwnd))
                {
                    _overlayHwnd = hwnd;
                    return hwnd;
                }

                // 3. Enumerate current process windows (exclude console window)
                IntPtr found = IntPtr.Zero;
                uint currentPid = (uint)Environment.ProcessId;
                EnumWindows((h, lParam) =>
                {
                    GetWindowThreadProcessId(h, out uint pid);
                    if (pid == currentPid)
                    {
                        var sb = new System.Text.StringBuilder(256);
                        GetClassName(h, sb, sb.Capacity);
                        if (!sb.ToString().Contains("ConsoleWindowClass"))
                        {
                            found = h;
                            return false;
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                if (found != IntPtr.Zero)
                {
                    _overlayHwnd = found;
                    return found;
                }

                return IntPtr.Zero;
            }

            public static void SetStreamerMode(bool enable)
            {
                _streamerMode = enable;
                IntPtr hwnd = GetWindowHandle();
                if (hwnd != IntPtr.Zero)
                {
                    uint affinity = enable ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE;
                    bool success = SetWindowDisplayAffinity(hwnd, affinity);
                    int err = success ? 0 : Marshal.GetLastWin32Error();
                    Log.WriteLine($"[StreamerMode] SetWindowDisplayAffinity(0x{affinity:X}) => {success} (err: {err}, HWND: 0x{hwnd.ToInt64():X})");
                }
                else
                {
                    Log.WriteLine("[StreamerMode] Could not resolve overlay HWND");
                }
            }

            public static void SetConsoleVisible(bool visible)
            {
                _showConsole = visible;
                try
                {
                    if (visible)
                    {
                        IntPtr hwnd = GetConsoleWindow();
                        if (hwnd == IntPtr.Zero)
                        {
                            if (AllocConsole())
                            {
                                Console.OutputEncoding = Encoding.UTF8;
                                Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8) { AutoFlush = true });
                                Console.SetError(new StreamWriter(Console.OpenStandardError(), Encoding.UTF8) { AutoFlush = true });
                                Log.AllocateConsoleWindow();
                            }
                        }
                        hwnd = GetConsoleWindow();
                        if (hwnd != IntPtr.Zero)
                        {
                            ShowWindow(hwnd, SW_SHOW);
                        }
                    }
                    else
                    {
                        IntPtr hwnd = GetConsoleWindow();
                        if (hwnd != IntPtr.Zero)
                        {
                            ShowWindow(hwnd, SW_HIDE);
                            FreeConsole();
                            Log.FreeConsoleWindow();
                        }
                    }
                }
                catch { }
            }

            private static string _lastConfigStatus = "";
            private static DateTime _configStatusTime = DateTime.MinValue;

            public static void SaveConfig()
            {
                try
                {
                    var cfg = new ConfigData
                    {
                        EspEnabled = _instance?._espEnabled ?? true,
                        BoundingBox = _instance?._boundingBox ?? true,
                        SkeletonEsp = _skeletonEsp,
                        HeadCircleEsp = _headCircleEsp,
                        HeadCircleRadius = _headCircleRadius,
                        NameTags = _instance?._nameTags ?? true,
                        HealthBar = _instance?._healthBar ?? true,
                        ShowScpIcons = _showScpIcons,
                        ScpIconSize = _scpIconSize,
                        MaxDistance = _maxDistance,

                        ShowRoomEsp = _instance?._showRoomEsp ?? true,
                        RoomCurrentZoneOnly = _roomCurrentZoneOnly,
                        ShowGeneratorEsp = _instance?._showGeneratorEsp ?? true,
                        GenCurrentZoneOnly = _genCurrentZoneOnly,

                        ItemEspEnabled = _instance?._itemEspEnabled ?? true,
                        ItemMaxDistance = _itemMaxDistance,
                        ShowItemIcons = _showItemIcons,
                        ShowItemText = _showItemText,
                        ItemIconSize = _itemIconSize,

                        ShowKeycards = _showKeycards,
                        ShowWeapons = _showWeapons,
                        ShowAmmo = _showAmmo,
                        ShowArmor = _showArmor,
                        ShowMedical = _showMedical,
                        ShowScpItems = _showScpItems,
                        ShowUtility = _showUtility,

                        FilterMaxAmmo = _filterMaxAmmo,
                        FilterSmartAmmo = _filterSmartAmmo,
                        FilterBestArmor = _filterBestArmor,
                        FilterInferiorArmor = _filterInferiorArmor,
                        FilterInferiorKeycards = _filterInferiorKeycards,
                        FilterDuplicateKeycards = _filterDuplicateKeycards,
                        FilterDuplicateUtility = _filterDuplicateUtility,
                        FilterMinKeycardTier = _filterMinKeycardTier,
                        FilterMinArmorTier = _filterMinArmorTier,

                        ShowHud = _instance?._showHud ?? false,
                        ShowWatermark = _instance?._showWatermark ?? false,
                        HudShowTitle = _instance?._hudShowTitle ?? true,
                        HudShowAlive = _instance?._hudShowAlive ?? true,
                        HudShowDead = _instance?._hudShowDead ?? true,
                        HudShowWarhead = _instance?._hudShowWarhead ?? true,
                        HudShowGenerators = _instance?._hudShowGenerators ?? true,
                        HudShowRound = _instance?._hudShowRound ?? true,
                        HudX = _instance?._hudX ?? 10,
                        HudY = _instance?._hudY ?? 10,
                        VSync = _instance?.VSync ?? true,

                        AutoSave = _instance?._autoSave ?? true,
                        MenuKey = MenuKey,
                        StreamerMode = _streamerMode,
                        ShowConsole = _showConsole,
                        ShowConsoleOnStart = _showConsoleOnStart,

                        MasterMemWritesEnabled = GameReader.MasterMemWritesEnabled,
                        WorldFovChangerEnabled = GameReader.WorldFovChangerEnabled,
                        CustomWorldFov = GameReader.CustomWorldFov,
                        ZoomFovEnabled = GameReader.ZoomFovEnabled,
                        ZoomFov = GameReader.ZoomFov,
                        ZoomKey = GameReader.ZoomKey,
                        ViewmodelFovChangerEnabled = GameReader.ViewmodelFovChangerEnabled,
                        CustomViewmodelFov = GameReader.CustomViewmodelFov,
                        NoSwayEnabled = GameReader.NoSwayEnabled,
                        NoRecoilEnabled = GameReader.NoRecoilEnabled,
                        RecoilIntensity = GameReader.RecoilIntensity,
                        AutoBunnyhopEnabled = GameReader.AutoBunnyhopEnabled,
                        ViewangleAimEnabled = GameReader.ViewangleAimEnabled,
                        ViewangleAimFov = GameReader.ViewangleAimFov,
                        ViewangleAimSmoothness = GameReader.ViewangleAimSmoothness,
                        ViewangleAimTargetBone = GameReader.ViewangleAimTargetBone,
                        ViewangleAimHeadOffset = GameReader.ViewangleAimHeadOffset,
                        ViewangleAimChestOffset = GameReader.ViewangleAimChestOffset,
                        ViewangleAimTeamCheck = GameReader.ViewangleAimTeamCheck,
                        ViewangleAimDrawFov = GameReader.ViewangleAimDrawFov,
                        ViewangleAimKey = GameReader.ViewangleAimKey,
                        ViewangleAimRequireKey = GameReader.ViewangleAimRequireKey,
                        ViewangleAimKeyMode = GameReader.ViewangleAimKeyMode,

                        NoFlashEnabled = GameReader.NoFlashEnabled,
                        LessFogEnabled = GameReader.LessFogEnabled,
                        FullNoFogEnabled = GameReader.FullNoFogEnabled,
                        LessFogClearness = GameReader.LessFogClearness,
                        LessFogScp244Enabled = GameReader.LessFogScp244Enabled,
                        BrightnessEnabled = GameReader.BrightnessEnabled,
                        BrightnessEv = GameReader.BrightnessEv,
                        GunFlashlightEnabled = GameReader.GunFlashlightEnabled,
                        GunFlashlightSpotAngle = GameReader.GunFlashlightSpotAngle,
                        GunFlashlightIntensityMult = GameReader.GunFlashlightIntensityMult,
                        GunFlashlightRange = GameReader.GunFlashlightRange,

                        InstantAdsEnabled = GameReader.InstantAdsEnabled,
                        TpvEnabled = GameReader.TpvEnabled,
                        TpvDistance = GameReader.TpvDistance,
                        TpvHeight = GameReader.TpvHeight,
                        TpvShoulderOffset = GameReader.TpvShoulderOffset,
                        TpvKey = GameReader.TpvKey,
                        TpvRequireKey = GameReader.TpvRequireKey,
                        TpvKeyMode = GameReader.TpvKeyMode
                    };

                    string json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                    File.WriteAllText(path, json);
                    _lastConfigStatus = "Config saved!";
                    _configStatusTime = DateTime.UtcNow;
                    Log.WriteLine("[Config] Successfully saved config.json");
                }
                catch (Exception ex)
                {
                    _lastConfigStatus = "Save failed";
                    _configStatusTime = DateTime.UtcNow;
                    Log.WriteLine($"[Config] Save failed: {ex.Message}");
                }
            }

            public static void LoadConfig()
            {
                try
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                    if (!File.Exists(path)) return;

                    string json = File.ReadAllText(path);
                    var cfg = JsonSerializer.Deserialize<ConfigData>(json);
                    if (cfg == null) return;

                    if (_instance != null)
                    {
                        _instance._espEnabled = cfg.EspEnabled;
                        _instance._boundingBox = cfg.BoundingBox;
                        _instance._nameTags = cfg.NameTags;
                        _instance._healthBar = cfg.HealthBar;
                        _instance._showRoomEsp = cfg.ShowRoomEsp;
                        _instance._showGeneratorEsp = cfg.ShowGeneratorEsp;
                        _instance._itemEspEnabled = cfg.ItemEspEnabled;

                        _instance._showHud = cfg.ShowHud;
                        _instance._showWatermark = cfg.ShowWatermark;
                        _instance._hudShowTitle = cfg.HudShowTitle;
                        _instance._hudShowAlive = cfg.HudShowAlive;
                        _instance._hudShowDead = cfg.HudShowDead;
                        _instance._hudShowWarhead = cfg.HudShowWarhead;
                        _instance._hudShowGenerators = cfg.HudShowGenerators;
                        _instance._hudShowRound = cfg.HudShowRound;
                        _instance._hudX = cfg.HudX;
                        _instance._hudY = cfg.HudY;
                        _instance.VSync = cfg.VSync;
                        _instance._autoSave = cfg.AutoSave;
                    }

                    _skeletonEsp = cfg.SkeletonEsp;
                    _headCircleEsp = cfg.HeadCircleEsp;
                    _headCircleRadius = cfg.HeadCircleRadius;
                    _showScpIcons = cfg.ShowScpIcons;
                    _scpIconSize = cfg.ScpIconSize;
                    _maxDistance = cfg.MaxDistance;

                    _roomCurrentZoneOnly = cfg.RoomCurrentZoneOnly;
                    _genCurrentZoneOnly = cfg.GenCurrentZoneOnly;

                    _itemMaxDistance = cfg.ItemMaxDistance;
                    _showItemIcons = cfg.ShowItemIcons;
                    _showItemText = cfg.ShowItemText;
                    _itemIconSize = cfg.ItemIconSize;

                    _showKeycards = cfg.ShowKeycards;
                    _showWeapons = cfg.ShowWeapons;
                    _showAmmo = cfg.ShowAmmo;
                    _showArmor = cfg.ShowArmor;
                    _showMedical = cfg.ShowMedical;
                    _showScpItems = cfg.ShowScpItems;
                    _showUtility = cfg.ShowUtility;

                    _filterMaxAmmo = cfg.FilterMaxAmmo;
                    _filterSmartAmmo = cfg.FilterSmartAmmo;
                    _filterBestArmor = cfg.FilterBestArmor;
                    _filterInferiorArmor = cfg.FilterInferiorArmor;
                    _filterInferiorKeycards = cfg.FilterInferiorKeycards;
                    _filterDuplicateKeycards = cfg.FilterDuplicateKeycards;
                    _filterDuplicateUtility = cfg.FilterDuplicateUtility;
                    _filterMinKeycardTier = cfg.FilterMinKeycardTier;
                    _filterMinArmorTier = cfg.FilterMinArmorTier;

                    MenuKey = cfg.MenuKey;
                    if (cfg.StreamerMode != _streamerMode)
                    {
                        SetStreamerMode(cfg.StreamerMode);
                    }
                    _showConsole = cfg.ShowConsole;
                    _showConsoleOnStart = cfg.ShowConsoleOnStart;
                    SetConsoleVisible(cfg.ShowConsole);

                    GameReader.MasterMemWritesEnabled = cfg.MasterMemWritesEnabled;
                    GameReader.WorldFovChangerEnabled = cfg.WorldFovChangerEnabled;
                    GameReader.CustomWorldFov = cfg.CustomWorldFov;
                    GameReader.ZoomFovEnabled = cfg.ZoomFovEnabled;
                    GameReader.ZoomFov = cfg.ZoomFov;
                    GameReader.ZoomKey = cfg.ZoomKey;
                    GameReader.ViewmodelFovChangerEnabled = cfg.ViewmodelFovChangerEnabled;
                    GameReader.CustomViewmodelFov = cfg.CustomViewmodelFov;
                    GameReader.NoSwayEnabled = cfg.NoSwayEnabled;
                    GameReader.NoRecoilEnabled = cfg.NoRecoilEnabled;
                    GameReader.RecoilIntensity = cfg.RecoilIntensity;
                    GameReader.AutoBunnyhopEnabled = cfg.AutoBunnyhopEnabled;
                    GameReader.ViewangleAimEnabled = cfg.ViewangleAimEnabled;
                    GameReader.ViewangleAimFov = cfg.ViewangleAimFov;
                    GameReader.ViewangleAimSmoothness = cfg.ViewangleAimSmoothness;
                    GameReader.ViewangleAimTargetBone = cfg.ViewangleAimTargetBone;
                    GameReader.ViewangleAimHeadOffset = cfg.ViewangleAimHeadOffset;
                    GameReader.ViewangleAimChestOffset = cfg.ViewangleAimChestOffset;
                    GameReader.ViewangleAimTeamCheck = cfg.ViewangleAimTeamCheck;
                    GameReader.ViewangleAimDrawFov = cfg.ViewangleAimDrawFov;
                    GameReader.ViewangleAimKey = cfg.ViewangleAimKey;
                    GameReader.ViewangleAimRequireKey = cfg.ViewangleAimRequireKey;
                    GameReader.ViewangleAimKeyMode = cfg.ViewangleAimKeyMode;

                    GameReader.NoFlashEnabled = cfg.NoFlashEnabled;
                    GameReader.LessFogEnabled = cfg.LessFogEnabled;
                    GameReader.FullNoFogEnabled = cfg.FullNoFogEnabled;
                    GameReader.LessFogClearness = cfg.LessFogClearness;
                    GameReader.LessFogScp244Enabled = cfg.LessFogScp244Enabled;
                    GameReader.BrightnessEnabled = cfg.BrightnessEnabled;
                    GameReader.BrightnessEv = cfg.BrightnessEv;
                    GameReader.GunFlashlightEnabled = cfg.GunFlashlightEnabled;
                    GameReader.GunFlashlightSpotAngle = cfg.GunFlashlightSpotAngle;
                    GameReader.GunFlashlightIntensityMult = cfg.GunFlashlightIntensityMult;
                    GameReader.GunFlashlightRange = cfg.GunFlashlightRange;

                    GameReader.InstantAdsEnabled = cfg.InstantAdsEnabled;
                    GameReader.TpvEnabled = cfg.TpvEnabled;
                    GameReader.TpvDistance = cfg.TpvDistance > 0f ? cfg.TpvDistance : 2.2f;
                    GameReader.TpvHeight = cfg.TpvHeight;
                    GameReader.TpvShoulderOffset = cfg.TpvShoulderOffset;
                    GameReader.TpvKey = cfg.TpvKey;
                    GameReader.TpvRequireKey = cfg.TpvRequireKey;
                    GameReader.TpvKeyMode = cfg.TpvKeyMode;


                    _lastConfigStatus = "Config loaded!";
                    _configStatusTime = DateTime.UtcNow;
                    Log.WriteLine("[Config] Successfully loaded config.json");
                }
                catch (Exception ex)
                {
                    _lastConfigStatus = "Load failed";
                    _configStatusTime = DateTime.UtcNow;
                    Log.WriteLine($"[Config] Load failed: {ex.Message}");
                }
            }

            public static void CleanStaleImguiIni()
            {
                try
                {
                    string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imgui.ini");
                    if (File.Exists(p1)) File.Delete(p1);
                }
                catch { }

                try
                {
                    string p2 = Path.Combine(Directory.GetCurrentDirectory(), "imgui.ini");
                    if (File.Exists(p2)) File.Delete(p2);
                }
                catch { }
            }

            public static void RestartApplication(bool deleteConfig = false, bool deleteMmap = false)
            {
                try
                {
                    if (deleteConfig)
                    {
                        _suppressAutoSave = true;
                        if (_instance != null) _instance._autoSave = false;

                        try
                        {
                            string cfgBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                            if (File.Exists(cfgBase)) File.Delete(cfgBase);
                        }
                        catch { }
                        try
                        {
                            string cfgCwd = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
                            if (File.Exists(cfgCwd)) File.Delete(cfgCwd);
                        }
                        catch { }
                    }

                    if (deleteMmap)
                    {
                        try
                        {
                            string mmapBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mmap.txt");
                            if (File.Exists(mmapBase)) File.Delete(mmapBase);
                        }
                        catch { }
                        try
                        {
                            string mmapCwd = Path.Combine(Directory.GetCurrentDirectory(), "mmap.txt");
                            if (File.Exists(mmapCwd)) File.Delete(mmapCwd);
                        }
                        catch { }
                    }

                    // Restore in-game memory values cleanly
                    GameReader.RestoreWorldFov();
                    GameReader.RestoreViewmodelFov();
                    GameReader.RestoreNoSway();
                    GameReader.RestoreNoRecoil();
                    GameReader.RestoreBunnyhop();
                    GameReader.RestoreViewangleAim();
                    GameReader.RestoreNoFlash();
                    GameReader.RestoreNoSmoke();
                    GameReader.RestoreBrightness();
                    GameReader.RestoreGunFlashlight();
                    GameReader.TryRestoreInstantAds();
                    GameReader.TryRestoreTpv();

                    CleanStaleImguiIni();

                    // Close FPGA connection
                    try
                    {
                        ScpslMemory.Instance?.CloseFPGA();
                    }
                    catch { }

                    string? exe = Environment.ProcessPath;
                    if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                    {
                        try { exe = Process.GetCurrentProcess().MainModule?.FileName; } catch { }
                    }
                    if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                    {
                        exe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SCPSLDMA.exe");
                    }

                    if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
                    {
                        string[] cmdArgs = Environment.GetCommandLineArgs();
                        string rawArgs = cmdArgs.Length > 1
                            ? string.Join(" ", cmdArgs.Skip(1).Select(a => a.Contains(' ') ? $"\"{a}\"" : a))
                            : "";

                        string workingDir = AppDomain.CurrentDomain.BaseDirectory;
                        string startCmd = string.IsNullOrEmpty(rawArgs)
                            ? $"start \"\" \"{exe}\""
                            : $"start \"\" \"{exe}\" {rawArgs}";

                        Log.WriteLine($"[Restart] Spawning detached relauncher for: {exe}");

                        var psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c ping 127.0.0.1 -n 2 >nul & {startCmd}",
                            WorkingDirectory = workingDir,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(psi);
                    }
                    else
                    {
                        Log.WriteLine($"[Restart Error] Could not find executable to restart: {exe}");
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"[Restart] Exception during restart: {ex.Message}");
                }

                Environment.Exit(0);
            }


            private static bool ToggleSwitch(string id, ref bool value, string label)
            {
                var drawList = ImGui.GetWindowDrawList();
                var cursorPos = ImGui.GetCursorScreenPos();
                float availWidth = ImGui.GetContentRegionAvail().X;
                float height = 24.0f;
                float switchWidth = 38.0f;
                float switchHeight = 20.0f;

                // Draw Label on the left
                Vector2 textPos = new(cursorPos.X, cursorPos.Y + (height - ImGui.GetTextLineHeight()) * 0.5f);
                drawList.AddText(textPos, PackColor(0.88f, 0.87f, 0.92f, 1.0f), label);

                // Calculate Switch position on the right
                Vector2 switchPos = new(cursorPos.X + availWidth - switchWidth, cursorPos.Y + (height - switchHeight) * 0.5f);

                // Invisible button to handle clicks across the full row
                ImGui.SetCursorScreenPos(cursorPos);
                bool clicked = ImGui.InvisibleButton(id, new Vector2(availWidth, height));
                if (clicked)
                {
                    value = !value;
                }

                bool hovered = ImGui.IsItemHovered();

                // Colors
                uint colBg = value 
                    ? (hovered ? PackColor(0.68f, 0.45f, 1.00f, 1f) : PackColor(0.58f, 0.35f, 0.95f, 1f))
                    : (hovered ? PackColor(0.24f, 0.22f, 0.30f, 1f) : PackColor(0.16f, 0.15f, 0.22f, 1f));

                uint colKnob = value 
                    ? PackColor(1.0f, 1.0f, 1.0f, 1f) 
                    : PackColor(0.48f, 0.46f, 0.58f, 1f);

                // Draw background pill
                float radius = switchHeight * 0.5f;
                drawList.AddRectFilled(switchPos, switchPos + new Vector2(switchWidth, switchHeight), colBg, radius);

                // Draw knob circle
                float knobRadius = radius - 2.5f;
                float knobX = value ? (switchPos.X + switchWidth - radius) : (switchPos.X + radius);
                float knobY = switchPos.Y + radius;
                drawList.AddCircleFilled(new Vector2(knobX, knobY), knobRadius, colKnob);

                return clicked;
            }

            private static bool ModernSlider(string id, string label, ref float value, float min, float max, string format = "{0:0}")
            {
                var drawList = ImGui.GetWindowDrawList();
                var cursorPos = ImGui.GetCursorScreenPos();
                float availWidth = ImGui.GetContentRegionAvail().X;
                float rowHeight = 24.0f;
                float sliderBarWidth = 140.0f;
                float valueTextWidth = 55.0f;

                // Draw Label on the left
                Vector2 labelPos = new(cursorPos.X, cursorPos.Y + (rowHeight - ImGui.GetTextLineHeight()) * 0.5f);
                drawList.AddText(labelPos, PackColor(0.88f, 0.87f, 0.92f, 1.0f), label);

                // Value text formatted
                string valStr = string.Format(format, value);
                Vector2 valTextSize = ImGui.CalcTextSize(valStr);
                float valTextX = cursorPos.X + availWidth - sliderBarWidth - valueTextWidth + (valueTextWidth - valTextSize.X) * 0.5f;
                Vector2 valTextPos = new(valTextX, cursorPos.Y + (rowHeight - valTextSize.Y) * 0.5f);
                drawList.AddText(valTextPos, PackColor(0.78f, 0.75f, 0.90f, 1.0f), valStr);

                // Slider bar coordinates on the right
                float barHeight = 6.0f;
                float barX = cursorPos.X + availWidth - sliderBarWidth;
                float barY = cursorPos.Y + (rowHeight - barHeight) * 0.5f;
                Vector2 barMin = new(barX, barY);
                Vector2 barMax = new(barX + sliderBarWidth, barY + barHeight);

                // Invisible button over the slider bar area for interaction
                ImGui.SetCursorScreenPos(new Vector2(barX - 4, cursorPos.Y));
                bool clickedOrDragged = ImGui.InvisibleButton(id, new Vector2(sliderBarWidth + 8, rowHeight));
                bool isActive = ImGui.IsItemActive();
                bool isHovered = ImGui.IsItemHovered();

                if (isActive)
                {
                    float mouseX = ImGui.GetIO().MousePos.X;
                    float fraction = Math.Clamp((mouseX - barX) / sliderBarWidth, 0f, 1f);
                    value = min + fraction * (max - min);
                    clickedOrDragged = true;
                }

                // Normalized progress (0 to 1)
                float progress = Math.Clamp((value - min) / (max - min), 0f, 1f);
                float fillWidth = sliderBarWidth * progress;

                // Draw Track (unfilled right part)
                uint trackCol = isHovered ? PackColor(0.25f, 0.22f, 0.32f, 1.0f) : PackColor(0.18f, 0.16f, 0.24f, 1.0f);
                drawList.AddRectFilled(barMin, barMax, trackCol, barHeight * 0.5f);

                // Draw Filled Progress (left part)
                if (fillWidth > 0)
                {
                    uint fillCol = (isActive || isHovered) ? PackColor(0.72f, 0.50f, 1.00f, 1.0f) : PackColor(0.60f, 0.38f, 0.95f, 1.0f);
                    drawList.AddRectFilled(barMin, new Vector2(barX + fillWidth, barMax.Y), fillCol, barHeight * 0.5f);
                }

                // Draw Knob Circle at the end of progress
                float knobRadius = 6.0f;
                Vector2 knobCenter = new(barX + fillWidth, barY + barHeight * 0.5f);
                uint knobCol = isActive ? PackColor(1.0f, 1.0f, 1.0f, 1.0f) : ((isHovered) ? PackColor(0.95f, 0.92f, 1.0f, 1.0f) : PackColor(0.88f, 0.85f, 0.98f, 1.0f));
                drawList.AddCircleFilled(knobCenter, knobRadius, knobCol);
                drawList.AddCircle(knobCenter, knobRadius, PackColor(0.58f, 0.35f, 0.95f, 0.8f), 0, 1.5f);

                // Advance cursor to next row
                ImGui.SetCursorScreenPos(new Vector2(cursorPos.X, cursorPos.Y + rowHeight));

                return clickedOrDragged;
            }

            private static bool SidebarTabButton(string label, bool isSelected, Vector2 size)
            {
                var drawList = ImGui.GetWindowDrawList();
                var pos = ImGui.GetCursorScreenPos();

                bool clicked = ImGui.InvisibleButton($"##tab_{label}", size);
                bool hovered = ImGui.IsItemHovered();

                // Background
                if (isSelected)
                {
                    drawList.AddRectFilled(pos, pos + size, PackColor(0.58f, 0.35f, 0.95f, 0.35f), 8f);
                    drawList.AddRect(pos, pos + size, PackColor(0.72f, 0.50f, 1.00f, 0.85f), 8f, ImDrawFlags.None, 1.2f);
                }
                else if (hovered)
                {
                    drawList.AddRectFilled(pos, pos + size, PackColor(0.18f, 0.16f, 0.25f, 0.65f), 8f);
                }

                // Text Centered
                uint colText = isSelected 
                    ? PackColor(0.96f, 0.94f, 1.00f, 1f) 
                    : (hovered ? PackColor(0.88f, 0.85f, 0.96f, 1f) : PackColor(0.60f, 0.58f, 0.70f, 1f));

                Vector2 textSize = ImGui.CalcTextSize(label);
                Vector2 textPos = pos + (size - textSize) * 0.5f;
                drawList.AddText(textPos, colText, label);

                return clicked;
            }

            protected override void Render()
            {
                if (MonitorChooser.IsExternalMode)
                {
                    MonitorChooser.UpdateExternal(this);
                }
                else if (MonitorChooser.NeedsSelection)
                {
                    MonitorChooser.RenderPicker(this);
                    return;
                }

                // Global Keybinds (works while game has focus)
                if (IsKeyPressedGlobal(VK_F6, ref _f6WasDown))
                {
                    _showDiag = !_showDiag;
                }

                if (IsKeyPressedGlobal(VK_END, ref _endWasDown))
                {
                    Environment.Exit(0);
                }

                if (IsKeyPressedGlobal(VK_F7, ref _f7WasDown))
                {
                    _isDraggingEnabled = !_isDraggingEnabled;
                }

                // Toggle Menu with configurable MenuKey or INSERT Key (Unity Input + GetAsyncKeyState)
                bool menuPressed = false;
                int menuVk = UnityInput.ToVirtualKey(MenuKey);
                bool asyncEdge = (menuVk != 0 && IsKeyPressedGlobal(menuVk, ref _menuKeyWasDown));
                bool insertEdge = IsKeyPressedGlobal(VK_INSERT, ref _insertWasDown);
                bool unityDown = UnityInput.IsKeyDown(MenuKey);
                bool unityEdge = unityDown && !_menuUnityWasDown;
                _menuUnityWasDown = unityDown;
                bool unityFrameDown = UnityInput.GetKeyDown(MenuKey);

                if (asyncEdge || insertEdge || unityEdge || unityFrameDown)
                {
                    menuPressed = true;
                }

                if (menuPressed && !_isSelectingMenuKey)
                {
                    _showMenu = !_showMenu;
                }

                // Render UI Menu
                if (_showMenu)
                {
                    ImGui.SetNextWindowSize(new Vector2(980, 620), ImGuiCond.FirstUseEver);
                    ImGui.SetNextWindowSizeConstraints(new Vector2(780, 480), new Vector2(float.MaxValue, float.MaxValue));
                    ImGui.SetNextWindowPos(new Vector2((_overlayWidth - 980) * 0.5f, (_overlayHeight - 620) * 0.5f), ImGuiCond.FirstUseEver);
                    if (ImGui.Begin("SCPSL##Menu", ref _showMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse))
                    {
                        float contentHeight = ImGui.GetContentRegionAvail().Y;

                        // ═══════════════════════════════════════════════════════════
                        // LEFT SIDEBAR (Tab Navigation)
                        // ═══════════════════════════════════════════════════════════
                        if (ImGui.BeginChild("##Sidebar", new Vector2(130, contentHeight), ImGuiChildFlags.Border, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
                        {
                            ImGui.Spacing();

                            // Navigation Tab Buttons (Original Names)
                            for (int i = 0; i < MenuTabs.Length; i++)
                            {
                                bool isSelected = (_selectedTab == i);
                                if (SidebarTabButton(MenuTabs[i], isSelected, new Vector2(108, 34)))
                                {
                                    _selectedTab = i;
                                }
                                ImGui.Spacing();
                            }

                            // Bottom Exit Button
                            float buttonY = ImGui.GetWindowHeight() - 44;
                            if (buttonY > ImGui.GetCursorPosY())
                                ImGui.SetCursorPosY(buttonY);

                            if (SidebarTabButton("Exit", false, new Vector2(108, 32)))
                            {
                                Environment.Exit(0);
                            }
                        }
                        ImGui.EndChild();

                        ImGui.SameLine();

                        // ═══════════════════════════════════════════════════════════
                        // MAIN CONTENT REGION
                        // ═══════════════════════════════════════════════════════════
                        float availWidth = ImGui.GetContentRegionAvail().X;
                        float columnWidth = (availWidth - 12f) * 0.5f;

                        if (_selectedTab == 0) // Esp Tab
                        {
                            if (ImGui.BeginChild("##PlayerEspCard", new Vector2(columnWidth, contentHeight), ImGuiChildFlags.Border))
                            {
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Player ESP");
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##EspEnabled", ref _espEnabled, "Enable ESP");
                                ImGui.Spacing();
                                ToggleSwitch("##BoundingBox", ref _boundingBox, "Bounding box");
                                ImGui.Spacing();
                                ToggleSwitch("##SkeletonEsp", ref _skeletonEsp, "Skeleton / Bones");
                                ImGui.Spacing();
                                ToggleSwitch("##HeadCircleEsp", ref _headCircleEsp, "Head circle");
                                ImGui.Spacing();
                                ToggleSwitch("##NameTags", ref _nameTags, "Name tags");
                                ImGui.Spacing();
                                ToggleSwitch("##HealthBar", ref _healthBar, "Health bar");
                                ImGui.Spacing();
                                ToggleSwitch("##ShowScpIcons", ref _showScpIcons, "Show SCP Class Icons");
                                ImGui.Spacing();
                                ToggleSwitch("##RoomEsp", ref _showRoomEsp, "Enable Room ESP");
                                if (_showRoomEsp)
                                {
                                    ImGui.Spacing();
                                    ToggleSwitch("##RoomZoneFilter", ref _roomCurrentZoneOnly, "  Only Current Zone (Rooms)");
                                }
                                ImGui.Spacing();
                                ToggleSwitch("##GenEsp", ref _showGeneratorEsp, "Enable Generator ESP");
                                if (_showGeneratorEsp)
                                {
                                    ImGui.Spacing();
                                    ToggleSwitch("##GenZoneFilter", ref _genCurrentZoneOnly, "  Only Current Zone (Gens)");
                                }

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ModernSlider("##MaxDistance", "Max distance", ref _maxDistance, 10f, 1000f, "{0:0}");
                                if (_headCircleEsp)
                                {
                                    ImGui.Spacing();
                                    ModernSlider("##HeadCircleRadius", "Head circle size", ref _headCircleRadius, 2f, 15f, "{0:0}px");
                                }
                                if (_showScpIcons)
                                {
                                    ImGui.Spacing();
                                    ModernSlider("##ScpIconSize", "SCP icon size", ref _scpIconSize, 16f, 64f, "{0:0}px");
                                }
                            }
                            ImGui.EndChild();

                            ImGui.SameLine();

                            if (ImGui.BeginChild("##ItemEspCard", new Vector2(columnWidth, contentHeight), ImGuiChildFlags.Border))
                            {
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Item ESP & Filters");
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##ItemEspEnabled", ref _itemEspEnabled, "Enable Item ESP");
                                ImGui.Spacing();
                                ModernSlider("##ItemMaxDistance", "Item Max Distance", ref _itemMaxDistance, 10f, 300f, "{0:0}m");
                                ImGui.Spacing();

                                if (_itemEspEnabled)
                                {
                                    // ── 1. DISPLAY STYLE ──
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.68f, 0.45f, 1.00f, 1.00f), "Display Style");
                                    ImGui.Spacing();

                                    ToggleSwitch("##ShowItemIcons", ref _showItemIcons, "Show Item Icons");
                                    ImGui.Spacing();
                                    ToggleSwitch("##ShowItemText", ref _showItemText, "Show Item Names (Text)");
                                    ImGui.Spacing();

                                    if (_showItemIcons)
                                    {
                                        ModernSlider("##ItemIconSize", "Icon Size", ref _itemIconSize, 16f, 48f, "{0:0}px");
                                        ImGui.Spacing();
                                    }

                                    // ── 2. ITEM CATEGORY FILTERS ──
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.68f, 0.45f, 1.00f, 1.00f), "Item Categories");
                                    ImGui.Spacing();

                                    ToggleSwitch("##CatKeycards", ref _showKeycards, "Keycards");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatWeapons", ref _showWeapons, "Weapons");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatAmmo", ref _showAmmo, "Ammo");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatArmor", ref _showArmor, "Armor");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatMedical", ref _showMedical, "Medical");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatScpItems", ref _showScpItems, "SCP Items");
                                    ImGui.Spacing();
                                    ToggleSwitch("##CatUtility", ref _showUtility, "Utility");
                                    ImGui.Spacing();

                                    // ── 3. SMART INVENTORY CULLING ──
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.68f, 0.45f, 1.00f, 1.00f), "Smart Inventory Culling");
                                    ImGui.Spacing();

                                    ToggleSwitch("##FilterMaxAmmo", ref _filterMaxAmmo, "Hide Ammo if Capacity Full");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterSmartAmmo", ref _filterSmartAmmo, "Only Show Ammo for Held Guns");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterBestArmor", ref _filterBestArmor, "Hide All Armor if Wearing Heavy");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterInferiorArmor", ref _filterInferiorArmor, "Hide Inferior Armor (Wear Better)");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterInferiorCards", ref _filterInferiorKeycards, "Hide Inferior Keycards (Hold Better)");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterDupCards", ref _filterDuplicateKeycards, "Hide Duplicate Keycards");
                                    ImGui.Spacing();
                                    ToggleSwitch("##FilterDupUtil", ref _filterDuplicateUtility, "Hide Duplicate Utility (Radio, Light)");
                                    ImGui.Spacing();

                                    // ── 4. TIER THRESHOLDS ──
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.68f, 0.45f, 1.00f, 1.00f), "Minimum Tiers");
                                    ImGui.Spacing();

                                    float minCardTierF = _filterMinKeycardTier;
                                    ModernSlider("##MinCardTier", "Min Keycard Tier", ref minCardTierF, 0f, 6f, "{0:0}");
                                    _filterMinKeycardTier = (int)minCardTierF;
                                    ImGui.TextDisabled("  0: All  1: Scientist+  2: Guard+  3: MTF+  4: Op+  5: Capt+  6: O5");
                                    ImGui.Spacing();

                                    float minArmorTierF = _filterMinArmorTier;
                                    ModernSlider("##MinArmorTier", "Min Armor Tier", ref minArmorTierF, 0f, 3f, "{0:0}");
                                    _filterMinArmorTier = (int)minArmorTierF;
                                    ImGui.TextDisabled("  0: All  1: Light+  2: Combat+  3: Heavy Only");
                                    ImGui.Spacing();
                                }
                            }
                            ImGui.EndChild();
                        }
                        else if (_selectedTab == 1) // Memory Writes Tab
                        {
                            if (ImGui.BeginChild("##MemCard", new Vector2(availWidth, contentHeight), ImGuiChildFlags.Border))
                            {
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Memory Writes");
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                bool prevMaster = GameReader.MasterMemWritesEnabled;
                                ToggleSwitch("##MasterMemWrites", ref GameReader.MasterMemWritesEnabled, "Enable Memory Writes");
                                if (!GameReader.MasterMemWritesEnabled && prevMaster)
                                {
                                    GameReader.RestoreWorldFov();
                                    GameReader.RestoreViewmodelFov();
                                    GameReader.RestoreNoSway();
                                    GameReader.RestoreNoRecoil();
                                    GameReader.RestoreBunnyhop();
                                    GameReader.RestoreViewangleAim();
                                    GameReader.RestoreNoFlash();
                                    GameReader.RestoreNoSmoke();
                                    GameReader.RestoreBrightness();
                                }

                                ImGui.Spacing();
                                ImGui.PushTextWrapPos();
                                ImGui.TextColored(new Vector4(0.92f, 0.70f, 0.25f, 1.00f), "Memwrites are not hidden in streamer mode, Recoil is risky as spectators can see less recoil, other memwrites are fine and only local.");
                                ImGui.PopTextWrapPos();
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                if (!GameReader.MasterMemWritesEnabled)
                                {
                                    ImGui.TextDisabled("Memory writes are currently disabled. Enable the master switch above to use memory writes.");
                                }
                                else
                                {
                                    ToggleSwitch("##WorldFov", ref GameReader.WorldFovChangerEnabled, "World Camera FOV");
                                ImGui.Spacing();

                                if (GameReader.WorldFovChangerEnabled)
                                {
                                    ModernSlider("##WorldFovVal", "Default World FOV", ref GameReader.CustomWorldFov, 60.0f, 120.0f, "{0:0}°");
                                    ImGui.Spacing();

                                    ToggleSwitch("##ZoomFov", ref GameReader.ZoomFovEnabled, "Hold-to-Zoom (Keybind)");
                                    ImGui.Spacing();

                                    if (GameReader.ZoomFovEnabled)
                                    {
                                        ModernSlider("##ZoomFovVal", "Zoom FOV", ref GameReader.ZoomFov, 15.0f, 60.0f, "{0:0}°");
                                        ImGui.Spacing();

                                        ImGui.Text("Zoom Keybind (Hold):");
                                        ImGui.SameLine();

                                        string bindLabel = _isSelectingZoomKey
                                            ? "[ Press key in SCP:SL... ]"
                                            : $"[ {FormatKeyName(GameReader.ZoomKey)} ]";

                                        if (_isSelectingZoomKey)
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.85f, 0.40f, 0.15f, 1.00f));
                                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.95f, 0.50f, 0.25f, 1.00f));
                                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.00f, 1.00f, 1.00f, 1.00f));
                                        }

                                        if (ImGui.Button(bindLabel + "##ZoomKeyBindBtn", new Vector2(210, 24)))
                                        {
                                            _isSelectingZoomKey = !_isSelectingZoomKey;
                                            if (_isSelectingZoomKey)
                                            {
                                                _selectingZoomKeyCooldown = 15;
                                            }
                                        }

                                        if (_isSelectingZoomKey)
                                        {
                                            ImGui.PopStyleColor(3);

                                            if (_selectingZoomKeyCooldown > 0)
                                            {
                                                _selectingZoomKeyCooldown--;
                                            }
                                            else
                                            {
                                                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                                                {
                                                    _isSelectingZoomKey = false;
                                                }
                                                else
                                                {
                                                    var pressed = UnityInput.GetFramePressedKeys();
                                                    if (pressed.Count == 0)
                                                    {
                                                        pressed = UnityInput.GetHeldKeys();
                                                    }

                                                    foreach (var k in pressed)
                                                    {
                                                        if (k != UnityKeyCode.None && k != UnityKeyCode.Mouse0 && k != UnityKeyCode.Escape)
                                                        {
                                                            GameReader.ZoomKey = k;
                                                            _isSelectingZoomKey = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        ImGui.SameLine();
                                        if (ImGui.Button("Reset##ResetZoomKey", new Vector2(50, 24)))
                                        {
                                            GameReader.ZoomKey = UnityKeyCode.Mouse1;
                                            _isSelectingZoomKey = false;
                                        }
                                        if (ImGui.IsItemHovered())
                                        {
                                            ImGui.SetTooltip("Reset zoom keybind to Right Click (Mouse1)");
                                        }

                                        if (_isSelectingZoomKey)
                                        {
                                            ImGui.TextColored(new Vector4(0.90f, 0.65f, 0.20f, 1.00f), "Listening for any key or mouse button in SCP:SL... (Esc to cancel)");
                                        }

                                        ImGui.Spacing();
                                    }
                                }

                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.WorldFovStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##VmFov", ref GameReader.ViewmodelFovChangerEnabled, "Viewmodel FOV Changer");
                                ImGui.Spacing();

                                if (GameReader.ViewmodelFovChangerEnabled)
                                {
                                    ModernSlider("##ViewmodelFov", "Custom Viewmodel FOV", ref GameReader.CustomViewmodelFov, 30.0f, 140.0f, "{0:0}°");
                                    ImGui.Spacing();
                                }

                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.ViewmodelFovStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##NoSway", ref GameReader.NoSwayEnabled, "No Weapon Sway & Bobbing");
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.NoSwayStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##NoRecoil", ref GameReader.NoRecoilEnabled, "Weapon Recoil Control");
                                if (GameReader.NoRecoilEnabled)
                                {
                                    ImGui.Spacing();
                                    ModernSlider("##RecoilIntensity", "Recoil Intensity", ref GameReader.RecoilIntensity, 0.0f, 100.0f, "{0:0}%");
                                }
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.NoRecoilStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##InstantAds", ref GameReader.InstantAdsEnabled, "Instant ADS (Aim-Down-Sights)");
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Snaps weapon directly into aim-down-sights instantly with zero animation delay");
                                }
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.InstantAdsStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##AutoBhop", ref GameReader.AutoBunnyhopEnabled, "Auto-Bunnyhop");
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.AutoBunnyhopStatus);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##ViewangleAim", ref GameReader.ViewangleAimEnabled, "Viewangle Aim (Memory Aimbot)");
                                if (GameReader.ViewangleAimEnabled)
                                {
                                    ImGui.Spacing();
                                    ModernSlider("##ViewangleAimFov", "Aim FOV Angle", ref GameReader.ViewangleAimFov, 5.0f, 180.0f, "{0:0}°");
                                    ImGui.Spacing();
                                    ModernSlider("##ViewangleAimSmooth", "Aim Smoothness", ref GameReader.ViewangleAimSmoothness, 0.05f, 1.0f, "{0:0.00}");
                                    ImGui.Spacing();

                                    ImGui.Text("Target Bone:");
                                    ImGui.SameLine();
                                    bool isHead = GameReader.ViewangleAimTargetBone == 0;
                                    bool isChest = GameReader.ViewangleAimTargetBone == 1;
                                    if (ImGui.RadioButton("Head##TargetHead", isHead)) GameReader.ViewangleAimTargetBone = 0;
                                    ImGui.SameLine();
                                    if (ImGui.RadioButton("Chest##TargetChest", isChest)) GameReader.ViewangleAimTargetBone = 1;
                                    ImGui.Spacing();

                                    if (GameReader.ViewangleAimTargetBone == 0)
                                    {
                                        ModernSlider("##AimHeadOffset", "Head Vertical Offset", ref GameReader.ViewangleAimHeadOffset, -0.30f, 0.30f, "{0:0.00}m");
                                    }
                                    else
                                    {
                                        ModernSlider("##AimChestOffset", "Chest Vertical Offset", ref GameReader.ViewangleAimChestOffset, -0.30f, 0.30f, "{0:0.00}m");
                                    }
                                    ImGui.Spacing();

                                    ToggleSwitch("##AimTeamCheck", ref GameReader.ViewangleAimTeamCheck, "Target Enemies Only (Team Check)");
                                    ImGui.Spacing();
                                    ToggleSwitch("##AimDrawFov", ref GameReader.ViewangleAimDrawFov, "Draw FOV Circle");
                                    ImGui.Spacing();
                                    ToggleSwitch("##AimRequireKey", ref GameReader.ViewangleAimRequireKey, "Require Aim Keybind");
                                    if (GameReader.ViewangleAimRequireKey)
                                    {
                                        ImGui.Spacing();
                                        ImGui.Text("Keybind Mode:");
                                        ImGui.SameLine();
                                        bool isHold = GameReader.ViewangleAimKeyMode == 0;
                                        bool isToggle = GameReader.ViewangleAimKeyMode == 1;
                                        if (ImGui.RadioButton("Hold##AimModeHold", isHold)) GameReader.ViewangleAimKeyMode = 0;
                                        ImGui.SameLine();
                                        if (ImGui.RadioButton("Toggle##AimModeToggle", isToggle)) GameReader.ViewangleAimKeyMode = 1;
                                        ImGui.Spacing();

                                        ImGui.Text("Aim Keybind:");
                                        ImGui.SameLine();

                                        string aimBindLabel = _isSelectingAimKey
                                            ? "[ Press key in SCP:SL... ]"
                                            : $"[ {FormatKeyName(GameReader.ViewangleAimKey)} ]";

                                        if (_isSelectingAimKey)
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.85f, 0.40f, 0.15f, 1.00f));
                                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.95f, 0.50f, 0.25f, 1.00f));
                                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.00f, 1.00f, 1.00f, 1.00f));
                                        }

                                        if (ImGui.Button(aimBindLabel + "##AimKeyBindBtn", new Vector2(210, 24)))
                                        {
                                            _isSelectingAimKey = !_isSelectingAimKey;
                                            if (_isSelectingAimKey)
                                            {
                                                _selectingAimKeyCooldown = 15;
                                            }
                                        }

                                        if (_isSelectingAimKey)
                                        {
                                            ImGui.PopStyleColor(3);

                                            if (_selectingAimKeyCooldown > 0)
                                            {
                                                _selectingAimKeyCooldown--;
                                            }
                                            else
                                            {
                                                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                                                {
                                                    _isSelectingAimKey = false;
                                                }
                                                else
                                                {
                                                    var pressed = UnityInput.GetFramePressedKeys();
                                                    if (pressed.Count == 0)
                                                    {
                                                        pressed = UnityInput.GetHeldKeys();
                                                    }

                                                    foreach (var k in pressed)
                                                    {
                                                        if (k != UnityKeyCode.None && k != UnityKeyCode.Mouse0 && k != UnityKeyCode.Escape)
                                                        {
                                                            GameReader.ViewangleAimKey = k;
                                                            _isSelectingAimKey = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        ImGui.SameLine();
                                        if (ImGui.Button("Reset##ResetAimKey", new Vector2(50, 24)))
                                        {
                                            GameReader.ViewangleAimKey = UnityKeyCode.Mouse1;
                                            _isSelectingAimKey = false;
                                        }
                                        if (ImGui.IsItemHovered())
                                        {
                                            ImGui.SetTooltip("Reset aim keybind to Right Click (Mouse1)");
                                        }

                                        if (_isSelectingAimKey)
                                        {
                                            ImGui.TextColored(new Vector4(0.90f, 0.65f, 0.20f, 1.00f), "Listening for any key or mouse button in SCP:SL... (Esc to cancel)");
                                        }
                                    }
                                }
                                ImGui.Spacing();
                                 ImGui.Spacing();
                                 ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), GameReader.ViewangleAimStatus);

                                 ImGui.Spacing();
                                 ImGui.Separator();
                                 ImGui.Spacing();

                                 // ── VISUAL ENHANCEMENTS ──
                                 ImGui.TextColored(new Vector4(0.40f, 0.85f, 0.95f, 1.00f), "Visual Enhancements");
                                 ImGui.Spacing();

                                  // 1. No Flash
                                  ToggleSwitch("##NoFlash", ref GameReader.NoFlashEnabled, "No Flash (Anti-Flashbang)");
                                  ImGui.Spacing();
                                  ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), $"Status: {GameReader.NoFlashStatus}");

                                  ImGui.Spacing();
                                  ImGui.Separator();
                                  ImGui.Spacing();

                                  // 2. No Fog
                                  ToggleSwitch("##NoFog", ref GameReader.LessFogEnabled, "No Fog");
                                  if (ImGui.IsItemHovered())
                                  {
                                      ImGui.SetTooltip("Enable No Fog options to remove atmospheric fog and thin SCP-244 freezing mist");
                                  }
                                  if (GameReader.LessFogEnabled)
                                  {
                                      ImGui.Spacing();
                                      ImGui.Indent(12f);
                                      ImGui.Checkbox("Eliminate Atmospheric Fog##FullNoFog", ref GameReader.FullNoFogEnabled);
                                      if (ImGui.IsItemHovered())
                                      {
                                          ImGui.SetTooltip("Completely eliminates 100% of atmospheric fog and sightline haze (0% fog density)");
                                      }
                                      ImGui.Spacing();
                                      ImGui.Checkbox("Thin SCP-244 Freezing Mist##LessFog244", ref GameReader.LessFogScp244Enabled);
                                      if (ImGui.IsItemHovered())
                                      {
                                          ImGui.SetTooltip("Clamps deployed SCP-244 freezing flask fog clouds to a light 20% mist");
                                      }
                                      ImGui.Unindent(12f);
                                  }
                                  ImGui.Spacing();
                                  ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), $"Status: {GameReader.NoFogStatus}");

                                  ImGui.Spacing();
                                  ImGui.Separator();
                                  ImGui.Spacing();

                                  // 3. Brightness / Fullbright
                                  ToggleSwitch("##Brightness", ref GameReader.BrightnessEnabled, "Brightness / Fullbright");
                                  if (GameReader.BrightnessEnabled)
                                  {
                                      ImGui.Spacing();
                                      ModernSlider("##BrightnessEv", "Exposure & Luminance Boost", ref GameReader.BrightnessEv, 0.0f, 4.0f, "+{0:0.0} EV");
                                  }
                                  ImGui.Spacing();
                                   ImGui.Spacing();
                                   ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), $"Status: {GameReader.BrightnessStatus}");

                                   ImGui.Spacing();
                                   ImGui.Separator();
                                   ImGui.Spacing();

                                   // 4. Flashlight Enhancement (Weapon + Handheld)
                                    ToggleSwitch("##GunFlashlight", ref GameReader.GunFlashlightEnabled, "Flashlight enhancement (gun flashlights included)");
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("Enhances beam angle, brightness, and throw distance on flashlights (weapon attachments & handheld flashlights)");
                                    }
                                    if (GameReader.GunFlashlightEnabled)
                                    {
                                        ImGui.Spacing();
                                        ImGui.Spacing();
                                        ModernSlider("##FlashlightAngle", "Beam Angle", ref GameReader.GunFlashlightSpotAngle, 10.0f, 150.0f, "{0:0}°");
                                        ImGui.Spacing();
                                        ModernSlider("##FlashlightIntensity", "Brightness Multiplier", ref GameReader.GunFlashlightIntensityMult, 1.0f, 10.0f, "{0:0.0}x");
                                        ImGui.Spacing();
                                        ModernSlider("##FlashlightRange", "Throw Distance", ref GameReader.GunFlashlightRange, 10.0f, 300.0f, "{0:0}m");
                                    }
                                    ImGui.Spacing();
                                    var statusCol = GameReader.GunFlashlightStatus.StartsWith("Active", StringComparison.OrdinalIgnoreCase)
                                        ? new Vector4(0.30f, 0.90f, 0.40f, 1.00f)
                                        : (GameReader.GunFlashlightStatus.Equals("Off", StringComparison.OrdinalIgnoreCase)
                                            ? new Vector4(0.55f, 0.55f, 0.65f, 1.00f)
                                            : new Vector4(1.00f, 0.75f, 0.30f, 1.00f));
                                    ImGui.TextColored(statusCol, $"Status: {GameReader.GunFlashlightStatus}");

                                    ImGui.Spacing();
                                    ImGui.Separator();
                                    ImGui.Spacing();

                                    // 5. Third-Person View (Corner Peek)
                                    ToggleSwitch("##TpvEnabled", ref GameReader.TpvEnabled, "Third-Person View (Corner Peek)");
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("Offsets camera into third-person shoulder view, enabling tactical corner and cover peeking");
                                    }
                                    if (GameReader.TpvEnabled)
                                    {
                                        ImGui.Spacing();
                                        ModernSlider("##TpvDist", "Camera Distance", ref GameReader.TpvDistance, 1.0f, 4.5f, "{0:0.0}m");
                                        ImGui.Spacing();
                                        ModernSlider("##TpvHeight", "Camera Height", ref GameReader.TpvHeight, -0.5f, 1.5f, "+{0:0.0}m");
                                        ImGui.Spacing();
                                        ModernSlider("##TpvShoulder", "Shoulder Offset", ref GameReader.TpvShoulderOffset, -1.2f, 1.2f, "{0:0.0}m");
                                        ImGui.Spacing();
                                        ImGui.Checkbox("Require Keybind##TpvReqKey", ref GameReader.TpvRequireKey);
                                        if (GameReader.TpvRequireKey)
                                        {
                                            ImGui.Spacing();
                                            ImGui.Indent(12f);
                                            string[] modes = { "Toggle", "Hold" };
                                            int mode = GameReader.TpvKeyMode;
                                            if (ImGui.Combo("Key Mode##TpvMode", ref mode, modes, modes.Length))
                                            {
                                                GameReader.TpvKeyMode = mode;
                                            }

                                            ImGui.Spacing();
                                            ImGui.Text("Keybind:");
                                            ImGui.SameLine();
                                            if (_isSelectingTpvKey)
                                            {
                                                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.85f, 0.40f, 0.20f, 1.00f));
                                                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.95f, 0.50f, 0.25f, 1.00f));
                                                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.75f, 0.35f, 0.15f, 1.00f));
                                            }
                                            if (ImGui.Button(_isSelectingTpvKey ? "Press Key..." : GameReader.TpvKey.ToString(), new Vector2(100, 24)))
                                            {
                                                _isSelectingTpvKey = true;
                                                _selectingTpvKeyCooldown = 10;
                                            }
                                            if (_isSelectingTpvKey)
                                            {
                                                ImGui.PopStyleColor(3);

                                                if (_selectingTpvKeyCooldown > 0)
                                                {
                                                    _selectingTpvKeyCooldown--;
                                                }
                                                else
                                                {
                                                    if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                                                    {
                                                        _isSelectingTpvKey = false;
                                                    }
                                                    else
                                                    {
                                                        var pressed = UnityInput.GetFramePressedKeys();
                                                        if (pressed.Count == 0)
                                                        {
                                                            pressed = UnityInput.GetHeldKeys();
                                                        }

                                                        foreach (var k in pressed)
                                                        {
                                                            if (k != UnityKeyCode.None && k != UnityKeyCode.Mouse0 && k != UnityKeyCode.Escape)
                                                            {
                                                                GameReader.TpvKey = k;
                                                                _isSelectingTpvKey = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            ImGui.SameLine();
                                            if (ImGui.Button("Reset##ResetTpvKey", new Vector2(50, 24)))
                                            {
                                                GameReader.TpvKey = UnityKeyCode.F4;
                                                _isSelectingTpvKey = false;
                                            }
                                            ImGui.Unindent(12f);
                                        }
                                    }
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.65f, 1.00f), $"Status: {GameReader.TpvStatus}");
                                 }
                            }
                            ImGui.EndChild();
                        }
                        else if (_selectedTab == 2) // Settings Tab
                        {
                            // --- LEFT CARD: HUD & DISPLAY ---
                            if (ImGui.BeginChild("##HudCard", new Vector2(columnWidth, contentHeight), ImGuiChildFlags.Border))
                            {
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "HUD & Display");
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ToggleSwitch("##ShowHud", ref _showHud, "Show HUD panel");
                                if (_showHud)
                                {
                                    ImGui.Indent(16f);
                                    ToggleSwitch("##HudTitle", ref _hudShowTitle, "Title (SCPSL)");
                                    ToggleSwitch("##HudAlive", ref _hudShowAlive, "Alive count");
                                    ToggleSwitch("##HudDead", ref _hudShowDead, "Dead count");
                                    ToggleSwitch("##HudWarhead", ref _hudShowWarhead, "Alpha Warhead");
                                    ToggleSwitch("##HudGens", ref _hudShowGenerators, "Generators");
                                    ToggleSwitch("##HudRound", ref _hudShowRound, "Round timer / status");
                                    ImGui.Unindent(16f);
                                }
                                ImGui.Spacing();
                                ToggleSwitch("##UnlockDrag", ref _isDraggingEnabled, "Unlock HUD dragging");
                                ImGui.Spacing();
                                ToggleSwitch("##ShowWatermark", ref _showWatermark, "Watermark");
                                ImGui.Spacing();
                                ToggleSwitch("##VSync", ref VSync, $"VSync ({_targetFps} Hz)");

                                if (!MonitorChooser.IsExternalMode && MonitorChooser.Monitors.Count > 1)
                                {
                                    ImGui.Spacing();
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Display Monitor");
                                    ImGui.Spacing();
                                    ImGui.Text($"Active: {MonitorChooser.CurrentMonitorName}");
                                    ImGui.Spacing();
                                    if (ImGui.Button("Change Monitor##ChangeMonBtn", new Vector2(140, 26)))
                                    {
                                        MonitorChooser.RequestSelection();
                                    }
                                }
                            }
                            ImGui.EndChild();

                            ImGui.SameLine();

                            // --- RIGHT CARD: PREFERENCES & KEYBINDS ---
                            if (ImGui.BeginChild("##PreferencesCard", new Vector2(columnWidth, contentHeight), ImGuiChildFlags.Border))
                            {
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Preferences");
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();

                                ImGui.Text("Menu Toggle Key:");
                                ImGui.SameLine();

                                string menuBindLabel = _isSelectingMenuKey
                                    ? "[ Press key... ]"
                                    : $"[ {FormatKeyName(MenuKey)} ]";

                                if (_isSelectingMenuKey)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.85f, 0.40f, 0.15f, 1.00f));
                                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.95f, 0.50f, 0.25f, 1.00f));
                                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.00f, 1.00f, 1.00f, 1.00f));
                                }

                                if (ImGui.Button(menuBindLabel + "##MenuKeyBindBtn", new Vector2(170, 24)))
                                {
                                    _isSelectingMenuKey = !_isSelectingMenuKey;
                                    if (_isSelectingMenuKey)
                                    {
                                        _selectingMenuKeyCooldown = 15;
                                    }
                                }

                                if (_isSelectingMenuKey)
                                {
                                    ImGui.PopStyleColor(3);

                                    if (_selectingMenuKeyCooldown > 0)
                                    {
                                        _selectingMenuKeyCooldown--;
                                    }
                                    else
                                    {
                                        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                                        {
                                            _isSelectingMenuKey = false;
                                        }
                                        else
                                        {
                                            var pressed = UnityInput.GetFramePressedKeys();
                                            if (pressed.Count == 0)
                                            {
                                                pressed = UnityInput.GetHeldKeys();
                                            }

                                            foreach (var k in pressed)
                                            {
                                                if (k != UnityKeyCode.None && k != UnityKeyCode.Mouse0 && k != UnityKeyCode.Escape)
                                                {
                                                    MenuKey = k;
                                                    _isSelectingMenuKey = false;
                                                    break;
                                                }
                                            }

                                            if (_isSelectingMenuKey)
                                            {
                                                for (int k = 0; k < 512; k++)
                                                {
                                                    var uKey = (UnityKeyCode)k;
                                                    int vk = UnityInput.ToVirtualKey(uKey);
                                                    if (vk != 0 && vk != 0x01 && vk != 0x1B)
                                                    {
                                                        if ((GetAsyncKeyState(vk) & 0x8000) != 0)
                                                        {
                                                            MenuKey = uKey;
                                                            _isSelectingMenuKey = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Reset##ResetMenuKey", new Vector2(50, 24)))
                                {
                                    MenuKey = UnityKeyCode.F1;
                                    _isSelectingMenuKey = false;
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Reset menu keybind to F1");
                                }

                                if (_isSelectingMenuKey)
                                {
                                    ImGui.TextColored(new Vector4(0.90f, 0.65f, 0.20f, 1.00f), "Press any key or mouse button... (Esc to cancel)");
                                }

                                ImGui.Spacing();
                                bool prevStreamerMode = _streamerMode;
                                ToggleSwitch("##StreamerMode", ref _streamerMode, "Streamer Mode");
                                if (_streamerMode != prevStreamerMode)
                                {
                                    SetStreamerMode(_streamerMode);
                                }

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Config");
                                ImGui.Spacing();
                                ToggleSwitch("##AutoSave", ref _autoSave, "Auto save config");
                                ImGui.Spacing();
                                if (ImGui.Button("Save Config", new Vector2(115, 24)))
                                {
                                    SaveConfig();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("Load Config", new Vector2(115, 24)))
                                {
                                    LoadConfig();
                                }
                                if (!string.IsNullOrEmpty(_lastConfigStatus) && (DateTime.UtcNow - _configStatusTime).TotalSeconds < 4.0)
                                {
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(0.40f, 0.85f, 0.40f, 1.0f), _lastConfigStatus);
                                }

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.77f, 0.71f, 0.99f, 1.00f), "Application Management");
                                ImGui.Spacing();

                                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.20f, 0.26f, 0.42f, 0.85f));
                                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.28f, 0.36f, 0.58f, 1.00f));
                                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.16f, 0.20f, 0.34f, 1.00f));
                                if (ImGui.Button("Reload / Restart Application##ReloadAppBtn", new Vector2(-1, 28)))
                                {
                                    RestartApplication(deleteConfig: false, deleteMmap: false);
                                }
                                ImGui.PopStyleColor(3);
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Closes the overlay and restarts the application cleanly.");
                                }

                                ImGui.Spacing();

                                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.50f, 0.16f, 0.16f, 0.85f));
                                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.68f, 0.20f, 0.20f, 1.00f));
                                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.40f, 0.12f, 0.12f, 1.00f));
                                if (ImGui.Button("Delete Config + mmap & Restart##ResetAllRestartBtn", new Vector2(-1, 28)))
                                {
                                    RestartApplication(deleteConfig: true, deleteMmap: true);
                                }
                                ImGui.PopStyleColor(3);
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Deletes config.json and mmap.txt, then restarts the application.");
                                }
                            }
                            ImGui.EndChild();
                        }
                    }
                    ImGui.End();
                }

                if (_prevShowMenu && !_showMenu && _autoSave)
                {
                    SaveConfig();
                }
                _prevShowMenu = _showMenu;

                var snapshot = _currentSnapshot;
                var players = snapshot.Players;
                var rooms = snapshot.Rooms;
                var cam = GetLatestCamera();
                var warhead = _latestWarhead;
                var gens = snapshot.Generators;

                ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
                Vector2 displaySize = ImGui.GetIO().DisplaySize;
                float sw = displaySize.X > 0 ? displaySize.X : _overlayWidth;
                float sh = displaySize.Y > 0 ? displaySize.Y : _overlayHeight;

                var camFrame = new CameraFrame(in cam, sw, sh);

                    // Player statistics count
                    int aliveCount = 0, deadCount = 0;
                    bool roundStarted = _latestRoundStarted;
                    var deadHubs = _latestDeadHubs;
                    var aliveInfo = _latestAliveInfo;

                    for (int i = 0; i < players.Count; i++)
                    {
                        var p = players[i];
                        if (p.Role == RoleTypeId.None || p.PlayerId <= 0) continue;
                        if (p.Role == RoleTypeId.Overwatch || p.Role == RoleTypeId.Filmmaker || p.Role == RoleTypeId.Destroyed) continue;

                        bool isDead;
                        if (!roundStarted)
                        {
                            // Lobby / Waiting for players: all connected players are waiting, none are dead
                            isDead = false;
                        }
                        else if (p.Role == RoleTypeId.Scp079)
                        {
                            // SCP-079 (Computer) is alive while active, dead when 3/3 generators recontain it
                            if (aliveInfo != null && aliveInfo.IsAvailable)
                                isDead = !aliveInfo.AliveHubs.Contains(p.Hub);
                            else
                                isDead = false;
                        }
                        else if (RoleIsAlive(p.Role))
                        {
                            // Confirmed active living role (Class-D, Scientist, Guard, MTF, Chaos, SCP)
                            isDead = false;
                            p.DeathRolePointer = 0;
                        }
                        else if (p.Role == RoleTypeId.Spectator)
                        {
                            // Distant players outside render distance (fog of war) have roles spoofed to Spectator by server.
                            // If they exist in SpectatableModuleBase.AllInstances, they are alive on the server.
                            if (aliveInfo != null && aliveInfo.IsAvailable)
                            {
                                if (aliveInfo.AliveHubs.Contains(p.Hub))
                                {
                                    isDead = false;
                                    if (aliveInfo.HubRoles.TryGetValue(p.Hub, out var trueRole) && RoleIsAlive(trueRole))
                                    {
                                        p.Role = trueRole;
                                    }
                                }
                                else
                                {
                                    isDead = true;
                                }
                            }
                            else
                            {
                                // Fallback: if a ragdoll exists, they have died
                                isDead = deadHubs != null && deadHubs.Contains(p.Hub);
                            }
                        }
                        else
                        {
                            isDead = true;
                        }

                        p.Alive = !isDead;
                        if (isDead) deadCount++; else aliveCount++;
                    }

                    // ── HUD Panel ──
                    if (_showHud)
                    {
                        float displayTime = WarheadTracker.GetSmoothTime(warhead);
                        string warheadStr = warhead.State switch
                        {
                            WarheadState.Active => $"Warhead: {displayTime:0.0}s",
                            WarheadState.Ready => "Warhead: Ready",
                            WarheadState.Cooldown => "Warhead: Cooldown",
                            WarheadState.Detonated => "Warhead: Detonated",
                            _ => "Warhead: Off"
                        };
                        uint warheadCol = warhead.State switch
                        {
                            WarheadState.Active => ColRed,
                            WarheadState.Ready => ColGreen,
                            WarheadState.Cooldown => PackColor(1.0f, 0.85f, 0.2f, 1.0f),
                            WarheadState.Detonated => ColRed,
                            _ => ColGray
                        };

                        int engagedGens = 0;
                        if (gens != null)
                        {
                            for (int i = 0; i < gens.Count; i++)
                            {
                                if (gens[i].IsEngaged) engagedGens++;
                            }
                        }
                        uint genCol = engagedGens == 3 ? ColGreen : (engagedGens > 0 ? PackColor(1.0f, 0.85f, 0.2f, 1.0f) : ColGray);

                        int rTime = _latestRoundTime > 0 ? _latestRoundTime : (int)_localRoundStopwatch.Elapsed.TotalSeconds;
                        string roundStr = roundStarted
                            ? $"Round: {rTime / 60:D2}:{rTime % 60:D2}"
                            : "Waiting for Players";
                        uint roundCol = roundStarted ? ColCyan : ColGray;

                        // Dynamic height and width calculation
                        float curY = 4f;
                        if (_hudShowTitle) curY += 16f;
                        if (_hudShowAlive) curY += 14f;
                        if (_hudShowDead) curY += 14f;
                        if (_hudShowWarhead) curY += 14f;
                        if (_hudShowGenerators) curY += 14f;
                        if (_hudShowRound) curY += 14f;

                        bool hasAnyItem = curY > 4f;
                        float hudHeight = hasAnyItem ? (curY + 3f) : 22f;
                        float hudWidth = (_hudShowRound || _hudShowWarhead) ? 135f : 115f;
                        Vector2 hudSize = new(hudWidth, hudHeight);

                        if (_isDraggingEnabled)
                        {
                            if (GetCursorPos(out POINT mousePos))
                            {
                                bool isLeftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;

                                if (isLeftDown)
                                {
                                    if (!_isDraggingHud)
                                    {
                                        if (mousePos.X >= _hudX && mousePos.X <= _hudX + hudSize.X &&
                                            mousePos.Y >= _hudY && mousePos.Y <= _hudY + hudSize.Y)
                                        {
                                            _isDraggingHud = true;
                                            _dragOffsetX = mousePos.X - _hudX;
                                            _dragOffsetY = mousePos.Y - _hudY;
                                        }
                                    }

                                    if (_isDraggingHud)
                                    {
                                        _hudX = mousePos.X - _dragOffsetX;
                                        _hudY = mousePos.Y - _dragOffsetY;
                                    }
                                }
                                else
                                {
                                    _isDraggingHud = false;
                                }
                            }
                        }

                        if (hasAnyItem || _isDraggingEnabled)
                        {
                            Vector2 hudPos = new(_hudX, _hudY);
                            drawList.AddRectFilled(hudPos, hudPos + hudSize, ColBg, 4f);

                            if (_isDraggingEnabled)
                            {
                                drawList.AddRect(hudPos, hudPos + hudSize, ColCyan, 4f, ImDrawFlags.None, 1.5f);
                            }

                            float renderY = 4f;
                            if (_hudShowTitle)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), ColWhite, "SCPSL");
                                renderY += 16f;
                            }
                            if (_hudShowAlive)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), ColGreen, $"Alive: {aliveCount}");
                                renderY += 14f;
                            }
                            if (_hudShowDead)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), ColRed, $"Dead:  {deadCount}");
                                renderY += 14f;
                            }
                            if (_hudShowWarhead)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), warheadCol, warheadStr);
                                renderY += 14f;
                            }
                            if (_hudShowGenerators)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), genCol, $"{engagedGens}/3 Engaged");
                                renderY += 14f;
                            }
                            if (_hudShowRound)
                            {
                                drawList.AddText(hudPos + new Vector2(4, renderY), roundCol, roundStr);
                                renderY += 14f;
                            }
                        }
                    }

                    // ── Diagnostics Panel ──
                    if (_showDiag)
                    {
                        Vector2 diagPos = new(10, 98);
                        Vector2 diagSize = new(320, 286);
                        drawList.AddRectFilled(diagPos, diagPos + diagSize, ColDiagBg, 4f);

                        string[] lines = {
                            $"Monitor:      {_overlayWidth}x{_overlayHeight} @ {_targetFps}Hz (VSync: {VSync})",
                            $"Unity Input:  {UnityInput.Status}",
                            $"Total:        {Diag.LastTotalMs,6:0.0} ms  (avg {Diag.AvgTotalMs:0.0})",
                            $"  ResolveHub: {Diag.LastResolveHubsMs,6:0.1} ms",
                            $"  HashSet:    {Diag.LastHashSetMs,6:0.1} ms",
                            $"  Players:    {Diag.LastPlayersMs,6:0.1} ms  ({Diag.LastPlayerCount} hubs)",
                            $"    Names:    {Diag.LastNameMs,6:0.1} ms",
                            $"    Roles:    {Diag.LastRoleMs,6:0.1} ms",
                            $"    Pos:      {Diag.LastPosMs,6:0.1} ms",
                            $"    Bones:    {Diag.LastBoneMs,6:0.1} ms",
                            $"    Health:   {Diag.LastHealthMs,6:0.1} ms",
                            $"  Camera:     {Diag.LastCameraMs,6:0.1} ms",
                            $"  Pickups:    {snapshot.Pickups?.Count ?? 0} (dict: {GameReader.LastSpawnedCount})",
                            $"  Inventory:  {(snapshot.LocalInventory ?? _latestInventory)?.SlotCount ?? 0}/8 slots",
                            $"Memory Reads: {Diag.LastDmaReads}",
                            $"Est FPS:      {(Diag.LastTotalMs > 0 ? 1000.0 / Diag.LastTotalMs : 0):0.0}",
                            $"Overlay FPS:  {ImGui.GetIO().Framerate:0.0}",
                        };

                        for (int i = 0; i < lines.Length; i++)
                        {
                            drawList.AddText(diagPos + new Vector2(4, 4 + i * 16), ColCyan, lines[i]);
                        }
                    }

                    // ── Input Debugger Panel ──
                    if (_debugInput)
                    {
                        Vector2 debugPos = new(10, _showDiag ? 392 : 98);
                        Vector2 debugSize = new(320, 92);
                        drawList.AddRectFilled(debugPos, debugPos + debugSize, ColDiagBg, 4f);
                        drawList.AddRect(debugPos, debugPos + debugSize, ColGreen, 4f, ImDrawFlags.None, 1.2f);

                        var held = UnityInput.GetHeldKeys();
                        string heldStr = held.Count > 0 ? string.Join(", ", held) : "(none)";
                        drawList.AddText(debugPos + new Vector2(4, 4), ColGreen, $"Held Keys ({held.Count}):");
                        drawList.AddText(debugPos + new Vector2(4, 20), ColWhite, heldStr.Length > 40 ? heldStr.Substring(0, 37) + "..." : heldStr);

                        drawList.AddText(debugPos + new Vector2(4, 38), ColGreen, "Recent Inputs (down/up):");
                        lock (_recentInputEvents)
                        {
                            int startIdx = Math.Max(0, _recentInputEvents.Count - 3);
                            for (int i = startIdx; i < _recentInputEvents.Count; i++)
                            {
                                int lineNum = i - startIdx;
                                drawList.AddText(debugPos + new Vector2(4, 54 + lineNum * 12), ColGray, _recentInputEvents[i]);
                            }
                        }
                    }

                    if (!cam.Valid) return;

                    // Determine current zone based on closest room (only needed when zone filters are active)
                    FacilityZone localZone = FacilityZone.None;
                    if ((_roomCurrentZoneOnly || _genCurrentZoneOnly) && rooms != null && rooms.Count > 0)
                    {
                        float minDistSq = float.MaxValue;
                        for (int i = 0; i < rooms.Count; i++)
                        {
                            var room = rooms[i];
                            if (room.Position == Vector3.Zero) continue;
                            if (room.Name == RoomName.Unnamed) continue;
                            if (room.Zone <= FacilityZone.None || room.Zone > FacilityZone.Other) continue;

                            float distSq = Vector3.DistanceSquared(cam.Position, room.Position);
                            if (distSq < minDistSq)
                            {
                                minDistSq = distSq;
                                localZone = room.Zone;
                            }
                        }
                    }

                    float maxDistSq = _maxDistance * _maxDistance;

                    // ── Room ESP ──
                    if (_showRoomEsp && rooms != null && rooms.Count > 0)
                    {
                        for (int i = 0; i < rooms.Count; i++)
                        {
                            var room = rooms[i];
                            if (room.Position == Vector3.Zero) continue;
                            if (RoomConfig.IsBlacklisted(room.Name)) continue;
                            if (_roomCurrentZoneOnly && localZone != FacilityZone.None && room.Zone != localZone) continue;

                            float distSq = Vector3.DistanceSquared(cam.Position, room.Position);
                            if (distSq > maxDistSq) continue;

                            if (camFrame.Project(room.Position, out float rx, out float ry))
                            {
                                float dist = MathF.Sqrt(distSq);
                                int distInt = (int)dist;
                                if (distInt != room.CachedDistanceInt || room.CachedDistanceLabel.Length == 0)
                                {
                                    room.CachedDistanceInt = distInt;
                                    room.CachedDistanceLabel = $"{RoomConfig.GetDisplayName(room.Name)} [{distInt}m]";
                                    room.CachedLabelSize = ImGui.CalcTextSize(room.CachedDistanceLabel);
                                }

                                drawList.AddCircleFilled(new Vector2(rx, ry), 2.5f, 0xFFFFFFFF);
                                drawList.AddText(new Vector2(rx - room.CachedLabelSize.X / 2f, ry - room.CachedLabelSize.Y - 2), 0xFFD8D8D8, room.CachedDistanceLabel);
                            }
                        }
                    }

                    // ── Generator ESP ──
                    if (_showGeneratorEsp && (!_genCurrentZoneOnly || localZone == FacilityZone.HeavyContainment || localZone == FacilityZone.None) && gens != null && gens.Count > 0)
                    {
                        for (int i = 0; i < gens.Count; i++)
                        {
                            var gen = gens[i];
                            if (gen.Position == Vector3.Zero) continue;

                            float distSq = Vector3.DistanceSquared(cam.Position, gen.Position);
                            if (distSq > maxDistSq) continue;

                            if (camFrame.Project(gen.Position, out float gx, out float gy))
                            {
                                float dist = MathF.Sqrt(distSq);
                                int distInt = (int)dist;
                                if (distInt != gen.CachedDistanceInt || gen.CachedFlags != gen.Flags || gen.SyncTime != gen.CachedSyncTime || gen.CachedDistanceLabel.Length == 0)
                                {
                                    gen.CachedDistanceInt = distInt;
                                    gen.CachedFlags = gen.Flags;
                                    gen.CachedSyncTime = gen.SyncTime;

                                    string statusText = gen.IsEngaged ? "ACTIVE" : gen.IsUnlocked ? $"STARTING ({gen.SyncTime}s)" : "LOCKED";
                                    gen.CachedDistanceLabel = $"[GEN] {statusText} [{distInt}m]";
                                    gen.CachedLabelSize = ImGui.CalcTextSize(gen.CachedDistanceLabel);
                                }

                                uint statusCol = gen.IsEngaged ? ColGreen : gen.IsUnlocked ? 0xFF00D7FF : ColRed;
                                drawList.AddCircleFilled(new Vector2(gx, gy), 3.5f, statusCol);
                                drawList.AddText(new Vector2(gx - gen.CachedLabelSize.X / 2f, gy - gen.CachedLabelSize.Y - 3f), statusCol, gen.CachedDistanceLabel);
                            }
                        }
                    }

                    // ── Item ESP ──
                    if (_itemEspEnabled && snapshot.Pickups != null && snapshot.Pickups.Count > 0 && (_showItemText || _showItemIcons))
                    {
                        float itemMaxDistSq = _itemMaxDistance * _itemMaxDistance;
                        var inv = snapshot.LocalInventory ?? _latestInventory;

                        for (int i = 0; i < snapshot.Pickups.Count; i++)
                        {
                            var item = snapshot.Pickups[i];
                            if (item.Position == Vector3.Zero) continue;

                            float distSq = Vector3.DistanceSquared(cam.Position, item.Position);
                            if (distSq > itemMaxDistSq) continue;

                            // Apply filtering rules (Category, Armor, Ammo, Keycard, Utility)
                            if (!ShouldRenderPickup(item, inv)) continue;

                            Vector3 drawPos = item.Position + new Vector3(0, 0.15f, 0);
                            if (camFrame.Project(drawPos, out float ix, out float iy))
                            {
                                float dist = MathF.Sqrt(distSq);
                                int distInt = (int)dist;
                                if (distInt != item.CachedDistanceInt || item.CachedDistanceLabel.Length == 0)
                                {
                                    item.CachedDistanceInt = distInt;
                                    item.CachedDistanceLabel = $"{item.DisplayName} [{distInt}m]";
                                    item.CachedLabelSize = ImGui.CalcTextSize(item.CachedDistanceLabel);
                                }

                                // Always draw subtle ground anchor circle
                                drawList.AddCircleFilled(new Vector2(ix, iy), 2.5f, item.Color);

                                IntPtr iconHandle = IntPtr.Zero;
                                Vector2 origIconSize = Vector2.Zero;
                                if (_showItemIcons)
                                {
                                    iconHandle = GetOrLoadItemIcon(item.ItemType, out origIconSize);
                                }

                                float iconDim = _itemIconSize;

                                if (iconHandle != IntPtr.Zero && _showItemText)
                                {
                                    // MODE: Icon + Text (Icon stacked cleanly above text label)
                                    Vector2 textSize = item.CachedLabelSize;
                                    float totalHeight = iconDim + 2f + textSize.Y;
                                    float topY = iy - totalHeight - 4f;

                                    Vector2 iconMin = new Vector2(ix - iconDim * 0.5f, topY);
                                    Vector2 iconMax = new Vector2(ix + iconDim * 0.5f, topY + iconDim);
                                    drawList.AddImage(iconHandle, iconMin, iconMax);

                                    Vector2 textPos = new Vector2(ix - textSize.X * 0.5f, topY + iconDim + 2f);
                                    drawList.AddText(textPos, item.Color, item.CachedDistanceLabel);
                                }
                                else if (iconHandle != IntPtr.Zero && !_showItemText)
                                {
                                    // MODE: Icon Only (Icon with small distance pill below)
                                    float topY = iy - iconDim - 14f;
                                    Vector2 iconMin = new Vector2(ix - iconDim * 0.5f, topY);
                                    Vector2 iconMax = new Vector2(ix + iconDim * 0.5f, topY + iconDim);
                                    drawList.AddImage(iconHandle, iconMin, iconMax);

                                    string distStr = $"{distInt}m";
                                    Vector2 dSize = ImGui.CalcTextSize(distStr);
                                    Vector2 dPos = new Vector2(ix - dSize.X * 0.5f, topY + iconDim + 1f);
                                    drawList.AddText(dPos, item.Color, distStr);
                                }
                                else if (_showItemText)
                                {
                                    // MODE: Text Only
                                    drawList.AddText(new Vector2(ix - item.CachedLabelSize.X / 2f, iy - item.CachedLabelSize.Y - 3f), item.Color, item.CachedDistanceLabel);
                                }
                            }
                        }
                    }

                    // ── Player ESP ──
                    if (players.Count > 0)
                    {
                        float playerMaxDistSq = _maxDistance * _maxDistance;
                        foreach (var p in players)
                    {
                        if (p.IsLocal || !p.HasPosition || !p.Alive) continue;

                        float distSq = Vector3.DistanceSquared(cam.Position, p.Position);
                        if (distSq > playerMaxDistSq) continue;

                        Vector3 headPos = p.Position + new Vector3(0, 1.2f, 0);
                        Vector3 feetPos = p.Position - new Vector3(0, 1.0f, 0);

                        if (!camFrame.Project(headPos, out float hx, out float hy)) continue;
                        if (!camFrame.Project(feetPos, out float fx, out float fy)) continue;

                        float height = Math.Abs(fy - hy);
                        if (height < 4) continue;
                        float width = height / 1.8f;
                        float bx = hx - width / 2f;

                        uint boxCol = GetTeamColor(p.Team);

                        if (!_espEnabled) continue;

                        // Bounding Box
                        if (_boundingBox)
                        {
                            drawList.AddRect(new Vector2(bx, hy), new Vector2(bx + width, hy + height), boxCol, 0f, ImDrawFlags.None, 1.5f);
                        }

                        // Skeleton / Bones Wireframe
                        if (_skeletonEsp && p.HasBones && p.BonePositions != null)
                        {
                            for (int s = 0; s < SkeletonData.Connections.Length; s++)
                            {
                                var (fromIdx, toIdx) = SkeletonData.Connections[s];
                                if (fromIdx < 0 || fromIdx >= p.BonePositions.Length || toIdx < 0 || toIdx >= p.BonePositions.Length)
                                    continue;

                                Vector3 posA = p.BonePositions[fromIdx];
                                Vector3 posB = p.BonePositions[toIdx];
                                if (posA == Vector3.Zero || posB == Vector3.Zero) continue;

                                if (camFrame.Project(posA, out float ax, out float ay) &&
                                    camFrame.Project(posB, out float bxPos, out float byPos))
                                {
                                    drawList.AddLine(new Vector2(ax, ay), new Vector2(bxPos, byPos), boxCol, 1.5f);
                                }
                            }
                        }

                        // Head Joint Circle (Fixed 1 size)
                        if (_headCircleEsp && p.HasBones && p.BonePositions != null)
                        {
                            int headIdx = SkeletonData.HeadHitboxIndex;
                            if (headIdx >= 0 && headIdx < p.BonePositions.Length)
                            {
                                Vector3 headWorld = p.BonePositions[headIdx];
                                if (headWorld != Vector3.Zero && camFrame.Project(headWorld, out float hxBone, out float hyBone))
                                {
                                    drawList.AddCircle(new Vector2(hxBone, hyBone), _headCircleRadius, boxCol, 0, 1.5f);
                                }
                            }
                        }

                        // Health Bar
                        if (_healthBar)
                        {
                            float hpPct = Math.Clamp(p.Health / Math.Max(p.MaxHealth, 1f), 0f, 1f);
                            float barH = height * hpPct;
                            drawList.AddRectFilled(new Vector2(bx - 6, hy), new Vector2(bx - 3, hy + height), ColRed);
                            drawList.AddRectFilled(new Vector2(bx - 6, hy + (height - barH)), new Vector2(bx - 3, hy + height), ColGreen);
                        }

                        // Pre-calculate distance label once for nametag and SCP icon offset
                        if (_nameTags)
                        {
                            int distInt = (int)MathF.Sqrt(distSq);
                            if (distInt != p.CachedDistanceInt || p.CachedDistanceLabel.Length == 0)
                            {
                                p.CachedDistanceInt = distInt;
                                p.CachedDistanceLabel = $"{p.Name} [{distInt}m]";
                                p.CachedLabelSize = ImGui.CalcTextSize(p.CachedDistanceLabel);
                            }
                        }

                        // SCP Class Icon
                        if (_showScpIcons && (p.Team == Team.SCPs || p.Team == Team.Flamingos))
                        {
                            IntPtr scpIcon = GetOrLoadScpIcon(p.Role, out _);
                            if (scpIcon != IntPtr.Zero)
                            {
                                float iconDim = _scpIconSize;
                                float iconY = hy - 4f - iconDim;
                                if (_nameTags)
                                {
                                    iconY = hy - p.CachedLabelSize.Y - 6f - iconDim;
                                }

                                Vector2 iconMin = new Vector2(hx - iconDim * 0.5f, iconY);
                                Vector2 iconMax = new Vector2(hx + iconDim * 0.5f, iconY + iconDim);
                                drawList.AddImage(scpIcon, iconMin, iconMax);
                            }
                        }

                        // Player Name Label
                        if (_nameTags)
                        {
                            drawList.AddText(new Vector2(hx - p.CachedLabelSize.X / 2f, hy - p.CachedLabelSize.Y - 2), ColWhite, p.CachedDistanceLabel);

                            // Role Label
                            int roleIdx = (int)p.Role;
                            string roleName = (roleIdx >= 0 && roleIdx < CachedRoleNames.Length && CachedRoleNames[roleIdx] != null)
                                ? CachedRoleNames[roleIdx]
                                : GameReader.RoleName(p.Role);
                            Vector2 roleSize;
                            if (roleIdx >= 0 && roleIdx < CachedRoleTextSizes.Length)
                            {
                                if (CachedRoleTextSizes[roleIdx].X <= 0)
                                {
                                    CachedRoleTextSizes[roleIdx] = ImGui.CalcTextSize(roleName);
                                }
                                roleSize = CachedRoleTextSizes[roleIdx];
                            }
                            else
                            {
                                roleSize = ImGui.CalcTextSize(roleName);
                            }

                            drawList.AddText(new Vector2(hx - roleSize.X / 2f, hy + height + 2), GetRoleColor(p.Role), roleName);
                        }
                    }
                }
                    // ── FOV Circle for Viewangle Aim ──
                    if (GameReader.MasterMemWritesEnabled && GameReader.ViewangleAimEnabled && GameReader.ViewangleAimDrawFov)
                    {
                        float camFov = cam.Fov > 10f && cam.Fov < 170f ? cam.Fov : 70.0f;
                        float fovRad = GameReader.ViewangleAimFov * (MathF.PI / 180.0f);
                        float camHalfRad = (camFov * 0.5f) * (MathF.PI / 180.0f);
                        float screenRadius = (MathF.Tan(fovRad) / MathF.Tan(camHalfRad)) * (sh * 0.5f);

                        if (screenRadius > 2f && screenRadius < sw * 2f)
                        {
                            Vector2 screenCenter = new Vector2(sw * 0.5f, sh * 0.5f);
                            drawList.AddCircle(screenCenter, screenRadius, PackColor(0.85f, 0.40f, 0.95f, 0.65f), 64, 1.5f);
                        }
                    }

                    // ── Watermark ──
                    if (_showWatermark)
                    {
                        string wmText = "SCPSL DMA | v1.3";
                        Vector2 wmSize = ImGui.CalcTextSize(wmText);
                        Vector2 wmPos = new(sw - wmSize.X - 16f, 12f);
                        drawList.AddRectFilled(wmPos - new Vector2(6, 3), wmPos + wmSize + new Vector2(6, 3), PackColor(0.05f, 0.05f, 0.08f, 0.60f), 4f);
                        drawList.AddText(wmPos, PackColor(0.35f, 0.85f, 0.95f, 0.90f), wmText);
                    }
            }
        }


    // ═══════════════════════════════════════════════════════════
    //  ENTRY POINT
    // ═══════════════════════════════════════════════════════════
    public static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        private static void EnableDpiAwareness()
        {
            try
            {
                if (!SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                {
                    SetProcessDPIAware();
                }
            }
            catch
            {
                try { SetProcessDPIAware(); } catch { }
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(string? lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        private const int ENUM_CURRENT_SETTINGS = -1;

        public static (int Width, int Height, int MaxFps) GetMonitorMetrics()
        {
            return MonitorChooser.GetInitialMetrics();
        }

        public static void RunDiagnostic()
        {
            Console.WriteLine("=== SCPSL DMA DIAGNOSTIC ===");
            Console.WriteLine($"UnityBase: 0x{DmaMemory.UnityBase:X}, GameAssemblyBase: 0x{DmaMemory.GameAssemblyBase:X}");

            // Round Status Diagnostic
            Console.WriteLine("\n--- ROUND STATUS DIAGNOSTIC ---");
            ulong rhSf = StaticFields(Offsets.ReferenceHub_TypeInfo);
            byte hostSet = Mem.Val<byte>(rhSf + Offsets.RH_static_hostHubSet);
            ulong hostHub = Mem.Ptr(rhSf + Offsets.RH_static_hostHub);
            byte localSet = Mem.Val<byte>(rhSf + Offsets.RH_static_localHubSet);
            ulong localHub = Mem.Ptr(rhSf + Offsets.RH_static_localHub);
            Console.WriteLine($"RH Static: 0x{rhSf:X}, hostSet={hostSet}, hostHub=0x{hostHub:X}, localSet={localSet}, localHub=0x{localHub:X}");
            if (localHub != 0)
            {
                ulong localCcm = Mem.Ptr(localHub + Offsets.RH_characterClassManager);
                byte localRs = (localCcm != 0) ? Mem.Val<byte>(localCcm + Offsets.CCM_RoundStarted) : (byte)0;
                Console.WriteLine($"LocalHub: ccm=0x{localCcm:X}, RoundStarted={localRs}");
            }
            if (hostHub != 0)
            {
                ulong hostCcm = Mem.Ptr(hostHub + Offsets.RH_characterClassManager);
                byte hostRs = (hostCcm != 0) ? Mem.Val<byte>(hostCcm + Offsets.CCM_RoundStarted) : (byte)0;
                Console.WriteLine($"HostHub: ccm=0x{hostCcm:X}, RoundStarted={hostRs}");
            }
            ulong rsSf = StaticFields(Offsets.RoundSummary_TypeInfo);
            int rTime = Mem.Val<int>(rsSf + Offsets.RoundSummary_roundTime);
            byte rsSet = Mem.Val<byte>(rsSf + Offsets.RoundSummary_singletonSet);
            ulong rsSingleton = Mem.Ptr(rsSf + Offsets.RoundSummary_singleton);
            byte rsEnded = (rsSingleton != 0) ? Mem.Val<byte>(rsSingleton + Offsets.RoundSummary_isRoundEnded) : (byte)0;
            Console.WriteLine($"RoundSummary: sf=0x{rsSf:X}, roundTime={rTime}, singletonSet={rsSet}, singleton=0x{rsSingleton:X}, isRoundEnded={rsEnded}");
            Console.WriteLine($"RoundStartTimer: running={GameReader.IsRoundStartTimerRunning()}, readRoundTime={GameReader.ReadRoundTime()}");
            Console.WriteLine($"Living Check: localAlive={GameReader.IsLocalPlayerAlive()}, hasLiving={GameReader.HasLivingPlayers(GameReader.ScpslOverlay.CurrentPlayers)}");
            Console.WriteLine($"IsRoundStarted(raw) = {GameReader.IsRoundStarted(GameReader.ScpslOverlay.CurrentPlayers)}, IsRoundEnded() = {GameReader.IsRoundEnded(GameReader.ScpslOverlay.CurrentPlayers)}, LatestRoundStarted = {GameReader.ScpslOverlay.LatestRoundStarted}");

            // UnityInput Diagnostic
            Console.WriteLine("\n--- UNITY INPUT DIAGNOSTIC ---");
            ulong imPtr = DmaMemory.ReadPtr(DmaMemory.UnityBase + UnityOffsets.ModuleBase.InputManager, false);
            Console.WriteLine($"Hardcoded InputManager (+0x{UnityOffsets.ModuleBase.InputManager:X}): 0x{imPtr:X}");
            Console.WriteLine($"UnityInput.IsConnected: {UnityInput.IsConnected}, Status: '{UnityInput.Status}', Addr: 0x{UnityInput.InputManagerAddress:X}");

            // Weapon & Flashlight Diagnostic
            Console.WriteLine("\n--- WEAPON & FLASHLIGHT DIAGNOSTIC ---");
            ulong rhLocal = GameReader.LocalHub();
            Console.WriteLine($"LocalHub: 0x{rhLocal:X}");
            if (rhLocal != 0)
            {
                if (GameReader.TryReadObjectField(rhLocal, "inventory", out ulong invDiag))
                {
                    GameReader.TryReadObjectField(invDiag, "_curInstance", out ulong curItem);
                    string itemKlass = GameReader.ClassName(curItem);
                    Console.WriteLine($"Inventory: 0x{invDiag:X}, CurInstance: 0x{curItem:X} ({itemKlass})");

                    ulong userInv = Mem.Ptr(invDiag + Offsets.Inventory_UserInventory);
                    if (userInv != 0 && userInv.IsValidVirtualAddress())
                    {
                        ulong itemsDict = Mem.Ptr(userInv + Offsets.InventoryInfo_Items);
                        if (itemsDict != 0 && itemsDict.IsValidVirtualAddress())
                        {
                            int count = Mem.Val<int>(itemsDict + Offsets.Dictionary_count);
                            ulong entriesArr = Mem.Ptr(itemsDict + Offsets.Dictionary_entries);
                            Console.WriteLine($"Inventory Items Count: {count}");
                            if (count > 0 && count <= 32 && entriesArr != 0 && entriesArr.IsValidVirtualAddress())
                            {
                                int bytesNeeded = count * 24;
                                Span<byte> buf = stackalloc byte[bytesNeeded];
                                DmaMemory.ReadBuffer<byte>(entriesArr + Offsets.Array_items, buf, false);
                                for (int i = 0; i < count; i++)
                                {
                                    int offset = i * 24;
                                    int hashCode = MemoryMarshal.Read<int>(buf.Slice(offset, 4));
                                    if (hashCode < 0) continue;
                                    ulong itemPtr = MemoryMarshal.Read<ulong>(buf.Slice(offset + 16, 8));
                                    if (itemPtr == 0 || !itemPtr.IsValidVirtualAddress()) continue;
                                    int itemTypeId = Mem.Val<int>(itemPtr + Offsets.ItemBase_ItemTypeId, false);
                                    Console.WriteLine($"  Item[{i}]: 0x{itemPtr:X} ({(ItemType)itemTypeId}, Class={GameReader.ClassName(itemPtr)})");
                                    if (curItem == 0 || GameReader.ClassName(curItem).IndexOf("Firearm", StringComparison.OrdinalIgnoreCase) < 0)
                                    {
                                        if (GameReader.ClassName(itemPtr).IndexOf("Firearm", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            curItem = itemPtr;
                                            Console.WriteLine($"  -> Testing Flashlight on inventory firearm: 0x{curItem:X}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool res = GameReader.TryResolveGunFlashlight(
                        rhLocal,
                        curItem,
                        out ulong vm,
                        out ulong ls,
                        out ulong hd,
                        out ulong nl,
                        out string fail);
                    Console.WriteLine($"TryResolveGunFlashlight: result={res}, fail='{fail}'");
                    Console.WriteLine($"  Viewmodel:   0x{vm:X} ({GameReader.ClassName(vm)})");
                    Console.WriteLine($"  LightSource: 0x{ls:X} ({GameReader.ClassName(ls)})");
                    Console.WriteLine($"  HDLightData: 0x{hd:X} ({GameReader.ClassName(hd)})");
                    Console.WriteLine($"  NativeLight: 0x{nl:X}");

                    if (ls != 0)
                    {
                        byte[] lsRaw = new byte[0x60];
                        DmaMemory.ReadBuffer<byte>(ls, lsRaw.AsSpan(), false);
                        Console.WriteLine("    Managed Light fields (8-byte ptrs):");
                        for (int o = 0; o < lsRaw.Length; o += 8)
                        {
                            ulong val = BitConverter.ToUInt64(lsRaw, o);
                            Console.WriteLine($"      +0x{o:X2}: 0x{val:X} ({GameReader.ClassName(val)})");
                        }
                    }

                    if (nl != 0)
                    {
                        byte[] nlRaw = new byte[0x100];
                        DmaMemory.ReadBuffer<byte>(nl, nlRaw.AsSpan(), false);
                        Console.WriteLine("    Native Light fields (4-byte floats & 8-byte ptrs):");
                        for (int o = 0; o < nlRaw.Length; o += 8)
                        {
                            ulong ptrVal = BitConverter.ToUInt64(nlRaw, o);
                            float f1 = BitConverter.ToSingle(nlRaw, o);
                            float f2 = BitConverter.ToSingle(nlRaw, o + 4);
                            string cls = ptrVal.IsValidVirtualAddress() ? GameReader.ClassName(ptrVal) : "";
                            Console.WriteLine($"      +0x{o:X2}: 0x{ptrVal:X} ({cls}) | f0={f1:0.###} f4={f2:0.###}");
                        }

                        // Inspect candidate native GameObjects: 0x18, 0x20, 0x28, 0x30
                        ulong[] candGo = [
                            BitConverter.ToUInt64(nlRaw, 0x18),
                            BitConverter.ToUInt64(nlRaw, 0x20),
                            BitConverter.ToUInt64(nlRaw, 0x28),
                            BitConverter.ToUInt64(nlRaw, 0x30)
                        ];
                        for (int cg = 0; cg < candGo.Length; cg++)
                        {
                            try
                            {
                                ulong cand = candGo[cg];
                                if (!cand.IsValidVirtualAddress()) continue;
                                Console.WriteLine($"    Inspecting candidate pointer [{cg}] = 0x{cand:X}:");
                                byte[] cBuf = new byte[0x100];
                                DmaMemory.ReadBuffer<byte>(cand, cBuf.AsSpan(), false);
                                for (int co = 0; co < cBuf.Length; co += 8)
                                {
                                    ulong cVal = BitConverter.ToUInt64(cBuf, co);
                                    string cCls = cVal.IsValidVirtualAddress() ? GameReader.ClassName(cVal) : "";
                                    string strAscii = "";
                                    if (cVal.IsValidVirtualAddress())
                                    {
                                        try {
                                            byte[] sbuf = new byte[32];
                                            DmaMemory.ReadBuffer<byte>(cVal, sbuf.AsSpan(), false);
                                            int slen = 0;
                                            while (slen < sbuf.Length && sbuf[slen] >= 32 && sbuf[slen] <= 126) slen++;
                                            if (slen >= 3) strAscii = $" \"{Encoding.ASCII.GetString(sbuf, 0, slen)}\"";
                                        } catch { }
                                    }
                                    Console.WriteLine($"      +0x{co:X2}: 0x{cVal:X} ({cCls}){strAscii}");

                                    if (cCls.IndexOf("HDAdditionalLightData", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        Console.WriteLine($"      ---> FOUND HDAdditionalLightData at 0x{cVal:X}!");
                                        byte[] hdRaw = new byte[0x70];
                                        DmaMemory.ReadBuffer<byte>(cVal, hdRaw.AsSpan(), false);

                                         float hdDimmer = BitConverter.ToSingle(hdRaw, 0x48);
                                         float hdIntensity = BitConverter.ToSingle(hdRaw, 0x38);
                                         float hdVolDimmer = BitConverter.ToSingle(hdRaw, 0x4C);
                                         float hdFadeDist = BitConverter.ToSingle(hdRaw, 0x50);
                                         float hdVolFadeDist = BitConverter.ToSingle(hdRaw, 0x54);
                                         Console.WriteLine($"      HD values: Dimmer={hdDimmer}, Intensity={hdIntensity}, VolDimmer={hdVolDimmer}, FadeDist={hdFadeDist}, VolFadeDist={hdVolFadeDist}");
                                     }
                                 }
                             }
                             catch { }
                         }

                         ulong lightData = Mem.Ptr(nl + 0x40, false);
                        if (lightData == 0 || !lightData.IsValidVirtualAddress())
                            lightData = nl + 0x90;

                        float intensity = Mem.Val<float>(lightData + 0x38, false);
                        float range = Mem.Val<float>(lightData + 0x3C, false);
                        float spot = Mem.Val<float>(lightData + 0x48, false);
                        float innerSpot = Mem.Val<float>(lightData + 0x4C, false);
                        Console.WriteLine($"    Native Light values: intensity={intensity}, range={range}, spotAngle={spot}, innerSpotAngle={innerSpot}");
                    }

                    if (vm != 0)
                    {
                        DumpClassFields("Viewmodel (vm)", vm);
                        if (GameReader.TryGetFieldOffset(Mem.Ptr(vm, false), "<Extensions>k__BackingField", out ulong extsOff))
                        {
                            ulong extsArr = Mem.Ptr(vm + extsOff, false);
                            if (extsArr != 0)
                            {
                                int len = Mem.Val<int>(extsArr + Offsets.Array_max_length, false);
                                for (int e = 0; e < len && e < 16; e++)
                                {
                                    ulong ext = Mem.Ptr(extsArr + Offsets.Array_items + (ulong)e * 8, false);
                                    if (ext != 0) DumpClassFields($"Viewmodel Extension [{e}]", ext);
                                }
                            }
                        }
                    }

                    ulong mccSf = StaticFields(Offsets.MainCameraController_TypeInfo);
                    if (mccSf != 0)
                    {
                        Console.WriteLine($"\n--- MCC Static Fields: 0x{mccSf:X} ---");
                        ulong mccInst = Mem.Ptr(mccSf + 0x0, false);
                        if (mccInst != 0) DumpClassFields("MainCameraController Instance", mccInst);
                    }

                    ulong cscSf = StaticFields(Offsets.CameraShakeController_TypeInfo);
                    if (cscSf != 0)
                    {
                        ulong cscInst = Mem.Ptr(cscSf + 0x0, false);
                        if (cscInst != 0) DumpClassFields("CameraShakeController Instance", cscInst);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to read inventory field from LocalHub.");
                }
            }

            // 1. Check Rooms
            Console.WriteLine("\n--- ROOMS ---");
            ulong roomSf = StaticFields(Offsets.RoomIdentifier_TypeInfo);
            Console.WriteLine($"RoomIdentifier static fields: 0x{roomSf:X}");
            if (roomSf != 0)
            {
                ulong hashSet = Mem.Ptr(roomSf + Offsets.RI_static_AllRoomIdentifiers);
                int lastIdx = Mem.Val<int>(hashSet + Offsets.HashSet_lastIndex);
                Console.WriteLine($"  AllRoomIdentifiers HashSet: 0x{hashSet:X}, lastIndex: {lastIdx}");
            }
            var rooms = GameReader.ReadRooms();
            Console.WriteLine($"  ReadRooms() returned {rooms.Count} rooms");
            var cam = GameReader.ReadCamera();
            Console.WriteLine($"  Camera Valid: {cam.Valid}, Pos: {cam.Position}");

            float minDistSq = float.MaxValue;
            RoomInfo? closestRoom = null;
            foreach (var r in rooms)
            {
                float d = Vector3.DistanceSquared(cam.Position, r.Position);
                if (d < minDistSq)
                {
                    minDistSq = d;
                    closestRoom = r;
                }
            }
            if (closestRoom != null)
                Console.WriteLine($"Closest Room: {closestRoom.Name} at 0x{closestRoom.Address:X} (dist: {MathF.Sqrt(minDistSq):0.1}m, Zone: {closestRoom.Zone})");

            // 2. Test GameReader.ReadGenerators()
            Console.WriteLine("\n--- TESTING GameReader.ReadGenerators() ---");
            var gens = GameReader.ReadGenerators();
            Console.WriteLine($"ReadGenerators() returned {gens.Count} generators");
            int engagedCount = 0;
            for (int i = 0; i < gens.Count; i++)
            {
                var g = gens[i];
                if (g.IsEngaged) engagedCount++;
                string statusText = g.IsEngaged ? "Engaged" : (g.IsActivating ? $"{g.SyncTime}s" : (g.IsUnlocked ? "Unlocked" : "Locked"));
                Console.WriteLine($"  Gen[{i}]: Room={g.Room}, Status=[{statusText}], Pos={g.Position}, Flags=0x{g.Flags:X2}, SyncTime={g.SyncTime}s");
            }
            Console.WriteLine($"HUD Counter: {engagedCount}/3 Engaged");

            // 3. Test Bone Discovery & Calculation via SetupPlayerBones & PollBones
            Console.WriteLine("\n--- TESTING BONE SETUP & POLLING ---");
            var players = GameReader.PollPlayerList();
            Console.WriteLine($"PollPlayerList() returned {players.Count} players");
            GameReader.PollPositions(players);
            GameReader.PollBones(players, cam.Position, 2000f);

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p.IsLocal) continue;
                float d = Vector3.Distance(cam.Position, p.Position);
                Console.WriteLine($"Player[{i:D2}]: Name='{p.Name}' Role={p.Role} Alive={p.Alive} Dist={d:0.1}m HasBones={p.HasBones} Pos={p.Position}");
                if (!p.HasBones && p.Alive)
                {
                    ulong fpc = p.FpcModule;
                    ulong cm = (fpc != 0) ? Mem.Ptr(fpc + Offsets.Fpm_CharacterModelInstance) : 0;
                    ulong hbArr = (cm != 0) ? Mem.Ptr(cm + Offsets.CM_hitboxesCache) : 0;
                    int hbLen = (hbArr != 0) ? Mem.Val<int>(hbArr + Offsets.Array_max_length) : -1;
                    Console.WriteLine($"   --> NO BONES: Role={p.Role} Fpc=0x{fpc:X} CM=0x{cm:X} hbLen={hbLen}");
                }
                else if (p.HasBones && p.BonePositions != null)
                {
                    bool allZero = true;
                    for (int b = 0; b < p.BonePositions.Length; b++)
                    {
                        if (p.BonePositions[b] != Vector3.Zero) { allZero = false; break; }
                    }
                    Console.WriteLine($"   --> BONES OK (Cap={p.Capacity}, AllZero={allZero}, Head={p.BonePositions[4]})");
                    for (int b = 0; b < p.BonePositions.Length; b++)
                    {
                        int hbIdx = (p.HitboxIndices != null && b < p.HitboxIndices.Length) ? p.HitboxIndices[b] : -1;
                        Console.WriteLine($"       Bone[{b:D2}] (hbTrIndex={hbIdx:D3}): {p.BonePositions[b]}");
                    }
                }
            }

            // 4. Test StatModules inspection
            Console.WriteLine("\n--- TESTING STATMODULES INSPECTION ---");
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                Console.WriteLine($"\nPlayer[{i:D2}]: Name='{p.Name}' Role={p.Role} Hub=0x{p.Hub:X} StatModules=0x{p.StatModules:X}");
                if (p.StatModules != 0)
                {
                    int len = Mem.Val<int>(p.StatModules + Offsets.Array_max_length, false);
                    Console.WriteLine($"  StatModules Array len={len}");
                    for (int m = 0; m < len && m < 16; m++)
                    {
                        ulong mod = Mem.Ptr(p.StatModules + Offsets.Array_items + (ulong)m * 8, false);
                        if (mod != 0)
                        {
                            string cls = GameReader.ClassName(mod);
                            float curVal = Mem.Val<float>(mod + Offsets.Stat_lastValue, false);
                            float maxVal = Mem.Val<float>(mod + Offsets.HealthStat_maxValue, false);
                            Console.WriteLine($"    [{m}]: mod=0x{mod:X} Class={cls} lastVal={curVal:0.##} maxVal={maxVal:0.##}");
                        }
                    }
                }
            }

            // 5. Test CameraShakeController & MainCameraController
            Console.WriteLine("\n--- TESTING CAMERA CONTROLLERS ---");
            ulong cscSf2 = StaticFields(Offsets.CameraShakeController_TypeInfo);
            if (cscSf2 != 0)
            {
                ulong cscInst = Mem.Ptr(cscSf2 + 0x0, false);
                Console.WriteLine($"CameraShakeController Singleton: 0x{cscInst:X}");
                if (cscInst != 0)
                {
                    ulong camObj = Mem.Ptr(cscInst + 0x20, false);
                    Vector3 startPos = Mem.Val<Vector3>(cscInst + 0x28, false);
                    ulong vmRoot = Mem.Ptr(cscInst + 0x38, false);
                    Console.WriteLine($"  _camera: 0x{camObj:X} ({GameReader.ClassName(camObj)})");
                    Console.WriteLine($"  Initial _startPos: {startPos}");
                    Console.WriteLine($"  _viewmodelRoot: 0x{vmRoot:X} ({GameReader.ClassName(vmRoot)})");

                    // Test writing TPV offset to _startPos
                    Vector3 testOffset = new Vector3(0.45f, 0.35f, -2.2f);
                    Console.WriteLine($"  --> Testing write of {testOffset} to _startPos (0x{cscInst + 0x28:X})...");
                    bool wOk = Mem.TryWriteValue<Vector3>(cscInst + 0x28, testOffset);
                    Console.WriteLine($"  --> Write result: {wOk}");
                    Thread.Sleep(150);
                    Vector3 readBack = Mem.Val<Vector3>(cscInst + 0x28, false);
                    Console.WriteLine($"  --> Read back after 150ms: {readBack}");

                    // Check MainCameraController LastPosition after 150ms
                    ulong mccSfTest = StaticFields(Offsets.MainCameraController_TypeInfo);
                    Vector3 mccPos = Mem.Val<Vector3>(mccSfTest + 0x14, false);
                    Console.WriteLine($"  --> MCC LastPosition after 150ms: {mccPos}");

                    // Restore _startPos back to original
                    Mem.TryWriteValue<Vector3>(cscInst + 0x28, startPos);
                    Console.WriteLine($"  --> Restored _startPos to {startPos}");
                }
            }

            ulong mccSf2 = StaticFields(Offsets.MainCameraController_TypeInfo);
            if (mccSf2 != 0)
            {
                ulong curCam = Mem.Ptr(mccSf2 + 0x30, false);
                ulong mccInst2 = Mem.Ptr(mccSf2 + 0x38, false);
                Vector3 lastPos = Mem.Val<Vector3>(mccSf2 + 0x14, false);
                Vector3 defaultPos = Mem.Val<Vector3>(mccSf2 + 0x40, false);
                Console.WriteLine($"MainCameraController Static Fields: 0x{mccSf2:X}");
                Console.WriteLine($"  _currentCamera: 0x{curCam:X} ({GameReader.ClassName(curCam)})");
                Console.WriteLine($"  _singleton: 0x{mccInst2:X} ({GameReader.ClassName(mccInst2)})");
                Console.WriteLine($"  LastPosition: {lastPos}");
                Console.WriteLine($"  _defaultPos: {defaultPos}");


                ulong sccRva = 0x846900;
                ulong sccAddr = DmaMemory.GameAssemblyBase + sccRva;
                byte[] sccBytes = new byte[128];
                DmaMemory.ReadBuffer<byte>(sccAddr, sccBytes.AsSpan(), false);
                Console.WriteLine($"  set_CurrentCamera bytes at 0x{sccAddr:X} (RVA 0x{sccRva:X}):");
                Console.WriteLine($"    {BitConverter.ToString(sccBytes)}");

                ulong cscLuRva = 0x7AE540;
                ulong cscLuAddr = DmaMemory.GameAssemblyBase + cscLuRva;
                byte[] cscLuBytes = new byte[128];
                DmaMemory.ReadBuffer<byte>(cscLuAddr, cscLuBytes.AsSpan(), false);
                Console.WriteLine($"  CameraShakeController.LateUpdate bytes at 0x{cscLuAddr:X} (RVA 0x{cscLuRva:X}):");
                Console.WriteLine($"    {BitConverter.ToString(cscLuBytes)}");

                // Inspect PlayerCameraReference on LocalHub
                ulong diagLocalHub = LocalHub();
                if (diagLocalHub != 0)
                {
                    ulong pcr = Mem.Ptr(diagLocalHub + 0x78, false);
                    Console.WriteLine($"  LocalHub (0x{diagLocalHub:X}) PlayerCameraReference (+0x78): 0x{pcr:X} ({GameReader.ClassName(pcr)})");
                    if (pcr != 0)
                    {
                        ulong pcrNat = Mem.Ptr(pcr + 0x10, false);
                        Console.WriteLine($"    PlayerCameraReference natTr: 0x{pcrNat:X}");
                        byte[] pcrBytes = new byte[0x40];
                        DmaMemory.ReadBuffer<byte>(pcrNat, pcrBytes.AsSpan(), false);
                        ulong pcrHier = BitConverter.ToUInt64(pcrBytes, 0x28);
                        int pcrIdx = BitConverter.ToInt32(pcrBytes, 0x30);
                        Console.WriteLine($"    pcrNat Hierarchy: 0x{pcrHier:X}, Index: {pcrIdx}");
                        if (pcrHier.IsValidVirtualAddress())
                        {
                            ulong pcrVerts = Mem.Ptr(pcrHier + 0x18, false);
                            if (pcrVerts.IsValidVirtualAddress())
                            {
                                var pcrTrs = Mem.Val<UnityTransform.TrsX>(pcrVerts + (ulong)pcrIdx * 48, false);
                                Console.WriteLine($"    pcr Node[{pcrIdx}]: t={pcrTrs.t}, q={pcrTrs.q}, s={pcrTrs.s}");

                                Console.WriteLine($"    --> Testing write to PlayerCameraReference TrsX.t (0x{pcrVerts + (ulong)pcrIdx * 48:X})...");
                                Vector3 testPcrOffset = new Vector3(0.45f, 0.35f, -2.2f);
                                bool wPcr = Mem.TryWriteValue<Vector3>(pcrVerts + (ulong)pcrIdx * 48, testPcrOffset);
                                Console.WriteLine($"    --> Write result: {wPcr}");
                                Thread.Sleep(150);
                                var readBackPcr = Mem.Val<UnityTransform.TrsX>(pcrVerts + (ulong)pcrIdx * 48, false);
                                ulong mccSfNow = StaticFields(Offsets.MainCameraController_TypeInfo);
                                Vector3 mccNow = Mem.Val<Vector3>(mccSfNow + 0x14, false);
                                Console.WriteLine($"    --> After 150ms pcr TrsX.t: {readBackPcr.t} (target was {testPcrOffset})");
                                Console.WriteLine($"    --> After 150ms MCC LastPosition: {mccNow}");
                                Mem.TryWriteValue<Vector3>(pcrVerts + (ulong)pcrIdx * 48, Vector3.Zero);
                                Console.WriteLine("    --> Restored pcr TrsX.t to <0, 0, 0>");
                            }
                        }
                    }

                    ulong rm = Mem.Ptr(diagLocalHub + Offsets.RH_roleManager, false);
                    ulong curRole = (rm != 0) ? Mem.Ptr(rm + Offsets.PRM_curRole, false) : 0;
                    Console.WriteLine($"  LocalHub curRole: 0x{curRole:X} ({GameReader.ClassName(curRole)})");
                    if (curRole != 0)
                    {
                        ulong camTrRole = Mem.Ptr(curRole + 0xC8, false);
                        Console.WriteLine($"    curRole._cameraTransform (+0xC8): 0x{camTrRole:X} ({GameReader.ClassName(camTrRole)})");

                        ulong fpcMod = Mem.Ptr(curRole + Offsets.Fpc_FpcModule, false);
                        Console.WriteLine($"    curRole.FpcModule (+0x80): 0x{fpcMod:X} ({GameReader.ClassName(fpcMod)})");
                        if (fpcMod != 0)
                        {
                            ulong sp = Mem.Ptr(fpcMod + 0x90, false);
                            Console.WriteLine($"      FpcModule.StateProcessor (+0x90): 0x{sp:X} ({GameReader.ClassName(sp)})");
                            if (sp != 0)
                            {
                                ulong cp = Mem.Ptr(sp + 0x50, false);
                                Console.WriteLine($"        StateProcessor._camPivot (+0x50): 0x{cp:X} ({GameReader.ClassName(cp)})");
                                if (cp != 0)
                                {
                                    ulong cpNat = Mem.Ptr(cp + 0x10, false);
                                    Console.WriteLine($"          _camPivot natTr: 0x{cpNat:X}");
                                }
                            }

                            ulong ml = Mem.Ptr(fpcMod + 0x88, false);
                            Console.WriteLine($"      FpcModule.MouseLook (+0x88): 0x{ml:X} ({GameReader.ClassName(ml)})");

                            DumpClassFields("curRole", curRole);
                            DumpClassFields("fpcMod", fpcMod);

                            ulong cm = Mem.Ptr(fpcMod + Offsets.Fpm_CharacterModelInstance, false);
                            Console.WriteLine($"      FpcModule.CharacterModel (+0xA0): 0x{cm:X} ({GameReader.ClassName(cm)})");
                            if (cm != 0)
                            {
                                DumpClassFields("CharacterModel", cm);
                                ulong cmNat = Mem.Ptr(cm + 0x10, false);
                                ulong cmGo = (cmNat != 0) ? Mem.Ptr(cmNat + 0x18, false) : 0;
                                Console.WriteLine($"      cm GameObject: 0x{cmGo:X}");
                                if (cmGo != 0)
                                {
                                    int cmLayer = Mem.Val<int>(cmGo + 0x38, false);
                                    Console.WriteLine($"      cm GameObject layer bytes at +0x38: {cmLayer} (0x{cmLayer:X})");
                                    ulong comps = Mem.Ptr(cmGo + UnityOffsets.GameObject.ComponentsOffset, false);
                                    if (comps != 0)
                                    {
                                        for (int c = 0; c < 16; c++)
                                        {
                                            ulong compPtr = Mem.Ptr(comps + (ulong)c * 8, false);
                                            if (compPtr != 0)
                                            {
                                                Console.WriteLine($"        cm Comp[{c}]: 0x{compPtr:X} ({GameReader.ClassName(compPtr)})");
                                            }
                                        }
                                    }
                                }
                            }

                            // Deep inspection of cm._renderers (+0xA0)
                            ulong rendArr = Mem.Ptr(cm + 0xA0, false);
                            Console.WriteLine($"      cm._renderers (+0xA0): 0x{rendArr:X}");
                            if (rendArr != 0)
                            {
                                int rendCount = Mem.Val<int>(rendArr + 0x18, false);
                                Console.WriteLine($"      cm._renderers Count: {rendCount}");
                                for (int r = 0; r < Math.Min(rendCount, 16); r++)
                                {
                                    ulong rendPtr = Mem.Ptr(rendArr + 0x20 + (ulong)r * 8, false);
                                    if (rendPtr != 0)
                                    {
                                        ulong natRend = Mem.Ptr(rendPtr + 0x10, false);
                                        byte mEnabled = Mem.Val<byte>(natRend + 0x38, false);
                                        Console.WriteLine($"        Renderer[{r}]: 0x{rendPtr:X} ({GameReader.ClassName(rendPtr)}), nat=0x{natRend:X}, m_Enabled={mEnabled}");
                                    }
                                }
                            }

                            // Test writing IsVisible = true (+0xD0)
                            byte curVis = Mem.Val<byte>(cm + 0xD0, false);
                            Console.WriteLine($"      cm.<IsVisible> (+0xD0): {curVis}");
                            Console.WriteLine("      --> Testing write of IsVisible = 1 to cm+0xD0...");
                            Mem.TryWriteValue<byte>(cm + 0xD0, 1);
                            Thread.Sleep(150);
                            byte readVis = Mem.Val<byte>(cm + 0xD0, false);
                            Console.WriteLine($"      --> Read back after 150ms: {readVis}");
                            Mem.TryWriteValue<byte>(cm + 0xD0, curVis);
                        }

                        ulong diagInv = Mem.Ptr(diagLocalHub + Offsets.RH_inventory, false);
                        Console.WriteLine($"  LocalHub Inventory (+0xA0): 0x{diagInv:X} ({GameReader.ClassName(diagInv)})");
                        if (diagInv != 0)
                        {
                            DumpClassFields("Inventory", diagInv);
                            if (GameReader.TryReadObjectField(diagInv, "_curInstance", out ulong curItem) && curItem != 0)
                            {
                                Console.WriteLine($"    _curInstance: 0x{curItem:X} ({GameReader.ClassName(curItem)})");
                                DumpClassFields("CurInstance", curItem);

                                ulong tpModel = Mem.Ptr(curItem + 0x28, false);
                                ulong vmModel = Mem.Ptr(curItem + 0x30, false);
                                ulong wmModel = Mem.Ptr(curItem + 0x120, false);
                                Console.WriteLine($"    tpModel (+0x28): 0x{tpModel:X} ({GameReader.ClassName(tpModel)})");
                                Console.WriteLine($"    vmModel (+0x30): 0x{vmModel:X} ({GameReader.ClassName(vmModel)})");
                                Console.WriteLine($"    wmModel (+0x120): 0x{wmModel:X} ({GameReader.ClassName(wmModel)})");

                                if (tpModel != 0) DumpClassFields("ThirdpersonModel", tpModel);
                                if (wmModel != 0) DumpClassFields("WorldModel", wmModel);
                            }
                        }
                    }
                }

                // Inspect native camera
                if (cscSf2 != 0)
                {
                    ulong cscInst = Mem.Ptr(cscSf2 + 0x0, false);
                    ulong camObj = (cscInst != 0) ? Mem.Ptr(cscInst + 0x20, false) : 0;
                    if (camObj != 0)
                    {
                        ulong natCam = Mem.Ptr(camObj + 0x10, false);
                        Console.WriteLine($"  natCam (camObj+0x10): 0x{natCam:X}");
                        if (natCam != 0)
                        {
                            byte[] camBytes = new byte[0x300];
                            DmaMemory.ReadBuffer<byte>(natCam, camBytes.AsSpan(), false);
                            Console.WriteLine("    Scanning natCam for matrix / position floats:");
                            for (int o = 0; o < 0x2E0; o += 16)
                            {
                                float f0 = BitConverter.ToSingle(camBytes, o);
                                float f1 = BitConverter.ToSingle(camBytes, o + 4);
                                float f2 = BitConverter.ToSingle(camBytes, o + 8);
                                float f3 = BitConverter.ToSingle(camBytes, o + 12);
                                if (!float.IsNaN(f0) && !float.IsNaN(f1) && !float.IsNaN(f2) && !float.IsNaN(f3))
                                {
                                    if (Math.Abs(f0) > 0.001f || Math.Abs(f1) > 0.001f || Math.Abs(f2) > 0.001f || Math.Abs(f3) > 0.001f)
                                    {
                                        Console.WriteLine($"      +0x{o:X3}: ({f0:0.###}, {f1:0.###}, {f2:0.###}, {f3:0.###})");
                                    }
                                }
                            }
                            ulong camGo = Mem.Ptr(natCam + 0x18, false);
                            Console.WriteLine($"  camGo (natCam+0x18): 0x{camGo:X}");
                            if (camGo != 0)
                            {
                                ulong comps = Mem.Ptr(camGo + UnityOffsets.GameObject.ComponentsOffset, false);
                                Console.WriteLine($"    camGo ComponentsArray (0x{UnityOffsets.GameObject.ComponentsOffset:X}): 0x{comps:X}");
                                if (comps != 0)
                                {
                                    for (int c = 0; c < 8; c++)
                                    {
                                        ulong compPtr = Mem.Ptr(comps + (ulong)c * 8, false);
                                        if (compPtr != 0)
                                        {
                                            string ccls = GameReader.ClassName(compPtr);
                                            Console.WriteLine($"      Comp[{c}]: 0x{compPtr:X} ({ccls})");
                                        }
                                    }
                                }

                                ulong camTr = Mem.Ptr(camGo + 0x60, false);
                                Console.WriteLine($"    Camera Transform (camGo+0x60): 0x{camTr:X} ({GameReader.ClassName(camTr)})");
                                if (camTr != 0)
                                {
                                    ulong natTr = Mem.Ptr(camTr + 0x10, false);
                                    Console.WriteLine($"      natTr (camTr+0x10): 0x{natTr:X}");
                                    if (natTr != 0)
                                    {
                                        ulong hier2 = Mem.Ptr(natTr + 0x28, false);
                                        int idx2 = Mem.Val<int>(natTr + 0x30, false);
                                        Console.WriteLine($"      camTr Hierarchy: 0x{hier2:X}, Index: {idx2}");
                                        if (hier2.IsValidVirtualAddress())
                                        {
                                            int cap2 = Mem.Val<int>(hier2 + 0x10, false);
                                            ulong verts2 = Mem.Ptr(hier2 + 0x18, false);
                                            ulong inds2 = Mem.Ptr(hier2 + 0x20, false);
                                            Console.WriteLine($"      camTr Cap: {cap2}, Verts: 0x{verts2:X}, Inds: 0x{inds2:X}");
                                            if (verts2.IsValidVirtualAddress() && inds2.IsValidVirtualAddress())
                                            {
                                                int parent2 = Mem.Val<int>(inds2 + (ulong)idx2 * 4, false);
                                                var myTrs2 = Mem.Val<UnityTransform.TrsX>(verts2 + (ulong)idx2 * (ulong)System.Runtime.CompilerServices.Unsafe.SizeOf<UnityTransform.TrsX>(), false);
                                                Console.WriteLine($"      camTr Node[{idx2}]: parent={parent2}, t={myTrs2.t}, q={myTrs2.q}, s={myTrs2.s}");
                                                if (parent2 >= 0)
                                                {
                                                    var parentTrs2 = Mem.Val<UnityTransform.TrsX>(verts2 + (ulong)parent2 * (ulong)System.Runtime.CompilerServices.Unsafe.SizeOf<UnityTransform.TrsX>(), false);
                                                    int gp2 = Mem.Val<int>(inds2 + (ulong)parent2 * 4, false);
                                                    Console.WriteLine($"      camTr Parent[{parent2}]: parent={gp2}, t={parentTrs2.t}, q={parentTrs2.q}, s={parentTrs2.s}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (curCam != 0)
                {
                    Console.WriteLine($"  _currentCamera: 0x{curCam:X} ({GameReader.ClassName(curCam)})");
                    ulong curCamNat = Mem.Ptr(curCam + 0x10, false);
                    ulong curCamGo = (curCamNat != 0) ? Mem.Ptr(curCamNat + 0x18, false) : 0;
                    Console.WriteLine($"    _currentCamera nat: 0x{curCamNat:X}, GameObject: 0x{curCamGo:X}");
                    if (curCamNat != 0)
                    {
                        byte[] rawTr = new byte[0x80];
                        DmaMemory.ReadBuffer<byte>(curCamNat, rawTr.AsSpan(), false);
                        Console.WriteLine($"    _currentCamera natTr bytes at 0x{curCamNat:X}:");
                        ulong hier = BitConverter.ToUInt64(rawTr, 0x28);
                        int trIndex = BitConverter.ToInt32(rawTr, 0x30);
                        Console.WriteLine($"    --> Hierarchy: 0x{hier:X}, Index: {trIndex}");
                        if (hier.IsValidVirtualAddress())
                        {
                            int cap = Mem.Val<int>(hier + 0x10, false);
                            ulong verts = Mem.Ptr(hier + 0x18, false);
                            ulong inds = Mem.Ptr(hier + 0x20, false);
                            Console.WriteLine($"    --> Cap: {cap}, Verts: 0x{verts:X}, Inds: 0x{inds:X}");
                            if (verts.IsValidVirtualAddress() && inds.IsValidVirtualAddress())
                            {
                                int parent = Mem.Val<int>(inds + (ulong)trIndex * 4, false);
                                var myTrs = Mem.Val<UnityTransform.TrsX>(verts + (ulong)trIndex * (ulong)System.Runtime.CompilerServices.Unsafe.SizeOf<UnityTransform.TrsX>(), false);
                                Console.WriteLine($"    --> Node[{trIndex}]: parent={parent}, t={myTrs.t}, q={myTrs.q}, s={myTrs.s}");
                                Console.WriteLine($"    --> Testing write to _currentCamera TrsX.t (0x{verts + (ulong)trIndex * 48:X})...");
                                Vector3 origT = myTrs.t;
                                Vector3 offsetT = origT + new Vector3(0.45f, 0.35f, -2.2f);
                                bool wTrs = Mem.TryWriteValue<Vector3>(verts + (ulong)trIndex * 48, offsetT);
                                Console.WriteLine($"    --> Write result: {wTrs}");
                                Thread.Sleep(150);
                                var readTrs = Mem.Val<UnityTransform.TrsX>(verts + (ulong)trIndex * 48, false);
                                Vector3 readMcc = Mem.Val<Vector3>(mccSf2 + 0x14, false);
                                Console.WriteLine($"    --> After 150ms TrsX.t: {readTrs.t} (orig was {origT})");
                                Console.WriteLine($"    --> After 150ms MCC LastPosition: {readMcc}");
                                Mem.TryWriteValue<Vector3>(verts + (ulong)trIndex * 48, origT);
                                Console.WriteLine($"    --> Restored TrsX.t to {origT}");
                            }
                        }
                    }

                    if (curCamGo != 0)
                    {
                        ulong comps = Mem.Ptr(curCamGo + UnityOffsets.GameObject.ComponentsOffset, false);
                        Console.WriteLine($"    _currentCamera GameObject (0x{curCamGo:X}) Comps: 0x{comps:X}");
                        if (comps != 0)
                        {
                            for (int c = 0; c < 10; c++)
                            {
                                ulong compPtr = Mem.Ptr(comps + (ulong)c * 8, false);
                                if (compPtr != 0)
                                {
                                    string ccls = GameReader.ClassName(compPtr);
                                    Console.WriteLine($"      Comp[{c}]: 0x{compPtr:X} ({ccls})");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DumpClassFields(string label, ulong obj)
        {
            if (obj == 0 || !obj.IsValidVirtualAddress()) return;
            ulong klass = Mem.Ptr(obj, false);
            Console.WriteLine($"\n--- DUMP FIELDS: {label} (obj=0x{obj:X}, klass=0x{klass:X}, Name={GameReader.ClassName(obj)}) ---");
            for (ulong curK = klass; curK != 0; curK = Mem.Ptr(curK + Offsets.Il2CppClass_parent, false))
            {
                Console.WriteLine($"  Class: 0x{curK:X} ({GameReader.ClassName(curK)})");
                ulong flds = Mem.Ptr(curK + Offsets.Il2CppClass_fields, false);
                if (flds != 0)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        ulong fi = flds + (ulong)i * Offsets.FieldInfo_size;
                        ulong parent = Mem.Ptr(fi + Offsets.FieldInfo_parent, false);
                        if (parent != curK) break;
                        ulong namePtr = Mem.Ptr(fi + Offsets.FieldInfo_name, false);
                        string fname = Mem.Str(namePtr, 128, false);
                        int foff = Mem.Val<int>(fi + Offsets.FieldInfo_offset, false);
                        float fval = (foff >= 0x10 && foff < 0x200) ? Mem.Val<float>(obj + (ulong)foff, false) : 0f;
                        ulong pval = (foff >= 0x10 && foff < 0x200) ? Mem.Ptr(obj + (ulong)foff, false) : 0;
                        Console.WriteLine($"    Field: {fname} @ 0x{foff:X} (float={fval:0.###}, ptr=0x{pval:X})");
                    }
                }
            }
        }

        private static void InspectComponentTransform(string name, ulong comp)
        {
            if (comp == 0) return;
            ulong nativeComp = Mem.Ptr(comp + 0x10);
            Console.WriteLine($"  [{name}] comp: 0x{comp:X}, nativeComp: 0x{nativeComp:X}");
            if (nativeComp != 0)
            {
                byte[] raw = new byte[0x80];
                DmaMemory.ReadBuffer<byte>(nativeComp, raw.AsSpan(), false);
                Console.Write("    nativeComp bytes: ");
                for (int o = 0; o < 0x80; o += 8)
                {
                    ulong val = BitConverter.ToUInt64(raw, o);
                    Console.Write($"[+{o:X2}]=0x{val:X} ");
                }
                Console.WriteLine();
            }
        }

        private static void InspectTransformObject(string name, ulong trObj)
        {
            if (trObj == 0) { Console.WriteLine($"  [{name}] null"); return; }
            Console.WriteLine($"  [{name}] trObj: 0x{trObj:X}");
            ulong nativeTr = Mem.Ptr(trObj + 0x10);
            Console.WriteLine($"    nativeTr (trObj+0x10): 0x{nativeTr:X}");
            if (nativeTr != 0)
            {
                byte[] raw = new byte[0xA0];
                DmaMemory.ReadBuffer<byte>(nativeTr, raw.AsSpan(), false);
                Console.Write("    nativeTr bytes: ");
                for (int o = 0; o < 0xA0; o += 8)
                {
                    ulong val = BitConverter.ToUInt64(raw, o);
                    Console.Write($"[+{o:X2}]=0x{val:X} ");
                }
                Console.WriteLine();

                for (int o = 0; o < 0xA0; o += 8)
                {
                    ulong val = BitConverter.ToUInt64(raw, o);
                    if (val.IsValidVirtualAddress())
                    {
                        try
                        {
                            byte[] sub = new byte[0x80];
                            DmaMemory.ReadBuffer<byte>(val, sub.AsSpan(), false);
                            ulong w0 = BitConverter.ToUInt64(sub, 0);
                            ulong w8 = BitConverter.ToUInt64(sub, 8);
                            ulong w18 = BitConverter.ToUInt64(sub, 0x18);
                            ulong w20 = BitConverter.ToUInt64(sub, 0x20);
                            ulong w38 = BitConverter.ToUInt64(sub, 0x38);
                            ulong w40 = BitConverter.ToUInt64(sub, 0x40);
                            ulong w68 = BitConverter.ToUInt64(sub, 0x68);
                            ulong w70 = BitConverter.ToUInt64(sub, 0x70);
                            ulong w78 = BitConverter.ToUInt64(sub, 0x78);
                            Console.WriteLine($"      sub at +0x{o:X2} (0x{val:X}): w0=0x{w0:X} w8=0x{w8:X} w18=0x{w18:X} w20=0x{w20:X} w38=0x{w38:X} w40=0x{w40:X} w68=0x{w68:X} w70=0x{w70:X} w78=0x{w78:X}");
                        }
                        catch { }
                    }
                }
            }
        }

        private sealed class DiagDualWriter : TextWriter
        {
            private readonly TextWriter _w1;
            private readonly TextWriter _w2;
            public DiagDualWriter(TextWriter w1, TextWriter w2) { _w1 = w1; _w2 = w2; }
            public override Encoding Encoding => Encoding.UTF8;
            public override void Write(char value) { _w1.Write(value); _w2.Write(value); }
            public override void Write(string? value) { _w1.Write(value); _w2.Write(value); }
            public override void WriteLine(string? value) { _w1.WriteLine(value); _w2.WriteLine(value); }
            public override void Flush() { _w1.Flush(); _w2.Flush(); }
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--diag")
            {
                Log.AllocateConsoleWindow();
                using var fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diag.txt"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var sw = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };
                var dual = new DiagDualWriter(Console.Out, sw);
                Console.SetOut(dual);
                Console.SetError(dual);

                Console.WriteLine("[DIAG] Starting SCPSLDMA in diagnostic mode...");
                var config = new ScpslConfig();
                SharedProgram.Initialize(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory), config);
                ScpslOverlay.LoadConfig();
                GameReader.MasterMemWritesEnabled = true;
                GameReader.GunFlashlightEnabled = true;
                var dma = new ScpslMemory("SCPSL.exe");
                int attempts = 0;
                while (!dma.Ready && attempts < 300)
                {
                    Thread.Sleep(100);
                    attempts++;
                }
                if (!dma.Ready)
                {
                    Console.WriteLine("[DIAG] DMA failed to attach after 30s!");
                    return;
                }
                RunDiagnostic();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                GameReader.RestoreWorldFov();
                GameReader.RestoreViewmodelFov();
                GameReader.RestoreNoSway();
                GameReader.RestoreNoRecoil();
                GameReader.RestoreBunnyhop();
                GameReader.RestoreViewangleAim();
                GameReader.RestoreNoFlash();
                GameReader.RestoreNoSmoke();
                GameReader.RestoreBrightness();
                GameReader.RestoreGunFlashlight();
                GameReader.TryRestoreInstantAds();
                GameReader.TryRestoreTpv();
                Log.WriteLine($"[FATAL CRASH] Unhandled Exception: {e.ExceptionObject}");
            };

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                GameReader.RestoreWorldFov();
                GameReader.RestoreViewmodelFov();
                GameReader.RestoreNoSway();
                GameReader.RestoreNoRecoil();
                GameReader.RestoreBunnyhop();
                GameReader.RestoreViewangleAim();
                GameReader.RestoreNoFlash();
                GameReader.RestoreNoSmoke();
                GameReader.RestoreBrightness();
                GameReader.RestoreGunFlashlight();
                GameReader.TryRestoreInstantAds();
                GameReader.TryRestoreTpv();
                ScpslOverlay.CleanStaleImguiIni();
            };

            try
            {
                ScpslOverlay.CleanStaleImguiIni();

                // Default console to hidden unless showconsole.cfg exists or --console / --show-console / --diag is passed
                bool keepConsole = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "showconsole.cfg")) ||
                                   Array.Exists(args, a => string.Equals(a, "--console", StringComparison.OrdinalIgnoreCase) ||
                                                           string.Equals(a, "--show-console", StringComparison.OrdinalIgnoreCase) ||
                                                           string.Equals(a, "--diag", StringComparison.OrdinalIgnoreCase));
                if (!keepConsole)
                {
                    ScpslOverlay.SetConsoleVisible(false);
                }

                // Force Per-Monitor V2 DPI awareness so Windows doesn't scale or clip the overlay
                EnableDpiAwareness();

                var (screenWidth, screenHeight, maxFps) = MonitorChooser.GetInitialMetrics();

                Log.WriteLine($"Detected Display: {screenWidth}x{screenHeight} @ {maxFps} Hz");
                Log.WriteLine($"Starting SCPSL Client ({screenWidth}x{screenHeight}, Max FPS: {maxFps})...");
                var config = new ScpslConfig();
                SharedProgram.Initialize(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory), config);

                var dma = new ScpslMemory("scpsl.exe");
                var overlay = new ScpslOverlay(screenWidth, screenHeight, maxFps);

                Log.WriteLine("Starting DMA memory poll thread...");
                ScpslOverlay.StartDMALoop(overlay);

                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    try
                    {
                        if (ScpslOverlay.AutoSaveEnabled)
                            ScpslOverlay.SaveConfig();
                    }
                    catch { }
                    ScpslOverlay.CleanStaleImguiIni();
                };

                Log.WriteLine("Launching DirectX 11 Overlay... (F1 or INSERT = Toggle Menu)");
                overlay.Start().Wait();
                try
                {
                    if (ScpslOverlay.AutoSaveEnabled)
                        ScpslOverlay.SaveConfig();
                }
                catch { }
                ScpslOverlay.CleanStaleImguiIni();
                GameReader.RestoreWorldFov();
                GameReader.RestoreViewmodelFov();
                GameReader.RestoreNoSway();
                GameReader.RestoreNoRecoil();
                GameReader.RestoreBunnyhop();
                GameReader.RestoreViewangleAim();
                GameReader.RestoreNoFlash();
                GameReader.RestoreNoSmoke();
                GameReader.RestoreBrightness();
                GameReader.RestoreGunFlashlight();
            }
            catch (Exception ex)
            {
                ScpslOverlay.SetConsoleVisible(true);
                GameReader.RestoreWorldFov();
                GameReader.RestoreViewmodelFov();
                GameReader.RestoreNoSway();
                GameReader.RestoreNoRecoil();
                GameReader.RestoreBunnyhop();
                GameReader.RestoreViewangleAim();
                GameReader.RestoreNoFlash();
                GameReader.RestoreNoSmoke();
                GameReader.RestoreBrightness();
                GameReader.RestoreGunFlashlight();
                Log.WriteLine($"[ERROR] Application encountered an error: {ex.Message}");
                Log.WriteLine(ex.ToString());
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
        }
    }
    }
}

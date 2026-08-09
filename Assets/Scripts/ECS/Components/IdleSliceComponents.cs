using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    /// <summary>One entry per top idle title in docs/idle_mechanics_matrix.md.</summary>
    public enum IdleArchetype : byte
    {
        CookieClicker = 0,
        ClickerHeroes = 1,
        AdventureCapitalist = 2,
        UniversalPaperclips = 3,
        ADarkRoom = 4,
        AntimatterDimensions = 5,
        RealmGrinder = 6,
        NguIdle = 7,
        MelvorIdle = 8,
        EggInc = 9,
        IdleMinerTycoon = 10,
        TapTitans2 = 11,
        IdleHeroes = 12,
        AfkArena = 13,
        LegendOfMushroom = 14,
        CapybaraGo = 15,
        CatsAndSoup = 16,
        NekoAtsume = 17,
        FalloutShelter = 18
    }

    /// <summary>Singleton-ish slice root: currency + progression for one idle MVP.</summary>
    public struct IdleSliceState : IComponentData
    {
        public IdleArchetype Archetype;
        public double PrimaryCurrency;
        public double PrestigeCurrency;
        public float GlobalMultiplier;
        public int ProgressionLevel;
        public float Timer;
        public double ClickPower;
        public double PassiveRate;
        public int OwnedGenerators;
        public int ManagersHired;
        public int AssignedWorkers;
        public int MaxWorkers;
        public int EnemyHp;
        public int EnemyMaxHp;
        public float AfkChestSeconds;
        public int PhaseIndex;
        public int FactionId;
        public float EnergyPool;
        public float EnergyAllocated;
        public int SkillXp;
        public int CheckInCats;
        public bool HasOfflineClaim;
        /// <summary>Accrued output waiting for Claim (Fallout Shelter / AFK-style).</summary>
        public double PendingClaim;
    }

    public struct IdleSliceTag : IComponentData
    {
        public IdleArchetype Archetype;
    }

    /// <summary>Buyable generator / business / dimension / shaft tier.</summary>
    public struct BuyableGenerator : IComponentData
    {
        public int GeneratorId;
        public int OwnedCount;
        public double BaseCost;
        public float CostGrowth;
        public double BaseCps;
        public bool RequiresManager;
        public bool IsAutomated;
    }

    public struct IdleManager : IComponentData
    {
        public int TargetGeneratorId;
        public double HireCost;
        public bool IsHired;
    }

    public struct IdleCombatState : IComponentData
    {
        public double TapDamage;
        public double HeroDps;
        public int Zone;
        public double GoldPerKill;
        public float EnemyHp;
        public float EnemyMaxHp;
    }

    public struct IdleAssignmentStation : IComponentData
    {
        public int StationId;
        public int AssignedCount;
        public int Capacity;
        public double OutputPerWorker;
        public float Interval;
        public float Timer;
    }

    public struct IdleSkillNode : IComponentData
    {
        public int SkillId;
        public int Level;
        public int Xp;
        public int XpToLevel;
        public float TickInterval;
        public float Timer;
        public bool IsActive;
    }

    public struct IdleNarrativeState : IComponentData
    {
        public int RoomOrStep;
        public int StokeCount;
        public int ExploreUnlocked;
        public double Wood;
        public double SoftCurrency;
    }

    public struct IdleGachaState : IComponentData
    {
        public int PullCount;
        public double PullCost;
        public int BestRarity;
        public int Stage;
        /// <summary>LoM auto-lamp interval accumulator (earned after Stage &gt;= 1).</summary>
        public float AutoTimer;
    }

    // --- Events (enableable one-shots) ---
    // TargetSlice binds the event to one slice entity (Entity.Null = sole-slice fallback).

    public struct IdleClickEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public float Multiplier;
    }

    public struct IdleBuyGeneratorEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public int GeneratorId;
        public int Amount;
    }

    public struct IdleHireManagerEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public int TargetGeneratorId;
    }

    public struct IdleAssignWorkerEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public int StationId;
        public int Delta;
    }

    public struct IdleGachaPullEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
    }

    public struct IdleNarrativeActionEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public int ActionId; // 0=stoke/step/rub, 1=explore/advance, 2=craft
    }

    public struct IdleAllocateEnergyEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public float Amount;
    }

    public struct IdleClaimOfflineEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
    }

    public struct IdlePhaseShiftEvent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
    }
}

using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    /// <summary>
    /// Adaptive UI Toolkit HUD for one idle MVP slice. Builds controls from IdleArchetype.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(IdleSliceBootstrap))]
    public class IdleSliceUIController : MonoBehaviour
    {
        private UIDocument _doc;
        private IdleSliceBootstrap _bootstrap;
        private Label _title;
        private Label _howto;
        private Label _stats;
        private bool _built;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _bootstrap = GetComponent<IdleSliceBootstrap>();
        }

        private void Update()
        {
            EnsureUi();
            RefreshStats();
        }

        private void EnsureUi()
        {
            if (_built) return;
            var root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null) return;

            root.Clear();
            root.style.flexGrow = 1;
            root.style.paddingTop = 16;
            root.style.paddingLeft = 16;
            root.style.paddingRight = 16;
            root.style.paddingBottom = 16;
            root.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);

            _title = new Label(_bootstrap.DisplayName) { name = "TitleLabel" };
            _title.style.fontSize = 28;
            _title.style.color = Color.white;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.style.marginBottom = 6;
            root.Add(_title);

            _howto = new Label(_bootstrap.HowToPlay) { name = "HowToLabel" };
            _howto.style.fontSize = 14;
            _howto.style.color = new Color(0.85f, 0.85f, 0.85f);
            _howto.style.whiteSpace = WhiteSpace.Normal;
            _howto.style.marginBottom = 10;
            root.Add(_howto);

            _stats = new Label("...") { name = "StatsLabel" };
            _stats.style.fontSize = 18;
            _stats.style.color = new Color(1f, 0.84f, 0f);
            _stats.style.marginBottom = 12;
            root.Add(_stats);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            root.Add(row);

            AddButtonsForArchetype(row, _bootstrap.Archetype);
            _built = true;
        }

        private void AddButtonsForArchetype(VisualElement row, IdleArchetype arch)
        {
            switch (arch)
            {
                case IdleArchetype.CookieClicker:
                    Btn(row, "Click Cookie", () => FireClick(1f));
                    Btn(row, "Buy Generator", () => FireBuy(1));
                    Btn(row, "Prestige", FirePrestige);
                    break;
                case IdleArchetype.AdventureCapitalist:
                    Btn(row, "Collect", () => FireClick(1f));
                    Btn(row, "Buy Business", () => FireBuy(1));
                    Btn(row, "Hire Manager", () => FireHire(1));
                    Btn(row, "Angel Reset", FirePrestige);
                    break;
                case IdleArchetype.ClickerHeroes:
                case IdleArchetype.TapTitans2:
                    Btn(row, "Tap / Attack", () => FireClick(1f));
                    Btn(row, "Buy Hero DPS", () => FireBuy(1));
                    Btn(row, "Prestige", FirePrestige);
                    break;
                case IdleArchetype.UniversalPaperclips:
                    Btn(row, "Make Paperclip", () => FireClick(1f));
                    Btn(row, "Buy Autoclipper", () => FireBuy(1));
                    Btn(row, "Phase Shift", FirePhase);
                    break;
                case IdleArchetype.AntimatterDimensions:
                    Btn(row, "Buy Dimension", () => FireBuy(1));
                    Btn(row, "Click Antimatter", () => FireClick(1f));
                    Btn(row, "Prestige Layer", FirePhase);
                    break;
                case IdleArchetype.ADarkRoom:
                    Btn(row, "Stoke Fire", () => FireNarrative(0));
                    Btn(row, "Explore", () => FireNarrative(1));
                    Btn(row, "Craft", () => FireNarrative(2));
                    break;
                case IdleArchetype.RealmGrinder:
                    Btn(row, "Build", () => FireBuy(1));
                    Btn(row, "Align Good", () => FireAlloc(1f));
                    Btn(row, "Align Evil", () => FireAlloc(2f));
                    break;
                case IdleArchetype.NguIdle:
                    Btn(row, "Allocate Energy", () => FireAlloc(10f));
                    Btn(row, "Idle Tick Boost", () => FireClick(2f));
                    Btn(row, "Rebirth", FirePrestige);
                    break;
                case IdleArchetype.MelvorIdle:
                    Btn(row, "Train Skill", () => FireClick(1f));
                    Btn(row, "Claim Offline", FireClaim);
                    break;
                case IdleArchetype.EggInc:
                    Btn(row, "Hatch Burst", () => FireClick(5f));
                    Btn(row, "Upgrade Habitat", () => FireBuy(1));
                    Btn(row, "Soul Prestige", FirePrestige);
                    break;
                case IdleArchetype.IdleMinerTycoon:
                    Btn(row, "Collect Shaft", () => FireClick(1f));
                    Btn(row, "Upgrade Shaft", () => FireBuy(1));
                    Btn(row, "Hire Super-Manager", () => FireHire(1));
                    Btn(row, "New Mine", FirePrestige);
                    break;
                case IdleArchetype.IdleHeroes:
                    Btn(row, "Auto Fight", () => FireClick(1f));
                    Btn(row, "Gacha Pull", FireGacha);
                    Btn(row, "Claim AFK", FireClaim);
                    break;
                case IdleArchetype.AfkArena:
                    Btn(row, "Push Campaign", () => FireClick(1f));
                    Btn(row, "Open AFK Chest", FireClaim);
                    break;
                case IdleArchetype.LegendOfMushroom:
                    Btn(row, "Rub Lamp", FireGacha);
                    Btn(row, "Farm Stage Gold", () => FireClick(3f));
                    break;
                case IdleArchetype.CapybaraGo:
                    Btn(row, "Next Step", () => FireNarrative(1));
                    Btn(row, "Lucky Find", () => FireClick(2f));
                    break;
                case IdleArchetype.CatsAndSoup:
                    Btn(row, "Assign Cat", () => FireAssign(1));
                    Btn(row, "Unassign", () => FireAssign(-1));
                    break;
                case IdleArchetype.NekoAtsume:
                    Btn(row, "Place Food", () => FireNarrative(0));
                    Btn(row, "Check In", FireClaim);
                    break;
                case IdleArchetype.FalloutShelter:
                    Btn(row, "Assign Dweller", () => FireAssign(1));
                    Btn(row, "Unassign", () => FireAssign(-1));
                    Btn(row, "Claim Production", FireClaim);
                    break;
                default:
                    Btn(row, "Primary Action", () => FireClick(1f));
                    Btn(row, "Buy", () => FireBuy(1));
                    Btn(row, "Prestige", FirePrestige);
                    break;
            }
        }

        private static void Btn(VisualElement parent, string text, System.Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.style.height = 48;
            b.style.marginRight = 8;
            b.style.marginBottom = 8;
            b.style.paddingLeft = 12;
            b.style.paddingRight = 12;
            b.style.backgroundColor = new Color(0.15f, 0.55f, 0.25f);
            b.style.color = Color.white;
            b.style.fontSize = 16;
            parent.Add(b);
        }

        private void RefreshStats()
        {
            if (_stats == null) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            using var q = em.CreateEntityQuery(typeof(IdleSliceState));
            if (q.IsEmptyIgnoreFilter) return;
            var arr = q.ToComponentDataArray<IdleSliceState>(Unity.Collections.Allocator.Temp);
            if (arr.Length == 0) { arr.Dispose(); return; }
            var s = arr[0];
            arr.Dispose();
            _stats.text =
                $"Currency: {s.PrimaryCurrency:N1} | Prestige: {s.PrestigeCurrency:N0} | " +
                $"Lv/Zone: {s.ProgressionLevel} | CPS: {s.PassiveRate:N2} | " +
                $"Gens: {s.OwnedGenerators} | Mult: {s.GlobalMultiplier:N2}\n" +
                $"Click: {s.ClickPower:N1} | Workers: {s.AssignedWorkers}/{s.MaxWorkers} | " +
                $"HP: {s.EnemyHp}/{s.EnemyMaxHp} | Cats: {s.CheckInCats} | Chest: {s.AfkChestSeconds:N0}s";
        }

        private static bool TryGetEm(out EntityManager em)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                em = default;
                return false;
            }

            em = world.EntityManager;
            return true;
        }

        private void FireClick(float mult)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleClickEvent { Multiplier = mult });
        }

        private void FireBuy(int genId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleBuyGeneratorEvent { GeneratorId = genId, Amount = 1 });
        }

        private void FireHire(int targetGenId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleHireManagerEvent { TargetGeneratorId = targetGenId });
        }

        private void FireAssign(int delta)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleAssignWorkerEvent { StationId = 1, Delta = delta });
        }

        private void FireNarrative(int actionId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleNarrativeActionEvent { ActionId = actionId });
        }

        private void FireAlloc(float amount)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleAllocateEnergyEvent { Amount = amount });
        }

        private void FireGacha()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleGachaPullEvent());
        }

        private void FireClaim()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleClaimOfflineEvent());
        }

        private void FirePhase()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdlePhaseShiftEvent());
        }

        private void FirePrestige()
        {
            FirePhase();
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new PrestigeEventComponent());
        }
    }
}

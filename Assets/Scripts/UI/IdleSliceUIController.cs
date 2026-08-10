using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.UI
{
    /// <summary>
    /// Sole live idle-slice HUD (UI Toolkit). Adaptive archetype controls + cosmetics.
    /// IdleGameHUD.uxml / IdleUIManagerSystem are deferred sandbox-only (see IdleGameHudBinder).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(IdleSliceBootstrap))]
    public class IdleSliceUIController : MonoBehaviour
    {
        private static readonly (string Label, int SkinIndex, double Cost)[] SkinRows =
        {
            ("Classic Blue", 0, 0.0),
            ("Crimson Red", 1, 5.0),
            ("Solid Gold", 2, 15.0),
            ("Emerald Neon", 3, 30.0),
        };

        private UIDocument _doc;
        private IdleSliceBootstrap _bootstrap;
        private Label _title;
        private Label _howto;
        private Label _stats;
        private VisualElement _actionsContainer;
        private VisualElement _cosmeticsContainer;
        private Button _actionsTabButton;
        private Button _cosmeticsTabButton;
        private readonly Button[] _skinButtons = new Button[4];
        private bool _built;

        private PanelSettings _runtimePanel;

        /// <summary>Root visual tree after HUD build (null until EnsureHudBuilt / first Update).</summary>
        public VisualElement RootVisualElement => _doc != null ? _doc.rootVisualElement : null;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _bootstrap = GetComponent<IdleSliceBootstrap>();
            EnsurePanelSettings();
        }

        private void EnsurePanelSettings()
        {
            if (_doc == null) _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.panelSettings != null) return;
            // TODO: [STUB] runtime PanelSettings so slices play without a project PanelSettings asset
            _runtimePanel = ScriptableObject.CreateInstance<PanelSettings>();
            _runtimePanel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _runtimePanel.referenceResolution = new Vector2Int(1920, 1080);
            _doc.panelSettings = _runtimePanel;
        }

        private void OnDestroy()
        {
            if (_runtimePanel != null)
            {
                if (Application.isPlaying)
                    Destroy(_runtimePanel);
                else
                    DestroyImmediate(_runtimePanel);
                _runtimePanel = null;
            }
        }

        private void Update()
        {
            EnsureUi();
            RefreshStats();
            RefreshSkinButtons();
        }

        /// <summary>
        /// Builds the Actions/Cosmetics HUD once without requiring Play Mode Update.
        /// Used by EditMode smoke tests (UI2-02).
        /// </summary>
        public void EnsureHudBuilt()
        {
            if (_doc == null) _doc = GetComponent<UIDocument>();
            if (_bootstrap == null) _bootstrap = GetComponent<IdleSliceBootstrap>();
            EnsurePanelSettings();
            EnsureUi();
        }

        private void EnsureUi()
        {
            if (_built) return;
            EnsurePanelSettings();
            var root = _doc != null ? _doc.rootVisualElement : null;
            if (root == null) return;

            root.Clear();
            // TODO: [STUB] inline styles — prefer USS classes when idle HUD graduates past prototype
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

            var tabRow = new VisualElement { name = "TabRow" };
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;
            root.Add(tabRow);

            _actionsTabButton = TabBtn(tabRow, "Actions", true);
            _cosmeticsTabButton = TabBtn(tabRow, "Cosmetics", false);
            _actionsTabButton.clicked += ShowActionsTab;
            _cosmeticsTabButton.clicked += ShowCosmeticsTab;

            _actionsContainer = new VisualElement { name = "ActionsContainer" };
            _actionsContainer.style.flexDirection = FlexDirection.Row;
            _actionsContainer.style.flexWrap = Wrap.Wrap;
            root.Add(_actionsContainer);
            AddButtonsForArchetype(_actionsContainer, _bootstrap.Archetype);

            _cosmeticsContainer = new VisualElement { name = "CosmeticsContainer" };
            _cosmeticsContainer.style.display = DisplayStyle.None;
            root.Add(_cosmeticsContainer);
            BuildCosmeticsPanel(_cosmeticsContainer);

            _built = true;
        }

        private static Button TabBtn(VisualElement parent, string text, bool selected)
        {
            var b = new Button { text = text, name = text + "TabButton" };
            // TODO: [STUB] inline tab styles
            b.style.flexGrow = 1;
            b.style.height = 40;
            b.style.marginRight = 4;
            b.style.fontSize = 18;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.color = Color.white;
            b.style.backgroundColor = selected
                ? new Color(0.35f, 0.35f, 0.4f)
                : new Color(0.2f, 0.2f, 0.22f);
            parent.Add(b);
            return b;
        }

        private void ShowActionsTab()
        {
            if (_actionsContainer != null) _actionsContainer.style.display = DisplayStyle.Flex;
            if (_cosmeticsContainer != null) _cosmeticsContainer.style.display = DisplayStyle.None;
            SetTabSelected(_actionsTabButton, true);
            SetTabSelected(_cosmeticsTabButton, false);
        }

        private void ShowCosmeticsTab()
        {
            if (_actionsContainer != null) _actionsContainer.style.display = DisplayStyle.None;
            if (_cosmeticsContainer != null) _cosmeticsContainer.style.display = DisplayStyle.Flex;
            SetTabSelected(_actionsTabButton, false);
            SetTabSelected(_cosmeticsTabButton, true);
        }

        private static void SetTabSelected(Button btn, bool selected)
        {
            if (btn == null) return;
            btn.style.backgroundColor = selected
                ? new Color(0.35f, 0.35f, 0.4f)
                : new Color(0.2f, 0.2f, 0.22f);
        }

        private void BuildCosmeticsPanel(VisualElement parent)
        {
            for (int i = 0; i < SkinRows.Length; i++)
            {
                var rowDef = SkinRows[i];
                var row = new VisualElement();
                // TODO: [STUB] inline cosmetics row styles
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 8;
                row.style.paddingLeft = 8;
                row.style.paddingRight = 8;
                row.style.paddingTop = 6;
                row.style.paddingBottom = 6;
                row.style.backgroundColor = new Color(1f, 1f, 1f, 0.1f);

                var label = new Label(rowDef.Cost <= 0
                    ? $"{rowDef.Label} (Default)"
                    : $"{rowDef.Label} ({rowDef.Cost:N0} P)");
                label.style.fontSize = 16;
                label.style.color = Color.white;
                label.style.flexGrow = 1;
                row.Add(label);

                int skinIndex = rowDef.SkinIndex;
                double cost = rowDef.Cost;
                var btn = new Button(() => FireCosmetic(skinIndex, cost))
                {
                    name = skinIndex == 0 ? "EquipSkin0Button" : $"BuySkin{skinIndex}Button"
                };
                btn.style.width = 140;
                btn.style.height = 40;
                btn.style.fontSize = 14;
                btn.style.color = Color.white;
                _skinButtons[i] = btn;
                row.Add(btn);
                parent.Add(row);
            }

            RefreshSkinButtons();
        }

        private void RefreshSkinButtons()
        {
            if (!_built) return;
            for (int i = 0; i < SkinRows.Length; i++)
            {
                UpdateSkinButtonState(_skinButtons[i], SkinRows[i].SkinIndex, SkinRows[i].Cost);
            }
        }

        private static void UpdateSkinButtonState(Button btn, int skinIndex, double cost)
        {
            if (btn == null) return;

            int currentEquipped = GameProgressData.CurrentSkinIndex;
            bool isUnlocked = GameProgressData.IsSkinUnlocked(skinIndex);

            if (currentEquipped == skinIndex)
            {
                btn.text = "EQUIPPED";
                btn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            }
            else if (isUnlocked)
            {
                btn.text = "EQUIP";
                btn.style.backgroundColor = new StyleColor(new Color(0f, 0.48f, 1f, 1f));
            }
            else
            {
                btn.text = $"BUY ({cost:N0} P)";
                btn.style.backgroundColor = new StyleColor(new Color(0.55f, 0.15f, 0.15f, 1f));
            }
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
                    Btn(row, "Align Good", () => FireAlign(1));
                    Btn(row, "Align Evil", () => FireAlign(2));
                    Btn(row, "Rebirth", FirePrestige);
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
                    // Auto-combat is passive (IdleCombatState DPS) — no click-for-gold button.
                    Btn(row, "Gacha Pull", FireGacha);
                    Btn(row, "Claim AFK", FireClaim);
                    break;
                case IdleArchetype.AfkArena:
                    // Chest-only MVP: campaign push advances stage/chest, no flat gold.
                    Btn(row, "Campaign Progress", () => FireClick(1f));
                    Btn(row, "Open AFK Chest", FireClaim);
                    break;
                case IdleArchetype.LegendOfMushroom:
                    Btn(row, "Rub Lamp", FireGacha);
                    Btn(row, "Farm Stage Gold", () => FireClick(3f));
                    break;
                case IdleArchetype.CapybaraGo:
                    Btn(row, "Take Step", () => FireNarrative(0));
                    Btn(row, "Next Step", () => FireNarrative(1));
                    Btn(row, "Lucky Find", () => FireClick(2f));
                    break;
                case IdleArchetype.CatsAndSoup:
                    Btn(row, "Assign Cat", () => FireAssign(1));
                    Btn(row, "Unassign", () => FireAssign(-1));
                    break;
                case IdleArchetype.NekoAtsume:
                    Btn(row, "Place Food", () => FireNarrative(0));
                    Btn(row, "Place Toys", () => FireNarrative(1));
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
            // TODO: [STUB] inline action button styles
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
            if (!TryGetEm(out var em)) return;

            IdleSliceState s;
            if (_bootstrap != null && _bootstrap.IsSpawned &&
                em.Exists(_bootstrap.SliceEntity) &&
                em.HasComponent<IdleSliceState>(_bootstrap.SliceEntity))
            {
                s = em.GetComponentData<IdleSliceState>(_bootstrap.SliceEntity);
            }
            else
            {
                using var q = em.CreateEntityQuery(typeof(IdleSliceState));
                if (q.IsEmptyIgnoreFilter) return;
                var arr = q.ToComponentDataArray<IdleSliceState>(Unity.Collections.Allocator.Temp);
                if (arr.Length == 0) { arr.Dispose(); return; }
                s = arr[0];
                arr.Dispose();
            }

            _stats.text =
                $"Currency: {s.PrimaryCurrency:N1} | Prestige: {s.PrestigeCurrency:N0} | " +
                $"Lv/Zone: {s.ProgressionLevel} | CPS: {s.PassiveRate:N2} | " +
                $"Gens: {s.OwnedGenerators} | Mult: {s.GlobalMultiplier:N2}\n" +
                $"Click: {s.ClickPower:N1} | Workers: {s.AssignedWorkers}/{s.MaxWorkers} | " +
                $"HP: {s.EnemyHp}/{s.EnemyMaxHp} | Cats: {s.CheckInCats} | " +
                $"Chest: {s.AfkChestSeconds:N0}s | Pending: {s.PendingClaim:N1}";

            Entity sliceEntity = Entity.Null;
            if (_bootstrap != null && _bootstrap.IsSpawned &&
                em.Exists(_bootstrap.SliceEntity))
                sliceEntity = _bootstrap.SliceEntity;

            if (sliceEntity != Entity.Null && em.HasComponent<IdleGachaState>(sliceEntity))
            {
                var g = em.GetComponentData<IdleGachaState>(sliceEntity);
                _stats.text += $"\nPulls: {g.PullCount} | BestRarity: {g.BestRarity} | Stage: {g.Stage}";
            }

            if (sliceEntity != Entity.Null && em.HasComponent<IdleCombatState>(sliceEntity) &&
                s.Archetype == IdleArchetype.IdleHeroes)
            {
                var c = em.GetComponentData<IdleCombatState>(sliceEntity);
                _stats.text += $" | HeroDps: {c.HeroDps:N1}";
            }

            if (sliceEntity != Entity.Null && em.HasComponent<IdleNarrativeState>(sliceEntity) &&
                s.Archetype == IdleArchetype.CapybaraGo)
            {
                var n = em.GetComponentData<IdleNarrativeState>(sliceEntity);
                string explore = n.ExploreUnlocked != 0 ? "unlocked" : "locked";
                _stats.text += $"\nRoomOrStep: {n.RoomOrStep} | Soft: {n.SoftCurrency:N0} | Explore: {explore}";
            }

            if (s.Archetype == IdleArchetype.NguIdle)
            {
                _stats.text += $"\nEnergy: {s.EnergyAllocated:N0}/{s.EnergyPool:N0}";
            }

            if (s.Archetype == IdleArchetype.AntimatterDimensions)
            {
                string band = IdlePrestigeMath.GetAntimatterPhaseBand(s.PhaseIndex);
                _stats.text += $"\nLayer: {band} (Phase {s.PhaseIndex})";
            }

            if (s.Archetype == IdleArchetype.RealmGrinder && s.FactionId != 0)
            {
                string faction = s.FactionId == 1 ? "Good" : s.FactionId == 2 ? "Evil" : "?";
                _stats.text += $" | Faction: {faction}";
            }
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

        private Entity ResolveTargetSlice(EntityManager em)
        {
            if (_bootstrap != null && _bootstrap.IsSpawned &&
                em.Exists(_bootstrap.SliceEntity) &&
                em.HasComponent<IdleSliceState>(_bootstrap.SliceEntity))
            {
                return _bootstrap.SliceEntity;
            }

            using var q = em.CreateEntityQuery(typeof(IdleSliceState));
            if (q.CalculateEntityCount() == 1)
                return q.GetSingletonEntity();
            return Entity.Null;
        }

        private void FireClick(float mult)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleClickEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                Multiplier = mult
            });
        }

        private void FireBuy(int genId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleBuyGeneratorEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                GeneratorId = genId,
                Amount = 1
            });
        }

        private void FireHire(int targetGenId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleHireManagerEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                TargetGeneratorId = targetGenId
            });
        }

        private void FireAssign(int delta)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleAssignWorkerEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                StationId = 1,
                Delta = delta
            });
        }

        private void FireNarrative(int actionId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleNarrativeActionEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                ActionId = actionId
            });
        }

        private void FireAlloc(float amount)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleAllocateEnergyEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                Amount = amount
            });
        }

        private void FireAlign(int factionId)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleFactionAlignEvent
            {
                TargetSlice = ResolveTargetSlice(em),
                FactionId = factionId
            });
        }

        private void FireGacha()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleGachaPullEvent { TargetSlice = ResolveTargetSlice(em) });
        }

        private void FireClaim()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdleClaimOfflineEvent { TargetSlice = ResolveTargetSlice(em) });
        }

        private void FirePhase()
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new IdlePhaseShiftEvent { TargetSlice = ResolveTargetSlice(em) });
        }

        private void FirePrestige()
        {
            // Single prestige path — do not also FirePhase (double conversion).
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new PrestigeEventComponent { TargetSlice = ResolveTargetSlice(em) });
        }

        private void FireCosmetic(int skinIndex, double cost)
        {
            if (!TryGetEm(out var em)) return;
            var e = em.CreateEntity();
            em.AddComponentData(e, new CosmeticPurchaseEventComponent
            {
                TargetSlice = ResolveTargetSlice(em),
                TargetSkinIndex = skinIndex,
                PrestigeCost = cost
            });
        }
    }
}

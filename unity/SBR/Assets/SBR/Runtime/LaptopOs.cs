using System;
using System.Globalization;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>The small, app-switching shell around the SureThing sportsbook.</summary>
    public sealed class LaptopOs
    {
        // SureThing Direction tokens. Keep the font seam separate: a licensed TMP asset replaces
        // LegacyRuntime when Allen resolves the Bell Centennial / fallback decision.
        internal static readonly Color Ink = new Color32(0x16, 0x16, 0x0F, 255);
        internal static readonly Color Surface = new Color32(0x1C, 0x1C, 0x13, 255);
        internal static readonly Color SurfaceRaised = new Color32(0x23, 0x23, 0x19, 255);
        internal static readonly Color Rule = new Color32(0x3C, 0x3C, 0x2C, 255);
        internal static readonly Color RuleSoft = new Color32(0x2C, 0x2C, 0x20, 255);
        internal static readonly Color Muted = new Color32(0x6E, 0x6B, 0x5E, 255);
        internal static readonly Color White = new Color32(0xD9, 0xD4, 0xC5, 255);
        internal static readonly Color TonerSecondary = new Color32(0x9C, 0x98, 0x88, 255);
        internal static readonly Color Accent = new Color32(0x5E, 0x86, 0xB8, 255); // biro
        internal static readonly Color BiroDeep = new Color32(0x3F, 0x69, 0x96, 255);
        internal static readonly Color MoneyGold = new Color32(0xD9, 0xA4, 0x41, 255); // wax
        internal static readonly Color WaxLit = new Color32(0xF0, 0xC0, 0x66, 255);
        internal static readonly Color WaxDeep = new Color32(0x8A, 0x66, 0x20, 255);
        internal static readonly Color MoneyGood = new Color32(0xD9, 0xA4, 0x41, 255);
        internal static readonly Color MoneyBad = new Color32(0xB4, 0x48, 0x3A, 255); // house stamp
        internal static readonly Color SignalCyan = new Color32(0x9C, 0x98, 0x88, 255);
        // Punched-out type on a solid wax field (PLACE TICKET's label). Distinct from Ink: Ink is the
        // general document ground, WaxInk is specifically the colour type takes when it sits ON wax.
        internal static readonly Color WaxInk = new Color32(0x1A, 0x13, 0x05, 255);

        // Document-layer tokens with no Color shape (opacity/rotation/height), kept here alongside the
        // palette so every SureThing constant traces back to one place.
        internal const float MarkedWashAlpha = 0.07f; // --marked-wash: rgba(94,134,184,.07) == Accent at this alpha
        internal const float WaxHighlightOpacity = 0.26f; // --wax-highlight-opacity
        internal const float WaxHighlightRotateDeg = -0.5f; // --wax-highlight-rotate
        internal const float WaxHighlightHeight = 6f; // --wax-highlight-h, px
        internal const float TonerGrainOpacity = 0.05f; // --toner-grain-opacity

        private enum App { Desktop, SureThing, OldSlips, Verdict }

        private readonly RectTransform _root;
        private readonly Font _font;
        private readonly Font _fontCond;
        private readonly LaptopScreen _host;
        private readonly int _width;
        private readonly int _height;
        private readonly RectTransform _desktop;
        private readonly RectTransform _app;
        private readonly SportsbookApp _sportsbook;
        private readonly OldSlipsApp _oldSlips;
        private App _activeApp = App.Desktop;
        private SportsbookApp.Tab _tab = SportsbookApp.Tab.Lobby;
        private Phase _lastPhase;
        private bool _hasPhase;
        private string _signature;
        private int _lastDisplayRevision = -1;
        private string _toast;
        private float _toastUntil;
        // S35(b): the destination a toast was raised for — captured at ShowToast time, so a
        // toast only ever draws on the screen that raised it and never bleeds onto one it does
        // not own (LEDGER, MY BETS' read-only mirror, or anywhere else the player navigates to).
        private App _toastApp;
        private SportsbookApp.Tab _toastTab;

        public LaptopOs(RectTransform root, Font font, Font fontCond, LaptopScreen host, int width, int height)
        {
            _root = root;
            _font = font;
            _fontCond = fontCond;
            _host = host;
            _width = width;
            _height = height;

            _desktop = LaptopUi.MakePanel(root, "Desktop", Vector2.zero, Vector2.zero,
                Vector2.zero, new Vector2(width, height), new Color(0f, 0f, 0f, 0f));
            // The wallpaper Graphic lives on its OWN child: MakePanel's GameObject already
            // carries an Image, and Unity allows one Graphic per object — AddComponent on
            // the panel returns null (the room-boot NRE this comment buries).
            var wallGo = new GameObject("Wallpaper", typeof(CanvasRenderer), typeof(LaptopWallpaperGraphic));
            wallGo.transform.SetParent(_desktop, false);
            var wallpaper = wallGo.GetComponent<LaptopWallpaperGraphic>();
            wallpaper.raycastTarget = false;
            RectTransform wallRt = wallpaper.rectTransform;
            wallRt.anchorMin = Vector2.zero;
            wallRt.anchorMax = Vector2.one;
            // S48: the wallpaper is the remainder, not the whole screen. The shared rail takes the
            // top 34px and the shared tray the bottom 34px, as on every other destination, and the
            // ground fills what is left. offsetMin insets the bottom edge, offsetMax the top.
            wallRt.offsetMin = new Vector2(0f, NotebookChrome.TrayHeight);
            wallRt.offsetMax = new Vector2(0f, -NotebookChrome.RailHeight);
            wallGo.transform.SetAsFirstSibling();
            _app = LaptopUi.MakePanel(root, "App", Vector2.zero, Vector2.zero,
                Vector2.zero, new Vector2(width, height), Ink);
            _app.gameObject.SetActive(false);

            _sportsbook = new SportsbookApp(_app, _font, _fontCond, _host, Invalidate, SelectTab, OpenHome, OpenLedger);
            // S31: OldSlipsApp reuses SportsbookApp.BuildTabStrip for its four-tab strip, so it
            // needs the same per-tab navigation the strip's own buttons drive elsewhere — SelectTab
            // is that mechanism, unchanged from what SportsbookApp above already uses.
            _oldSlips = new OldSlipsApp(_app, _font, _fontCond, OpenHome, OpenSportsbook, SelectTab);
            BuildDesktop();

            // The document's own toner grain (palette-surething.css --toner-grain-opacity), built
            // once here and parented directly to the top-level canvas root rather than to Desktop or
            // App. Render()/ClearChildren only ever touch _app's children, so this survives every
            // rebuild untouched — zero per-rebuild cost — and being the last sibling under _root, it
            // sits above whichever of Desktop/App is currently active, matching the reference kit
            // (app.jsx z-index:9 over the whole 1024x704 sheet).
            // Re-enabled: the shader that makes this a grain rather than a wash now exists. It was
            // disabled while the pass used normal alpha blending, which could only add light and so
            // lifted the ground from (24,24,16) to (52,52,48). SBR/TonerGrain blends around a 0.5
            // midpoint instead, so the mean effect is zero and only the texture changes.
            LaptopUi.MakeTonerGrain(_root);
        }

        public void Tick(Run run, BetslipModel slip)
        {
            if (_toast != null && Time.unscaledTime > _toastUntil)
            {
                _toast = null;
                _signature = null;
            }
            if (!_hasPhase || run.Phase != _lastPhase)
            {
                _lastPhase = run.Phase;
                _hasPhase = true;
                ApplyPhaseDefault(run.Phase);
                Invalidate();
            }

            // Fast display values (clock/prob/score/suspension) refresh IN PLACE — only
            // structural revisions rebuild the canvas (Sol, F_0.3.0 performance finding).
            RevealedView view = _host.tv != null ? _host.tv.RevealedView : null;
            if (view != null && view.DisplayRevision != _lastDisplayRevision
                && _activeApp == App.SureThing && _tab == SportsbookApp.Tab.MyBets)
            {
                _lastDisplayRevision = view.DisplayRevision;
                _sportsbook.UpdateMirrorDisplay(view);
            }

            string viewRevision = view != null
                ? view.Revision.ToString(CultureInfo.InvariantCulture) : "-";
            string signature = string.Concat(
                _host.director.RunGeneration, "|", run.Phase, "|", run.Round, "|",
                ((long)run.Bank).ToString(CultureInfo.InvariantCulture), "|", run.Tickets.Count, "|",
                run.ShopOffers.Count, "|", run.ConsumableOffers.Count, "|", run.OwnedRelics.Count, "|",
                run.OwnedConsumables.Count, "|", slip.Picks.Count, "|", ((long)slip.Stake).ToString(CultureInfo.InvariantCulture), "|",
                (int)slip.Modifier, "|", slip.BoostLeg, "|", _activeApp, "|", _tab, "|", viewRevision,
                "|", _toast);
            if (signature == _signature) return;
            _signature = signature;
            Rebuild(run, slip);
        }

        /// <summary>Test/debug surface — the same transitions the desktop icons and tabs
        /// drive, exposed so PlayMode can walk the OS without simulating cursor clicks.</summary>
        public bool OnDesktop => _activeApp == App.Desktop;
        public SportsbookApp.Tab CurrentTab => _tab;

        public void OpenSportsbook(SportsbookApp.Tab tab)
        {
            _activeApp = App.SureThing;
            _tab = tab;
            Invalidate();
        }

        public void OpenDesktop()
        {
            _activeApp = App.Desktop;
            Invalidate();
        }

        private void OpenLedger()
        {
            _activeApp = App.OldSlips;
            Invalidate();
        }

        public void ResetForRun()
        {
            _activeApp = App.Desktop;
            _tab = SportsbookApp.Tab.Lobby;
            _hasPhase = false;
            _signature = null;
            _toast = null;
        }

        private void ApplyPhaseDefault(Phase phase)
        {
            switch (phase)
            {
                case Phase.Betting:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.Lobby;
                    break;
                case Phase.Sweat:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.MyBets;
                    break;
                case Phase.Shop:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.Rewards;
                    // S26: states the fact (rewards is open, when it closes), never an imperative —
                    // "spend your comps" told the player what to do, which this surface's toasts don't.
                    ShowToast("REWARDS OPEN UNTIL THE NEXT PAYMENT");
                    break;
                case Phase.RunWon:
                case Phase.RunLost:
                    _activeApp = App.Verdict;
                    break;
            }
        }

        private void Rebuild(Run run, BetslipModel slip)
        {
            _desktop.gameObject.SetActive(_activeApp == App.Desktop);
            _app.gameObject.SetActive(_activeApp != App.Desktop);
            if (_activeApp == App.Desktop) return;

            if (_activeApp == App.SureThing)
                _sportsbook.Render(run, slip, _tab, run.Phase == Phase.Sweat);
            else if (_activeApp == App.OldSlips)
                _oldSlips.Render(run);
            else
                RenderVerdict(run);

            // S35(b): a toast belongs to the destination that raised it and never draws over a
            // read-only mirror — the shop's toast used to persist across navigation and had been
            // observed rendering on top of LEDGER. Gated on both the current app AND tab matching
            // the ones recorded in ShowToast, so it is also suppressed on MY BETS (the other
            // read-only mirror living inside App.SureThing) and reappears only if the player
            // returns to the exact destination that raised it before it expires.
            if (_toast != null && _activeApp == _toastApp && _tab == _toastTab)
            {
                // S9 defect 6: this used to float 62px above the tray, which is inside the Rewards
                // board's own content band — it rendered across the offer rows and over the LEAVE
                // button rather than beside them (same occlusion class already fixed for
                // SportsbookApp's LockReason). The whole work area and masthead are packed on every
                // destination, but the rail has a genuinely empty stretch between the sticker and the
                // clock (NotebookChrome.BuildRail, x 350-870) on every screen, toast included, so the
                // toast sits there now — its own space, not drawn over anyone else's. White rather
                // than Accent: a house-generated status line is not a mark HE chose (Law Two).
                // Overflow (not the Wrap default) for the same single-line reason as TaskbarText —
                // this must not re-flow into a second line and spill past the rail band.
                LaptopUi.MakeText(_app, "Toast", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(360f, -5f), new Vector2(500f, 24f), 13, TextAnchor.MiddleCenter,
                    White, _toast, _font).horizontalOverflow = HorizontalWrapMode.Overflow;
            }
        }

        private void BuildDesktop()
        {
            // S44 + S45: the wallpaper is the lifted ground and its toner grain, and nothing else.
            //
            // What stood here: "SURE" in toner beside "THING." in biro at 23px — the house's
            // wordmark, the largest element on the screen, half of it drawn in the one ink that is
            // only ever what HE chose. The two-ink rule settles it before the story question is
            // reached, and the story question has an answer anyway: S8 is Design-verified on the
            // finding that this chrome reads as *his* machine, and a machine wearing the operator's
            // logo contradicts that on the same surface. The house owns the app; the player owns
            // the machine.
            //
            // And beneath it, "the number never lies" — deleted, not softened (S45). Alone on an
            // otherwise empty screen the slot is the whole screen, so the line stops reading as the
            // bookmaker's marketing and starts reading as the product's promise of a guaranteed
            // win. C10's shape: there is no smaller version of that claim that is not the claim.
            //
            // The band this vacated is NOT headroom (R30). It is where S48's shared 34px rail
            // lands when the desktop folds into NotebookChrome, and that rail already carries the
            // machine's own marks — "NOTEBOOK" and the PROPERTY OF NOBODY sticker. S44 permits an
            // optional dead-manufacturer wordmark here in --toner-3; it is deliberately not built,
            // because it would be a second instance of exactly what the rail is about to bring.
            //
            // The toner grain S44 names is already on this screen: MakeTonerGrain is parented to
            // _root in the constructor, above whichever of Desktop/App is active, so it needs
            // nothing here.

            // S46: one name, SURETHING, everywhere the player sees it. "Sportsbook" was a second
            // name for the same app, in a third case — the desktop called it one thing, the tray
            // another, the masthead a third. The GameObject name stays "SureThing" (S16 exempts
            // code identifiers, and SureThingLedgerTests reaches the tray slot by it).
            // S44 again, via S47's wording — "the S loses its biro under S44". The app's own icon
            // is not something the player drew, so it cannot be in his ink; the same rule that
            // deleted the wordmark above takes the glyph. It goes to full --toner, which is also
            // where S47 lands an installed glyph anyway, so it was not a colour to revisit here.
            // S48: the icon and the tray slot below it are two controls for one app on one screen,
            // so they route through one action. They did not before the fold — the icon set
            // _activeApp inline and left the tab alone, while the tray slot calls OpenSportsbook,
            // which restores the tab the current phase expects. Clicking the icon and clicking the
            // slot would have landed the player on different tabs. That divergence existed only
            // because the two controls had never been on the same screen.
            MakeDesktopIcon("SureThing", "S", "SURETHING", new Vector2(34f, -120f),
                IconState.Installed, OpenSportsbook);
            // S47: LEDGER takes the installed treatment with its "$" at full toner. It was being
            // drawn in --ground-3 — the chip colour, in the glyph's argument — which is the same
            // value as the tile behind it, so the one destination on this machine that is not the
            // sportsbook announced itself with a glyph a step off invisible.
            MakeDesktopIcon("OldSlips", "$", "LEDGER", new Vector2(34f, -225f),
                IconState.Installed, OpenOldSlips);
            // S47: "(soon)" is deleted. The product does not put its own roadmap on his desktop,
            // and the treatment already says these do not open.
            MakeDesktopIcon("Mail", "@", "MAIL", new Vector2(34f, -330f), IconState.NotInstalled, null);
            MakeDesktopIcon("Bank", "¤", "BANK", new Vector2(34f, -435f), IconState.NotInstalled, null);

            // S48: the desktop carries the same NotebookChrome as every other destination. That was
            // S8's whole finding — one chrome, built once, consumed everywhere — and the desktop's
            // own 54px taskbar was the last copy of it. Everything that bar carried lands somewhere
            // real: HOME on the rail's identity band, the centre "SURETHING · LEDGER" label on the
            // tray's actual app slots, and "02:47 · 12%" on the rail's own clock and battery.
            //
            // The two drifts already fixed in this method are the argument for folding rather than
            // against it. That bar was `rgba(.025, .02, .05, .94)` — effectively black AND
            // blue-tinted, breaking the lifted-black rule and the no-cool-colour rule at once — and
            // its clock read "03:17 AM · 12%" while the rail one click away read 02:47, one machine
            // claiming two times. Both were a copy drifting from the original it was copied from,
            // and both were found by eye rather than by anything that could have failed.
            //
            // Running.None because nothing is running here: both apps are backgrounded, both slots
            // read raised and muted, and both launch. `minimize` is null for the same reason —
            // there is no running app to drop out of, and under None no slot can reach that action.
            //
            // This changes a Design-verified surface: S8 returns to review against a desktop frame.
            NotebookChrome.BuildRail(_desktop, _width, _font);
            NotebookChrome.BuildTray(_desktop, _width, _font, NotebookChrome.Running.None,
                OpenSportsbook, OpenOldSlips, null);
        }

        /// <summary>S47: installed versus not installed is a two-state vocabulary, not a value.
        ///
        /// Every appearance difference between the two is derived here from the state itself, so a
        /// third combination cannot be authored at a call site. That is not tidiness: before this,
        /// the caller passed a glyph colour by hand and the caption's strength was inferred from
        /// whether an `onClick` happened to be null, and those two facts drifted apart exactly the
        /// way you would expect — the LEDGER icon was drawing its `$` in `--ground-3`, the chip's
        /// colour handed to the glyph, which put an installed app's glyph one step off the ground
        /// it sat on. It was on every desktop capture ever taken and nobody saw it, because there
        /// was no state to disagree with.</summary>
        private enum IconState
        {
            /// Full `--toner` glyph and caption, over a `--ground-3` chip.
            Installed,

            /// Glyph and caption at `--toner-3`, and no chip at all. An icon that does not open
            /// reads as not-installed by treatment — which is the whole reason `(soon)` is not
            /// merely unnecessary but forbidden: it is the product putting its roadmap on his
            /// desktop to say something the treatment already said.
            NotInstalled,
        }

        private void MakeDesktopIcon(string name, string glyph, string label, Vector2 position,
            IconState state, Action onClick)
        {
            bool installed = state == IconState.Installed;
            // One ink for glyph and caption. Splitting them across two arguments is what let them
            // drift, and the ruling treats them as one statement.
            Color ink = installed ? White : Muted;
            // No chip means no chip, not a fainter one. The chip is the button's own Image, so
            // this is also the only thing that draws the tile at all.
            Color chip = installed ? SurfaceRaised : new Color(0f, 0f, 0f, 0f);
            // Interactability comes from the state too, so an icon cannot look installed and
            // refuse to open, or vice versa. A test holds that pairing across all four icons —
            // a runtime throw on a desktop build would help nobody.
            Button button = LaptopUi.MakeButton(_desktop, name, glyph, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(86f, 76f), 28, chip, ink, onClick, _font, installed);
            // S46: icon labels take the machine's voice — caps, condensed. Condensed is set here,
            // once, for the class rather than per icon; the caps live in each caller's string.
            // S46 left `Mail (soon)` and `Bank (soon)` in sentence case because their text was
            // S47's to rule; S47 has now deleted the parenthetical and they take the voice with
            // the rest of them.
            //
            // The authored 11 renders at 13: MakeText clamps every size to 13 (as does
            // MeasureWidth, so measurement and render still agree). Left as authored rather than
            // "corrected" to a number that changes nothing on the frame.
            // Named "Caption", not "Label": MakeButton already gives every button a text child
            // called "Label" — the glyph, here — so this was a second sibling under the same name
            // and the caption could not be reached by lookup at all. Nothing was drawn wrong, both
            // Texts rendered; it was only unaddressable, which is why it survived. Found by the
            // S46 test below asking the icon what it calls the app and being handed "S".
            LaptopUi.MakeText(button.GetComponent<RectTransform>(), "Caption", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, -25f), new Vector2(150f, 22f), 11,
                TextAnchor.UpperCenter, ink, label, _fontCond);
        }

        private void RenderVerdict(Run run)
        {
            LaptopUi.ClearChildren(_app);
            bool won = run.Phase == Phase.RunWon;
            LaptopUi.MakePanel(_app, "VerdictBg", Vector2.zero, Vector2.zero, Vector2.zero,
                new Vector2(_width, _height), new Color(0.03f, 0.02f, 0.06f, 1f));
            LaptopUi.MakeText(_app, "VerdictBrand", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -54f), new Vector2(800f, 36f), 22, TextAnchor.UpperCenter, White,
                // S46: was "SureThing." — a fifth spelling, mixed case with a full stop, on the one
                // screen a player only reaches at the end of a run. Typeface left alone: S46 rules
                // the name and gives a voice only for icon labels, and this screen is unruled.
                "SURETHING", _font);
            Text verdict = LaptopUi.MakeText(_app, "Verdict", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(900f, 60f), 30, TextAnchor.MiddleCenter,
                won ? MoneyGold : MoneyBad,
                won ? "THE HOUSE BLINKS FIRST" : "THE BOOKIE COLLECTS", _font);
            verdict.horizontalOverflow = HorizontalWrapMode.Overflow;
            Text final = LaptopUi.MakeText(_app, "Final", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 14f), new Vector2(900f, 36f), 18, TextAnchor.MiddleCenter, White,
                $"FINAL BANK {LaptopUi.Money(run.Bank)}   ·   SEED {run.Rng.RunSeed}", _font);
            final.horizontalOverflow = HorizontalWrapMode.Overflow;
            LaptopUi.MakeButton(_app, "NewRun", "NEW RUN", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 58f), new Vector2(300f, 52f), 19, Accent, White,
                () => { _host.director.StartNewRun(); Invalidate(); }, _font);
        }

        private void ShowToast(string toast)
        {
            _toast = toast;
            _toastUntil = Time.unscaledTime + 4f;
            // S35(b): record the destination raising this toast. Safe to read _activeApp/_tab
            // here — every call site sets them before calling ShowToast (ApplyPhaseDefault's
            // Phase.Shop case sets both immediately above its own ShowToast call).
            _toastApp = _activeApp;
            _toastTab = _tab;
        }

        private void Invalidate()
        {
            _signature = null;
            if (_toast != null && Time.unscaledTime > _toastUntil) _toast = null;
        }

        internal void OpenHome()
        {
            _activeApp = App.Desktop;
            Invalidate();
        }

        internal void OpenSportsbook()
        {
            _activeApp = App.SureThing;
            if (_lastPhase == Phase.Shop) _tab = SportsbookApp.Tab.Rewards;
            else if (_lastPhase == Phase.Sweat) _tab = SportsbookApp.Tab.MyBets;
            else _tab = SportsbookApp.Tab.Lobby;
            Invalidate();
        }

        internal void OpenOldSlips()
        {
            _activeApp = App.OldSlips;
            Invalidate();
        }

        internal void SelectTab(SportsbookApp.Tab tab)
        {
            _activeApp = App.SureThing;
            _tab = tab;
            Invalidate();
        }
    }

    /// <summary>Code-built, texture-free wallpaper: four corner colors interpolate across the canvas.</summary>
    internal sealed class LaptopWallpaperGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color topLeft = LaptopOs.SurfaceRaised;
            Color topRight = LaptopOs.Ink;
            Color bottomLeft = LaptopOs.Ink;
            Color bottomRight = LaptopOs.Surface;
            vh.AddVert(new Vector3(r.xMin, r.yMin), (Color32)bottomLeft, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), (Color32)topLeft, Vector2.up);
            vh.AddVert(new Vector3(r.xMax, r.yMax), (Color32)topRight, Vector2.one);
            vh.AddVert(new Vector3(r.xMax, r.yMin), (Color32)bottomRight, Vector2.right);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }

    /// <summary>Code-built wash behind a marked form entry: a left-to-right fade from a flat colour
    /// to fully transparent by 70% of the element's width, matching the reference kit's
    /// <c>linear-gradient(90deg, var(--marked-wash), transparent 70%)</c>. Same per-vertex-gradient
    /// technique as <see cref="LaptopWallpaperGraphic"/> — four vertices, no texture — because the
    /// rest of the fade (70%-100%) is already fully transparent and costs nothing left undrawn.</summary>
    internal sealed class MarkedWashGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color32 tint = color;
            Color32 clear = new Color32(tint.r, tint.g, tint.b, 0);
            float stopX = r.xMin + r.width * 0.7f;
            vh.AddVert(new Vector3(r.xMin, r.yMin), tint, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), tint, Vector2.up);
            vh.AddVert(new Vector3(stopX, r.yMax), clear, Vector2.one);
            vh.AddVert(new Vector3(stopX, r.yMin), clear, Vector2.right);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }

    /// <summary>S34: the 26px ruled-paper ground margin.jsx paints behind every margin —
    /// <c>repeating-linear-gradient(180deg, transparent 0 25px, var(--rule-soft) 25px 26px)</c> —
    /// as untextured geometry rather than a texture asset. One shared class, the same technique as
    /// <see cref="LaptopWallpaperGraphic"/> and <see cref="MarkedWashGraphic"/>: it emits one flat
    /// quad per 26px line directly in <see cref="OnPopulateMesh"/> (no texture, no per-rebuild
    /// allocation — everything here is stack arithmetic feeding VertexHelper's own buffers), so the
    /// same GameObject/Component this stack already recreates on every rebuild of the panel it
    /// sits on costs one extra ~20-quad (80-vertex, 40-triangle) mesh for a 530px-tall margin —
    /// negligible next to the panel's own text and button churn, and drawn in the same batch as
    /// every other flat-colour Graphic here (no material or shader of its own).</summary>
    internal sealed class MarginRuledPaperGraphic : Graphic
    {
        private const float Period = 26f; // margin.jsx: repeating-linear-gradient period
        private const float LineHeight = 1f; // the "25px 26px" band — the rule itself

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color32 rule = LaptopOs.RuleSoft;
            int vertexIndex = 0;
            for (float depth = Period; depth - LineHeight < r.height; depth += Period)
            {
                float bandTop = r.yMax - (depth - LineHeight);
                float bandBottom = r.yMax - depth;
                if (bandBottom < r.yMin) bandBottom = r.yMin;
                vh.AddVert(new Vector3(r.xMin, bandBottom), rule, Vector2.zero);
                vh.AddVert(new Vector3(r.xMin, bandTop), rule, Vector2.up);
                vh.AddVert(new Vector3(r.xMax, bandTop), rule, Vector2.one);
                vh.AddVert(new Vector3(r.xMax, bandBottom), rule, Vector2.right);
                vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex);
                vertexIndex += 4;
            }
        }
    }

    internal static class LaptopUi
    {
        public static void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
        }

        public static Text MakeText(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, TextAnchor align, Color color, string content, Font font)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            if (font != null) text.font = font;
            text.fontSize = Mathf.Max(13, fontSize);
            text.alignment = align;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;

            // Truncate clips whole LINES, so a box shorter than one line of its own font renders
            // NOTHING — not a clipped glyph, nothing at all. Silent and total, with every test still
            // green because the Text object exists and holds the right string.
            //
            // Wiring the production faces cost three display elements exactly this way in one go:
            // Archivo's line metrics are taller than the built-in fallback's, so boxes authored at
            // 1.08x and 1.16x their font size stopped fitting a single line, and the masthead and
            // the payout figure simply vanished.
            //
            // A box may legitimately be shorter than its content — clipping a long wrapped paragraph
            // is a real layout choice. It is never useful for a box to be too short for its FIRST
            // line, so that case falls back to Overflow rather than rendering emptiness. Callers keep
            // their authored height and position; only the failure mode changes. Evaluated after
            // sizeDelta because preferredHeight depends on the wrap width.
            if (text.preferredHeight > size.y)
                text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        /// <summary>Measures a string's natural (unwrapped) width in a dynamic font at a given size,
        /// requesting the glyphs into the font's atlas first so a cold cache never reports zero.</summary>
        public static float MeasureWidth(Font font, string text, int fontSize)
        {
            if (font == null || string.IsNullOrEmpty(text)) return 0f;
            int size = Mathf.Max(13, fontSize);
            font.RequestCharactersInTexture(text, size, FontStyle.Normal);
            float width = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                if (font.GetCharacterInfo(text[i], out CharacterInfo info, size, FontStyle.Normal))
                    width += info.advance;
            }
            return width;
        }

        /// <summary>Shortens content to fit maxWidth, trailing with an ellipsis rather than ever
        /// silently cutting mid-word. A no-op when the string already fits.</summary>
        public static string FitText(Font font, string content, int fontSize, float maxWidth)
        {
            if (font == null || string.IsNullOrEmpty(content)) return content;
            if (MeasureWidth(font, content, fontSize) <= maxWidth) return content;
            const string ellipsis = "…";
            float budget = maxWidth - MeasureWidth(font, ellipsis, fontSize);
            if (budget <= 0f) return ellipsis;
            int lo = 0, hi = content.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (MeasureWidth(font, content.Substring(0, mid), fontSize) <= budget) lo = mid;
                else hi = mid - 1;
            }
            string trimmed = content.Substring(0, lo).TrimEnd();
            return trimmed.Length > 0 ? trimmed + ellipsis : ellipsis;
        }

        /// <summary>Fits a variable-length label between a fixed prefix and suffix (e.g. "1. " and
        /// the price) so the parts that always carry meaning — the index, the price — never get
        /// truncated; only the label in between ever loses characters, and only behind an ellipsis.</summary>
        public static string FitLabelKeepingSuffix(Font font, string prefix, string label, string suffix,
            int fontSize, float maxWidth)
        {
            if (font == null) return prefix + label + suffix;
            float reserved = MeasureWidth(font, prefix, fontSize) + MeasureWidth(font, suffix, fontSize);
            string fitLabel = FitText(font, label, fontSize, Mathf.Max(0f, maxWidth - reserved));
            return prefix + fitLabel + suffix;
        }

        public static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return rt;
        }

        public static Image MakeStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return image;
        }

        public static Button MakeButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, Color background, Color foreground, Action onClick,
            Font font, bool interactable = true)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = background;
            image.raycastTarget = interactable;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(Mathf.Max(44f, size.x), Mathf.Max(32f, size.y));
            rt.anchoredPosition = position;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => onClick());
            Text text = MakeText(rt, "Label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, rt.sizeDelta, fontSize, TextAnchor.MiddleCenter, foreground, label, font);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return button;
        }

        /// <summary>Ruling S18: a wax primary action is a wax field, wax-ink type, and a 2px
        /// wax-deep edge — never all three by hand at each call site. PLACE TICKET and LEAVE — NEXT
        /// ROUND are the two controls on this surface that qualify (the phase-advancing or
        /// ticket-committing action on their screen, never a mark the player chose — that stays
        /// biro), so this is written once and both route through it.
        ///
        /// The edge is four 2px panels drawn INSET inside the button's own rect, added after
        /// MakeButton has already clamped that rect to the >=44x32 hit-target floor — so the edge
        /// spends none of that budget. The button's sizeDelta, and therefore its hit target and
        /// layout footprint, is identical to a plain MakeButton call.
        ///
        /// <paramref name="interactable"/> gates the edge, not the passed-in colours: callers already
        /// pass their own muted background/foreground for the disabled case exactly as before, and a
        /// disabled Button additionally gets Unity's own automatic dim tint on top of whatever colour
        /// it was given. So "disabled" here means Unity's ColorTint-dimmed look, and the edge is
        /// skipped for it — the greyed-out state keeps its current appearance untouched, per the
        /// ruling, even in the edge case where a caller's colours are still nominally wax but
        /// interactable is false.</summary>
        public static Button MakeWaxPrimary(RectTransform parent, string name, string label, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, int fontSize, Color background, Color foreground,
            Action onClick, Font font, bool interactable = true)
        {
            Button button = MakeButton(parent, name, label, anchor, pivot, position, size, fontSize,
                background, foreground, onClick, font, interactable);
            if (interactable)
            {
                RectTransform rt = button.GetComponent<RectTransform>();
                Vector2 rectSize = rt.sizeDelta;
                const float t = 2f;
                MakePanel(rt, "WaxEdgeTop", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero,
                    new Vector2(rectSize.x, t), LaptopOs.WaxDeep);
                MakePanel(rt, "WaxEdgeBottom", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero,
                    new Vector2(rectSize.x, t), LaptopOs.WaxDeep);
                MakePanel(rt, "WaxEdgeLeft", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, t),
                    new Vector2(t, rectSize.y - t * 2f), LaptopOs.WaxDeep);
                MakePanel(rt, "WaxEdgeRight", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, t),
                    new Vector2(t, rectSize.y - t * 2f), LaptopOs.WaxDeep);

                // Keep the label drawn on top of the frame: the edges are added after MakeButton's
                // "Label" child, so without this they'd be the last (topmost) siblings. Same
                // SetAsLastSibling convention SportsbookApp.BuildSlip uses for PayoutHighlight.
                Text labelText = button.GetComponentInChildren<Text>();
                if (labelText != null) labelText.transform.SetAsLastSibling();
            }
            return button;
        }

        public static Image MakeSprite(RectTransform parent, string name, Sprite sprite, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, Color tint)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = tint;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return image;
        }

        /// <summary>F3: the only way to draw a rule. Defaults to <see cref="LaptopOs.RuleSoft"/>
        /// (--rule-soft) so every pre-existing call site — all horizontal, all internal-content
        /// rules — keeps rendering exactly the pixel it always has without passing a 7th argument.
        /// Pass <paramref name="color"/> explicitly (typically <see cref="LaptopOs.Rule"/>,
        /// --rule, the stronger token) for a seam between document bands rather than a rule inside
        /// one. Before this, LaptopOs.Rule had zero references anywhere in the runtime — the
        /// strong token existed in the palette but nothing could reach it.</summary>
        public static RectTransform MakeRule(RectTransform parent, string name, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, Color? color = null)
            => MakePanel(parent, name, anchor, pivot, position, size, color ?? LaptopOs.RuleSoft);

        /// <summary>The marked-form-entry wash (palette-surething.css --marked-wash), stretched to
        /// fill <paramref name="parent"/> exactly — sized this way (rather than a hand-picked rect)
        /// so it is trivially contained within whatever row it marks. Caller is responsible for only
        /// adding this when the row is actually selected, and for adding it before any sibling text/
        /// buttons so it draws underneath them.</summary>
        public static void MakeMarkedWash(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(CanvasRenderer), typeof(MarkedWashGraphic));
            go.transform.SetParent(parent, false);
            MarkedWashGraphic wash = go.GetComponent<MarkedWashGraphic>();
            wash.color = new Color(LaptopOs.Accent.r, LaptopOs.Accent.g, LaptopOs.Accent.b,
                LaptopOs.MarkedWashAlpha);
            wash.raycastTarget = false;
            RectTransform rt = wash.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>S34: the ruled-paper ground (margin.jsx, MarginRuledPaperGraphic), stretched to
        /// fill <paramref name="parent"/> exactly. Ships on the working margin and every passive
        /// margin alike from this one call — there is no second implementation. Caller adds this
        /// before any sibling text/buttons so it sits behind them, same convention as
        /// <see cref="MakeMarkedWash"/>.</summary>
        public static void MakeMarginRuledPaper(RectTransform parent, string name)
        {
            // typeof(CanvasRenderer) is NOT optional and NOT decoration. Graphic declares it via
            // [RequireComponent], but that attribute is only honoured by AddComponent — the
            // GameObject constructor's type list ignores it. A Graphic without a CanvasRenderer is
            // never asked for geometry at all: OnPopulateMesh simply never runs, nothing renders,
            // nothing errors, and every test stays green. All three Graphic subclasses on this
            // surface were built this way and none of them had ever drawn.
            var go = new GameObject(name, typeof(CanvasRenderer), typeof(MarginRuledPaperGraphic));
            go.transform.SetParent(parent, false);
            MarginRuledPaperGraphic graphic = go.GetComponent<MarginRuledPaperGraphic>();
            graphic.raycastTarget = false;
            RectTransform rt = graphic.rectTransform;
            // Explicit size from the parent's already-resolved rect, NOT anchor-stretch.
            //
            // Stretching leaves the size to a layout pass, and this canvas is built imperatively:
            // the rect read zero at build time, UGUI culls a zero-size graphic before asking it for
            // geometry, and OnPopulateMesh was never called once — the diagnostic added to it
            // printed nothing at all, which is what pointed here. Every panel on this surface is
            // built with an explicit sizeDelta for the same reason; this was the one that was not.
            //
            // The parent is always a MakePanel-built panel with its own explicit size, so its rect
            // is resolved by the time this runs.
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = parent.rect.size;
            rt.anchoredPosition = Vector2.zero;

        }

        /// S27: the printed position rail's own width — reserved on the right edge of every
        /// scrolling body (<see cref="MakeScrollBody"/>) whether or not the rail ends up drawn
        /// (<see cref="FinishScrollBody"/>), so a list's content column never shifts width
        /// between a state that scrolls and one that doesn't.
        public const float RailReserve = 4f;

        /// <summary>S25(amended)/S42/S27: the one place an interior list becomes scrollable.
        /// Builds a fixed-footprint host at (anchor/pivot/position/size) carrying a standard
        /// UGUI <see cref="ScrollRect"/> — the room's own InputSystemUIInputModule already
        /// routes wheel/drag input to it; nothing here re-plumbs input — over a
        /// <see cref="RectMask2D"/>-clipped viewport, and returns the Content rect callers fill
        /// top-down. Content's own height is the caller's to set once every row is built
        /// (<see cref="FinishScrollBody"/>), matching this file's existing convention of a
        /// build-time y-cursor returning its own total (BuildStagedReceipt, BuildRewardOffer).
        ///
        /// The viewport is built via <see cref="MakePanel"/> — not a hand-rolled Graphic
        /// construction — specifically because a custom Graphic missing
        /// <c>typeof(CanvasRenderer)</c> in its GameObject constructor is this stack's own
        /// four-times-repeated defect (see MakeMarginRuledPaper above, S49). MakePanel's Image
        /// is left fully transparent but RAYCASTABLE: without some raycastable Graphic under the
        /// pointer, Unity's GraphicRaycaster never finds a hit inside this body at all, so wheel/
        /// drag events never reach the ScrollRect it climbs up to find — fatal for a body like
        /// the ledger's, whose rows carry zero buttons (S32/S43, read-only) and would otherwise
        /// offer nothing raycastable to hit anywhere in the list.</summary>
        public static RectTransform MakeScrollBody(RectTransform parent, string name, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, out RectTransform host, out ScrollRect scrollRect)
        {
            host = MakePanel(parent, name, anchor, pivot, position, size, new Color(0f, 0f, 0f, 0f));
            scrollRect = host.gameObject.AddComponent<ScrollRect>();

            RectTransform viewport = MakePanel(host, "Viewport", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.pivot = new Vector2(0f, 1f);
            viewport.offsetMin = Vector2.zero;
            // Reserves the rail's own width on the right, whether or not FinishScrollBody ends
            // up drawing it — see RailReserve.
            viewport.offsetMax = new Vector2(-RailReserve, 0f);
            viewport.GetComponent<Image>().raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content: top-anchored, growing downward as rows are added. Width tracks the
            // viewport via stretch anchors (anchorMin/Max.x = 0/1, sizeDelta.x = 0 = "exactly
            // parent width"); height is 0 until FinishScrollBody sets it from the caller's own
            // measured total.
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            RectTransform content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            // Clamped, not the default Elastic: this surface is a printed document, not a
            // bouncy app, and Clamped keeps the rail's thumb position (FinishScrollBody,
            // ScrollRailThumb) a direct, non-overshooting read of ScrollRect's own normalized
            // position at every moment, including mid-drag.
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            return content;
        }

        /// <summary>Closes out a body opened by <see cref="MakeScrollBody"/> once the caller
        /// knows its content's real total height: sets Content's final size, then S27's rail —
        /// present iff <paramref name="contentHeight"/> actually exceeds
        /// <paramref name="viewportHeight"/>, absent otherwise — as two plain panels (never a
        /// Unity Scrollbar+handle sprite), 4px at the body's right edge, full body height,
        /// <see cref="LaptopOs.RuleSoft"/> track (--rule-soft), <see cref="LaptopOs.Muted"/>
        /// thumb (--toner-3) sized to the visible fraction and floored at 24px. The thumb's own
        /// position then tracks live scrolling via <see cref="ScrollRailThumb"/>, because this
        /// canvas only rebuilds when LaptopOs's own state signature changes (Tick()) — scrolling
        /// the wheel is not part of that signature, so nothing would otherwise move the thumb
        /// again after this one build.</summary>
        public static void FinishScrollBody(RectTransform host, ScrollRect scrollRect, RectTransform content,
            float contentHeight, float viewportHeight)
        {
            content.sizeDelta = new Vector2(0f, contentHeight);
            bool scrolls = contentHeight > viewportHeight + 0.5f;
            scrollRect.vertical = scrolls;
            if (!scrolls) return;

            RectTransform track = MakePanel(host, "RailTrack", new Vector2(1f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(RailReserve, viewportHeight), LaptopOs.RuleSoft);
            float thumbHeight = Mathf.Max(24f, viewportHeight * viewportHeight / contentHeight);
            RectTransform thumb = MakePanel(track, "RailThumb", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(RailReserve, thumbHeight), LaptopOs.Muted);

            var binderGo = new GameObject("ScrollRailBinder", typeof(ScrollRailThumb));
            binderGo.transform.SetParent(host, false);
            binderGo.GetComponent<ScrollRailThumb>().Bind(scrollRect, thumb, viewportHeight, thumbHeight);
        }

        /// <summary>Builds the document's own toner grain (palette-surething.css
        /// --toner-grain-opacity) exactly once and stretches it to fill <paramref name="root"/>.
        /// Cost: one 128x128 runtime texture, one material and one Image, built a single time per
        /// laptop — never regenerated, never touched by a rebuild.
        ///
        /// Noise is centred on 0.5 and drawn through SBR/TonerGrain, which blends DstColor SrcColor
        /// so that 0.5 is a no-op, above it lightens and below it darkens. That is what makes this a
        /// grain pass rather than a wash: the mean effect on the ground is zero.
        ///
        /// The first version of this was an ordinary white Image at 5% alpha, and it bleached the
        /// sheet — measured (24,24,16) to (52,52,48), double the luminance and neutral grey against
        /// a warm olive ground — because normal alpha blending can only add light. If this ever
        /// reverts to a plain UI material, that is the failure it will reintroduce.
        ///
        /// Still an approximation, not a match: the reference kit's SVG feTurbulence is a filter, and
        /// this is a static tile.</summary>
        public static void MakeTonerGrain(RectTransform root)
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "SureThingTonerGrain",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            // Fixed seed: the grain is part of the document, not an animation. It must be identical
            // on every boot and every rebuild, or the sheet would visibly reshuffle its own texture.
            var rng = new System.Random(0xC0FFEE);
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                // Full-range luminance noise. 128 (= 0.5) is the shader's "leave this pixel alone"
                // midpoint, and _Strength alone decides how far from it the pass actually pulls —
                // which is what --toner-grain-opacity means. An earlier version also narrowed the
                // noise to 108..148 before applying strength, so the two limits multiplied and the
                // grain landed at about +/-0.004 of a luminance level: measurably present, visually
                // nothing. Constrain this in one place, not two.
                byte v = (byte)rng.Next(0, 256);
                pixels[i] = new Color32(v, v, v, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), size);

            var go = new GameObject("TonerGrain", typeof(Image));
            go.transform.SetParent(root, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Tiled;

            Shader shader = Shader.Find("SBR/TonerGrain");
            if (shader == null)
            {
                // Without the signed-blend material this element does active harm, so it removes
                // itself rather than falling back to a plain white overlay.
                Debug.LogWarning("[LaptopOs] SBR/TonerGrain shader missing; skipping toner grain "
                    + "rather than bleaching the sheet with an additive fallback.");
                UnityEngine.Object.Destroy(go);
                return;
            }
            var material = new Material(shader) { name = "SureThingTonerGrain" };
            material.SetFloat("_Strength", LaptopOs.TonerGrainOpacity);
            image.material = material;
            // Full-bleed and on top of everything: without this it silently eats every click on the
            // laptop screen.
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Color Dim(Color color) => new Color(color.r, color.g, color.b, 0.55f);

        public static string TeamShort(Team team)
        {
            int split = team.Name.LastIndexOf(' ');
            return (split >= 0 ? team.Name.Substring(split + 1) : team.Name).ToUpperInvariant();
        }

        public static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static Color FromRgb(uint rgb)
        {
            return new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f,
                (rgb & 0xff) / 255f, 1f);
        }
    }

    /// <summary>S27: keeps a scroll body's rail thumb tracking the ScrollRect's live scroll
    /// offset between full-tree rebuilds. LaptopOs only rebuilds a screen's canvas when its own
    /// state signature changes (LaptopOs.Tick) — dragging the rail or spinning the mouse wheel
    /// is not part of that signature — so nothing would otherwise move the thumb again after the
    /// one build LaptopUi.FinishScrollBody performs it in. Not a Graphic; carries no
    /// CanvasRenderer requirement.</summary>
    internal sealed class ScrollRailThumb : MonoBehaviour
    {
        private RectTransform _thumb;
        private float _travel;

        public void Bind(ScrollRect scrollRect, RectTransform thumb, float trackHeight, float thumbHeight)
        {
            _thumb = thumb;
            _travel = Mathf.Max(0f, trackHeight - thumbHeight);
            scrollRect.onValueChanged.AddListener(OnScroll);
            // No initial OnScroll(scrollRect.normalizedPosition) call: that property reads
            // ScrollRect's own content bounds, which are only current after UGUI's layout pass
            // has run at least once and are not reliable to query synchronously in the same frame
            // the ScrollRect was just configured. Unnecessary anyway — thumb is already built at
            // the top (MakePanel's own (0,0) anchoredPosition), matching a freshly-built
            // ScrollRect's own top-scrolled Content exactly; only future scroll events need this
            // listener at all.
        }

        private void OnScroll(Vector2 normalized)
        {
            // The bound thumb is destroyed (with the rest of the canvas) on the next full
            // rebuild; Unity's lifetime-aware == keeps this call a no-op afterward rather than
            // throwing on a listener the destroyed ScrollRect never had a chance to clear.
            if (_thumb == null) return;
            float y = -Mathf.Clamp01(1f - normalized.y) * _travel;
            _thumb.anchoredPosition = new Vector2(_thumb.anchoredPosition.x, y);
        }
    }

    /// <summary>
    /// The machine's own chrome — the rail across the top and the tray across the bottom. It
    /// belongs to the notebook, not to whichever app is running, so it is defined once here and
    /// every screen calls into it.
    ///
    /// It used to be copy-pasted into each screen's builder, which let the two copies drift: the
    /// sportsbook drew the rail at 12px and the ledger drew the same rail at 13px, so the machine's
    /// own furniture changed size when you switched app. The same roles also carried different
    /// object names in each copy ("AppName" vs "Messages", "Clock" vs "SystemFacts"), which made
    /// the duplication invisible to a name-based test.
    ///
    /// Chrome text is 12px by deliberate exception: the type floor is 13px for anything stating a
    /// product fact, and 12px is allowed only for OS furniture carrying no product meaning. Every
    /// string here — machine mark, sticker, clock, unread count, disk and update state — is set
    /// dressing. Nothing a player needs to make a decision may be added at this size.
    /// </summary>
    internal static class NotebookChrome
    {
        public const float RailHeight = 34f;
        public const float TrayHeight = 34f;

        /// OS-furniture size. See the class note before reusing it for anything else.
        private const int ChromeText = 12;

        /// --st-rail-pad-x (space.css). Shared by the rail's own edges and the tray's left edge
        /// (F8), and now also by the battery/clock group and the MESSAGES slot's dot, so every
        /// element that anchors off "the rail's own inset" reads from one place.
        private const float RailPadX = 11f;

        public const string MachineMark = "NOTEBOOK";
        public const string StickerText = "PROPERTY OF NOBODY";

        /// Fixed fiction, not a live clock: the shared spec pins the machine at 02:47 so every
        /// capture and every direction concept is comparable. W2: the battery is now its own
        /// element (swatch/border/fill, built in BuildRail) rather than a "▰" appended to this
        /// string, so it can represent a state at all.
        public const string ClockText = "02:47";

        /// The machine's charge, stated once. The desktop prints it as a percentage and the rail
        /// draws it as a bar, and before this they disagreed about the machine they describe: the
        /// desktop said 12% while the rail's fill was hardcoded to the stamp colour because the
        /// kit's JSX happens to default batteryLow to true in its demo. A demo default is not a
        /// fact about this machine. Both now read the same number.
        public const int BatteryPercent = 12;

        /// Below this the bar takes the house's stamp. It is the one place oxide is allowed to mean
        /// something other than the house acting on the document — the kit spells it out
        /// (OsRail.jsx: batteryLow ? --stamp : --toner-3), and it is the machine's own hardware
        /// warning rather than a status tint on a product fact.
        public const int BatteryLowThreshold = 20;

        private const string MessagesLabel = "MESSAGES";
        private const string MessagesBadge = "1";
        private const string SystemFactsText = "DISK 61% FULL    NO UPDATES";

        /// <summary>Which app the tray draws as running. <see cref="None"/> is the desktop: both
        /// apps are backgrounded, both slots read raised and muted, and both launch.
        ///
        /// The two-value version of this encoded "exactly one of them is running" — the ledger's
        /// state was derived as `!sportsbookRunning` and its action from the other branch of the
        /// sportsbook's own ternary. That held while only the two apps consumed this chrome, and it
        /// made the desktop's state unrepresentable rather than merely unwritten (S48).</summary>
        public enum Running { None, Sportsbook, Ledger }

        public static RectTransform BuildRail(RectTransform parent, float width, Font font)
        {
            RectTransform rail = LaptopUi.MakePanel(parent, "NotebookRail", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(width, RailHeight),
                LaptopOs.SurfaceRaised);
            // F8: OsRail.jsx's own padding is --st-rail-pad-x (11px) on both edges — this rail
            // (and the tray below) used 14px. Shared here, so the correction lands on every screen
            // that calls BuildRail/BuildTray, not just the ledger.

            // W2 (identity mark): OsRail.jsx pairs an 11x11 --toner-3 swatch with the identity
            // word in --toner-2, 7px apart — not the single "■  NOTEBOOK" string this used to be,
            // which faked the swatch as a glyph rather than drawing one. Weight 600 is unreachable
            // here (S20/C15: production faces are variable fonts, legacy UI.Text renders only the
            // default instance) and is rendered at normal weight rather than faked with
            // FontStyle.Bold — the signed gap, not a fix.
            const float swatchSize = 11f;
            const float identityGap = 7f; // OsRail's identity-mark gap: swatch -> word
            const float groupGap = 11f;   // OsRail's own flex gap between its three children
            LaptopUi.MakePanel(rail, "IdentitySwatch", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(RailPadX, 0f), new Vector2(swatchSize, swatchSize), LaptopOs.Muted);
            float wordX = RailPadX + swatchSize + identityGap;
            LaptopUi.MakeText(rail, "Machine", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(wordX, 0f), new Vector2(160f, 24f), ChromeText, TextAnchor.MiddleLeft,
                LaptopOs.TonerSecondary, MachineMark, font);

            // W2 (sticker): a bordered chip — --biro text, a rule-w border in --biro-deep, 2px 6px
            // padding, tilted -.6deg. Sized to hug its own measured text (MeasureWidth, the same
            // primitive F7/FitText already use elsewhere on this surface) rather than a fixed
            // guess, and started from the identity word's own measured width plus the rail's own
            // 11px gap — so its left edge is provably clear of "NOTEBOOK" for any font metrics,
            // not just the ones this was eyeballed against.
            const float ruleW = 1f; // --rule-w
            const float stickerPadX = 6f;
            const float stickerPadY = 2f;
            float machineWidth = LaptopUi.MeasureWidth(font, MachineMark, ChromeText);
            float stickerX = wordX + machineWidth + groupGap;
            float stickerTextW = LaptopUi.MeasureWidth(font, StickerText, ChromeText);
            const float stickerTextH = 14f;
            float stickerW = stickerTextW + stickerPadX * 2f;
            float stickerH = stickerTextH + stickerPadY * 2f;
            RectTransform sticker = LaptopUi.MakePanel(rail, "Sticker", new Vector2(0f, .5f),
                new Vector2(0f, .5f), new Vector2(stickerX, 0f), new Vector2(stickerW, stickerH),
                Color.clear);
            // Rotation is a pure render-space transform: it never touches sticker's own rect
            // (sizeDelta/anchoredPosition), and every neighbour on this rail is placed from those
            // same authored values, not from sticker's rotated corners — so nothing here reflows
            // from the rotation, by construction. The rotated bounding box itself grows by well
            // under 2px on every edge at -.6deg (Δw ≈ stickerH·sin(.6°) ≈ 0.2px, Δh ≈
            // stickerW·sin(.6°) ≈ 1.7px for a chip this size), against >=20px of measured clearance
            // to both the identity word on its left and the clock group on its right — confirmed by
            // this geometry, not by a capture.
            sticker.localEulerAngles = new Vector3(0f, 0f, -0.6f);
            LaptopUi.MakePanel(sticker, "StickerBorderTop", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(stickerW, ruleW), LaptopOs.BiroDeep);
            LaptopUi.MakePanel(sticker, "StickerBorderBottom", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(stickerW, ruleW), LaptopOs.BiroDeep);
            LaptopUi.MakePanel(sticker, "StickerBorderLeft", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, ruleW), new Vector2(ruleW, stickerH - ruleW * 2f), LaptopOs.BiroDeep);
            LaptopUi.MakePanel(sticker, "StickerBorderRight", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, ruleW), new Vector2(ruleW, stickerH - ruleW * 2f), LaptopOs.BiroDeep);
            LaptopUi.MakeText(sticker, "StickerLabel", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                Vector2.zero, new Vector2(stickerTextW + 2f, stickerTextH), ChromeText,
                TextAnchor.MiddleCenter, LaptopOs.Accent, StickerText, font);

            // W2 (battery): a 20x9 rect bordered --toner-3, with an inner fill inset 1.5px on
            // top/bottom/left, 5px wide — --stamp when low, --toner-3 otherwise. The clock fiction
            // is pinned at 02:47 (not live), and so is the battery: OsRail's own default prop is
            // batteryLow=true, so this always draws the low state, same as the clock always reads
            // the same time. That is a real, representable state now — the old "▰" glyph could not
            // distinguish low from full because it was never anything but a fixed character.
            const float batteryW = 20f;
            const float batteryH = 9f;
            const float clockBatteryGap = 13f;
            RectTransform battery = LaptopUi.MakePanel(rail, "Battery", new Vector2(1f, .5f),
                new Vector2(1f, .5f), new Vector2(-RailPadX, 0f), new Vector2(batteryW, batteryH),
                Color.clear);
            LaptopUi.MakePanel(battery, "BatteryBorderTop", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(batteryW, ruleW), LaptopOs.Muted);
            LaptopUi.MakePanel(battery, "BatteryBorderBottom", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(batteryW, ruleW), LaptopOs.Muted);
            LaptopUi.MakePanel(battery, "BatteryBorderLeft", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, ruleW), new Vector2(ruleW, batteryH - ruleW * 2f), LaptopOs.Muted);
            LaptopUi.MakePanel(battery, "BatteryBorderRight", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, ruleW), new Vector2(ruleW, batteryH - ruleW * 2f), LaptopOs.Muted);
            LaptopUi.MakePanel(battery, "BatteryFill", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(1.5f, 0f), new Vector2(5f, batteryH - 3f),
                BatteryPercent <= BatteryLowThreshold ? LaptopOs.MoneyBad : LaptopOs.Muted);

            LaptopUi.MakeText(rail, "Clock", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-(RailPadX + batteryW + clockBatteryGap), 0f), new Vector2(90f, 24f),
                ChromeText, TextAnchor.MiddleRight, LaptopOs.Muted, ClockText, font);
            // F1: OsRail.jsx's own border-bottom (--rule-w solid var(--rule)) — the rail was a flat
            // colour step into whatever the app draws next (FormTabs, or the ledger's own copy of
            // it), with no seam actually drawn.
            LaptopUi.MakeRule(rail, "RailRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(width, 1f), LaptopOs.Rule);
            return rail;
        }

        /// <param name="minimize">
        /// Invoked by the slot of the app that is already running. A tray slot for the running app
        /// cannot "launch" it, so it drops to the desktop instead — the same thing a real taskbar
        /// button does. This is why the running slot stays clickable rather than being disabled.
        /// </param>
        public static RectTransform BuildTray(RectTransform parent, float width, Font font,
            Running running, Action openSportsbook, Action openLedger, Action minimize)
        {
            RectTransform tray = LaptopUi.MakePanel(parent, "NotebookTray", new Vector2(0f, 0f),
                new Vector2(0f, 0f), Vector2.zero, new Vector2(width, TrayHeight),
                LaptopOs.SurfaceRaised);

            bool sportsbookRunning = running == Running.Sportsbook;
            bool ledgerRunning = running == Running.Ledger;
            // F8: --st-rail-pad-x (11px) on the left edge too — was 12px, the one inset in this
            // class that did not already match the pattern (rail's left/right corrected above).
            //
            // S48: each slot now asks about itself — am I the running app? — rather than the ledger
            // reading its state off the sportsbook's negation. Behaviour is identical for both
            // states that already existed; what changes is that a third can be expressed.
            MakeSlot(tray, "SureThing", "SURETHING", RailPadX, 110f, sportsbookRunning,
                sportsbookRunning ? minimize : openSportsbook, font);
            MakeSlot(tray, "Ledger", "LEDGER", 132f, 88f, ledgerRunning,
                ledgerRunning ? minimize : openLedger, font);

            // W3 (dot + badge, MESSAGES slot): OsTray.jsx renders every slot — including one with
            // no destination in this build — with its own dot, and MESSAGES carries the one
            // default badge. MESSAGES has no Running case and no onClick here, so it stays
            // presentation-only exactly as before; only its furniture changes, not its behaviour.
            // Badge and label are both sized from their own measured text (MeasureWidth, same
            // primitive the sticker above and F7/FitText elsewhere already use), so the layout is
            // a left-to-right flow built from real widths, not a fixed guess that could overlap.
            const float messagesX = 232f;
            const float dotSize = 5f;
            const float dotGap = 6f;
            LaptopUi.MakePanel(tray, "MessagesDot", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(messagesX, 0f), new Vector2(dotSize, dotSize), LaptopOs.Muted);
            float messagesLabelX = messagesX + dotSize + dotGap;
            LaptopUi.MakeText(tray, "Messages", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(messagesLabelX, 0f), new Vector2(140f, 24f), ChromeText,
                TextAnchor.MiddleLeft, LaptopOs.Muted, MessagesLabel, font);
            float messagesLabelW = LaptopUi.MeasureWidth(font, MessagesLabel, ChromeText);
            float badgeX = messagesLabelX + messagesLabelW + dotGap;
            const float badgePadX = 5f; // OsTray badge "0 5px" padding
            const float badgeH = 16f;
            float badgeTextW = LaptopUi.MeasureWidth(font, MessagesBadge, ChromeText);
            float badgeW = badgeTextW + badgePadX * 2f;
            RectTransform badge = LaptopUi.MakePanel(tray, "MessagesBadge", new Vector2(0f, .5f),
                new Vector2(0f, .5f), new Vector2(badgeX, 0f), new Vector2(badgeW, badgeH),
                LaptopOs.MoneyBad);
            LaptopUi.MakeText(badge, "MessagesBadgeLabel", new Vector2(.5f, .5f),
                new Vector2(.5f, .5f), Vector2.zero, new Vector2(badgeTextW + 2f, badgeH),
                ChromeText, TextAnchor.MiddleCenter, LaptopOs.White, MessagesBadge, font);

            LaptopUi.MakeText(tray, "SystemFacts", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-RailPadX, 0f), new Vector2(270f, 24f), ChromeText, TextAnchor.MiddleRight,
                LaptopOs.Muted, SystemFactsText, font);
            return tray;
        }

        /// The running app reads as pressed-in — ink ground, full-strength label. A backgrounded
        /// app reads as raised and muted. That is the only state difference, and it is carried by
        /// ground and weight rather than colour alone.
        ///
        /// W3: does not delegate to LaptopUi.MakeButton, because that helper always centres its
        /// label across the full button rect — there is nowhere in that layout to insert a dot
        /// without either overlapping the centred text or guessing at its rendered width. This
        /// duplicates MakeButton's few lines of Button/ColorBlock wiring verbatim and instead lays
        /// the dot and label out left-to-right (dot at the slot's own padding, label immediately
        /// after it), so the two are never overlapping by construction. The button's own rect —
        /// its hit target — is untouched: same width/height clamp MakeButton itself applies.
        private static void MakeSlot(RectTransform tray, string name, string label, float x,
            float width, bool running, Action onClick, Font font)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(tray, false);
            Image image = go.GetComponent<Image>();
            image.color = running ? LaptopOs.Ink : LaptopOs.SurfaceRaised;
            image.raycastTarget = true;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, .5f);
            rt.pivot = new Vector2(0f, .5f);
            rt.sizeDelta = new Vector2(Mathf.Max(44f, width), Mathf.Max(32f, 32f));
            rt.anchoredPosition = new Vector2(x, 0f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = true;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => onClick());

            // W3 (dot): OsTray.jsx's per-slot dot — 5x5, --wax when this slot is the running app,
            // --toner-3 otherwise.
            const float slotPadX = 10f; // OsTray's own slot padding, "0 10px"
            const float dotSize = 5f;
            const float dotLabelGap = 6f;
            LaptopUi.MakePanel(rt, "Dot", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(slotPadX, 0f), new Vector2(dotSize, dotSize),
                running ? LaptopOs.MoneyGold : LaptopOs.Muted);

            float labelX = slotPadX + dotSize + dotLabelGap;
            float labelW = Mathf.Max(0f, rt.sizeDelta.x - labelX - slotPadX);
            Text text = LaptopUi.MakeText(rt, "Label", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(labelX, 0f), new Vector2(labelW, 24f), ChromeText, TextAnchor.MiddleLeft,
                running ? LaptopOs.White : LaptopOs.Muted, label, font);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
    }
}

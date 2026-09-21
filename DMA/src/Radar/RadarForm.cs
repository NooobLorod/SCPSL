using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DmaBase.Misc;
using ScpslApp;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Vector3 = System.Numerics.Vector3;

namespace DmaBase.Radar
{
    public sealed class RadarForm : Form
    {
        private static RadarForm? _instance;
        public static RadarForm? Instance => _instance;

        // ── SCP-079 Color Palette (Option 2: High-Contrast OLED Stealth) ──
        private static readonly Color ColorBg = Color.FromArgb(6, 11, 20);
        private static readonly Color ColorToolbarBg = Color.FromArgb(10, 18, 30);
        private static readonly Color ColorRoomNormal = Color.FromArgb(35, 95, 175);
        private static readonly Color ColorRoomActive = Color.FromArgb(100, 220, 255);
        private static readonly Color ColorLocalPlayer = Color.FromArgb(255, 215, 60); // Bright golden amber wedge
        private static readonly Color ColorTextCyan = Color.FromArgb(240, 248, 255);

        // Cached pre-tinted room sprites (reconstructed 256x256 official game assets)
        private static readonly Dictionary<string, Bitmap> _normalSprites = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Bitmap> _activeSprites = new(StringComparer.OrdinalIgnoreCase);
        private static bool _spritesLoaded;

        private static void EnsureSpritesLoaded()
        {
            if (_spritesLoaded) return;
            _spritesLoaded = true;

            var asm = typeof(RadarForm).Assembly;
            var resourceNames = asm.GetManifestResourceNames();

            foreach (var rName in resourceNames)
            {
                if (!rName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                if (!rName.Contains("MapSprites")) continue;
                if (rName.Contains("cctv", StringComparison.OrdinalIgnoreCase)) continue; // Never load CCTV!

                try
                {
                    using var stream = asm.GetManifestResourceStream(rName);
                    if (stream == null) continue;

                    using var rawBmp = new Bitmap(stream);
                    string shortName = rName;
                    int mapIdx = shortName.IndexOf("MapSprites", StringComparison.OrdinalIgnoreCase);
                    if (mapIdx >= 0)
                    {
                        shortName = shortName.Substring(mapIdx + "MapSprites".Length).TrimStart('.');
                        if (shortName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            shortName = shortName.Substring(0, shortName.Length - 4);
                        }
                    }

                    _normalSprites[shortName] = CreateTintedBitmap(rawBmp, ColorRoomNormal);
                    _activeSprites[shortName] = CreateTintedBitmap(rawBmp, ColorRoomActive);
                }
                catch { }
            }

            // Fallback to local disk directory if not found in embedded resources
            if (_normalSprites.Count == 0)
            {
                string[] candidateDirs =
                [
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Radar", "MapSprites"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapSprites"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Radar", "MapSprites"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "DMA", "src", "Radar", "MapSprites")
                ];

                foreach (var dir in candidateDirs)
                {
                    if (Directory.Exists(dir))
                    {
                        foreach (var file in Directory.GetFiles(dir, "*.png"))
                        {
                            string fname = Path.GetFileNameWithoutExtension(file);
                            if (fname.Contains("cctv", StringComparison.OrdinalIgnoreCase)) continue;
                            try
                            {
                                using var rawBmp = new Bitmap(file);
                                _normalSprites[fname] = CreateTintedBitmap(rawBmp, ColorRoomNormal);
                                _activeSprites[fname] = CreateTintedBitmap(rawBmp, ColorRoomActive);
                            }
                            catch { }
                        }
                        if (_normalSprites.Count > 0) break;
                    }
                }
            }
        }

        private static Bitmap CreateTintedBitmap(Bitmap src, Color tint)
        {
            var bmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            var srcData = src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var dstData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte* pSrc = (byte*)srcData.Scan0;
                byte* pDst = (byte*)dstData.Scan0;
                int totalBytes = src.Width * src.Height * 4;

                for (int i = 0; i < totalBytes; i += 4)
                {
                    byte a = pSrc[i + 3];
                    if (a == 0)
                    {
                        pDst[i] = 0; pDst[i + 1] = 0; pDst[i + 2] = 0; pDst[i + 3] = 0;
                    }
                    else
                    {
                        float alphaF = a / 255.0f;
                        pDst[i]     = (byte)Math.Clamp((int)(tint.B * alphaF), 0, 255);
                        pDst[i + 1] = (byte)Math.Clamp((int)(tint.G * alphaF), 0, 255);
                        pDst[i + 2] = (byte)Math.Clamp((int)(tint.R * alphaF), 0, 255);
                        pDst[i + 3] = a;
                    }
                }
            }

            src.UnlockBits(srcData);
            bmp.UnlockBits(dstData);
            return bmp;
        }

        // Configuration & State
        private float _zoom = 1.0f;
        private bool _followPlayer = true;
        private PointF _panOffset = PointF.Empty;
        private bool _isDragging;
        private Point _dragStart;
        private PointF _dragPanStart;
        private int _zoneMode; // 0 = Auto, 1 = HCZ+EZ, 2 = LCZ, 3 = Surface

        // Display Toggles (Rooms & Players Only)
        private bool _showNames = true;
        private bool _showPlayers = true;
        private bool _showScps = true;
        private float _lastDirX = 0f;
        private float _lastDirY = -1f;

        // UI Controls
        private readonly Panel _toolbar;
        private readonly Button _btnFollow;
        private readonly ComboBox _cbZone;
        private readonly ComboBox _cbMonitor;
        private readonly Button _btnZoomIn;
        private readonly Button _btnZoomOut;
        private readonly Button _btnResetZoom;
        private readonly CheckBox _chkNames;
        private readonly CheckBox _chkPlayers;
        private readonly CheckBox _chkScps;
        private readonly Label _lblStatus;

        // Drawing Objects (reusable)
        private readonly Font _fontTitle = new("Segoe UI", 10.0f, FontStyle.Bold);
        private readonly Font _fontRoom = new("Consolas", 8.0f, FontStyle.Bold);
        private readonly Font _fontBadge = new("Consolas", 7.5f, FontStyle.Bold);
        private readonly Font _fontSmall = new("Segoe UI", 7.5f, FontStyle.Bold);
        private readonly Font _fontStatus = new("Consolas", 8.5f, FontStyle.Regular);

        private readonly StringFormat _sfCenter = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private readonly System.Windows.Forms.Timer _renderTimer;

        // Static Map Bitmap Cache (pre-renders static room layout to eliminate sub-pixel seam jumping)
        private Bitmap? _cachedMapBitmap;
        private bool _cachedMapDirty = true;
        private float _cachedMapScale = 0f;
        private FacilityZone _cachedActiveZone = FacilityZone.None;
        private bool _cachedIsCombinedHczEz = false;
        private int _cachedRoomCount = 0;
        private float _cachedMapMinX, _cachedMapMaxX, _cachedMapMinZ, _cachedMapMaxZ;
        private const float MapPaddingMeters = 15.0f;

        // Smooth camera focal point tracking
        private float _smoothFocalX = float.NaN;
        private float _smoothFocalZ = float.NaN;
        private DateTime _lastFrameTime = DateTime.UtcNow;

        // Cached room mapping for connections
        private readonly Dictionary<ulong, RoomInfo> _roomsByAddr = new();

        public RadarForm()
        {
            _instance = this;

            Text = "SCPSL Tactical Radar (SCP-079)";
            BackColor = ColorBg;
            ForeColor = Color.FromArgb(200, 225, 255);
            DoubleBuffered = true;
            KeyPreview = true;
            MinimumSize = new Size(640, 480);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            // Load saved config
            var cfg = GameReader.ScpslOverlay.GetConfig();
            _zoom = Math.Clamp(cfg.RadarZoom, 0.3f, 6.0f);
            _followPlayer = cfg.RadarFollowPlayer;
            _showNames = cfg.RadarShowRoomNames;
            _showPlayers = cfg.RadarShowPlayers;
            _showScps = cfg.RadarShowSCPs;
            _zoneMode = Math.Clamp(cfg.RadarZoneMode, 0, 3);

            // Restore Window Bounds
            ApplyInitialBounds(cfg);

            // ── Toolbar Header ──
            _toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(12, 20, 32),
                Padding = new Padding(6, 4, 6, 4)
            };

            int curX = 6;

            // Follow Me Toggle
            _btnFollow = new Button
            {
                Text = _followPlayer ? "Follow: ON" : "Follow: OFF",
                Location = new Point(curX, 4),
                Size = new Size(95, 27),
                FlatStyle = FlatStyle.Flat,
                BackColor = _followPlayer ? Color.FromArgb(20, 75, 45) : Color.FromArgb(32, 42, 56),
                ForeColor = Color.White,
                Font = _fontSmall
            };
            _btnFollow.FlatAppearance.BorderSize = 1;
            _btnFollow.FlatAppearance.BorderColor = _followPlayer ? Color.FromArgb(40, 180, 100) : Color.FromArgb(60, 75, 95);
            _btnFollow.Click += (_, _) =>
            {
                _followPlayer = !_followPlayer;
                _btnFollow.Text = _followPlayer ? "Follow: ON" : "Follow: OFF";
                _btnFollow.BackColor = _followPlayer ? Color.FromArgb(20, 75, 45) : Color.FromArgb(32, 42, 56);
                _btnFollow.FlatAppearance.BorderColor = _followPlayer ? Color.FromArgb(40, 180, 100) : Color.FromArgb(60, 75, 95);
                if (_followPlayer)
                {
                    _panOffset = PointF.Empty;
                    _smoothFocalX = float.NaN;
                    _smoothFocalZ = float.NaN;
                }
                Invalidate();
            };
            _toolbar.Controls.Add(_btnFollow);
            curX += 102;

            // Zone Selector
            var lblZone = new Label
            {
                Text = "Zone:",
                Location = new Point(curX, 8),
                AutoSize = true,
                Font = _fontSmall,
                ForeColor = Color.FromArgb(160, 190, 225)
            };
            _toolbar.Controls.Add(lblZone);
            curX += 40;

            _cbZone = new ComboBox
            {
                Location = new Point(curX, 5),
                Size = new Size(115, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(24, 34, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = _fontSmall
            };
            _cbZone.Items.AddRange(new object[] { "Auto (Follow)", "Heavy + Entrance", "Light Containment", "Surface Zone" });
            _cbZone.SelectedIndex = _zoneMode;
            _cbZone.SelectedIndexChanged += (_, _) =>
            {
                _zoneMode = _cbZone.SelectedIndex;
                _panOffset = PointF.Empty;
                _cachedMapDirty = true;
                _smoothFocalX = float.NaN;
                _smoothFocalZ = float.NaN;
                Invalidate();
            };
            _toolbar.Controls.Add(_cbZone);
            curX += 122;

            // Monitor Selector
            var lblMon = new Label
            {
                Text = "Mon:",
                Location = new Point(curX, 8),
                AutoSize = true,
                Font = _fontSmall,
                ForeColor = Color.FromArgb(160, 190, 225)
            };
            _toolbar.Controls.Add(lblMon);
            curX += 36;

            _cbMonitor = new ComboBox
            {
                Location = new Point(curX, 5),
                Size = new Size(100, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(24, 34, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = _fontSmall
            };
            PopulateMonitors();
            _cbMonitor.SelectedIndexChanged += (_, _) =>
            {
                if (_cbMonitor.SelectedIndex >= 0 && _cbMonitor.SelectedIndex < Screen.AllScreens.Length)
                {
                    MoveToMonitor(Screen.AllScreens[_cbMonitor.SelectedIndex]);
                }
            };
            _toolbar.Controls.Add(_cbMonitor);
            curX += 108;

            // Zoom Controls
            _btnZoomOut = CreateToolButton("-", curX, 26, () => AdjustZoom(0.85f));
            curX += 30;
            _btnZoomIn = CreateToolButton("+", curX, 26, () => AdjustZoom(1.18f));
            curX += 30;
            _btnResetZoom = CreateToolButton("1:1", curX, 36, () =>
            {
                _zoom = 1.0f;
                _panOffset = PointF.Empty;
                _cachedMapDirty = true;
                _smoothFocalX = float.NaN;
                _smoothFocalZ = float.NaN;
                Invalidate();
            });
            curX += 44;

            // Filter Checkboxes (Rooms & Players Only)
            _chkNames = CreateFilterCheck("Names", ref curX, _showNames, v => _showNames = v);
            _chkPlayers = CreateFilterCheck("Players", ref curX, _showPlayers, v => _showPlayers = v);
            _chkScps = CreateFilterCheck("SCPs", ref curX, _showScps, v => _showScps = v);

            Controls.Add(_toolbar);

            // ── Status Bar ──
            _lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                BackColor = Color.FromArgb(8, 14, 24),
                ForeColor = Color.FromArgb(120, 160, 205),
                Font = _fontStatus,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Text = "Initializing Tactical Radar..."
            };
            Controls.Add(_lblStatus);

            // Mouse Events for Pan & Zoom
            MouseDown += OnCanvasMouseDown;
            MouseMove += OnCanvasMouseMove;
            MouseUp += OnCanvasMouseUp;
            MouseWheel += OnCanvasMouseWheel;
            MouseDoubleClick += (_, _) =>
            {
                _followPlayer = true;
                _btnFollow.Text = "Follow: ON";
                _btnFollow.BackColor = Color.FromArgb(20, 75, 45);
                _btnFollow.FlatAppearance.BorderColor = Color.FromArgb(40, 180, 100);
                _panOffset = PointF.Empty;
                _smoothFocalX = float.NaN;
                _smoothFocalZ = float.NaN;
                Invalidate();
            };

            // Window move/resize persistence
            Move += (_, _) => SaveWindowBounds();
            Resize += (_, _) => SaveWindowBounds();
            FormClosing += (_, e) => SaveWindowBounds();

            // 60 FPS Render Timer
            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += (_, _) => Invalidate();
            _renderTimer.Start();
        }

        private Button CreateToolButton(string text, int x, int width, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 4),
                Size = new Size(width, 27),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 36, 50),
                ForeColor = Color.White,
                Font = _fontBadge
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(50, 70, 95);
            btn.Click += (_, _) => onClick();
            _toolbar.Controls.Add(btn);
            return btn;
        }

        private CheckBox CreateFilterCheck(string text, ref int curX, bool initial, Action<bool> onChange)
        {
            var chk = new CheckBox
            {
                Text = text,
                Location = new Point(curX, 8),
                AutoSize = true,
                Checked = initial,
                Font = _fontSmall,
                ForeColor = Color.FromArgb(180, 210, 245)
            };
            chk.CheckedChanged += (_, _) =>
            {
                onChange(chk.Checked);
                Invalidate();
            };
            _toolbar.Controls.Add(chk);
            curX += chk.PreferredSize.Width + 8;
            return chk;
        }

        private void AdjustZoom(float factor)
        {
            _zoom = Math.Clamp(_zoom * factor, 0.3f, 6.0f);
            _cachedMapDirty = true;
            Invalidate();
        }

        private void PopulateMonitors()
        {
            _cbMonitor.Items.Clear();
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                _cbMonitor.Items.Add($"Mon {i + 1}{(screens[i].Primary ? " *" : "")}");
                if (screens[i].Bounds.Contains(Location))
                {
                    _cbMonitor.SelectedIndex = i;
                }
            }
            if (_cbMonitor.SelectedIndex < 0 && _cbMonitor.Items.Count > 0)
                _cbMonitor.SelectedIndex = 0;
        }

        private void MoveToMonitor(Screen screen)
        {
            var bounds = screen.WorkingArea;
            int newW = Math.Min(Width, bounds.Width - 40);
            int newH = Math.Min(Height, bounds.Height - 40);
            int newX = bounds.Left + (bounds.Width - newW) / 2;
            int newY = bounds.Top + (bounds.Height - newH) / 2;

            StartPosition = FormStartPosition.Manual;
            Location = new Point(newX, newY);
            Size = new Size(newW, newH);
            SaveWindowBounds();
            Invalidate();
        }

        private void ApplyInitialBounds(ConfigData cfg)
        {
            var targetRect = new Rectangle(cfg.RadarWindowX, cfg.RadarWindowY, cfg.RadarWindowWidth, cfg.RadarWindowHeight);
            bool intersectsAny = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.IntersectsWith(targetRect))
                {
                    intersectsAny = true;
                    break;
                }
            }

            if (intersectsAny && cfg.RadarWindowWidth >= 400 && cfg.RadarWindowHeight >= 300)
            {
                StartPosition = FormStartPosition.Manual;
                Location = targetRect.Location;
                Size = targetRect.Size;
            }
            else
            {
                var prim = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
                StartPosition = FormStartPosition.Manual;
                Location = new Point(prim.Left + (prim.Width - 1000) / 2, prim.Top + (prim.Height - 850) / 2);
                Size = new Size(1000, 850);
            }

            if (cfg.RadarWindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void SaveWindowBounds()
        {
            if (WindowState == FormWindowState.Minimized) return;

            var cfg = GameReader.ScpslOverlay.GetConfig();
            cfg.RadarWindowMaximized = (WindowState == FormWindowState.Maximized);
            if (WindowState == FormWindowState.Normal)
            {
                cfg.RadarWindowX = Location.X;
                cfg.RadarWindowY = Location.Y;
                cfg.RadarWindowWidth = Size.Width;
                cfg.RadarWindowHeight = Size.Height;
            }
            cfg.RadarZoom = _zoom;
            cfg.RadarFollowPlayer = _followPlayer;
            cfg.RadarShowRoomNames = _showNames;
            cfg.RadarShowPlayers = _showPlayers;
            cfg.RadarShowSCPs = _showScps;
            cfg.RadarZoneMode = _zoneMode;
        }

        // ── Mouse Drag / Pan / Zoom ──
        private void OnCanvasMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Y < _toolbar.Bottom || e.Y > ClientSize.Height - _lblStatus.Height) return;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                _isDragging = true;
                _dragStart = e.Location;
                _dragPanStart = _panOffset;
            }
        }

        private void OnCanvasMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            // Direct 1:1 pixel drag: moving right shifts screen right; moving down shifts screen down
            float dx = e.X - _dragStart.X;
            float dy = e.Y - _dragStart.Y;

            _panOffset = new PointF(_dragPanStart.X + dx, _dragPanStart.Y + dy);
            if (_followPlayer && (Math.Abs(dx) > 10 || Math.Abs(dy) > 10))
            {
                _followPlayer = false;
                _btnFollow.Text = "Follow: OFF";
                _btnFollow.BackColor = Color.FromArgb(32, 42, 56);
                _btnFollow.FlatAppearance.BorderColor = Color.FromArgb(60, 75, 95);
            }
            Invalidate();
        }

        private void OnCanvasMouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void OnCanvasMouseWheel(object? sender, MouseEventArgs e)
        {
            float factor = e.Delta > 0 ? 1.15f : 0.87f;
            AdjustZoom(factor);
        }

        // ═════════════════════════════════════════════════════════════════════════
        //  PAINTING ENGINE (Double-Buffered GDI+ Tactical Radar - SCP-079 Official)
        // ═════════════════════════════════════════════════════════════════════════
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            int canvasTop = _toolbar.Bottom;
            int canvasBottom = ClientSize.Height - _lblStatus.Height;
            int canvasW = ClientSize.Width;
            int canvasH = canvasBottom - canvasTop;
            if (canvasW <= 0 || canvasH <= 0) return;

            // Clip drawing to canvas area
            g.SetClip(new Rectangle(0, canvasTop, canvasW, canvasH));

            // Background Fill (Deep dark terminal navy)
            using (var bgBrush = new SolidBrush(ColorBg))
            {
                g.FillRectangle(bgBrush, 0, canvasTop, canvasW, canvasH);
            }

            var snap = GameReader.ScpslOverlay.CurrentSnapshot;
            var rooms = snap.Rooms;
            var players = snap.Players;
            var cam = GameReader.ScpslOverlay.GetLatestCamera();

            // Find Local Player
            PlayerInfo? localPlayer = null;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].IsLocal)
                {
                    localPlayer = players[i];
                    break;
                }
            }

            // Determine Active Zone
            FacilityZone activeZone = FacilityZone.HeavyContainment;
            bool isCombinedHczEz = false;

            if (_zoneMode == 1)
            {
                isCombinedHczEz = true;
                activeZone = FacilityZone.HeavyContainment;
            }
            else if (_zoneMode == 2)
            {
                activeZone = FacilityZone.LightContainment;
            }
            else if (_zoneMode == 3)
            {
                activeZone = FacilityZone.Surface;
            }
            else
            {
                // Auto Mode: follow player position / elevation
                if (localPlayer != null && localPlayer.HasPosition && localPlayer.Alive)
                {
                    float y = localPlayer.Position.Y;
                    if (y > 200f)
                    {
                        activeZone = FacilityZone.Surface;
                    }
                    else if (y > 40f && y < 160f)
                    {
                        activeZone = FacilityZone.LightContainment;
                    }
                    else
                    {
                        isCombinedHczEz = true;
                        activeZone = FacilityZone.HeavyContainment;
                    }
                }
                else
                {
                    if (cam.Valid && cam.Position.Y > 200f)
                    {
                        activeZone = FacilityZone.Surface;
                    }
                    else if (cam.Valid && cam.Position.Y > 40f && cam.Position.Y < 160f)
                    {
                        activeZone = FacilityZone.LightContainment;
                    }
                    else
                    {
                        isCombinedHczEz = true;
                        activeZone = FacilityZone.HeavyContainment;
                    }
                }
            }

            // Filter Rooms for Active View
            List<RoomInfo> activeRooms;
            if (isCombinedHczEz)
            {
                activeRooms = rooms.Where(r => r.Zone == FacilityZone.HeavyContainment || r.Zone == FacilityZone.Entrance).ToList();
            }
            else
            {
                activeRooms = rooms.Where(r => r.Zone == activeZone).ToList();
            }

            // Update rooms dictionary for fast connection lookups
            _roomsByAddr.Clear();
            for (int i = 0; i < rooms.Count; i++)
            {
                _roomsByAddr[rooms[i].Address] = rooms[i];
            }

            // Focal Point & Coordinate Scaling
            float targetFocalX, targetFocalZ;
            float canvasCenterX = canvasW * 0.5f;
            float canvasCenterY = canvasTop + canvasH * 0.5f;

            if (_followPlayer && localPlayer != null && localPlayer.HasPosition && localPlayer.Alive)
            {
                targetFocalX = localPlayer.Position.X;
                targetFocalZ = localPlayer.Position.Z;
            }
            else if (_followPlayer && cam.Valid)
            {
                targetFocalX = cam.Position.X;
                targetFocalZ = cam.Position.Z;
            }
            else
            {
                if (activeRooms.Count > 0)
                {
                    targetFocalX = activeRooms.Average(r => r.Position.X);
                    targetFocalZ = activeRooms.Average(r => r.Position.Z);
                }
                else
                {
                    targetFocalX = 80f; targetFocalZ = 80f;
                }
            }

            // Exponential smoothing for focal point to absorb micro-stutters from DMA timing
            if (float.IsNaN(_smoothFocalX) || float.IsNaN(_smoothFocalZ))
            {
                _smoothFocalX = targetFocalX;
                _smoothFocalZ = targetFocalZ;
                _lastFrameTime = DateTime.UtcNow;
            }
            else
            {
                var now = DateTime.UtcNow;
                float dt = (float)(now - _lastFrameTime).TotalSeconds;
                _lastFrameTime = now;
                dt = Math.Clamp(dt, 0.001f, 0.1f);
                float blend = MathF.Min(1.0f, dt * 20.0f); // Responsive 20Hz exponential smoothing
                _smoothFocalX += (targetFocalX - _smoothFocalX) * blend;
                _smoothFocalZ += (targetFocalZ - _smoothFocalZ) * blend;
            }

            float focalX = _followPlayer ? _smoothFocalX : targetFocalX;
            float focalZ = _followPlayer ? _smoothFocalZ : targetFocalZ;

            // Base Scale: ~4.5 pixels per meter at 1.0x zoom
            float scale = 4.5f * _zoom;

            // ═════════════════════════════════════════════════════════════════
            //  COORDINATE PROJECTION (OFFICIAL SCP-079 TERMINAL ORIENTATION)
            // ═════════════════════════════════════════════════════════════════
            // In the official SCP-079 terminal map (HczMap / ProceduralZoneMap):
            // - Heavy Containment is situated at the BOTTOM (+screenY)
            // - Entrance Zone extends upward toward the TOP (-screenY)
            // - World +X (Entrance) maps to -screenY (UP)
            // - World -X (Heavy) maps to +screenY (DOWN)
            // - World +Z (Warhead / Gate B) maps to -screenX (LEFT)
            // - World -Z (939 / Gate A) maps to +screenX (RIGHT)
            PointF WorldToScreen(float wx, float wz)
            {
                float screenX = canvasCenterX - (wz - focalZ) * scale + _panOffset.X;
                float screenY = canvasCenterY - (wx - focalX) * scale + _panOffset.Y;
                return new PointF(screenX, screenY);
            }

            // ── 1. Draw Official Room Sprites (Pre-rendered static map canvas + cyan active room overlay) ──
            DrawRoomTiles(g, activeRooms, localPlayer, cam, WorldToScreen, scale, activeZone, isCombinedHczEz);

            // ── 2. Draw Players & SCPs (Directional Wedges, Golden Amber for Local Player) ──
            if (_showPlayers && players != null && players.Count > 0)
            {
                DrawPlayers(g, players, activeZone, isCombinedHczEz, localPlayer, cam, WorldToScreen, scale);
            }

            // Update Status Bar
            string zoneName = isCombinedHczEz ? "Heavy Containment & Entrance" : (activeZone == FacilityZone.LightContainment ? "Light Containment" : "Surface Zone");
            int aliveCount = players.Count(p => p.Alive);
            string playerCoordStr = localPlayer != null && localPlayer.HasPosition ? $"Pos: {localPlayer.Position.X:0.0}, {localPlayer.Position.Z:0.0}" : (cam.Valid ? $"Cam: {cam.Position.X:0.0}, {cam.Position.Z:0.0}" : "Waiting for player...");
            _lblStatus.Text = $" Zone: {zoneName}  |  Rooms: {activeRooms.Count}  |  Alive: {aliveCount}/{players.Count}  |  Zoom: {_zoom:0.0}x  |  {playerCoordStr}  |  (Double-Click: Center)";
        }

        private void EnsureMapCache(List<RoomInfo> activeRooms, float scale, FacilityZone activeZone, bool isCombinedHczEz)
        {
            EnsureSpritesLoaded();

            if (!_cachedMapDirty &&
                _cachedMapBitmap != null &&
                Math.Abs(_cachedMapScale - scale) < 0.001f &&
                _cachedActiveZone == activeZone &&
                _cachedIsCombinedHczEz == isCombinedHczEz &&
                _cachedRoomCount == activeRooms.Count)
            {
                return;
            }

            _cachedMapBitmap?.Dispose();
            _cachedMapBitmap = null;

            if (activeRooms.Count == 0) return;

            _cachedMapMinX = activeRooms.Min(r => r.Position.X) - MapPaddingMeters;
            _cachedMapMaxX = activeRooms.Max(r => r.Position.X) + MapPaddingMeters;
            _cachedMapMinZ = activeRooms.Min(r => r.Position.Z) - MapPaddingMeters;
            _cachedMapMaxZ = activeRooms.Max(r => r.Position.Z) + MapPaddingMeters;

            float mapWidthMeters = _cachedMapMaxZ - _cachedMapMinZ;
            float mapHeightMeters = _cachedMapMaxX - _cachedMapMinX;

            int bmpW = Math.Clamp((int)MathF.Ceiling(mapWidthMeters * scale), 10, 8192);
            int bmpH = Math.Clamp((int)MathF.Ceiling(mapHeightMeters * scale), 10, 8192);

            var bmp = new Bitmap(bmpW, bmpH, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var mg = Graphics.FromImage(bmp))
            {
                mg.SmoothingMode = SmoothingMode.AntiAlias;
                mg.InterpolationMode = InterpolationMode.HighQualityBilinear;
                mg.PixelOffsetMode = PixelOffsetMode.HighQuality;
                mg.Clear(Color.Transparent);

                float roomTileSize = 15.0f * scale + 0.5f;
                float halfTileSize = roomTileSize * 0.5f;

                for (int i = 0; i < activeRooms.Count; i++)
                {
                    var r = activeRooms[i];
                    string spriteKey = GetSpriteKey(r);
                    if (!_normalSprites.TryGetValue(spriteKey, out var spriteBmp) || spriteBmp == null)
                        continue;

                    float rotDeg = GetRoomRotationDegrees(r, _roomsByAddr);

                    // Position within the map bitmap:
                    // X on bitmap: corresponds to world Z (increasing Z is left on screen, so (_cachedMapMaxZ - r.Position.Z) * scale)
                    // Y on bitmap: corresponds to world X (increasing X is up on screen, so (_cachedMapMaxX - r.Position.X) * scale)
                    float bx = (_cachedMapMaxZ - r.Position.Z) * scale;
                    float by = (_cachedMapMaxX - r.Position.X) * scale;

                    var state = mg.Save();
                    mg.TranslateTransform(bx, by);
                    if (rotDeg != 0)
                    {
                        mg.RotateTransform(rotDeg);
                    }
                    mg.DrawImage(spriteBmp, -halfTileSize, -halfTileSize, roomTileSize, roomTileSize);
                    mg.Restore(state);
                }
            }

            _cachedMapBitmap = bmp;
            _cachedMapScale = scale;
            _cachedActiveZone = activeZone;
            _cachedIsCombinedHczEz = isCombinedHczEz;
            _cachedRoomCount = activeRooms.Count;
            _cachedMapDirty = false;
        }

        // ── 2. Official Game Room Sprites (Pre-rendered static map canvas + cyan active room overlay) ──
        private void DrawRoomTiles(Graphics g, List<RoomInfo> activeRooms, PlayerInfo? localPlayer, CameraInfo cam, Func<float, float, PointF> toScreen, float scale, FacilityZone activeZone, bool isCombinedHczEz)
        {
            if (activeRooms.Count == 0) return;

            EnsureMapCache(activeRooms, scale, activeZone, isCombinedHczEz);

            // 1. Draw static pre-rendered map canvas (1 single draw call, zero seam flicker)
            if (_cachedMapBitmap != null)
            {
                var originScreen = toScreen(_cachedMapMaxX, _cachedMapMaxZ);
                g.DrawImage(_cachedMapBitmap, originScreen.X, originScreen.Y);
            }

            // Identify active room where local player is located (bright cyan highlight)
            RoomInfo? activePlayerRoom = null;
            if (localPlayer != null && localPlayer.HasPosition && localPlayer.Alive)
            {
                float closestDistSq = float.MaxValue;
                for (int i = 0; i < activeRooms.Count; i++)
                {
                    float dSq = Vector3.DistanceSquared(localPlayer.Position, activeRooms[i].Position);
                    if (dSq < closestDistSq && dSq < 225f) // within 15m grid radius
                    {
                        closestDistSq = dSq;
                        activePlayerRoom = activeRooms[i];
                    }
                }
            }

            float roomTileSize = 15.0f * scale + 0.5f;
            float halfTileSize = roomTileSize * 0.5f;

            // 2. Overlay active room in bright cyan highlight
            if (activePlayerRoom != null)
            {
                string activeKey = GetSpriteKey(activePlayerRoom);
                if (_activeSprites.TryGetValue(activeKey, out var activeBmp) && activeBmp != null)
                {
                    var pt = toScreen(activePlayerRoom.Position.X, activePlayerRoom.Position.Z);
                    float rotDeg = GetRoomRotationDegrees(activePlayerRoom, _roomsByAddr);

                    var state = g.Save();
                    g.TranslateTransform(pt.X, pt.Y);
                    if (rotDeg != 0)
                    {
                        g.RotateTransform(rotDeg);
                    }
                    g.DrawImage(activeBmp, -halfTileSize, -halfTileSize, roomTileSize, roomTileSize);
                    g.Restore(state);
                }
            }

            // 3. Draw Crisp Monospace Room Labels
            if (_showNames)
            {
                for (int i = 0; i < activeRooms.Count; i++)
                {
                    var r = activeRooms[i];
                    string label = GetShortRoomName(r.Name);
                    if (string.IsNullOrEmpty(label)) continue;

                    var pt = toScreen(r.Position.X, r.Position.Z);
                    bool isActive = (r == activePlayerRoom);

                    // Text Drop Shadow (Dark outline)
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                    {
                        g.DrawString(label, _fontRoom, shadowBrush, pt.X + 1, pt.Y + 1, _sfCenter);
                    }
                    // Main Text
                    Color textCol = isActive ? Color.FromArgb(100, 220, 255) : ColorTextCyan;
                    using (var textBrush = new SolidBrush(textCol))
                    {
                        g.DrawString(label, _fontRoom, textBrush, pt.X, pt.Y, _sfCenter);
                    }
                }
            }
        }

        // ── Room Sprite Key Resolution (Authentic Game Mapping, strictly NO CCTV) ──
        private static string GetSpriteKey(RoomInfo room)
        {
            // Heavy Containment
            if (room.Zone == FacilityZone.HeavyContainment)
            {
                switch (room.Name)
                {
                    case RoomName.Hcz049: return "HCZ_049";
                    case RoomName.Hcz127: return "HCZ_ThreeWay";
                    case RoomName.Hcz939: return "HCZ_939";
                    case RoomName.HczArmory: return "HCZ_Armory";
                    case RoomName.HczMicroHID: return "HCZ_Hid";
                    case RoomName.HczWarhead: return "HCZ_Nuke";
                    case RoomName.HczServers: return "HCZ_ServerRoom";
                    case RoomName.HczTestroom: return "HCZ_Testroom";
                    case RoomName.HczTesla: return "HCZ_Tesla";
                    case RoomName.Hcz096: return "HCZ_EndSmall";
                    case RoomName.Hcz106: return "HCZ_EndSmall";
                    case RoomName.HczCheckpointA:
                    case RoomName.HczCheckpointB:
                        return "HCZ_Elevators";
                    case RoomName.HczCheckpointToEntranceZone:
                        return "HCZ_Straight";
                    case RoomName.HczWaysideIncinerator: return "HCZ_Wayside";
                    case RoomName.HczRampTunnel: return "HCZ_ThreeWay";
                    case RoomName.HczAcroamaticAbatement: return "HCZ_CrossingWater";
                    case RoomName.Hcz079: return "HCZ_EndLarge";
                }

                return room.Shape switch
                {
                    RoomShape.Straight => "HCZ_Straight",
                    RoomShape.Curve => "HCZ_Curve",
                    RoomShape.TShape => "HCZ_ThreeWay",
                    RoomShape.XShape => "HCZ_Crossing",
                    RoomShape.Endroom => "HCZ_EndSmall",
                    _ => "HCZ_Straight"
                };
            }

            // Light Containment
            if (room.Zone == FacilityZone.LightContainment)
            {
                switch (room.Name)
                {
                    case RoomName.Lcz330: return "LCZ_330";
                    case RoomName.Lcz914: return "LCZ_914";
                    case RoomName.LczAirlock: return "LCZ_Airlock";
                    case RoomName.LczArmory: return "LCZ_Armory";
                    case RoomName.LczCheckpointA:
                    case RoomName.LczCheckpointB:
                        return "LCZ_Checkpoint";
                    case RoomName.LczClassDSpawn: return "LCZ_LargeEnd";
                    case RoomName.Lcz173: return "LCZ_LargeEnd";
                    case RoomName.LczComputerRoom: return "LCZ_PC15";
                    case RoomName.LczToilets: return "LCZ_Toilets";
                    case RoomName.LczGlassroom: return "LCZ_Straight";
                    case RoomName.LczGreenhouse: return "LCZ_Straight";
                }

                return room.Shape switch
                {
                    RoomShape.Straight => "LCZ_Straight",
                    RoomShape.Curve => "LCZ_Curve",
                    RoomShape.TShape => "LCZ_ThreeWay",
                    RoomShape.XShape => "LCZ_Crossing",
                    RoomShape.Endroom => "LCZ_LargeEnd",
                    _ => "LCZ_Straight"
                };
            }

            // Entrance Zone
            if (room.Zone == FacilityZone.Entrance)
            {
                switch (room.Name)
                {
                    case RoomName.HczCheckpointToEntranceZone: return "EZ_Straight";
                    case RoomName.EzGateA: return "EZ_GateA";
                    case RoomName.EzGateB: return "EZ_GateB";
                    case RoomName.EzIntercom: return "EZ_Intercom";
                    case RoomName.EzCollapsedTunnel: return "EZ_Collapsed";
                    case RoomName.EzRedroom: return "EZ_NonGateEnd";
                    case RoomName.EzEvacShelter: return "EZ_NonGateEnd";
                    case RoomName.EzOfficeLarge:
                    case RoomName.EzOfficeSmall:
                    case RoomName.EzOfficeStoried:
                        return room.Shape switch
                        {
                            RoomShape.Straight => "EZ_Straight",
                            RoomShape.Curve => "EZ_Curve",
                            RoomShape.TShape => "EZ_ThreeWay",
                            RoomShape.XShape => "EZ_Crossing",
                            _ => "EZ_Straight"
                        };
                }

                return room.Shape switch
                {
                    RoomShape.Straight => "EZ_Straight",
                    RoomShape.Curve => "EZ_Curve",
                    RoomShape.TShape => "EZ_ThreeWay",
                    RoomShape.XShape => "EZ_Crossing",
                    RoomShape.Endroom => "EZ_NonGateEnd",
                    _ => "EZ_Straight"
                };
            }

            // Fallback
            return room.Shape switch
            {
                RoomShape.Straight => "HCZ_Straight",
                RoomShape.Curve => "HCZ_Curve",
                RoomShape.TShape => "HCZ_ThreeWay",
                RoomShape.XShape => "HCZ_Crossing",
                _ => "HCZ_Straight"
            };
        }

        // ── Room Rotation Calculation (Aligned with 15m integer grid topology and native yaw) ──
        private static float GetRoomRotationDegrees(RoomInfo r, Dictionary<ulong, RoomInfo> roomsByAddr)
        {
            // In our screen projection:
            // Screen TOP (-screenY) is +X world (towards Entrance)
            // Screen BOTTOM (+screenY) is -X world (towards Heavy)
            // Screen LEFT (-screenX) is +Z world (towards Warhead / Gate B)
            // Screen RIGHT (+screenX) is -Z world (towards 939 / Gate A)
            bool hasTop = false, hasBottom = false, hasLeft = false, hasRight = false;

            if (r.ConnectedRoomAddrs != null && r.ConnectedRoomAddrs.Count > 0)
            {
                for (int i = 0; i < r.ConnectedRoomAddrs.Count; i++)
                {
                    if (roomsByAddr.TryGetValue(r.ConnectedRoomAddrs[i], out var neighbor))
                    {
                        int dx = neighbor.MainCoords.X - r.MainCoords.X;
                        int dz = neighbor.MainCoords.Z - r.MainCoords.Z;

                        // Only consider immediate adjacent orthogonal neighbors on the 15m grid
                        if (Math.Abs(dx) + Math.Abs(dz) == 1)
                        {
                            if (dx == 1) hasTop = true;
                            else if (dx == -1) hasBottom = true;
                            else if (dz == 1) hasLeft = true;
                            else if (dz == -1) hasRight = true;
                        }
                    }
                }
            }

            // Fallback to geometric grid proximity if ConnectedRoomAddrs was incomplete
            if (!hasTop && !hasBottom && !hasLeft && !hasRight)
            {
                foreach (var neighbor in roomsByAddr.Values)
                {
                    if (neighbor.Address == r.Address) continue;
                    if (neighbor.Zone != r.Zone && !(r.Zone == FacilityZone.HeavyContainment && neighbor.Zone == FacilityZone.Entrance) && !(r.Zone == FacilityZone.Entrance && neighbor.Zone == FacilityZone.HeavyContainment))
                        continue;

                    int dx = neighbor.MainCoords.X - r.MainCoords.X;
                    int dz = neighbor.MainCoords.Z - r.MainCoords.Z;
                    if (Math.Abs(dx) + Math.Abs(dz) == 1)
                    {
                        if (dx == 1) hasTop = true;
                        else if (dx == -1) hasBottom = true;
                        else if (dz == 1) hasLeft = true;
                        else if (dz == -1) hasRight = true;
                    }
                }
            }

            string key = GetSpriteKey(r);

            // 1. Straight Corridors
            // EZ_Straight / EZ_Checkpoint / HCZ_ServerRoom natural orientation at 0° is Vertical: {TOP, BOTTOM}
            if (key == "EZ_Straight" || key == "EZ_Checkpoint" || key == "EZ_Thicc" || key == "HCZ_ServerRoom")
            {
                if (hasLeft || hasRight) return 90f;
                return 0f;
            }

            // All other straight corridors (HCZ_Straight, LCZ_Straight, HCZ_Tesla, HCZ_Testroom, HCZ_049, etc.)
            // natural orientation at 0° is Horizontal: {LEFT, RIGHT}
            if (r.Shape == RoomShape.Straight ||
                key.EndsWith("Straight") ||
                key == "HCZ_MicroHID" || key == "HCZ_Hid" ||
                key == "HCZ_049" ||
                key == "HCZ_Testroom" ||
                key == "HCZ_Tesla" ||
                key == "LCZ_Airlock" ||
                key == "LCZ_Toilets")
            {
                if (hasTop || hasBottom) return 90f;
                return 0f;
            }

            // 2. Curves / Corners
            if (key == "EZ_Curve" || key == "EZ_Intercom")
            {
                // EZ_Curve / EZ_Intercom natural at 0°: {BOTTOM, LEFT}
                if (hasBottom && hasLeft) return 0f;
                if (hasTop && hasLeft) return 90f;
                if (hasTop && hasRight) return 180f;
                if (hasBottom && hasRight) return 270f;
                if (hasBottom) return 0f;
                if (hasLeft) return 0f;
                return 0f;
            }

            if (r.Shape == RoomShape.Curve || key.EndsWith("Curve") || key == "HCZ_939")
            {
                // HCZ_Curve / LCZ_Curve natural at 0°: {BOTTOM, RIGHT}
                if (hasBottom && hasRight) return 0f;
                if (hasBottom && hasLeft) return 90f;
                if (hasTop && hasLeft) return 180f;
                if (hasTop && hasRight) return 270f;
                if (hasBottom) return 0f;
                if (hasRight) return 0f;
                return 0f;
            }

            // 3. Three-Way Junctions (T-Shapes)
            if (key == "EZ_ThreeWay")
            {
                // EZ_ThreeWay natural at 0°: {TOP, LEFT, RIGHT}, missing BOTTOM
                if (!hasBottom && (hasLeft || hasRight || hasTop)) return 0f;
                if (!hasLeft && (hasTop || hasBottom || hasRight)) return 90f;
                if (!hasTop && (hasLeft || hasRight || hasBottom)) return 180f;
                if (!hasRight && (hasTop || hasBottom || hasLeft)) return 270f;
                return 0f;
            }

            if (r.Shape == RoomShape.TShape || key.EndsWith("ThreeWay") || key == "HCZ_Warhead" || key == "HCZ_Nuke" || key == "HCZ_Armory")
            {
                // HCZ_ThreeWay / LCZ_ThreeWay natural at 0°: {TOP, BOTTOM, LEFT}, missing RIGHT
                if (!hasRight && (hasLeft || hasTop || hasBottom)) return 0f;
                if (!hasBottom && (hasTop || hasLeft || hasRight)) return 90f;
                if (!hasLeft && (hasRight || hasTop || hasBottom)) return 180f;
                if (!hasTop && (hasBottom || hasLeft || hasRight)) return 270f;
                return 0f;
            }

            // 4. Four-Way Crossings (Rotationally symmetrical)
            if (r.Shape == RoomShape.XShape || key.EndsWith("Crossing") || key.EndsWith("CrossingWater"))
            {
                return 0f;
            }

            // 5. Special Gate Endrooms
            // EZ_GateA, EZ_GateB, EZ_Collapsed, EZ_NonGateEnd natural at 0°: door is at TOP
            if (key == "EZ_GateA" || key == "EZ_GateB" || key == "EZ_Collapsed" || key == "EZ_NonGateEnd")
            {
                if (hasTop) return 0f;
                if (hasRight) return 90f;
                if (hasBottom) return 180f;
                if (hasLeft) return 270f;
                return 0f;
            }

            // 6. Standard Endrooms
            // HCZ_EndSmall, LCZ_LargeEnd, LCZ_330, LCZ_914, LCZ_Armory, LCZ_PC15, HCZ_Elevators natural at 0°: door is at LEFT
            if (r.Shape == RoomShape.Endroom || key == "HCZ_EndSmall" || key.EndsWith("LargeEnd") || key == "HCZ_Elevators" || key == "LCZ_330" || key == "LCZ_914" || key == "LCZ_Armory" || key == "LCZ_PC15")
            {
                if (hasLeft) return 0f;
                if (hasTop) return 90f;
                if (hasRight) return 180f;
                if (hasBottom) return 270f;
                return 0f;
            }

            // Fallback to native yaw if available
            if (r.RotationYaw != 0)
            {
                int snapped = ((int)MathF.Round(r.RotationYaw / 90f) * 90) % 360;
                if (snapped < 0) snapped += 360;
                return snapped;
            }

            return 0f;
        }

        // ── 3. Players & SCPs (Directional Chevrons, NO CONE, NO "YOU" TEXT) ──
        private void DrawPlayers(Graphics g, List<PlayerInfo> players, FacilityZone activeZone, bool isCombinedHczEz, PlayerInfo? localPlayer, CameraInfo cam, Func<float, float, PointF> toScreen, float scale)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (!p.Alive || !p.HasPosition || p.Position == Vector3.Zero) continue;

                // Zone Filter
                if (isCombinedHczEz)
                {
                    if (p.Position.Y > 0f || p.Position.Y < -200f) continue;
                }
                else if (activeZone == FacilityZone.LightContainment)
                {
                    if (p.Position.Y < 30f || p.Position.Y > 180f) continue;
                }
                else if (activeZone == FacilityZone.Surface)
                {
                    if (p.Position.Y < 200f) continue;
                }

                var pt = toScreen(p.Position.X, p.Position.Z);

                // Local Player Rendering (Clean directional chevron, NO CONE, NO "YOU" text)
                if (p.IsLocal)
                {
                    DrawLocalPlayer(g, pt, cam);
                    continue;
                }

                // Other Players & SCPs
                bool isScp = p.Role.ToString().StartsWith("Scp");
                if (isScp && !_showScps) continue;

                Color roleCol = isScp ? Color.FromArgb(255, 60, 90) : GetRoleColor(p.Role);
                string tag = isScp ? p.Role.ToString() : (_showNames ? p.Name : string.Empty);
                DrawPlayerBlip(g, pt, roleCol, isScp ? 7.0f : 5.5f, tag);
            }
        }

        private void DrawPlayerBlip(Graphics g, PointF pt, Color color, float size, string label)
        {
            PointF[] diamond =
            {
                new(pt.X, pt.Y - size * 1.3f),
                new(pt.X + size, pt.Y),
                new(pt.X, pt.Y + size * 1.3f),
                new(pt.X - size, pt.Y)
            };

            using var brush = new SolidBrush(color);
            using var pen = new Pen(Color.FromArgb(15, 15, 15), 1.5f);
            g.FillPolygon(brush, diamond);
            g.DrawPolygon(pen, diamond);

            if (!string.IsNullOrEmpty(label))
            {
                using var textBrush = new SolidBrush(color);
                g.DrawString(label, _fontSmall, textBrush, pt.X, pt.Y + size * 1.3f + 4, _sfCenter);
            }
        }

        private void DrawLocalPlayer(Graphics g, PointF pt, CameraInfo cam)
        {
            // View Direction in screen mapping (Heavy at bottom, Entrance at top):
            // screenX is -World Z -> dirX = -cam.Forward.Z
            // screenY is -World X -> dirY = -cam.Forward.X
            float dirX = -cam.Forward.Z;
            float dirY = -cam.Forward.X;
            float len = MathF.Sqrt(dirX * dirX + dirY * dirY);
            if (len > 0.001f)
            {
                dirX /= len;
                dirY /= len;
                _lastDirX = dirX;
                _lastDirY = dirY;
            }
            else
            {
                dirX = _lastDirX;
                dirY = _lastDirY;
            }

            // NO CONE! (Removed per user request)
            // NO "YOU" TEXT! (Removed per user request)
            // Option 2 golden amber directional chevron
            DrawPlayerWedge(g, pt, dirX, dirY, ColorLocalPlayer, 9.5f, string.Empty);
        }

        private void DrawPlayerWedge(Graphics g, PointF pt, float dirX, float dirY, Color color, float size, string label)
        {
            float len = MathF.Sqrt(dirX * dirX + dirY * dirY);
            if (len > 0.001f)
            {
                dirX /= len;
                dirY /= len;
            }
            else
            {
                dirX = 0; dirY = -1;
            }

            float perpX = -dirY;
            float perpY = dirX;

            PointF tip = new(pt.X + dirX * size * 1.5f, pt.Y + dirY * size * 1.5f);
            PointF left = new(pt.X - dirX * size * 0.8f + perpX * size * 0.85f, pt.Y - dirY * size * 0.8f + perpY * size * 0.85f);
            PointF notch = new(pt.X - dirX * size * 0.25f, pt.Y - dirY * size * 0.25f);
            PointF right = new(pt.X - dirX * size * 0.8f - perpX * size * 0.85f, pt.Y - dirY * size * 0.8f - perpY * size * 0.85f);

            PointF[] poly = { tip, left, notch, right };

            using var brush = new SolidBrush(color);
            using var pen = new Pen(Color.FromArgb(15, 15, 15), 1.6f);

            g.FillPolygon(brush, poly);
            g.DrawPolygon(pen, poly);

            if (!string.IsNullOrEmpty(label))
            {
                using var textBrush = new SolidBrush(color);
                g.DrawString(label, _fontSmall, textBrush, pt.X, pt.Y + size + 5, _sfCenter);
            }
        }

        // ── Helper Utilities (Official In-Game SCP-079 Names) ──
        private static string GetShortRoomName(RoomName name)
        {
            return name switch
            {
                RoomName.Lcz914 => "914",
                RoomName.Lcz173 => "173",
                RoomName.Lcz330 => "CANDY",
                RoomName.LczArmory => "ARMORY",
                RoomName.LczCheckpointA => "CHECKPOINT",
                RoomName.LczCheckpointB => "CHECKPOINT",
                RoomName.LczClassDSpawn => "CLASS-D",
                RoomName.LczComputerRoom => "COMPUTER",
                RoomName.LczGlassroom => "GR18",
                RoomName.LczGreenhouse => "GARDEN",
                RoomName.LczToilets => "WC",
                RoomName.LczAirlock => "AIRLOCK",

                RoomName.Hcz049 => "049/173 CONT",
                RoomName.Hcz079 => "079 CONT",
                RoomName.Hcz096 => "096",
                RoomName.Hcz106 => "106",
                RoomName.Hcz939 => "939 CONT",
                RoomName.Hcz127 => "127",
                RoomName.HczMicroHID => "H.I.D.",
                RoomName.HczArmory => "ARMORY",
                RoomName.HczServers => "SERVERS",
                RoomName.HczTesla => "TESLA",
                RoomName.HczTestroom => "TESTROOM",
                RoomName.HczWarhead => "WARHEAD",
                RoomName.HczCheckpointA => "CHECKPOINT",
                RoomName.HczCheckpointB => "CHECKPOINT",
                RoomName.HczCheckpointToEntranceZone => "CHECKPOINT",
                RoomName.HczAcroamaticAbatement => "WATERFALL",
                RoomName.HczWaysideIncinerator => "FURNACE",
                RoomName.HczRampTunnel => "RAMP",

                RoomName.EzGateA => "GATE A",
                RoomName.EzGateB => "GATE B",
                RoomName.EzIntercom => "INTERCOM",
                RoomName.EzEvacShelter => "EVAC",
                RoomName.EzRedroom => "REDROOM",
                RoomName.EzCollapsedTunnel => "COLLAPSED",
                RoomName.EzOfficeLarge => "OFFICE L",
                RoomName.EzOfficeSmall => "OFFICE S",
                RoomName.EzOfficeStoried => "OFFICE 2F",

                RoomName.Outside => "SURFACE",
                RoomName.Pocket => "POCKET",
                _ => string.Empty
            };
        }

        private static Color GetRoleColor(RoleTypeId role)
        {
            string s = role.ToString();
            if (s.StartsWith("Scp")) return Color.FromArgb(255, 60, 90);
            if (s.StartsWith("ClassD")) return Color.FromArgb(255, 140, 20);
            if (s.StartsWith("Scientist")) return Color.FromArgb(255, 220, 30);
            if (s.StartsWith("FacilityGuard")) return Color.FromArgb(110, 165, 220);
            if (s.StartsWith("Ntf") || s.StartsWith("Facility")) return Color.FromArgb(0, 220, 255);
            if (s.StartsWith("Chaos")) return Color.FromArgb(0, 235, 95);
            return Color.FromArgb(160, 185, 210);
        }
    }
}

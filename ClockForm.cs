using System.Drawing.Drawing2D;

namespace DesktopDigitalClock;

internal sealed class ClockForm : Form
{
    private const int EdgeMargin = 16;
    private readonly SevenSegmentDisplay _clockDisplay;
    private readonly System.Windows.Forms.Timer _clockTimer;
    private readonly ContextMenuStrip _menu;
    private readonly ToolStripMenuItem _twentyFourHourItem;
    private readonly ToolStripMenuItem _twelveHourItem;
    private readonly ToolStripMenuItem _topMostItem;
    private readonly ToolStripMenuItem _transparentItem;

    private bool _dragging;
    private bool _minimizedToTaskbar;
    private Point _dragStartCursor;
    private Point _dragStartWindow;

    public ClockForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(16, 21, 27);
        ClientSize = new Size(360, 108);
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = nameof(ClockForm);
        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        Opacity = 0.92;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Text = "桌面电子时钟";
        TopMost = true;

        _clockDisplay = new SevenSegmentDisplay
        {
            Dock = DockStyle.Fill,
            IsTwentyFourHour = true
        };
        Controls.Add(_clockDisplay);

        _twentyFourHourItem = new ToolStripMenuItem("24 小时制");
        _twelveHourItem = new ToolStripMenuItem("12 小时制");
        _topMostItem = new ToolStripMenuItem("始终置顶")
        {
            CheckOnClick = true,
            Checked = true
        };
        _transparentItem = new ToolStripMenuItem("半透明背景")
        {
            CheckOnClick = true,
            Checked = true
        };

        _twentyFourHourItem.Click += (_, _) => SetHourMode(true);
        _twelveHourItem.Click += (_, _) => SetHourMode(false);
        _topMostItem.CheckedChanged += (_, _) => TopMost = _topMostItem.Checked;
        _transparentItem.CheckedChanged += (_, _) =>
            Opacity = _transparentItem.Checked ? 0.92 : 1.0;

        var hourModeMenu = new ToolStripMenuItem("时间制式");
        hourModeMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            _twentyFourHourItem,
            _twelveHourItem
        });

        var restorePositionItem = new ToolStripMenuItem("恢复到右上角");
        restorePositionItem.Click += (_, _) => MoveToTopRight(Screen.FromControl(this));

        var hideToTaskbarItem = new ToolStripMenuItem("隐藏到任务栏");
        hideToTaskbarItem.Click += (_, _) =>
        {
            _minimizedToTaskbar = true;
            ShowIcon = true;
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = true;
        };

        var exitItem = new ToolStripMenuItem("退出");
        exitItem.Click += (_, _) => Close();

        _menu = new ContextMenuStrip();
        _menu.Items.AddRange(new ToolStripItem[]
        {
            hourModeMenu,
            _topMostItem,
            _transparentItem,
            new ToolStripSeparator(),
            restorePositionItem,
            hideToTaskbarItem,
            new ToolStripSeparator(),
            exitItem
        });
        _menu.Opening += (_, _) => UpdateMenuChecks();
        ContextMenuStrip = _menu;
        _clockDisplay.ContextMenuStrip = _menu;

        RegisterDragSurface(this);
        RegisterDragSurface(_clockDisplay);

        _clockTimer = new System.Windows.Forms.Timer
        {
            Interval = 100
        };
        _clockTimer.Tick += (_, _) => _clockDisplay.CurrentTime = DateTime.Now;

        Resize += (_, _) =>
        {
            if (_minimizedToTaskbar && WindowState == FormWindowState.Normal)
            {
                _minimizedToTaskbar = false;
                ShowInTaskbar = false;
                ShowIcon = false;
            }
        };

        Shown += (_, _) =>
        {
            MoveToTopRight(Screen.PrimaryScreen ?? Screen.FromControl(this));
            _clockDisplay.CurrentTime = DateTime.Now;
            _clockTimer.Start();
        };
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        using var path = new GraphicsPath();
        const int radius = 18;
        var bounds = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);
        path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);
        path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();

        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clockTimer.Dispose();
            _menu.Dispose();
        }

        base.Dispose(disposing);
    }

    private void SetHourMode(bool useTwentyFourHour)
    {
        _clockDisplay.IsTwentyFourHour = useTwentyFourHour;
        UpdateMenuChecks();
    }

    private void UpdateMenuChecks()
    {
        _twentyFourHourItem.Checked = _clockDisplay.IsTwentyFourHour;
        _twelveHourItem.Checked = !_clockDisplay.IsTwentyFourHour;
    }

    private void MoveToTopRight(Screen screen)
    {
        Rectangle workingArea = screen.WorkingArea;
        Location = new Point(
            workingArea.Right - Width - EdgeMargin,
            workingArea.Top + EdgeMargin);
    }

    private void RegisterDragSurface(Control control)
    {
        control.MouseDown += (_, eventArgs) =>
        {
            if (eventArgs.Button != MouseButtons.Left)
            {
                return;
            }

            _dragging = true;
            _dragStartCursor = Cursor.Position;
            _dragStartWindow = Location;
            control.Capture = true;
        };

        control.MouseMove += (_, _) =>
        {
            if (!_dragging)
            {
                return;
            }

            Point cursor = Cursor.Position;
            Location = new Point(
                _dragStartWindow.X + cursor.X - _dragStartCursor.X,
                _dragStartWindow.Y + cursor.Y - _dragStartCursor.Y);
        };

        control.MouseUp += (_, eventArgs) =>
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                _dragging = false;
                control.Capture = false;
            }
        };
    }
}

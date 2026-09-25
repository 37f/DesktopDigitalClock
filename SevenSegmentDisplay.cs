using System.Drawing.Drawing2D;

namespace DesktopDigitalClock;

internal sealed class SevenSegmentDisplay : Control
{
    private static readonly Segment[] DigitSegments =
    {
        Segment.A | Segment.B | Segment.C | Segment.D | Segment.E | Segment.F,
        Segment.B | Segment.C,
        Segment.A | Segment.B | Segment.D | Segment.E | Segment.G,
        Segment.A | Segment.B | Segment.C | Segment.D | Segment.G,
        Segment.B | Segment.C | Segment.F | Segment.G,
        Segment.A | Segment.C | Segment.D | Segment.F | Segment.G,
        Segment.A | Segment.C | Segment.D | Segment.E | Segment.F | Segment.G,
        Segment.A | Segment.B | Segment.C,
        Segment.A | Segment.B | Segment.C | Segment.D | Segment.E | Segment.F | Segment.G,
        Segment.A | Segment.B | Segment.C | Segment.D | Segment.F | Segment.G
    };

    private DateTime _currentTime = DateTime.Now;
    private bool _isTwentyFourHour = true;

    public SevenSegmentDisplay()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    public DateTime CurrentTime
    {
        get => _currentTime;
        set
        {
            if (_currentTime.Hour == value.Hour &&
                _currentTime.Minute == value.Minute &&
                _currentTime.Second == value.Second)
            {
                return;
            }

            _currentTime = value;
            Invalidate();
        }
    }

    public bool IsTwentyFourHour
    {
        get => _isTwentyFourHour;
        set
        {
            if (_isTwentyFourHour == value)
            {
                return;
            }

            _isTwentyFourHour = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(Color.FromArgb(16, 21, 27));

        RectangleF inner = new(12f, 10f, ClientSize.Width - 24f, ClientSize.Height - 20f);
        if (inner.Width <= 0 || inner.Height <= 0)
        {
            return;
        }

        using var borderPen = new Pen(Color.FromArgb(70, 97, 116), 1f);
        graphics.DrawRoundedRectangle(borderPen, new RectangleF(1f, 1f, Width - 2f, Height - 2f), 16f);

        float suffixWidth = IsTwentyFourHour ? 0f : 34f;
        float clockWidth = inner.Width - suffixWidth;
        float digitWidth = Math.Min(clockWidth / 7.42f, inner.Height / 1.85f);
        float digitHeight = digitWidth * 1.85f;
        float colonWidth = digitWidth * 0.36f;
        float gap = digitWidth * 0.10f;
        float totalWidth = digitWidth * 6f + colonWidth * 2f + gap * 7f;
        float x = inner.Left + (clockWidth - totalWidth) / 2f;
        float y = inner.Top + (inner.Height - digitHeight) / 2f;

        int hour = IsTwentyFourHour
            ? _currentTime.Hour
            : (_currentTime.Hour % 12 == 0 ? 12 : _currentTime.Hour % 12);
        string timeText = $"{hour:00}{_currentTime.Minute:00}{_currentTime.Second:00}";

        for (int index = 0; index < timeText.Length; index++)
        {
            DrawDigit(graphics, timeText[index] - '0', x, y, digitWidth, digitHeight);
            x += digitWidth + gap;

            if (index is 1 or 3)
            {
                DrawColon(graphics, x, y, colonWidth, digitHeight);
                x += colonWidth + gap;
            }
        }

        if (!IsTwentyFourHour)
        {
            string suffix = _currentTime.Hour < 12 ? "AM" : "PM";
            using var suffixFont = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            using var suffixBrush = new SolidBrush(Color.FromArgb(195, 82, 235, 214));
            var suffixArea = new RectangleF(inner.Right - suffixWidth, inner.Top, suffixWidth, inner.Height);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            graphics.DrawString(suffix, suffixFont, suffixBrush, suffixArea, format);
        }
    }

    private static void DrawDigit(
        Graphics graphics,
        int number,
        float x,
        float y,
        float width,
        float height)
    {
        float thickness = Math.Max(3f, width * 0.16f);
        float horizontalX = x + thickness * 0.42f;
        float horizontalWidth = width - thickness * 0.84f;
        float upperVerticalY = y + thickness * 0.42f;
        float verticalHeight = height / 2f - thickness * 0.64f;
        float lowerVerticalY = y + height / 2f + thickness * 0.22f;

        Segment activeSegments = DigitSegments[number];

        DrawHorizontalSegment(graphics, activeSegments.HasFlag(Segment.A), horizontalX, y, horizontalWidth, thickness);
        DrawVerticalSegment(graphics, activeSegments.HasFlag(Segment.F), x, upperVerticalY, thickness, verticalHeight);
        DrawVerticalSegment(graphics, activeSegments.HasFlag(Segment.B), x + width - thickness, upperVerticalY, thickness, verticalHeight);
        DrawHorizontalSegment(
            graphics,
            activeSegments.HasFlag(Segment.G),
            horizontalX,
            y + height / 2f - thickness / 2f,
            horizontalWidth,
            thickness);
        DrawVerticalSegment(graphics, activeSegments.HasFlag(Segment.E), x, lowerVerticalY, thickness, verticalHeight);
        DrawVerticalSegment(graphics, activeSegments.HasFlag(Segment.C), x + width - thickness, lowerVerticalY, thickness, verticalHeight);
        DrawHorizontalSegment(
            graphics,
            activeSegments.HasFlag(Segment.D),
            horizontalX,
            y + height - thickness,
            horizontalWidth,
            thickness);
    }

    private static void DrawHorizontalSegment(
        Graphics graphics,
        bool active,
        float x,
        float y,
        float width,
        float thickness)
    {
        PointF[] points =
        {
            new(x + thickness / 2f, y),
            new(x + width - thickness / 2f, y),
            new(x + width, y + thickness / 2f),
            new(x + width - thickness / 2f, y + thickness),
            new(x + thickness / 2f, y + thickness),
            new(x, y + thickness / 2f)
        };

        using var brush = new SolidBrush(active
            ? Color.FromArgb(240, 82, 235, 214)
            : Color.FromArgb(24, 82, 235, 214));
        graphics.FillPolygon(brush, points);
    }

    private static void DrawVerticalSegment(
        Graphics graphics,
        bool active,
        float x,
        float y,
        float thickness,
        float height)
    {
        PointF[] points =
        {
            new(x + thickness / 2f, y),
            new(x + thickness, y + thickness / 2f),
            new(x + thickness, y + height - thickness / 2f),
            new(x + thickness / 2f, y + height),
            new(x, y + height - thickness / 2f),
            new(x, y + thickness / 2f)
        };

        using var brush = new SolidBrush(active
            ? Color.FromArgb(240, 82, 235, 214)
            : Color.FromArgb(24, 82, 235, 214));
        graphics.FillPolygon(brush, points);
    }

    private static void DrawColon(
        Graphics graphics,
        float x,
        float y,
        float width,
        float height)
    {
        float dotSize = Math.Max(4f, width * 0.42f);
        float dotX = x + (width - dotSize) / 2f;
        using var brush = new SolidBrush(Color.FromArgb(240, 82, 235, 214));
        graphics.FillEllipse(brush, dotX, y + height * 0.30f - dotSize / 2f, dotSize, dotSize);
        graphics.FillEllipse(brush, dotX, y + height * 0.70f - dotSize / 2f, dotSize, dotSize);
    }

    [Flags]
    private enum Segment
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        E = 1 << 4,
        F = 1 << 5,
        G = 1 << 6
    }
}

internal static class GraphicsExtensions
{
    public static void DrawRoundedRectangle(
        this Graphics graphics,
        Pen pen,
        RectangleF bounds,
        float radius)
    {
        float diameter = radius * 2f;
        using var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
    }
}

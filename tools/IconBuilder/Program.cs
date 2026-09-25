using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: IconBuilder <output.ico> <preview.png>");
    return 1;
}

string iconPath = Path.GetFullPath(args[0]);
string previewPath = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(iconPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(previewPath)!);

int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
var images = new List<byte[]>(sizes.Length);

foreach (int size in sizes)
{
    using Bitmap bitmap = RenderIcon(size);
    using var stream = new MemoryStream();
    bitmap.Save(stream, ImageFormat.Png);
    images.Add(stream.ToArray());

    if (size == 256)
    {
        bitmap.Save(previewPath, ImageFormat.Png);
    }
}

using (var file = File.Create(iconPath))
using (var writer = new BinaryWriter(file))
{
    writer.Write((ushort)0);
    writer.Write((ushort)1);
    writer.Write((ushort)sizes.Length);

    int imageOffset = 6 + sizes.Length * 16;
    for (int index = 0; index < sizes.Length; index++)
    {
        int size = sizes[index];
        writer.Write((byte)(size == 256 ? 0 : size));
        writer.Write((byte)(size == 256 ? 0 : size));
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        writer.Write(images[index].Length);
        writer.Write(imageOffset);
        imageOffset += images[index].Length;
    }

    foreach (byte[] image in images)
    {
        writer.Write(image);
    }
}

Console.WriteLine(iconPath);
Console.WriteLine(previewPath);
return 0;

static Bitmap RenderIcon(int size)
{
    var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using Graphics graphics = Graphics.FromImage(bitmap);
    graphics.Clear(Color.Transparent);
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

    float inset = size * 0.055f;
    var face = new RectangleF(inset, inset, size - inset * 2f, size - inset * 2f);
    float radius = size * 0.19f;

    using GraphicsPath facePath = CreateRoundedRectangle(face, radius);
    using var background = new LinearGradientBrush(
        face,
        Color.FromArgb(255, 28, 39, 50),
        Color.FromArgb(255, 8, 12, 17),
        LinearGradientMode.Vertical);
    graphics.FillPath(background, facePath);

    float borderWidth = Math.Max(1f, size * 0.012f);
    using var border = new Pen(Color.FromArgb(230, 71, 101, 116), borderWidth);
    graphics.DrawPath(border, facePath);

    DrawTime(graphics, size, "12:34");
    return bitmap;
}

static void DrawTime(Graphics graphics, int size, string text)
{
    float digitWidth = size * 0.17f;
    float digitHeight = size * 0.42f;
    float colonWidth = size * 0.065f;
    float gap = size * 0.018f;
    float totalWidth = digitWidth * 4f + colonWidth + gap * 4f;
    float x = (size - totalWidth) / 2f;
    float y = (size - digitHeight) / 2f;
    Color segmentColor = Color.FromArgb(255, 82, 235, 214);

    foreach (char character in text)
    {
        if (character == ':')
        {
            DrawColon(graphics, x, y, colonWidth, digitHeight, segmentColor);
            x += colonWidth + gap;
            continue;
        }

        DrawDigit(graphics, character - '0', x, y, digitWidth, digitHeight, segmentColor);
        x += digitWidth + gap;
    }
}

static void DrawDigit(
    Graphics graphics,
    int number,
    float x,
    float y,
    float width,
    float height,
    Color color)
{
    int[] masks =
    {
        0b0111111,
        0b0000110,
        0b1011011,
        0b1001111,
        0b1100110,
        0b1101101,
        0b1111101,
        0b0000111,
        0b1111111,
        0b1101111
    };

    int active = masks[number];
    float thickness = Math.Max(1f, width * 0.17f);
    float horizontalX = x + thickness * 0.42f;
    float horizontalWidth = width - thickness * 0.84f;
    float verticalHeight = height / 2f - thickness * 0.64f;
    float upperY = y + thickness * 0.42f;
    float lowerY = y + height / 2f + thickness * 0.22f;

    if ((active & (1 << 0)) != 0) DrawHorizontal(graphics, horizontalX, y, horizontalWidth, thickness, color);
    if ((active & (1 << 1)) != 0) DrawVertical(graphics, x + width - thickness, upperY, thickness, verticalHeight, color);
    if ((active & (1 << 2)) != 0) DrawVertical(graphics, x + width - thickness, lowerY, thickness, verticalHeight, color);
    if ((active & (1 << 3)) != 0) DrawHorizontal(graphics, horizontalX, y + height - thickness, horizontalWidth, thickness, color);
    if ((active & (1 << 4)) != 0) DrawVertical(graphics, x, lowerY, thickness, verticalHeight, color);
    if ((active & (1 << 5)) != 0) DrawVertical(graphics, x, upperY, thickness, verticalHeight, color);
    if ((active & (1 << 6)) != 0) DrawHorizontal(graphics, horizontalX, y + height / 2f - thickness / 2f, horizontalWidth, thickness, color);
}

static void DrawHorizontal(
    Graphics graphics,
    float x,
    float y,
    float width,
    float thickness,
    Color color)
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

    using var brush = new SolidBrush(color);
    graphics.FillPolygon(brush, points);
}

static void DrawVertical(
    Graphics graphics,
    float x,
    float y,
    float thickness,
    float height,
    Color color)
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

    using var brush = new SolidBrush(color);
    graphics.FillPolygon(brush, points);
}

static void DrawColon(
    Graphics graphics,
    float x,
    float y,
    float width,
    float height,
    Color color)
{
    float dotSize = Math.Max(1f, width * 0.48f);
    float dotX = x + (width - dotSize) / 2f;
    using var brush = new SolidBrush(color);
    graphics.FillEllipse(brush, dotX, y + height * 0.32f - dotSize / 2f, dotSize, dotSize);
    graphics.FillEllipse(brush, dotX, y + height * 0.68f - dotSize / 2f, dotSize, dotSize);
}

static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
{
    float diameter = radius * 2f;
    var path = new GraphicsPath();
    path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
    path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
    path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();
    return path;
}

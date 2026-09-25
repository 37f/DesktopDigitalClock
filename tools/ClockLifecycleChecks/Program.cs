using System.Reflection;

namespace ClockLifecycleChecks;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();

        try
        {
            HideToTaskbarCanBeRestoredWithoutLosingState();
            Console.WriteLine("PASS: hide to taskbar and restore preserve the clock state");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"FAIL: {error.Message}");
            return 1;
        }
    }

    private static void HideToTaskbarCanBeRestoredWithoutLosingState()
    {
        // Exercise the real form and menu, not a copy of its minimize logic.
        Type clockType = Assembly.Load("DesktopDigitalClock")
            .GetType("DesktopDigitalClock.ClockForm", throwOnError: true)!;
        using var clock = (Form)Activator.CreateInstance(clockType, nonPublic: true)!;
        clock.Show();
        Application.DoEvents();

        var twelveHourItem = FindMenuItem(clock.ContextMenuStrip!, "12 小时制")
            ?? throw new Exception("12-hour menu action is missing");
        twelveHourItem.PerformClick();
        clock.Location = new Point(100, 100);

        var hideItem = FindMenuItem(clock.ContextMenuStrip!, "隐藏到任务栏")
            ?? throw new Exception("Hide-to-taskbar menu action is missing");
        hideItem.PerformClick();
        Application.DoEvents();

        Require(clock.WindowState == FormWindowState.Minimized, "clock was not minimized");
        Require(clock.ShowInTaskbar, "minimized clock has no taskbar button");
        Require(clock.ShowIcon, "taskbar button has no application icon");

        // Simulate the normal restore action performed by the taskbar button.
        clock.WindowState = FormWindowState.Normal;
        Application.DoEvents();

        Require(clock.Visible, "clock did not reappear");
        Require(!clock.ShowInTaskbar, "restored clock kept an unwanted taskbar button");
        Require(!clock.ShowIcon, "restored clock unexpectedly shows an icon");
        Require(clock.Location == new Point(100, 100), "restoring changed the dragged position");

        FieldInfo displayField = clockType.GetField("_clockDisplay", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Exception("clock display was not found");
        object display = displayField.GetValue(clock)!;
        bool isTwentyFourHour = (bool)display.GetType().GetProperty("IsTwentyFourHour")!.GetValue(display)!;
        Require(!isTwentyFourHour, "restoring reset the 12-hour selection");
    }

    private static ToolStripMenuItem? FindMenuItem(ContextMenuStrip menu, string text)
    {
        foreach (ToolStripMenuItem item in menu.Items.OfType<ToolStripMenuItem>())
        {
            if (item.Text == text)
            {
                return item;
            }

            foreach (ToolStripMenuItem child in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (child.Text == text)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
}

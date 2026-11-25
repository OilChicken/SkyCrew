using Godot;

namespace SkyCrew.World;

public partial class WorldClock : Node
{
    private const double HoursPerDay = 24.0;

    private SkyRig _sky = null!;

    [Export(PropertyHint.Range, "0,24,0.016667")]
    public double StartHour { get; set; } = 8.5;

    [Export]
    public bool StartRunning { get; set; } = true;

    [Export(PropertyHint.Range, "1,3600,1,or_greater,suffix:s")]
    public double DayDurationSeconds { get; set; } = 1800.0;

    public double Hour { get; private set; }
    public bool Running { get; private set; }

    public override void _Ready()
    {
        _sky = GetParent<SkyRig>();
        SetHour(StartHour);
        SetRunning(StartRunning);
    }

    public override void _Process(double delta)
    {
        var hoursElapsed = HoursPerDay * delta / System.Math.Max(1.0, DayDurationSeconds);
        SetHour(Mathf.PosMod(Hour + hoursElapsed, HoursPerDay));
    }

    public void SetHour(double hour)
    {
        Hour = Mathf.Clamp(hour, 0.0, HoursPerDay);
        _sky.ApplyHour(Hour);
    }

    public void SetRunning(bool running)
    {
        Running = running;
        SetProcess(running);
    }
}

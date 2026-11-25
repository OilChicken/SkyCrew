using Godot;
using SkyCrew.World;

namespace SkyCrew.Debug;

public partial class EnvironmentDebugPanel : CanvasLayer
{
	private static readonly StringName FlightPreview = new("fly_through");

	[Export] public WorldClock Clock { get; set; } = null!;
	[Export] public Camera3D OverviewCamera { get; set; } = null!;
	[Export] public Camera3D AboveCloudsCamera { get; set; } = null!;
	[Export] public Camera3D FlightCamera { get; set; } = null!;
	[Export] public Camera3D UnderCloudsCamera { get; set; } = null!;
	[Export] public AnimationPlayer FlightAnimation { get; set; } = null!;

	[Export] private HSlider _timeSlider = null!;
	[Export] private Label _timeLabel = null!;
	[Export] private CheckButton _playToggle = null!;
	[Export] private Control _panel = null!;

	private int _lastMinute = -1;

	public override void _Ready()
	{
		_playToggle.SetPressedNoSignal(Clock.Running);
		RefreshTime();
	}

	public override void _Process(double delta)
	{
		if (_panel.Visible && Clock.Running)
		{
			RefreshTime();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F3 })
		{
			_panel.Visible = !_panel.Visible;
			RefreshTime();
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnTimeChanged(double hour)
	{
		OnTimeDragStarted();
		Clock.SetHour(hour);
		RefreshTime();
	}

	private void OnTimeDragStarted()
	{
		Clock.SetRunning(false);
		_playToggle.SetPressedNoSignal(false);
	}

	private void OnPlayToggled(bool pressed)
	{
		Clock.SetRunning(pressed);
	}

	private void OnCameraSelected(long index)
	{
		switch (index)
		{
			case 0:
				FlightAnimation.Stop();
				OverviewCamera.MakeCurrent();
				break;
			case 1:
				FlightAnimation.Stop();
				AboveCloudsCamera.MakeCurrent();
				break;
			case 2:
				FlightCamera.MakeCurrent();
				FlightAnimation.Play(FlightPreview);
				break;
			case 3:
				FlightAnimation.Stop();
				UnderCloudsCamera.MakeCurrent();
				break;
		}
	}

	private void RefreshTime()
	{
		var hour = Clock.Hour;
		_timeSlider.SetValueNoSignal(hour);

		var minutes = Mathf.RoundToInt(hour * 60);
		if (minutes != _lastMinute)
		{
			_timeLabel.Text = $"{minutes / 60:00}:{minutes % 60:00}";
			_lastMinute = minutes;
		}
	}
}

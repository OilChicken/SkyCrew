using Godot;

namespace SkyCrew.World;

[Tool]
public partial class SkyRig : Node3D
{
	private static readonly StringName SunDirectionUniform = new("skycrew_sun_direction");

	private Node3D? _orbit;
	private DirectionalLight3D? _sun;
	private WorldEnvironment? _worldEnvironment;

	private double _previewHour = 8.5;

	[Export(PropertyHint.Range, "0,24,0.05,suffix:h")]
	public double PreviewHour
	{
		get => _previewHour;
		set
		{
			_previewHour = value;
			if (Engine.IsEditorHint() && IsNodeReady())
			{
				ApplyHour(value);
			}
		}
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			ApplyHour(PreviewHour);
		}
	}

	public void ApplyHour(double hour)
	{
		_orbit ??= GetNode<Node3D>("SunFrame/Orbit");
		_sun ??= GetNode<DirectionalLight3D>("SunFrame/Orbit/Sun");
		_worldEnvironment ??= GetNode<WorldEnvironment>("WorldEnvironment");
		_orbit.RotationDegrees = new Vector3(0, 0, 90f - 15f * (float)hour);
		var direction = _sun.GlobalBasis.Z.Normalized();
		RenderingServer.GlobalShaderParameterSet(SunDirectionUniform, direction);

		var day = Mathf.SmoothStep(-0.12f, 0.36f, direction.Y);
		var direct = Mathf.SmoothStep(-0.04f, 0.43f, direction.Y);
		var twilight = Mathf.SmoothStep(-0.23f, -0.065f, direction.Y)
			* (1f - Mathf.SmoothStep(0.04f, 0.36f, direction.Y));

		_sun.LightEnergy = 1.08f * direct;
		_sun.LightColor = new Color(1f, 0.57f, 0.32f).Lerp(new Color(1f, 0.95f, 0.84f), direct);

		var environment = _worldEnvironment.Environment;
		environment.AmbientLightEnergy = Mathf.Lerp(0.24f, 0.76f, day);
		environment.AmbientLightColor = new Color(0.25f, 0.35f, 0.57f)
			.Lerp(new Color(0.74f, 0.85f, 0.95f), day)
			.Lerp(new Color(0.67f, 0.51f, 0.63f), twilight * 0.65f);
		PublishCloudLighting();
	}

	public override void _Process(double delta)
	{
		// Also pick up Inspector edits to the existing light and environment resources.
		PublishCloudLighting();
	}

	private void PublishCloudLighting()
	{
		if (_sun == null || _worldEnvironment == null)
		{
			return;
		}

		var environment = _worldEnvironment.Environment;
		var ambient = environment.AmbientLightColor;
		var ambientStrength = Mathf.Sqrt(Mathf.Max(0, environment.AmbientLightEnergy));
		var sunColor = _sun.LightColor;
		var sunEnergy = _sun.Visible ? Mathf.Max(0, _sun.LightEnergy) : 0;
		var light = sunColor * (sunEnergy * 0.80f) + ambient * (ambientStrength * 0.75f);
		var shadow = ambient * (ambientStrength * 0.55f);
		RenderingServer.GlobalShaderParameterSet(SunDirectionUniform, _sun.GlobalBasis.Z.Normalized());
		RenderingServer.GlobalShaderParameterSet("skycrew_cloud_light", new Color(light.R, light.G, light.B, 1));
		RenderingServer.GlobalShaderParameterSet("skycrew_cloud_shadow", new Color(shadow.R, shadow.G, shadow.B, 1));
	}
}

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

<<<<<<< Updated upstream
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
=======
	[ExportGroup("Horizon")]
	[Export(PropertyHint.Range, "500,10000,100,suffix:m")]
	public float HorizonFadeStart { get; set; } = 2200;
	[Export(PropertyHint.Range, "2000,20000,100,suffix:m")]
	public float HorizonFadeEnd { get; set; } = 9000;

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
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
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
=======
		var environment = _worldEnvironment.Environment;
		var ambient = environment.AmbientLightColor;
		var ambientStrength = Mathf.Sqrt(Mathf.Max(0, environment.AmbientLightEnergy));
		var sunColor = _sun.LightColor;
		var sunEnergy = _sun.Visible ? Mathf.Max(0, _sun.LightEnergy) : 0;
		var light = sunColor * (sunEnergy * 0.80f) + ambient * (ambientStrength * 0.75f);
		var shadow = ambient * (ambientStrength * 0.55f);
		var day = Mathf.SmoothStep(-0.10f, 0.22f, _sun.GlobalBasis.Z.Y);
		var dusk = Mathf.SmoothStep(-0.22f, -0.06f, _sun.GlobalBasis.Z.Y)
			* (1 - Mathf.SmoothStep(0.07f, 0.32f, _sun.GlobalBasis.Z.Y));
		environment.FogEnabled = true;
		environment.FogMode = Godot.Environment.FogModeEnum.Depth;
		environment.FogDensity = 1;
		environment.FogDepthBegin = Mathf.Max(0, HorizonFadeStart);
		environment.FogDepthEnd = Mathf.Max(environment.FogDepthBegin + 500, HorizonFadeEnd);
		environment.FogDepthCurve = 1.6f;
		environment.FogSkyAffect = 0;
		environment.FogAerialPerspective = 1;
		environment.FogLightColor = new Color(0.025f, 0.038f, 0.075f)
			.Lerp(new Color(0.61f, 0.78f, 0.85f), day)
			.Lerp(new Color(0.55f, 0.30f, 0.26f), dusk * 0.5f).LinearToSrgb();
		RenderingServer.GlobalShaderParameterSet(SunDirectionUniform, _sun.GlobalBasis.Z.Normalized());
		RenderingServer.GlobalShaderParameterSet("skycrew_cloud_light", new Color(light.R, light.G, light.B, 1));
		RenderingServer.GlobalShaderParameterSet("skycrew_cloud_shadow", new Color(shadow.R, shadow.G, shadow.B, 1));
	}
>>>>>>> Stashed changes
}

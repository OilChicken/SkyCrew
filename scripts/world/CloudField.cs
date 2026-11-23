using Godot;
using System.Collections.Generic;

namespace SkyCrew.World;

[Tool]
public partial class CloudField : Node3D
{
	[Export] public PackedScene CloudScene { get; set; } = null!;
	[Export] public int Seed { get; set; } = 128;
	[Export] public Vector2 CoverageSize { get; set; } = new(5200, 5200);
	[Export(PropertyHint.Range, "200,2000,10,suffix:m")]
	public float CellSpacing { get; set; } = 720;
	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CellOccupancy { get; set; } = 0.65f;
	[Export(PropertyHint.Range, "0,5000,10,suffix:m")]
	public float MinimumAltitude { get; set; } = 450;
	[Export(PropertyHint.Range, "0,2000,10,suffix:m")]
	public float HeightBand { get; set; } = 300;
	[Export] public Vector2 ScaleRange { get; set; } = new(8, 13);
	[Export(PropertyHint.Range, "1,128,1")]
	public int MaxClouds { get; set; } = 48;
	[Export] public Vector2 WindVelocity { get; set; } = new(6, 1.5f);

	[ExportToolButton("重新生成云")]
	public Callable Regenerate => Callable.From(Rebuild);

	private readonly List<Node3D> _clouds = new();
	private (PackedScene, int, Vector2, float, float, float, float, Vector2, int) _configuration;
	private Vector2 _size;
	private float _shapeMargin;

	public override void _Ready()
	{
		Rebuild();
	}

	public override void _Process(double delta)
	{
		// Editor previews regenerate from the same seed and never drift or serialize instances.
		if (Engine.IsEditorHint())
		{
			var margin = _clouds.Count > 0
				? DisplacementMargin((ShaderMaterial)_clouds[0].GetNode<MeshInstance3D>("CloudModel").MaterialOverride, 1)
				: 0;
			if (_configuration != Configuration() || !Mathf.IsEqualApprox(margin, _shapeMargin))
			{
				Rebuild();
			}
			return;
		}

		var motion = WindVelocity * (float)delta;
		var center = GlobalPosition;
		foreach (var cloud in _clouds)
		{
			var point = cloud.GlobalPosition;
			point.X = center.X + Mathf.PosMod(point.X - center.X + motion.X + _size.X / 2, _size.X) - _size.X / 2;
			point.Z = center.Z + Mathf.PosMod(point.Z - center.Z + motion.Y + _size.Y / 2, _size.Y) - _size.Y / 2;
			cloud.GlobalPosition = point;
		}
	}

	private (PackedScene, int, Vector2, float, float, float, float, Vector2, int) Configuration()
	{
		return (CloudScene, Seed, CoverageSize, CellSpacing, CellOccupancy,
			MinimumAltitude, HeightBand, ScaleRange, MaxClouds);
	}

	public void Rebuild()
	{
		if (!IsInsideTree())
		{
			return;
		}

		foreach (var cloud in _clouds)
		{
			RemoveChild(cloud);
			cloud.QueueFree();
		}
		_clouds.Clear();
		_shapeMargin = 0;
		_configuration = Configuration();
		_size = new Vector2(Mathf.Max(200, CoverageSize.X), Mathf.Max(200, CoverageSize.Y));
		if (CloudScene == null)
		{
			return;
		}

		var random = new RandomNumberGenerator { Seed = unchecked((ulong)Seed) };
		var spacing = Mathf.Max(200, CellSpacing);
		var columns = Mathf.Clamp((int)(_size.X / spacing), 1, 64);
		var rows = Mathf.Clamp((int)(_size.Y / spacing), 1, 64);
		var cell = _size / new Vector2(columns, rows);
		var minimumScale = Mathf.Max(1, Mathf.Min(ScaleRange.X, ScaleRange.Y));
		var maximumScale = Mathf.Max(minimumScale, Mathf.Max(ScaleRange.X, ScaleRange.Y));

		// Shuffle cells so a population limit still covers the whole field.
		var cells = new List<int>(columns * rows);
		for (var i = 0; i < columns * rows; i++)
		{
			cells.Add(i);
		}
		for (var i = cells.Count - 1; i > 0; i--)
		{
			var other = random.RandiRange(0, i);
			(cells[i], cells[other]) = (cells[other], cells[i]);
		}

		foreach (var index in cells)
		{
			if (_clouds.Count >= Mathf.Clamp(MaxClouds, 1, 128))
			{
				break;
			}
			if (random.Randf() >= Mathf.Clamp(CellOccupancy, 0, 1))
			{
				continue;
			}

			var cloud = CloudScene.Instantiate<Node3D>();
			cloud.Name = $"Cloud{_clouds.Count + 1:00}";
			AddChild(cloud);
			var scale = random.RandfRange(minimumScale, maximumScale);
			var yaw = random.RandfRange(-Mathf.Pi, Mathf.Pi);
			cloud.GlobalBasis = new Basis(Vector3.Up, yaw).Scaled(Vector3.One * scale);
			var model = cloud.GetNode<MeshInstance3D>("CloudModel");
			var bounds = model.Mesh.GetAabb();
			var material = (ShaderMaterial)model.MaterialOverride;
			var displacementMargin = DisplacementMargin(material, scale);
			_shapeMargin = DisplacementMargin(material, 1);
			var bottomOffset = (model.Position.Y + bounds.Position.Y * model.Scale.Y) * scale;
			var worldPoint = new Vector3(
				(index % columns + 0.5f + random.RandfRange(-0.18f, 0.18f)) * cell.X - _size.X / 2,
				MinimumAltitude - bottomOffset + displacementMargin + random.RandfRange(0, Mathf.Max(0, HeightBand)),
				(index / columns + 0.5f + random.RandfRange(-0.18f, 0.18f)) * cell.Y - _size.Y / 2);
			cloud.GlobalPosition = worldPoint + new Vector3(GlobalPosition.X, 0, GlobalPosition.Z);
			model.CustomAabb = bounds.Grow(displacementMargin / (1000 * scale));
			model.VisibilityRangeEnd = 4800;
			_clouds.Add(cloud);
		}
	}

	private static float DisplacementMargin(ShaderMaterial material, float scale)
	{
		var amplitudes = Mathf.Abs(material.GetShaderParameter("noise_strength_a").AsSingle())
			+ Mathf.Abs(material.GetShaderParameter("noise_strength_b").AsSingle())
			+ Mathf.Abs(material.GetShaderParameter("noise_strength_c").AsSingle());
		var margin = amplitudes * Mathf.Abs(material.GetShaderParameter("procedural_displacement_strength").AsSingle())
			+ 0.78f * Mathf.Abs(material.GetShaderParameter("procedural_vertical_lift").AsSingle());
		return margin * (material.GetShaderParameter("use_reference_space").AsBool() ? scale : 1);
	}
}

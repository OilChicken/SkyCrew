using Godot;
using System.Collections.Generic;

namespace SkyCrew.World;

[Tool]
public partial class CloudField : Node3D
{
    [Export] public PackedScene CloudScene { get; set; } = null!;
    [Export] public int Seed { get; set; } = 128;
<<<<<<< Updated upstream
    [Export] public Vector2 CoverageSize { get; set; } = new(5200, 5200);
    [Export(PropertyHint.Range, "200,2000,10,suffix:m")]
    public float CellSpacing { get; set; } = 720;
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float CellOccupancy { get; set; } = 0.65f;
=======
    [Export] public Vector2 CoverageSize { get; set; } = new(24000, 24000);
    [Export(PropertyHint.Range, "200,2000,10,suffix:m")]
    public float CellSpacing { get; set; } = 850;
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float CellOccupancy { get; set; } = 0.72f;
>>>>>>> Stashed changes
    [Export(PropertyHint.Range, "0,5000,10,suffix:m")]
    public float MinimumAltitude { get; set; } = 450;
    [Export(PropertyHint.Range, "0,2000,10,suffix:m")]
    public float HeightBand { get; set; } = 300;
    [Export] public Vector2 ScaleRange { get; set; } = new(8, 13);
<<<<<<< Updated upstream
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
=======
    [Export(PropertyHint.Range, "1,4096,1")]
    public int MaxClouds { get; set; } = 768;

    [ExportGroup("Visibility")]
    [Export(PropertyHint.Range, "500,12000,100,suffix:m")]
    public float RenderDistance { get; set; } = 3000;
    [Export(PropertyHint.Range, "200,4000,100,suffix:m")]
    public float FadeDistance { get; set; } = 1200;
    [Export(PropertyHint.Range, "200,3000,100,suffix:m")]
    public float PreloadMargin { get; set; } = 600;
    [Export(PropertyHint.Range, "0.2,5,0.1,suffix:s")]
    public float RevealSeconds { get; set; } = 1.2f;

    [ExportToolButton("重新生成云")]
    public Callable Regenerate => Callable.From(Rebuild);

    public int GeneratedCount => _placements.Count;
    public int LoadedCount { get; private set; }

    private sealed class Placement
    {
        public Vector3 Position;
        public float Scale;
        public float Yaw;
        public float Radius;
        public Node3D? Instance;
        public MeshInstance3D? Model;
        public float Reveal;
    }

    private readonly List<Placement> _placements = new();
    private (PackedScene, int, Vector2, float, float, float, float, Vector2, int, Transform3D) _configuration;
    private float _shapeMargin;
    private Mesh? _mesh;
    private ShaderMaterial? _material;

    public override void _Ready()
    {
        Rebuild();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && (_configuration != Configuration()
            || !Mathf.IsEqualApprox(DisplacementMargin(_material, 1), _shapeMargin)))
        {
            Rebuild();
        }
        UpdateVisibility((float)delta);
    }

    private (PackedScene, int, Vector2, float, float, float, float, Vector2, int, Transform3D) Configuration()
    {
        return (CloudScene, Seed, CoverageSize, CellSpacing, CellOccupancy,
            MinimumAltitude, HeightBand, ScaleRange, MaxClouds, GlobalTransform);
    }

    public void Rebuild()
    {
        if (!IsInsideTree())
        {
            return;
        }
        foreach (var placement in _placements)
        {
            Release(placement);
        }
        _placements.Clear();
        LoadedCount = 0;
        _configuration = Configuration();
        _shapeMargin = 0;
        if (CloudScene == null)
        {
            return;
        }

        // Inspect the existing prefab once. All visible instances share these resources.
        var template = CloudScene.Instantiate<Node3D>();
        var model = template.GetNode<MeshInstance3D>("CloudModel");
        _mesh = model.Mesh;
        _material = (ShaderMaterial)model.MaterialOverride;
        var bounds = _mesh.GetAabb();
        var modelOffset = model.Position.Y;
        var modelScale = model.Scale.Y;
        _shapeMargin = DisplacementMargin(_material, 1);
        template.Free();

        var size = new Vector2(Mathf.Max(200, CoverageSize.X), Mathf.Max(200, CoverageSize.Y));
        var spacing = Mathf.Max(200, CellSpacing);
        var columns = Mathf.Clamp((int)(size.X / spacing), 1, 128);
        var rows = Mathf.Clamp((int)(size.Y / spacing), 1, 128);
        var cell = size / new Vector2(columns, rows);
        var minimumScale = Mathf.Max(1, Mathf.Min(ScaleRange.X, ScaleRange.Y));
        var maximumScale = Mathf.Max(minimumScale, Mathf.Max(ScaleRange.X, ScaleRange.Y));
        var random = new RandomNumberGenerator { Seed = unchecked((ulong)Seed) };
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
            if (_placements.Count >= Mathf.Clamp(MaxClouds, 1, 4096))
>>>>>>> Stashed changes
            {
                break;
            }
            if (random.Randf() >= Mathf.Clamp(CellOccupancy, 0, 1))
            {
                continue;
            }
<<<<<<< Updated upstream

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
=======
            var scale = random.RandfRange(minimumScale, maximumScale);
            var yaw = random.RandfRange(-Mathf.Pi, Mathf.Pi);
            var margin = DisplacementMargin(_material, scale);
            var bottomOffset = (modelOffset + bounds.Position.Y * modelScale) * scale;
            var point = new Vector3(
                (index % columns + 0.5f + random.RandfRange(-0.18f, 0.18f)) * cell.X - size.X / 2,
                MinimumAltitude - bottomOffset + margin + random.RandfRange(0, Mathf.Max(0, HeightBand)),
                (index / columns + 0.5f + random.RandfRange(-0.18f, 0.18f)) * cell.Y - size.Y / 2);
            _placements.Add(new Placement
            {
                Position = point + new Vector3(GlobalPosition.X, 0, GlobalPosition.Z),
                Scale = scale,
                Yaw = yaw,
                Radius = bounds.Size.Length() * modelScale * scale / 2 + Mathf.Abs(modelOffset) * scale + margin
            });
        }
        UpdateVisibility(0);
    }

    private void UpdateVisibility(float delta)
    {
        var camera = GetViewport().GetCamera3D();
#if TOOLS
        if (Engine.IsEditorHint())
        {
            camera = EditorInterface.Singleton.GetEditorViewport3D(0)?.GetCamera3D() ?? camera;
        }
#endif
        var eye = camera?.GlobalPosition ?? new Vector3(GlobalPosition.X, 24, GlobalPosition.Z + 1200);
        var start = Mathf.Max(500, RenderDistance);
        var end = start + Mathf.Max(200, FadeDistance);
        var preload = end + Mathf.Max(200, PreloadMargin);
        var creations = 0;
        foreach (var placement in _placements)
        {
            var distance = eye.DistanceTo(placement.Position);
            if (placement.Instance != null && distance > preload + placement.Radius + 400)
            {
                Release(placement);
            }
            if (placement.Instance == null && distance <= preload + placement.Radius && creations < 8)
            {
                Create(placement);
                creations++;
            }
            if (placement.Model == null)
            {
                continue;
            }
            placement.Reveal = Mathf.MoveToward(placement.Reveal, 1, delta / Mathf.Max(0.2f, RevealSeconds));
            placement.Model.SetInstanceShaderParameter("cloud_fade_start", start);
            placement.Model.SetInstanceShaderParameter("cloud_fade_end", end);
            placement.Model.SetInstanceShaderParameter("cloud_spawn_visibility", placement.Reveal);
            placement.Model.VisibilityRangeEnd = preload + placement.Radius + 600;
        }
    }

    private void Create(Placement placement)
    {
        var cloud = CloudScene.Instantiate<Node3D>();
        cloud.Name = $"Cloud{_placements.IndexOf(placement):0000}";
        AddChild(cloud);
        cloud.GlobalBasis = new Basis(Vector3.Up, placement.Yaw).Scaled(Vector3.One * placement.Scale);
        cloud.GlobalPosition = placement.Position;
        placement.Instance = cloud;
        placement.Model = cloud.GetNode<MeshInstance3D>("CloudModel");
        placement.Model.CustomAabb = _mesh!.GetAabb().Grow(DisplacementMargin(_material, placement.Scale) / (1000 * placement.Scale));
        placement.Model.SetInstanceShaderParameter("cloud_spawn_visibility", 0f);
        placement.Reveal = 0;
        LoadedCount++;
    }

    private void Release(Placement placement)
    {
        if (placement.Instance == null)
        {
            return;
        }
        RemoveChild(placement.Instance);
        placement.Instance.QueueFree();
        placement.Instance = null;
        placement.Model = null;
        placement.Reveal = 0;
        LoadedCount--;
    }

    private static float DisplacementMargin(ShaderMaterial? material, float scale)
    {
        if (material == null)
        {
            return 0;
        }
>>>>>>> Stashed changes
        var amplitudes = Mathf.Abs(material.GetShaderParameter("noise_strength_a").AsSingle())
            + Mathf.Abs(material.GetShaderParameter("noise_strength_b").AsSingle())
            + Mathf.Abs(material.GetShaderParameter("noise_strength_c").AsSingle());
        var margin = amplitudes * Mathf.Abs(material.GetShaderParameter("procedural_displacement_strength").AsSingle())
            + 0.78f * Mathf.Abs(material.GetShaderParameter("procedural_vertical_lift").AsSingle());
        return margin * (material.GetShaderParameter("use_reference_space").AsBool() ? scale : 1);
    }
}

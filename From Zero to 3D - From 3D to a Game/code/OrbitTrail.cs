using Godot;
using System.Collections.Generic;

namespace VisA.OrbitWorkshop;

/// <summary>Displays recent sampled rocket positions; optional visual aid.</summary>
public partial class OrbitTrail : MeshInstance3D
{
    [Export] private RocketOrbit _rocket = null!;
    private readonly List<Vector3> _points = new List<Vector3>();
    private readonly ImmediateMesh _mesh = new ImmediateMesh();
    private int _resetVersion = -1;

    public override void _Ready()
    {
        Mesh = _mesh;
        MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.4f, 0.85f, 1.0f)
        };
        CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_rocket == null) return;
        if (_resetVersion != _rocket.ResetVersion)
        {
            _points.Clear();
            _mesh.ClearSurfaces();
            _resetVersion = _rocket.ResetVersion;
        }

        if (!_rocket.IsFlying) return;
        Vector3 point = ToLocal(_rocket.GlobalPosition);
        if (_points.Count > 0 && _points[_points.Count - 1].DistanceSquaredTo(point) < 0.0025f) return;
        _points.Add(point);
        if (_points.Count > 720)
        {
            _points.RemoveAt(0);
        }

        _mesh.ClearSurfaces();
        if (_points.Count < 2) return;
        _mesh.SurfaceBegin(Godot.Mesh.PrimitiveType.Lines);
        for (int i = 1; i < _points.Count; i++)
        {
            _mesh.SurfaceAddVertex(_points[i - 1]);
            _mesh.SurfaceAddVertex(_points[i]);
        }
        _mesh.SurfaceEnd();
    }
}

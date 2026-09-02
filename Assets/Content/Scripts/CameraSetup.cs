using Unity.Cinemachine;
using UnityEngine;

public class CameraSetup
{
    private CinemachineTargetGroup _targetGroup;
    private PixelPaintGrid _pixelPaintGrid;
    private Transform[] _cornerTransforms;

    public void Initialize(CinemachineTargetGroup targetGroup, PixelPaintGrid pixelPaintGrid)
    {
        _targetGroup = targetGroup;
        _pixelPaintGrid = pixelPaintGrid;
        _pixelPaintGrid.OnGridInitialized += () => SetUpCamera(null);
    }

    public void SetUpCamera(Transform parentTransform)
    {
        if (_pixelPaintGrid == null || _targetGroup == null) return;

        Bounds bounds = _pixelPaintGrid.GetBoardBounds();

        if (_cornerTransforms is not { Length: 4 })
        {
            _cornerTransforms = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject obj = new GameObject($"BoardCorner_{i}");
                obj.transform.SetParent(parentTransform);
                _cornerTransforms[i] = obj.transform;
                _targetGroup.AddMember(_cornerTransforms[i], 1f, 0f);
            }
        }

        _cornerTransforms[0].position = new Vector3(bounds.min.x, bounds.center.y, bounds.min.z);
        _cornerTransforms[1].position = new Vector3(bounds.max.x, bounds.center.y, bounds.min.z);
        _cornerTransforms[2].position = new Vector3(bounds.min.x, bounds.center.y, bounds.max.z);
        _cornerTransforms[3].position = new Vector3(bounds.max.x, bounds.center.y, bounds.max.z);
    }
}
using Settings;
using Unity.Cinemachine;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private PixelPaintGrid _pixelPaintGrid;
    [SerializeField] private InputSystem _inputSystem;
    [SerializeField] private GameFeelSettings _gameFeelSettings;
    [SerializeField] private CinemachineTargetGroup _cinemachineTargetGroup;

    private BoardInteractionManager _boardInteractionManager;
    private CameraSetup _cameraSetup;

    private void Awake()
    {
        _boardInteractionManager = new BoardInteractionManager();
        _cameraSetup = new CameraSetup();

        _boardInteractionManager.Initialize(_inputSystem, _pixelPaintGrid, _gameFeelSettings);
        
        _cameraSetup.Initialize(_cinemachineTargetGroup, _pixelPaintGrid);
    }
}
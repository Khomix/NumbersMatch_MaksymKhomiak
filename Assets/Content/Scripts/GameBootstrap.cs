using LandingEffect;
using Settings;
using Unity.Cinemachine;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private PixelPaintGrid _pixelPaintGrid;
    [SerializeField] private InputSystem _inputSystem;
    [SerializeField] private GameFeelSettings _gameFeelSettings;
    [SerializeField] private CinemachineTargetGroup _cinemachineTargetGroup;
    [SerializeField] private LandingEffectView _landingEffectPrefab;
    [SerializeField] private CinemachineImpulseSource _cinemachineImpulseSource;
    [SerializeField] private FeedbackManager _feedbackManager;
    
    
    private BoardInteractionManager _boardInteractionManager;
    private CameraSetup _cameraSetup;
    private LandingEffectService _effectService;
    private RevealController _revealController;

    private void Awake()
    {
        _feedbackManager.Initialize(_cinemachineImpulseSource);
        _boardInteractionManager = new BoardInteractionManager();
        _cameraSetup = new CameraSetup();
        _effectService = new LandingEffectService(_landingEffectPrefab, transform);
        
        _boardInteractionManager.Initialize(_inputSystem, _pixelPaintGrid, _gameFeelSettings, _effectService,_feedbackManager);
        _cameraSetup.Initialize(_cinemachineTargetGroup, _pixelPaintGrid);
    }

    private void Start()
    {
        _revealController = new RevealController(ColorGroupTracker.Instance, _pixelPaintGrid,_feedbackManager);
    }

    private void OnDestroy()
    {
        _revealController?.Dispose();
        _boardInteractionManager?.Dispose();
    }
}
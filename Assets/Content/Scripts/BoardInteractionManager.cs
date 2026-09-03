using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LandingEffect;
using Settings;
using UnityEngine;
using Object = UnityEngine.Object;

public class BoardInteractionManager : IDisposable
{
    private InputSystem _inputSystem;
    private PixelPaintGrid _pixelPaintGrid;
    private GameFeelSettings _gameFeel;
    private LandingEffectService _landingEffectService;
    private FeedbackManager _feedbackManager;

    private Camera _mainCamera;
    private PaintPiece _currentPiece;
    private PaintPiece _hoveredPiece;
    private Vector3 _pieceStartPosition;
    private bool _isFlyingBack;

    public void Initialize(InputSystem inputSystem, PixelPaintGrid pixelPaintGrid, GameFeelSettings gameFeel,
        LandingEffectService landingEffectService, FeedbackManager feedbackManager)
    {
        _inputSystem = inputSystem;
        _pixelPaintGrid = pixelPaintGrid;
        _gameFeel = gameFeel;
        _landingEffectService = landingEffectService;
        _feedbackManager = feedbackManager;

        _inputSystem.OnPieceSelected += OnPieceSelected;
        _inputSystem.OnPieceMoved += OnPieceMoved;
        _inputSystem.OnPieceDropped += OnPieceDropped;

        _mainCamera = Camera.main;
    }

    public void Dispose()
    {
        _inputSystem.OnPieceSelected -= OnPieceSelected;
        _inputSystem.OnPieceMoved -= OnPieceMoved;
        _inputSystem.OnPieceDropped -= OnPieceDropped;
        ClearHoverEffect();
    }

    private void OnPieceSelected(Vector2 screenPosition)
    {
        if (TryGetPiece(_inputSystem.SelectedPiece, out PaintPiece piece))
        {
            if (!piece.IsTrayPiece || piece.IsPlaced) return;

            _currentPiece = piece;
            _pieceStartPosition = _currentPiece.transform.position;
            MovePiece(screenPosition);
            
            _feedbackManager?.Play(FeedbackType.ButtonClick);
        }
    }

    private void OnPieceMoved(Vector2 screenPosition)
    {
        if (_currentPiece == null) return;
        MovePiece(screenPosition);
        UpdateHoverEffect();
    }

    private void OnPieceDropped()
    {
        if (_currentPiece == null) return;

        ClearHoverEffect();

        if (CheckIfCanBePlaced(out PaintPiece targetPiece))
        {
            if (CheckForColor(targetPiece.ColorNumber, _currentPiece.ColorNumber) && !targetPiece.IsOccupied)
            {
                targetPiece.IsOccupied = true;
                _currentPiece.IsPlaced = true;

                _pixelPaintGrid.ReplacePiece(targetPiece, _currentPiece);

                bool willCompleteGroup = ColorGroupTracker.Instance.WillCompleteGroup(_currentPiece.ColorNumber);
                PaintPiece pieceToLand = _currentPiece;

                MoveToPosition(targetPiece.transform.position,
                    () =>
                    {
                        PlayBounceScale(pieceToLand.transform, punchAmount: 0.2f, duration: 0.2f);
                        if (!willCompleteGroup)
                        {
                            _landingEffectService.PlayAsync(pieceToLand).Forget();
                        }
                    }).Forget();

                _feedbackManager?.Play(FeedbackType.PieceLand);

                ColorGroupTracker.Instance.OnPiecePlaced(_currentPiece.ColorNumber);

                _currentPiece.SetPlaced();
    
                Object.Destroy(targetPiece.gameObject);
                _currentPiece = null;
                return;
            }
        }

        _feedbackManager?.Play(FeedbackType.WrongPlacement);
        MoveBackPiece().Forget();
        _currentPiece = null;
    }

    private void MovePiece(Vector2 screenPosition)
    {
        Vector3 targetWorldPosition =
            _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _gameFeel.DragZDepth));
        Vector3 targetWithOffset = targetWorldPosition + _gameFeel.DragOffset;

        _currentPiece.transform.position = Vector3.Lerp(
            _currentPiece.transform.position,
            targetWithOffset,
            Time.deltaTime * _gameFeel.DragSmoothSpeed
        );
    }

    private void UpdateHoverEffect()
    {
        if (CheckIfCanBePlaced(out PaintPiece targetPiece))
        {
            if (_hoveredPiece != targetPiece)
            {
                ClearHoverEffect();
                _hoveredPiece = targetPiece;
                _hoveredPiece.SetTemporaryColor(Color.black);
            }
        }
        else
        {
            ClearHoverEffect();
        }
    }

    private void ClearHoverEffect()
    {
        if (_hoveredPiece != null)
        {
            _hoveredPiece.ResetColor();
            _hoveredPiece = null;
        }
    }

    private bool CheckIfCanBePlaced(out PaintPiece targetPiece)
    {
        Ray ray = new Ray(_currentPiece.transform.position, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != _currentPiece.gameObject)
            {
                if (hit.collider.TryGetComponent(out targetPiece))
                {
                    return true;
                }
            }
        }

        targetPiece = null;
        return false;
    }

    private bool TryGetPiece(GameObject obj, out PaintPiece piece)
    {
        if (obj != null)
        {
            return obj.TryGetComponent(out piece);
        }

        piece = null;
        return false;
    }

    private bool CheckForColor(int color, int color2) => color == color2;

    private async UniTask MoveBackPiece()
    {
        _isFlyingBack = true;
    
        PaintPiece pieceToMove = _currentPiece;
    
        if (pieceToMove)
        {
            await pieceToMove.transform.DOShakePosition(0.2f, 0.2f, 20, 90f).AsyncWaitForCompletion().AsUniTask();

            if (pieceToMove && pieceToMove.transform)
            {
                await pieceToMove.transform.DOMove(_pieceStartPosition, _gameFeel.MoveBackDuration)
                    .SetEase(_gameFeel.MoveBackEase)
                    .AsyncWaitForCompletion().AsUniTask();
            }
        }

        _isFlyingBack = false;
    }

    private async UniTask MoveToPosition(Vector3 position, Action onComplete)
    {
        if (_currentPiece != null)
        {
            await _currentPiece.transform.DOMove(position, _gameFeel.MoveToPositionDuration)
                .SetEase(_gameFeel.MoveToPositionEase)
                .AsyncWaitForCompletion().AsUniTask();

            onComplete?.Invoke();
        }
    }

    public void PlayBounceScale(Transform target, float punchAmount = 0.2f, float duration = 0.3f)
    {
        target.DOPunchScale(Vector3.one * punchAmount, duration, 5, 1f);
    }
}
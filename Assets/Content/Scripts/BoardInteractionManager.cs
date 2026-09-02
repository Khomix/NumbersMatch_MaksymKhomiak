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

    private Camera _mainCamera;
    private PaintPiece _currentPiece;
    private PaintPiece _hoveredPiece;
    private Vector3 _pieceStartPosition;
    private bool _isFlyingBack;

    public void Initialize(InputSystem inputSystem, PixelPaintGrid pixelPaintGrid, GameFeelSettings gameFeel, LandingEffectService landingEffectService)
    {
        _inputSystem = inputSystem;
        _pixelPaintGrid = pixelPaintGrid;
        _gameFeel = gameFeel;
        _landingEffectService = landingEffectService;

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
            _currentPiece = piece;
            _pieceStartPosition = _currentPiece.transform.position;
            MovePiece(screenPosition);
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
                MoveToPosition(targetPiece.transform.position,
                    () =>
                    {
                        PlayBounceScale(_currentPiece.transform, punchAmount: 0.2f, duration: 0.2f);
                        _landingEffectService.PlayAsync(_currentPiece).Forget();
                        _currentPiece = null;
                    }).Forget();

                ColorGroupTracker.Instance.OnPiecePlaced(_currentPiece.ColorNumber);


                _currentPiece.SetPlaced();


                Object.Destroy(targetPiece.gameObject);
                return;
            }
        }

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


        // Vector3 currentPos = _currentPiece.transform.position;
        // float smoothedX = Mathf.Lerp(currentPos.x, targetWithOffset.x, Time.deltaTime * _gameFeel.DragSmoothSpeed);
        //
        // _currentPiece.transform.position = new Vector3(smoothedX, targetWithOffset.y, targetWithOffset.z);
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
        if (_currentPiece != null)
        {
            await _currentPiece.transform.DOMove(_pieceStartPosition, _gameFeel.MoveBackDuration)
                .SetEase(_gameFeel.MoveBackEase)
                .AsyncWaitForCompletion().AsUniTask();
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
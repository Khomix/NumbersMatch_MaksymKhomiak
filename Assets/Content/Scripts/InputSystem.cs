using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    private Camera _mainCamera;
    private GameObject _gameObject;
    private Vector3 _offset;

    public event Action<Vector2> OnPieceSelected;
    public event Action<Vector2> OnPieceMoved;
    public event Action OnPieceDropped;

    public GameObject SelectedPiece => _gameObject;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            var phase = touch.phase.ReadValue();
            Vector2 touchPos = touch.position.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                TrySelectPiece(touchPos);
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Moved && _gameObject != null)
            {
                OnMoved(touchPos);
            }
            else if ((phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled) && _gameObject != null)
            {
                TryRelease();
            }
            return;
        }

        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TrySelectPiece(mousePos);
            }

            if (Mouse.current.leftButton.isPressed && _gameObject != null)
            {
                OnMoved(mousePos);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _gameObject != null)
            {
                TryRelease();
            }
        }
    }

    private void TrySelectPiece(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _gameObject = hit.collider.gameObject;
            OnPieceSelected?.Invoke(screenPosition);
        }
    }

    private void OnMoved(Vector2 screenPosition)
    {
        OnPieceMoved?.Invoke(screenPosition);
    }

    private void TryRelease()
    {
        OnPieceDropped?.Invoke();
        _gameObject = null;
    }
}
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RevealController : IDisposable
{
    private readonly ColorGroupTracker _tracker;
    private readonly PixelPaintGrid _grid;
    private readonly FeedbackManager _feedbackManager;

    public RevealController(ColorGroupTracker tracker, PixelPaintGrid grid, FeedbackManager feedbackManager)
    {
        _tracker = tracker;
        _grid = grid;
        _feedbackManager = feedbackManager;
        _tracker.OnGroupRevealed += HandleGroupRevealed;
        _tracker.OnGameCompleted += HandleGameCompleted;
    }

    private void HandleGroupRevealed(int colorNumber)
    {
        List<PaintPiece> piecesToReveal = _grid.GetPiecesByColor(colorNumber);
        if (piecesToReveal.Count == 0) return;

        Vector3 groupCenter = Vector3.zero;
        foreach (var piece in piecesToReveal)
        {
            groupCenter += piece.transform.position;
        }
        groupCenter /= piecesToReveal.Count;

        float maxDistance = 0f;
        foreach (var piece in piecesToReveal)
        {
            float dist = Vector3.Distance(groupCenter, piece.transform.position);
            if (dist > maxDistance) maxDistance = dist;
        }

        foreach (var piece in piecesToReveal)
        {
            float dist = Vector3.Distance(groupCenter, piece.transform.position);
            float delay = maxDistance > 0 ? (dist / maxDistance) * 0.4f : 0f;
        
            piece.RevealColor(delay);
        }

        _feedbackManager?.Play(FeedbackType.GroupComplete);
    }

    private void HandleGameCompleted()
    {
        List<PaintPiece> boardPieces = _grid.BoardPieces;
        if (boardPieces == null || boardPieces.Count == 0) return;

        Bounds bounds = _grid.GetBoardBounds();
        Vector3 topLeft = new Vector3(bounds.min.x, bounds.center.y, bounds.max.z);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.center.y, bounds.min.z);

        Vector3 diagVector = bottomRight - topLeft;
        float totalDiagLength = diagVector.magnitude;
        Vector3 diagDir = totalDiagLength > 0.001f ? diagVector.normalized : Vector3.forward;

        float waveDuration = 1.0f;

        foreach (var piece in boardPieces)
        {
            if (piece == null) continue;

            Vector3 offset = piece.transform.position - topLeft;
            float projection = Vector3.Dot(offset, diagDir);
            float progress = totalDiagLength > 0f ? Mathf.Clamp01(projection / totalDiagLength) : 0f;
            float delay = progress * waveDuration;

            Vector3 startPos = piece.transform.position;
            piece.transform.DOKill();
            piece.transform.DOJump(startPos, jumpPower: 0.6f, numJumps: 1, duration: 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);
        }
    }

    public void Dispose()
    {
        if (_tracker != null)
        {
            _tracker.OnGroupRevealed -= HandleGroupRevealed;
            _tracker.OnGameCompleted -= HandleGameCompleted;
        }
    }
}
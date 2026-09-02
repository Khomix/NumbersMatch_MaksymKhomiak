using System;
using System.Collections.Generic;
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

    public void Dispose()
    {
        if (_tracker != null)
        {
            _tracker.OnGroupRevealed -= HandleGroupRevealed;
        }
    }
}
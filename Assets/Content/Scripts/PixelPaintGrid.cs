using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PieceColor
{
    public int colorNumber;
    public Color color;
}

public class PixelPaintGrid : MonoBehaviour
{
    private float _pieceSize;
    private int _width;
    private int _height;
    private float _colorTolerance;

    private Texture2D _sourceImage;
    private PaintPiece _paintPiece;
    private List<PieceColor> _colorsList = new();
    private List<PieceColor> _invisibleColorsList = new();
    private readonly List<PaintPiece> _leftoverPieces = new();
    private readonly List<PaintPiece> _boardPieces = new();
    private readonly Dictionary<int, List<PaintPiece>> _piecesByColor = new();
    private float _boardOffsetZ;

    public List<PaintPiece> LeftoverPieces => _leftoverPieces;
    public List<PaintPiece> BoardPieces => _boardPieces;
    public event Action OnGridInitialized;

    void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return new WaitUntil(() => LevelSettings.Instance != null);

        LevelSettings settings = LevelSettings.Instance;

        _sourceImage = settings.GetPaintingSprite();
        _paintPiece = settings.PiecePrefab;
        _width = settings.GridWidth;
        _height = settings.GridHeight;
        _pieceSize = settings.PieceSize;
        _colorTolerance = settings.ColorTolerance;

        if (_sourceImage == null || _paintPiece == null) yield break;

        GeneratePixelGrid();

        int trayRows = Mathf.CeilToInt((float)_invisibleColorsList.Count / _width);
        _boardOffsetZ = (trayRows > 0 ? trayRows + 1 : 0) * _pieceSize;

        foreach (var piece in _boardPieces)
        {
            piece.transform.position += new Vector3(0, 0, _boardOffsetZ);
        }

        SpawnLeftoverPieces();
        OnGridInitialized?.Invoke();
    }

    void GeneratePixelGrid()
    {
        float startX = -(_width - 1) * 0.5f * _pieceSize;

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                Vector3 position = transform.position + new Vector3(startX + x * _pieceSize, 0, z * _pieceSize);
                PaintPiece piece = Instantiate(_paintPiece, position, Quaternion.identity, transform);
                piece.transform.localScale *= _pieceSize;

                Color originalColor = _sourceImage.GetPixelBilinear((float)x / _width, (float)z / _height);
                PieceColor closestPieceColor = GetOrAddClosestColor(originalColor);

                int pieceIndex = x * _height + z;
                piece.Init(closestPieceColor.color, closestPieceColor.colorNumber);

                bool isPieceVisible = LoadVisibilityState(pieceIndex);
                piece.SetMasked();
                piece.SetVisible(isPieceVisible);

                if (!_piecesByColor.ContainsKey(closestPieceColor.colorNumber))
                {
                    _piecesByColor[closestPieceColor.colorNumber] = new List<PaintPiece>();
                }
                _piecesByColor[closestPieceColor.colorNumber].Add(piece);

                if (!isPieceVisible)
                {
                    _invisibleColorsList.Add(closestPieceColor);
                    ColorGroupTracker.Instance.RegisterSlot(closestPieceColor.colorNumber);
                }

                _boardPieces.Add(piece);
            }
        }

        HashSet<int> colorsWithGaps = new HashSet<int>();
        foreach (var invisible in _invisibleColorsList)
        {
            colorsWithGaps.Add(invisible.colorNumber);
        }

        foreach (var pair in _piecesByColor)
        {
            int colorNum = pair.Key;
            if (!colorsWithGaps.Contains(colorNum))
            {
                foreach (var piece in pair.Value)
                {
                    piece.RevealColor(0f);
                }
            }
        }
    }

    void SpawnLeftoverPieces()
    {
        float gridStartX = -(_width - 1) * 0.5f * _pieceSize;
        float startZ = 0f;
        int totalItems = _invisibleColorsList.Count;
        int totalRows = Mathf.CeilToInt((float)totalItems / _width);

        for (int i = 0; i < totalItems; i++)
        {
            PieceColor pieceColor = _invisibleColorsList[i];

            int row = i / _width;
            int col = i % _width;

            int itemsInThisRow = (row == totalRows - 1) ? (totalItems - row * _width) : _width;
            float rowXOffset = (_width - itemsInThisRow) * 0.5f * _pieceSize;

            Vector3 position = transform.position + new Vector3(gridStartX + rowXOffset + col * _pieceSize, 0, startZ + row * _pieceSize);
            PaintPiece piece = Instantiate(_paintPiece, position, Quaternion.identity, transform);
            piece.transform.localScale *= _pieceSize;

            piece.Init(pieceColor.color, pieceColor.colorNumber);
            piece.SetTrayVisible();

            _leftoverPieces.Add(piece);
        }
    }

    PieceColor GetOrAddClosestColor(Color color)
    {
        foreach (PieceColor existingPieceColor in _colorsList)
        {
            if (IsColorSimilar(color, existingPieceColor.color))
                return existingPieceColor;
        }

        PieceColor newPieceColor = new PieceColor { color = color, colorNumber = _colorsList.Count + 1 };
        _colorsList.Add(newPieceColor);
        return newPieceColor;
    }

    bool IsColorSimilar(Color color1, Color color2)
    {
        float diffR = Mathf.Abs(color1.r - color2.r);
        float diffG = Mathf.Abs(color1.g - color2.g);
        float diffB = Mathf.Abs(color1.b - color2.b);

        return (diffR < _colorTolerance) && (diffG < _colorTolerance) && (diffB < _colorTolerance);
    }

    public void Regenerate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        _colorsList.Clear();
        _invisibleColorsList.Clear();
        _leftoverPieces.Clear();
        _boardPieces.Clear();
        _piecesByColor.Clear();
        _boardOffsetZ = 0f;

        StartCoroutine(Init());
    }

    bool LoadVisibilityState(int index)
    {
        string key = $"PieceVisibility_{index}";
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key) == 1;
        }
        else
        {
            bool isVisible = Random.value < LevelSettings.Instance.VisibilityRate;
            SaveVisibilityState(index, isVisible);
            return isVisible;
        }
    }

    public void ReplacePiece(PaintPiece oldPiece, PaintPiece newPiece)
    {
        int boardIndex = _boardPieces.IndexOf(oldPiece);
        if (boardIndex != -1)
        {
            _boardPieces[boardIndex] = newPiece;
        }

        _leftoverPieces.Remove(newPiece);

        if (_piecesByColor.TryGetValue(oldPiece.ColorNumber, out var list))
        {
            int index = list.IndexOf(oldPiece);
            if (index != -1)
            {
                list[index] = newPiece;
            }
        }
    }

    void SaveVisibilityState(int index, bool isVisible)
    {
        string key = $"PieceVisibility_{index}";
        PlayerPrefs.SetInt(key, isVisible ? 1 : 0);
    }

    public Bounds GetBoardBounds()
    {
        Bounds bounds = new Bounds();
        bool hasFirst = false;

        foreach (var piece in _boardPieces)
        {
            if (piece == null) continue;
            if (!hasFirst)
            {
                bounds = new Bounds(piece.transform.position, Vector3.zero);
                hasFirst = true;
            }
            else
            {
                bounds.Encapsulate(piece.transform.position);
            }
        }

        if (!hasFirst) return new Bounds(transform.position, Vector3.one);
        return bounds;
    }

    public Vector3[] GetBoardCorners()
    {
        Bounds bounds = GetBoardBounds();
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 bottomLeft = new Vector3(min.x, min.y, min.z);
        Vector3 bottomRight = new Vector3(max.x, min.y, min.z);
        Vector3 topLeft = new Vector3(min.x, min.y, max.z);
        Vector3 topRight = new Vector3(max.x, min.y, max.z);

        return new Vector3[] { bottomLeft, bottomRight, topLeft, topRight };
    }

    public List<PaintPiece> GetPiecesByColor(int colorNumber)
    {
        if (_piecesByColor.TryGetValue(colorNumber, out var list))
            return list;
        return new List<PaintPiece>();
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
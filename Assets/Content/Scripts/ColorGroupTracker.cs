using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorGroupTracker : MonoBehaviour
{
    public static ColorGroupTracker Instance;

    private Dictionary<int, int> _totalSlotsPerColor = new();
    private Dictionary<int, int> _filledSlotsPerColor = new();

    private int _completedGroups = 0;

    public event Action<int> OnGroupRevealed;
    public event Action OnGameCompleted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterSlot(int colorNumber)
    {
        if (_totalSlotsPerColor.TryAdd(colorNumber, 0))
        {
            _filledSlotsPerColor[colorNumber] = 0;
        }

        _totalSlotsPerColor[colorNumber]++;
    }

    public void OnPiecePlaced(int colorNumber)
    {
        if (_filledSlotsPerColor.ContainsKey(colorNumber))
        {
            _filledSlotsPerColor[colorNumber]++;

            if (_filledSlotsPerColor[colorNumber] >= _totalSlotsPerColor[colorNumber])
            {
                _completedGroups++;
                OnGroupRevealed?.Invoke(colorNumber);

                if (_completedGroups >= _totalSlotsPerColor.Count)
                {
                    OnGameCompleted?.Invoke();
                }
            }
        }
    }

    public bool WillCompleteGroup(int colorNumber)
    {
        if (_filledSlotsPerColor.TryGetValue(colorNumber, out int filled) &&
            _totalSlotsPerColor.TryGetValue(colorNumber, out int total))
        {
            return filled + 1 >= total;
        }
        return false;
    }
}
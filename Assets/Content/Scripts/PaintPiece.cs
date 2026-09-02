using TMPro;
using UnityEngine;

public class PaintPiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer _spriteRendererHighlight;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private Renderer meshRenderer;

    public int ColorNumber;
    public Color OriginalColor;
    public Vector3 TrayOriginalPosition;
    public bool IsPlaced;
    public bool IsTrayPiece;
    public bool IsOccupied;

    private Color _currentColor;

    public void Init(Color color, int colorNumber)
    {
        OriginalColor = color;
        ColorNumber = colorNumber;
        _currentColor = color;

        SetColor(color);
        numberText.text = colorNumber.ToString();
    }

    private void SetColor(Color color)
    {
        meshRenderer.material.color = new Color(color.r, color.g, color.b, 1);
        spriteRenderer.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
        numberText.enabled = !visible;
    }

    public void SetTrayVisible()
    {
        meshRenderer.enabled = true;
        numberText.enabled = true;
    }

    public void SetMasked()
    {
        meshRenderer.material.color = Color.black;
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        meshRenderer.enabled = true;
        numberText.enabled = true;
    }

    public void SetTemporaryColor(Color color)
    {
        
        _spriteRendererHighlight.enabled = true;
        if (meshRenderer != null)
        {
            //meshRenderer.material.color = new Color(color.r, color.g, color.b, 1);
        }
    }

    public void ResetColor()
    {
        _spriteRendererHighlight.enabled = false;
        // if (IsPlaced)
        // {
        //     SetColor(OriginalColor);
        // }
        // else
        // {
        //     SetMasked();
        // }
    }
}
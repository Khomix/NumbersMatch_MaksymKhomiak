using TMPro;
using UnityEngine;
using DG.Tweening;

public class PaintPiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer _spriteRendererHighlight;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private ParticleSystem _revealParticles;

    public int ColorNumber;
    public Color OriginalColor;
    public Vector3 TrayOriginalPosition;
    public bool IsPlaced;
    public bool IsTrayPiece;
    public bool IsOccupied;

    private MaterialPropertyBlock _propBlock;
    
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    public void Init(Color color, int colorNumber)
    {
        OriginalColor = color;
        ColorNumber = colorNumber;
        numberText.text = colorNumber.ToString();

        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(BaseColorID, color);
        
        _propBlock.SetFloat(DissolveAmountID, 1f); 
        meshRenderer.SetPropertyBlock(_propBlock);

        spriteRenderer.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
        numberText.enabled = !visible;
    }

    public void SetPlaced()
    {
        numberText.enabled = false;
    }

    public void SetTrayVisible()
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(DissolveAmountID, 1f); 
        meshRenderer.SetPropertyBlock(_propBlock);

        meshRenderer.enabled = true;
        numberText.enabled = true;
    }

    public void SetMasked()
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(DissolveAmountID, 0f); 
        meshRenderer.SetPropertyBlock(_propBlock);

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        meshRenderer.enabled = true;
        numberText.enabled = true;
    }

    public void SetTemporaryColor(Color color)
    {
        _spriteRendererHighlight.enabled = true;
    }

    public void ResetColor()
    {
        _spriteRendererHighlight.enabled = false;
    }

    public void RevealColor(float delay = 0f)
    {
        numberText.enabled = false;
    
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(_propBlock);

        float dissolveValue = 0f;
    
        DOTween.To(() => dissolveValue, x => 
            {
                dissolveValue = x;
                _propBlock.SetFloat(DissolveAmountID, dissolveValue);
                meshRenderer.SetPropertyBlock(_propBlock);
            }, 1f, 0.6f)
            .SetDelay(delay)
            .SetEase(Ease.OutQuad)
            .OnStart(() => 
            {
                transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 2, 0.5f);
        
                if (_revealParticles)
                {
                    var main = _revealParticles.main;
                    main.startColor = OriginalColor;
                    _revealParticles.Play();
                }
            });
    }
}
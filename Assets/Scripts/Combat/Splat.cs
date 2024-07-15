using System;
using UnityEngine;

public class Splat : MonoBehaviour
{
    public enum SplatLocation
    {
        Foreground,
        Background,
    }

    [SerializeField] private Color _backgroundTint;
    [SerializeField] private float _minSizeMod = 0.8f;
    [SerializeField] private float _maxSizeMod = 1.5f;
    [SerializeField] private Sprite[] _sprites;

    private SplatLocation _location;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(SplatLocation splatLocation)
    {
        _location = splatLocation;
        SetSprite();
        SetSize();
        SetRotation();
        SetLocationProperties();
    }

    private void SetSprite()
    {
        int spriteIndex = UnityEngine.Random.Range(0, _sprites.Length);
        _spriteRenderer.sprite = _sprites[spriteIndex];

    }

    private void SetSize()
    {
        float sizeMod = UnityEngine.Random.Range(_minSizeMod, _maxSizeMod);
        transform.localScale *= sizeMod;
    }

    private void SetRotation()
    {
        float randomRotation = UnityEngine.Random.Range(-360f, 360f);
        transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);
    }

    private void SetLocationProperties()
    {
        switch (_location)
        {
            case SplatLocation.Background:
                _spriteRenderer.color = _backgroundTint;
                _spriteRenderer.sortingOrder = 0;
                break;
            case SplatLocation.Foreground:
                _spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _spriteRenderer.sortingOrder = 2;
                break;
            default:
                break;
        }
    }
}

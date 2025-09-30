using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
    [SerializeField] private Texture2D[] _textures;
    [SerializeField] private float _speed;

    private float _animationTime;
    private int _animationIndex;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _materialPropertyBlock = new();
    }

    private void Update()
    {
        _animationTime += Time.deltaTime;

        if (_animationTime > _speed)
        {
            _animationTime -= _speed;
            _animationIndex = (_animationIndex + 1) % _textures.Length;

            _materialPropertyBlock.SetTexture("_MainTex", _textures[_animationIndex]);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}

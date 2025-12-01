using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class ChunkFX : MonoBehaviour
{
    public Material dissolveMaterial;
    public float DissolveDelay = 5f;
    public float DissolveTime = 2f;
    public float DestroyTime = 11f;
    public ParticleSystem gasPS;

    private Renderer _renderer;
    private Material _material;
    private Material _dissolveMat;

    private ParticleSystem _PS;

    private float _startTime;

    private enum chunkState { fall, dissolve, destroy};
    private chunkState _state;

    private void Start()
    {
        _renderer = GetComponentInParent<Renderer>();
        _PS = transform.Find("Flakes").GetComponent<ParticleSystem>();

        if(!_renderer || !_PS)
        {
            Debug.LogError("Error! Cannot Find Components in ChunkFX!");
            return;
        }

        _startTime = Time.time;
        _state = chunkState.fall;

        transform.parent.AddComponent<ChunkCollision>().gasPS = gasPS;
    }

    float GetBias(float time, float bias)
    {
        return (time / ((((1f / bias) - 2f) * (1f - time)) + 1f));
    }
    private void Update()
    {
        float T = Time.time - _startTime;

        if(_state == chunkState.fall)
        {
            if(T >= DissolveDelay)
            {
                _state = chunkState.dissolve;
                StartDissolve();
                _startTime = Time.time;
                T = 0;
            }
        }
        if(_state == chunkState.dissolve)
        {
            if( T < DissolveTime )
            {
                float t = GetBias(1f - T / DissolveTime, 0.2f);
                _dissolveMat.SetFloat("_AnimTime", t);
            }
            else _dissolveMat.SetFloat("_AnimTime", -1f);
            if (T > DestroyTime)
            {
                _state = chunkState.destroy;
            }
        }
        if(_state == chunkState.destroy )
        {
            Destroy(transform.parent.gameObject);
        }

    }
    public void StartDissolve()
    {
        _material = _renderer.sharedMaterial;
        _dissolveMat = new Material(dissolveMaterial);

        if (_material.HasProperty("_BaseMap"))
        {
            _dissolveMat.SetTexture("_Main_Texture", _material.GetTexture("_BaseMap"));
            _dissolveMat.SetColor("_Main_Color", _material.GetColor("_BaseColor"));
        }
        else if (_material.HasProperty("_MainTex"))
        {
            _dissolveMat.SetTexture("_BaseMap", _material.GetTexture("_MainTex"));
            if (_material.HasProperty("_Color"))
                _dissolveMat.SetColor("_BaseColor", _material.GetColor("_Color"));
        }

        if (_material.HasProperty("_BumpMap"))
        {
            _dissolveMat.SetTexture("_BumpMap", _material.GetTexture("_BumpMap"));
        }

        if (_material.HasProperty("_SpecColor"))
        {
            _dissolveMat.SetColor("_SpecColor", _material.GetColor("_SpecColor"));
        }
        else
        {
            _dissolveMat.SetColor("_SpecColor", Color.black);
        }
        if (_material.HasProperty("_SpecGlossMap"))
        {
            Texture specMap = _material.GetTexture("_SpecGlossMap");
            if (specMap != null)
            {
                _dissolveMat.SetTexture("_SpecGlossMap", specMap);
                _dissolveMat.SetColor("_SpecColor", Color.white);
                if (_material.HasProperty("_GlossMapScale"))
                    _dissolveMat.SetFloat("_Smoothness", _material.GetFloat("_GlossMapScale"));
                else
                    _dissolveMat.SetFloat("_Smoothness", 1.0f);
            }
            else
            {
                TransferSmoothnessValue(_material, _dissolveMat);
            }
        }
        else
        {
            TransferSmoothnessValue(_material, _dissolveMat);
        }

            _renderer.material = _dissolveMat;
        //_PS.Play();

    }
    private void TransferSmoothnessValue(Material source, Material dest)
    {
        float smoothVal = 0.5f;

        if (source.HasProperty("_Glossiness"))
        {
            smoothVal = source.GetFloat("_Glossiness");
        }
        else if (source.HasProperty("_Smoothness"))
        {
            smoothVal = source.GetFloat("_Smoothness");
        }
        dest.SetFloat("_Smoothness", smoothVal);
    }
}

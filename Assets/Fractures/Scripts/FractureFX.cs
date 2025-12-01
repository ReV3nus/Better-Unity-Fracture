using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.ParticleSystem;

public class FractureFX : MonoBehaviour
{
    // Public FX Configurations
    public float _MaxLightMeshHeight = 0.8f;
    public int _MaxLightParticlesCount = 32;


    // Parent Gameobject Component
    private FracturedObject fo;

    // Children Gameobjects
    private GameObject _LightFX;
    private GameObject _LightPS;

    // Light FX
    private Material _lightMat;
    private Mesh _lightMesh;
    private MeshRenderer _lightMeshRenderer;

    // PS
    private GameObject _PS;
    private ParticleSystem _lightPS;
    private ParticleSystem _explosionPS;
    private ParticleSystem _burstPS;
    private ParticleSystem _gasPS;

    // Animation
    private float _startTime;
    public float crackGrowthTime = 1.0f;
    public float crackGrowthDistance = 1.0f;
    public float crackFadeTime = 0.5f;
    enum FXState { Idle, Grow, Fade, Burst, Disintegrate};
    FXState animState;


    public void SetParticleSystems(GameObject PS)
    {
        _PS = PS;
        _lightPS = _PS.transform.Find("LightParticle").GetComponent<ParticleSystem>();
        _explosionPS = _PS.transform.Find("ExplosionParticle").GetComponent<ParticleSystem>();
        _burstPS = _PS.transform.Find("BurstParticle").GetComponent<ParticleSystem>();
        _gasPS = _PS.transform.Find("ExplosionParticle").GetComponent<ParticleSystem>();
        bool checkFlag = _lightPS && _explosionPS &&_burstPS && _gasPS;
        if(!checkFlag)
        {
            Debug.LogError("Error! Are you using the correct Particle System(BUF Particle System)?");
        }
    }
    public void StartLightFX(FracturedObject _fo)
    {
        _startTime = Time.time;
        fo = _fo;
        GenerateLightMesh();
    }

    private void Awake()
    {
        // Load children
        _LightFX = transform.Find("LightFX").gameObject;

        bool checkFlag = _LightFX;
        if(!checkFlag)
        {
            Debug.LogError("Error! Wrong use of FractureFX! Do you use the correct prefab(FractureFXPrefab)?");
            return;
        }

        // Load Components
        _lightMat = _LightFX.GetComponent<MeshRenderer>().material;
        _lightMat.SetFloat("_HeightExtension", _MaxLightMeshHeight);

        animState = FXState.Idle;

    }

    private void Update()
    {
        DoAnimate(Time.time - _startTime);
    }

    private void DoAnimate(float time)
    {
        if (animState == FXState.Idle)
        {
            if (time < 0)
                return;

            Vector3 explodePos = fo.transform.TransformPoint(fo._fracturePosition - fo._fractureDirection * .1f);
            Vector3 explodeDir = fo.transform.TransformDirection(-fo._fractureDirection);
            InstantiateAndPlay(_explosionPS, explodePos, explodeDir);
            //InstantiateAndPlay(_burstPS, explodePos, explodeDir);
            emitAtPoint(_burstPS, explodePos, explodeDir);
            Debug.Log("Explode " + explodePos);

            animState = FXState.Grow;
            StartCoroutine(EmitLightParticles());
        }
        if (animState == FXState.Grow)
        {
            if (time <= crackGrowthTime)
            {
                float t = time / crackGrowthTime;
                _lightMat.SetFloat("_CrackGrowthDistance", t * crackGrowthDistance);
                return;
            }
            else
            {
                animState = FXState.Fade;
                _startTime += time;
                time = 0;
            }
        }
        if (animState == FXState.Fade)
        {
            if (time < crackFadeTime)
            {
                float t = time / crackFadeTime;
                _lightMat.SetFloat("_TotalFade", 1f - t);
                return;
            }
            else
            {
                _lightMat.SetFloat("_TotalFade", 0f);
                animState = FXState.Burst;
            }
        }
        if (animState == FXState.Burst)
        {
            fo.DoFracture();
            StartCoroutine(DestroyAnimation(5, 5));
            animState = FXState.Disintegrate;
        }

    }

    private void GenerateLightMesh()
    {
        _lightMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        int vertIndex = 0;

        foreach (var edge in fo._cracks)
        {
            Vector3 p0 = edge.sp + edge.sn * 0.01f;
            Vector3 p1 = edge.ep + edge.en * 0.01f;
            Vector3 p2 = p0 + edge.sn * 0.1f;
            Vector3 p3 = p1 + edge.en * 0.1f;

            //Vector3 offset = (edge.ep - edge.sp).normalized * _MaxLightMeshHeight * 0.5f;
            //p2 -= offset;
            //p3 += offset;

            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            uvs.Add(new Vector2(edge.sd, 0));
            uvs.Add(new Vector2(edge.ed, 0));
            uvs.Add(new Vector2(edge.sd, 1));
            uvs.Add(new Vector2(edge.ed, 1));

            normals.Add(edge.sn);
            normals.Add(edge.en);
            normals.Add(edge.sn);
            normals.Add(edge.en);

            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 1);

            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 3);

            vertIndex += 4;
        }

        _lightMesh.SetVertices(vertices);
        _lightMesh.SetNormals(normals);
        _lightMesh.SetUVs(0, uvs);
        _lightMesh.SetTriangles(triangles, 0);
        _lightMesh.RecalculateBounds();

        var _lightMeshFilter = _LightFX.GetComponent<MeshFilter>();
        _lightMeshRenderer = _LightFX.GetComponent<MeshRenderer>();

        _lightMeshFilter.mesh = _lightMesh;
    }

    private void InstantiateAndPlay(ParticleSystem ps, Vector3 pos, Vector3 norm)
    {
        GameObject vfxInstance = Instantiate(ps.gameObject, pos, Quaternion.LookRotation(norm));

        var controller = vfxInstance.GetComponent<ParticleSystem>();
        controller.Play();
    }
    private void emitAtPoint(ParticleSystem ps, Vector3 p, Vector3 n, int num = 1)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        emitParams.velocity = n * 0.001f;
        emitParams.position = p;

        ps.Emit(emitParams, num);
    }


    private IEnumerator EmitLightParticles()
    {
        int emitParticlePerCrack = fo._crackCount / _MaxLightParticlesCount;
        float emitItg = crackGrowthTime / _MaxLightParticlesCount;

        var mat = fo.transform.localToWorldMatrix;
        var rot = fo.transform.rotation;

        int idx = 0;
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        float globalScale = fo.transform.lossyScale.x;

        float baseSize = Random.Range(0.10f, 0.15f);
        float baseVelocity = Random.Range(0.7f, 1.3f);
        emitParams.startSize = baseSize * fo.transform.lossyScale.x;

        var emission = _lightPS.emission;
        emission.rateOverTime = 0f;

        var shape = _lightPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.mesh = _lightMesh;

        shape.meshShapeType = ParticleSystemMeshShapeType.Edge;

        while (idx < fo._crackCount)
        {
            SimpleCrackEdge edge = fo._cracks[idx];
            float t = Random.Range(0, 1f);
            Vector3 p = Vector3.Lerp(edge.sp, edge.ep, t);
            Vector3 n = Vector3.Lerp(edge.sn, edge.en, t);

            p = fo.transform.TransformPoint(p);
            n = fo.transform.TransformDirection(n).normalized;
            p += n * globalScale * 0.5f;

            Quaternion lookRot = Quaternion.FromToRotation(Vector3.up, n);

            emitParams.rotation3D = lookRot.eulerAngles;
            emitParams.position = p;
            emitParams.velocity = n * baseVelocity;

            _lightPS.Emit(emitParams, 1);

            idx += emitParticlePerCrack;

            yield return new WaitForSeconds(emitItg);
        }
    }
    private IEnumerator DestroyAnimation(int batchNum = 5, int batchSize = 10)
    {
        GameObject vfxInstance = Instantiate(_burstPS.gameObject, Vector3.zero, Quaternion.identity);
        ParticleSystem burstPsInstance = vfxInstance.GetComponent<ParticleSystem>();

        var emission = burstPsInstance.emission;
        emission.rateOverTime = 10f;

        var shape = burstPsInstance.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.MeshRenderer;
        shape.meshRenderer = _lightMeshRenderer;
        shape.useMeshColors = false;
        shape.meshShapeType = ParticleSystemMeshShapeType.Edge;

        for(int i = 0; i < batchNum; i++)
        {
            burstPsInstance.Emit(batchSize);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(2f);
        Destroy(vfxInstance);
    }
}

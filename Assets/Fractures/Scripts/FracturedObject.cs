using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class FracturedObject : MonoBehaviour
{
    public Mesh _mesh;
    public NvMesh _nvMesh;

    public int _crackCount;
    public List<SimpleCrackEdge> _cracks;

    public int _chunkCount;
    public List<Mesh> _chunkMeshes;

    public Vector3 _fracturePosition;
    public Vector3 _fractureDirection;
    public float _fractureForce;
    public float _fractureRadius;

    private GameObject bufFXSystems;
    private GameObject chunkFXPrefab;
    private Material interiorMaterial;

    private bool __debug = false;

    void OnDrawGizmos()
    {
        if (!__debug) return;
        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        for (int i = 0; i < _crackCount; i++)
        {
            Gizmos.DrawLine(_cracks[i].sp, _cracks[i].ep);
        }
        //Gizmos.DrawSphere(transform.position + sites[i], 0.01f);
    }
    public void SetupForFracture(Vector3 pos, Vector3 dir, float force, float radius)
    {
        _fracturePosition = pos;
        _fractureDirection = dir;
        _fractureForce = force;
        _fractureRadius = radius;

        _mesh = GetLocalMesh(this.gameObject);

        if(_nvMesh != null)
            _nvMesh.Dispose();

        _nvMesh = new NvMesh(
                _mesh.vertices,
                _mesh.normals,
                _mesh.uv,
                _mesh.vertexCount,
                _mesh.GetIndices(0),
                (int)_mesh.GetIndexCount(0)
            );
    }

    public void GetAndCompositeCracks(NvFractureTool tool, Vector3 center = new Vector3(), bool doComposite = true)
    {
        int cntC = tool.getCracksCount();
        _cracks = new List<SimpleCrackEdge>();

        NvVertex[] vertices = new NvVertex[cntC << 1];

        tool.getCrackVertices(vertices);
        for (int i = 0; i < cntC; i++)
        {
            SimpleCrackEdge edge = new SimpleCrackEdge(vertices[i << 1].p,
                                             vertices[(i << 1) + 1].p,
                                             vertices[i << 1].n,
                                             vertices[(i << 1) + 1].n);
            edge.SwapIfNeeded(center);
            _cracks.Add(edge);
        }

        _cracks.Sort((a, b) => a.CompareTo(b, center));

        var endNodeMap = new Dictionary<Vector3Int, List<int>>();
        float precision = 100000f;
        Vector3Int GetHashKey(Vector3 v) => new Vector3Int(
            Mathf.RoundToInt(v.x * precision),
            Mathf.RoundToInt(v.y * precision),
            Mathf.RoundToInt(v.z * precision)
        );
        var endKey0 = GetHashKey(_cracks[0].ep);
        endNodeMap[endKey0] = new List<int>();
        endNodeMap[endKey0].Add(0);

        int idx = 1;
        while (idx < _cracks.Count)
        {
            if (_cracks[idx].sp.EpsEqual(_cracks[idx].ep) || _cracks[idx].EpsEqual(_cracks[idx - 1]))
            {
                _cracks.RemoveAt(idx);
            }
            else
            {
                if(!doComposite)
                {
                    idx++;
                    continue;
                }

                bool flag = false;
                var startKey = GetHashKey(_cracks[idx].sp);
                if (endNodeMap.TryGetValue(startKey, out List<int> candidates))
                {
                    for (int k = candidates.Count - 1; k >= 0; k--)
                    {
                        int jdx = candidates[k];
                        SimpleCrackEdge ce = _cracks[jdx];
                        if (ce.ConnectTo(_cracks[idx]))
                        {
                            flag = true;
                            SimpleCrackEdge newEdge = _cracks[jdx];
                            (newEdge.ep, newEdge.en) = (_cracks[idx].ep, _cracks[idx].en);
                            _cracks[jdx] = newEdge;

                            candidates.RemoveAt(k);
                            var endKey = GetHashKey(_cracks[idx].ep);
                            if(!endNodeMap.ContainsKey(endKey))
                            {
                                endNodeMap[endKey] = new List<int>();
                            }
                            endNodeMap[endKey].Add(jdx);

                            break;
                        }
                    }
                }
                if(!flag)
                {
                    var endKey = GetHashKey(_cracks[idx].ep);
                    if (!endNodeMap.ContainsKey(endKey))
                    {
                        endNodeMap[endKey] = new List<int>();
                    }
                    endNodeMap[endKey].Add(idx);
                    idx++;
                }
            }
        }
        _crackCount = _cracks.Count;
    }

    public void GetAndExtractChunkMeshes(NvFractureTool tool)
    {
        _chunkCount = tool.getChunkCount() - 1;
        _chunkMeshes = new List<Mesh>(_chunkCount);

        for (var i = 0; i < _chunkCount; i++)
        {
            _chunkMeshes.Add(ExtractChunkMesh(tool, i + 1));
        }
    }

    public void SetInteriorMaterial(Material mat)
    {
        interiorMaterial = mat;
    }

    public void StartFracture(GameObject FX, GameObject sys)
    {
        if (!FX)
        {
            Debug.LogError("Error! Cannot get correct FX prefab in Fractured Object! Is this object created by BetterFracture?");
            return;
        }
        FX.SetActive(true);
        var sfx = FX.GetComponent<FractureFX>();

        bufFXSystems = sys;
        chunkFXPrefab = bufFXSystems.transform.Find("ChunkFXPrefab").gameObject;

        sfx.SetParticleSystems(bufFXSystems);
        sfx.StartLightFX(this);
    }
    public void DoFracture()
    {
        var obj = this.gameObject;
        var mat = obj.GetComponent<MeshRenderer>()?.sharedMaterial;

        obj.GetComponent<MeshRenderer>().enabled = false;
        var collider = obj.GetComponent<Collider>();
        if (collider) collider.enabled = false;

        if (!interiorMaterial)
        {
            interiorMaterial = mat;
        }

        for (int i = 0; i < _chunkCount; i++)
        {
            Mesh chunkMesh = _chunkMeshes[i];
            if (chunkMesh == null) continue;

            var chunkObj = new GameObject(obj.name + "_chunk_" + i);
            chunkObj.transform.SetParent(obj.transform, false);

            var mf = chunkObj.AddComponent<MeshFilter>();
            var mr = chunkObj.AddComponent<MeshRenderer>();
            mf.sharedMesh = chunkMesh;
            mr.sharedMaterials = new[]
            {
                //mat,
                mat
            };

            var rb = chunkObj.AddComponent<Rigidbody>();
            rb.mass = 1.0f;

            var _collider = chunkObj.AddComponent<MeshCollider>();
            _collider.sharedMesh = chunkMesh;
            _collider.convex = true;


            GameObject fxInstance = Instantiate(chunkFXPrefab, chunkObj.transform);
            fxInstance.GetComponent<ChunkFX>().gasPS = bufFXSystems.transform.Find("GasParticle").GetComponent<ParticleSystem>();
            fxInstance.transform.localPosition = Vector3.zero;
            fxInstance.transform.localRotation = Quaternion.identity;
            fxInstance.transform.localScale = Vector3.one;
            fxInstance.SetActive(true);

            rb.AddExplosionForce(_fractureForce, _fracturePosition, _fractureRadius, 1.0f, ForceMode.Impulse);
        }

    }


    private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
    {
        var outside = fractureTool.getChunkMesh(index, false);
        var inside = fractureTool.getChunkMesh(index, true);
        var chunkMesh = outside.toUnityMesh();
        chunkMesh.subMeshCount = 2;
        chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return chunkMesh;
    }
    private static Mesh GetLocalMesh(GameObject gameObject)
    {
        var combineInstances = gameObject
            .GetComponentsInChildren<MeshFilter>()
            .Where(mf => ValidateMesh(mf.mesh))
            .Select(mf => new CombineInstance()
            {
                mesh = mf.mesh,
                transform = Matrix4x4.identity,
            }).ToArray();

        var totalMesh = new Mesh();
        totalMesh.CombineMeshes(combineInstances, true);
        return totalMesh;
    }
    private static Mesh GetWorldMesh(GameObject gameObject)
    {
        var combineInstances = gameObject
            .GetComponentsInChildren<MeshFilter>()
            .Where(mf => ValidateMesh(mf.mesh))
            .Select(mf => new CombineInstance()
            {
                mesh = mf.mesh,
                transform = mf.transform.localToWorldMatrix
            }).ToArray();

        var totalMesh = new Mesh();
        totalMesh.CombineMeshes(combineInstances, true);
        return totalMesh;
    }
    private static bool ValidateMesh(Mesh mesh)
    {
        if (mesh.isReadable == false)
        {
            Debug.LogError($"Mesh [{mesh}] has to be readable.");
            return false;
        }

        if (mesh.vertices == null || mesh.vertices.Length == 0)
        {
            Debug.LogError($"Mesh [{mesh}] does not have any vertices.");
            return false;
        }

        if (mesh.uv == null || mesh.uv.Length == 0)
        {
            Debug.LogError($"Mesh [{mesh}] does not have any uvs.");
            return false;
        }

        return true;
    }
}

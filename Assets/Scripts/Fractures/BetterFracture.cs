using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class BetterFracture : MonoBehaviour
{
    public void Start()
    {
        FractureObject(gameObject);
    }

    public void FractureObject(GameObject obj)
    {
        if (obj == null)
            obj = gameObject;

        Mesh mesh = GetWorldMesh(obj);

        NvBlastUnityExtWrapper.setSeed(123);

        NvMesh nvMesh = new NvMesh(
                mesh.vertices,
                mesh.normals,
                mesh.uv,
                mesh.vertexCount,
                mesh.GetIndices(0),
                (int)mesh.GetIndexCount(0)
            );

        var fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(50);
        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        // Extract meshes
        var meshCount = fractureTool.getChunkCount();
        var meshes = new List<Mesh>(fractureTool.getChunkCount());
        for (var i = 1; i < meshCount; i++)
        {
            meshes.Add(ExtractChunkMesh(fractureTool, i));
        }


        var mat = obj.GetComponent<MeshRenderer>()?.sharedMaterial;

        for (int i = 0; i < meshes.Count; i++)
        {
            Mesh chunkMesh = meshes[i];
            if (chunkMesh == null) continue;

            var chunkObj = new GameObject(obj.name + "_chunk_" + i);
            chunkObj.transform.SetParent(obj.transform, false);

            var mf = chunkObj.AddComponent<MeshFilter>();
            var mr = chunkObj.AddComponent<MeshRenderer>();
            mf.sharedMesh = chunkMesh;
            mr.sharedMaterials = new[]
            {
                mat,
                mat
            };

            var rb = chunkObj.AddComponent<Rigidbody>();
            rb.mass = 1.0f;

            var collider = chunkObj.AddComponent<MeshCollider>();
            collider.sharedMesh = chunkMesh;
            collider.convex = true;

            // ø…—°£∫…‘Œ¢∆´“∆ªÚÃÌº”±¨’®¡¶£¨»√ÀÈøÈ∑……¢
            // rb.AddExplosionForce(50f, original.transform.position, 5f);
        }

        //obj.SetActive(false);

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;


[StructLayout(LayoutKind.Sequential)]
public struct NoiseConfiguration
{
    public float amplitude;//0 - disabled
    public float frequency;//:1
    public int octaveNumber;//:1
    public int surfaceResolution;//:1
};

[StructLayout(LayoutKind.Sequential)]
public struct SlicingConfiguration
{
    public Vector3Int slices;
    public float offset_variations;//0-1:0
    public float angle_variations;//0-1:0
    public NoiseConfiguration noise;
};

public class NvMesh : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;


    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshRelease(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetVertices(IntPtr mesh, [In, Out] Vector3[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetNormals(IntPtr mesh, [In, Out] Vector3[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetIndexes(IntPtr mesh, [In, Out] int[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetUVs(IntPtr mesh, [In, Out] Vector2[] arr);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtMeshGetVerticeCount(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtMeshGetIndexCount(IntPtr mesh);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount);

    public NvMesh(IntPtr mesh)
    {
        Initialize(mesh);
    }

    public NvMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount)
    {
        Initialize(NvBlastExtAuthoringCreateMesh(positions, normals, uv, verticesCount, indices, indicesCount));
    }

    public Vector3[] getVertices()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        NvBlastUnityExtMeshGetVertices(this.ptr, v);
        return v;
    }
    public Vector3[] getNormals()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        NvBlastUnityExtMeshGetNormals(this.ptr, v);
        return v;
    }
    public Vector2[] getUVs()
    {
        Vector2[] v = new Vector2[getVerticesCount()];
        NvBlastUnityExtMeshGetUVs(this.ptr, v);
        return v;
    }
    public int[] getIndexes()
    {
        int[] v = new int[getIndexesCount()];
        NvBlastUnityExtMeshGetIndexes(this.ptr, v);
        return v;
    }

    public int getVerticesCount()
    {
        return NvBlastUnityExtMeshGetVerticeCount(this.ptr);
    }

    public int getIndexesCount()
    {
        return NvBlastUnityExtMeshGetIndexCount(this.ptr);
    }

    protected override void Release()
    {
        NvBlastUnityExtMeshRelease(this.ptr);
    }

    //Unity Helper Functions
    public Mesh toUnityMesh()
    {
        Mesh m = new Mesh();
        m.vertices = getVertices();
        m.normals = getNormals();
        m.uv = getUVs();
        m.SetIndices(getIndexes(), MeshTopology.Triangles, 0, true);
        return m;
    }
}

public class NvMeshCleaner : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshCleanerRelease(IntPtr cleaner);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtMeshCleanerCleanMesh(IntPtr cleaner, IntPtr mesh);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateMeshCleaner();

    public NvMeshCleaner()
    {
        Initialize(NvBlastExtAuthoringCreateMeshCleaner());
    }

    public NvMesh cleanMesh(NvMesh mesh)
    {
        return new NvMesh(NvBlastUnityExtMeshCleanerCleanMesh(this.ptr, mesh.ptr));
    }

    protected override void Release()
    {
        NvBlastUnityExtMeshCleanerRelease(this.ptr);
    }
}

public class NvFractureTool : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolRelease(IntPtr tool);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateFractureTool();

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolSetSourceMesh(IntPtr tool, IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolSetRemoveIslands(IntPtr tool, bool remove);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolVoronoiFracturing(IntPtr tool, int chunkId, IntPtr vsg);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolSlicing(IntPtr tool, int chunkId, [Out] SlicingConfiguration conf, bool replaceChunk);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolFinalizeFracturing(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetChunkCount(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtFractureToolGetChunkMesh(IntPtr tool, int chunkId, bool inside);

    public NvFractureTool()
    {
        Initialize(NvBlastExtAuthoringCreateFractureTool());
    }

    public void setSourceMesh(NvMesh mesh)
    {
        NvBlastUnityExtFractureToolSetSourceMesh(this.ptr, mesh.ptr);
    }

    public void setRemoveIslands(bool remove)
    {
        NvBlastUnityExtFractureToolSetRemoveIslands(this.ptr, remove);
    }

    public bool voronoiFracturing(int chunkId, NvVoronoiSitesGenerator vsg)
    {
        return NvBlastUnityExtFractureToolVoronoiFracturing(this.ptr, chunkId, vsg.ptr);
    }

    public bool slicing(int chunkId, SlicingConfiguration conf, bool replaceChunk)
    {
        return NvBlastUnityExtFractureToolSlicing(this.ptr, chunkId, conf, replaceChunk);
    }

    public void finalizeFracturing()
    {
        NvBlastUnityExtFractureToolFinalizeFracturing(this.ptr);
    }

    public int getChunkCount()
    {
        return NvBlastUnityExtFractureToolGetChunkCount(this.ptr);
    }

    public NvMesh getChunkMesh(int chunkId, bool inside)
    {
        return new NvMesh(NvBlastUnityExtFractureToolGetChunkMesh(this.ptr, chunkId, inside));
    }

    protected override void Release()
    {
        NvBlastUnityExtFractureToolRelease(this.ptr);
    }
}

public class NvVoronoiSitesGenerator : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGRelease(IntPtr site);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGCreate(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(IntPtr tool, int count);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGAddSite(IntPtr tool, [In] Vector3 site);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtVSGClusteredSitesGeneration(IntPtr tool, int numberOfClusters, int sitesPerCluster, float clusterRadius);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtVSGGetSitesCount(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGGetSites(IntPtr tool, [In, Out] Vector3[] arr);

    public NvVoronoiSitesGenerator(NvMesh mesh)
    {
        Initialize(NvBlastUnityExtVSGCreate(mesh.ptr));
    }

    public void uniformlyGenerateSitesInMesh(int count)
    {
        NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(this.ptr, count);
    }

    public void addSite(Vector3 site)
    {
        NvBlastUnityExtVSGAddSite(this.ptr, site);
    }

    public void clusteredSitesGeneration(int numberOfClusters, int sitesPerCluster, float clusterRadius)
    {
        NvBlastUnityExtVSGClusteredSitesGeneration(this.ptr, numberOfClusters, sitesPerCluster, clusterRadius);
    }

    public Vector3[] getSites()
    {
        Vector3[] v = new Vector3[getSitesCount()];
        NvBlastUnityExtVSGGetSites(this.ptr, v);
        return v;
    }

    public int getSitesCount()
    {
        return NvBlastUnityExtVSGGetSitesCount(this.ptr);
    }

    protected override void Release()
    {
        NvBlastUnityExtVSGRelease(this.ptr);
    }

    //Unity Specific
    public void boneSiteGeneration(SkinnedMeshRenderer smr)
    {
        if (smr == null)
        {
            Debug.Log("No Skinned Mesh Renderer");
            return;
        }

        Animator anim = smr.transform.root.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.Log("Missing Animator");
            return;
        }

        if (anim.GetBoneTransform(HumanBodyBones.Head)) addSite(anim.GetBoneTransform(HumanBodyBones.Head).position);
        if (anim.GetBoneTransform(HumanBodyBones.Neck)) addSite(anim.GetBoneTransform(HumanBodyBones.Neck).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.RightShoulder).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftHand)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftHand).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightHand)) addSite(anim.GetBoneTransform(HumanBodyBones.RightHand).position);

        if (anim.GetBoneTransform(HumanBodyBones.Chest)) addSite(anim.GetBoneTransform(HumanBodyBones.Chest).position);
        if (anim.GetBoneTransform(HumanBodyBones.Spine)) addSite(anim.GetBoneTransform(HumanBodyBones.Spine).position);
        if (anim.GetBoneTransform(HumanBodyBones.Hips)) addSite(anim.GetBoneTransform(HumanBodyBones.Hips).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.RightFoot).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftEye)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftEye).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightEye)) addSite(anim.GetBoneTransform(HumanBodyBones.RightEye).position);
    }
}

public class NvBlastUnityExtWrapper
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;


    [DllImport(DLL_NAME)]
    public static extern void setSeed(int seed);

    [DllImport(DLL_NAME)]
    public static extern int getDebugInfo();


}

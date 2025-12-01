using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class BetterFracture : MonoBehaviour
{
    Vector3[] lines;
    int Nlines;

    public GameObject bufFXSystems;
    public Material InterialMaterial;

    public float fractureForce = 50f;
    public float fractureRadius = 1f;

    private GameObject fractureFXPrefab;
    private GameObject chunkFXPrefab;

    private NvFractureTool _tool;
    private NvVoronoiSitesGenerator _vsg;

    private Vector3 _fracturePosition = new Vector3(0.5f, 0f, 0f);
    private Vector3 _fractureDirection = new Vector3(-1f, 0f, 0f);

    public void Start()
    {
        fractureFXPrefab = bufFXSystems.transform.Find("FractureFXPrefab").gameObject;
        chunkFXPrefab = bufFXSystems.transform.Find("ChunkFXPrefab").gameObject;
        if(!fractureFXPrefab || !chunkFXPrefab)
        {
            Debug.LogError("Error! Unknown BUF FX Systems Prefab.");
        }
        //FractureObject(this.gameObject);
    }
    public void SetFractureInfo(Vector3 pos, Vector3 norm)
    {
        _fracturePosition = pos;
        _fractureDirection = norm;
    }
    public void FractureObject(GameObject obj)
    {
        if (obj == null)
            obj = gameObject;
        if (obj.GetComponent<FracturedObject>() == null)
            obj.AddComponent<FracturedObject>();

        FracturedObject fo = obj.GetComponent<FracturedObject>();
        fo.SetupForFracture(_fracturePosition, _fractureDirection, fractureForce, fractureRadius);

        NvBlastUnityExtWrapper.setSeed((int)(Time.time * 1000));

        if (_tool != null)
            _tool.Dispose();
        _tool = new NvFractureTool();
        _tool.setRemoveIslands(false);
        _tool.setSourceMesh(fo._nvMesh);

        if (_vsg != null)
            _vsg.Dispose();
        _vsg = new NvVoronoiSitesGenerator(fo._nvMesh);

        _vsg.blastPattern(BUFutils.CreateSimpleBlastConf(_fracturePosition, _fractureDirection));

        _tool.voronoiFracturing(0, _vsg);
        _tool.finalizeFracturing();

        fo.GetAndCompositeCracks(_tool, _fracturePosition, true);
        fo.GetAndExtractChunkMeshes(_tool);

        GameObject fxInstance = Instantiate(fractureFXPrefab, fo.transform); 
        fxInstance.transform.localPosition = Vector3.zero;
        fxInstance.transform.localRotation = Quaternion.identity;
        fxInstance.transform.localScale = Vector3.one;
        fxInstance.SetActive(false);

        fo.SetInteriorMaterial(InterialMaterial);
        fo.StartFracture(fxInstance, bufFXSystems);
    }
}

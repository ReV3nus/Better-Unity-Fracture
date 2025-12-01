using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 0.1f;

    public LineRenderer beamLine;
    public float beamDuration = 0.1f;

    public BetterFracture fracturer;
    public LayerMask targetLayers;

    bool flag = false;
    private float _xRotation = 0f;
    private float _yRotation = 0f;

    private void Start()
    {
        fracturer = GetComponent<BetterFracture>();
        beamLine = GetComponent<LineRenderer>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //ManualFracture();
    }

    private void ManualFracture()
    {
        fracturer.SetFractureInfo(new Vector3(0.5f, 0f, 0f), new Vector3(-1f, 0f, 0f));
        fracturer.FractureObject(GameObject.Find("Cube"));
        flag = true;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        HandleMouseLook(mouse);
        if (flag) return;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            //Debug.Log("PRESS");
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;


            if (Physics.Raycast(rayOrigin, cam.transform.forward, out hit, 1000f, targetLayers))
            {
                StartCoroutine(FireBeam(cam.ViewportToWorldPoint(new Vector3(0.9f, 0.1f, cam.nearClipPlane)), hit.point));
                Transform hitTransform = hit.collider.transform;
                Vector3 pos = hit.point;
                Vector3 nor = hit.point - cam.transform.position;

                pos = hitTransform.InverseTransformPoint(pos);
                nor = nor.normalized;

                Debug.Log("Hit : "+ pos + " | " + nor);
                fracturer.SetFractureInfo(pos, nor);
                fracturer.FractureObject(hit.collider.gameObject);
                flag = true;
            }
            else
            {
                StartCoroutine(FireBeam(cam.ViewportToWorldPoint(new Vector3(0.9f, 0.1f, cam.nearClipPlane)), rayOrigin + 5f * cam.transform.forward));
            }
        }
    }
    void HandleMouseLook(Mouse mouse)
    {
        Vector2 mouseDelta = mouse.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -89f, 89f); 
        _yRotation += mouseX;
        Camera.main.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
    private IEnumerator FireBeam(Vector3 startPos, Vector3 endPos)
    {
        beamLine.enabled = true;

        beamLine.SetPosition(0, startPos);
        beamLine.SetPosition(1, endPos);

        yield return new WaitForSeconds(beamDuration);

        beamLine.enabled = false;
    }
}
    
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderData;

public class ChunkCollision : MonoBehaviour
{
    public ParticleSystem gasPS;

    private float _lastSoundTime;
    private const float COOLDOWN = 0.1f;
    private void Start()
    {
        if (!GetComponent<Collider>())
            transform.AddComponent<MeshCollider>();
    }
    void emitAtPoint(ParticleSystem ps, Vector3 p, Vector3 n, int num = 3)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        emitParams.velocity = n * 0.001f;
        emitParams.position = p;

        ps.Emit(emitParams, num);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (Time.time - _lastSoundTime < COOLDOWN) return;

        float impactForce = collision.relativeVelocity.sqrMagnitude;
        if (impactForce > 0.1f)
        {
            ContactPoint contact = collision.GetContact(0);
            emitAtPoint(gasPS, contact.point + contact.normal * 0.01f, contact.normal);
            _lastSoundTime = Time.time;
        }
    }
}

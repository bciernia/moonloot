using System.Collections;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    public ParticleSystem bloodEffect;

    public void SpawnBlood(Vector3 hitPoint, Vector3 attackerPosition)
    {
        var direction = (hitPoint - attackerPosition).normalized;
        bloodEffect.transform.position = hitPoint;
        bloodEffect.transform.rotation = Quaternion.LookRotation(direction);
        bloodEffect.Play();

        StartCoroutine(DestroyAfterParticles());
    }

    private IEnumerator DestroyAfterParticles()
    {
        yield return new WaitWhile(() => bloodEffect.IsAlive(true));
        
        Destroy(gameObject);
    }
}
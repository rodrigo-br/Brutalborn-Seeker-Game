using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem _splatParticles;
    [SerializeField] private GameObject _splatPrefab;
    [SerializeField] private Transform _splatHolder;
    [SerializeField] private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(_splatParticles, other, _collisionEvents);

        int count = _collisionEvents.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject splat = Instantiate(_splatPrefab, _collisionEvents[i].intersection, Quaternion.identity) as GameObject;
            splat.transform.SetParent(_splatHolder, true);
            Splat splatScript = splat.GetComponent<Splat>();
            splatScript.Init(Splat.SplatLocation.Foreground);
        }
    }
}

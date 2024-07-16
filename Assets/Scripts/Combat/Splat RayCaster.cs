using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatRayCaster : MonoBehaviour
{
    [SerializeField] private ParticleSystem _splatParticles;
    [SerializeField] private GameObject _splatPrefab;
    [SerializeField] private Transform _splatHolder;

    private void OnEnable()
    {
        Health.OnDeath += CastRay;
    }

    private void OnDisable()
    {
        Health.OnDeath -= CastRay;
    }

    [Button]
    private void CastRay(Health sender)
    {
        GameObject splat = Instantiate(_splatPrefab, sender.transform.position, Quaternion.identity) as GameObject;
        splat.transform.SetParent(_splatHolder, true);
        Splat _splatScript = splat.GetComponent<Splat>();

        _splatParticles.transform.position = sender.transform.position;
        _splatParticles.Play();

        _splatScript.Init(Splat.SplatLocation.Foreground);
    }
}

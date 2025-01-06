using System;
using System.Collections;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private ParticleSystem _ps;
    private Action OnStopped;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _ps.Play();
        if (_ps.main.loop == false)
            StartCoroutine(CoReserveDestroy());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetInfo(Action action, float angle = 0)
    {
        OnStopped = action;
    }

    public void DestroyParticle()
    {
        StopAllCoroutines();
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoReserveDestroy()
    {
        yield return new WaitForSeconds(_ps.main.duration);
        OnStopped?.Invoke();
        DestroyParticle();
    }
}

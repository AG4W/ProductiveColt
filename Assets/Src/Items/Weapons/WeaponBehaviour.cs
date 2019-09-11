using UnityEngine;

using System.Collections;

public class WeaponBehaviour : MonoBehaviour
{
    [Header("Cooldown before next shot (in seconds).")]
    [Range(.001f, 10f)][SerializeField]float _fireRate = 1f;
    [Range(1f, 100f)][SerializeField]float _damage = 50f;

    [SerializeField]AudioClip[] _gunSFX;
    AudioSource _audio;

    bool _hasCooldown = false;

    void Start()
    {
        _audio = this.GetComponentInChildren<AudioSource>();
    }

    public void Fire(Vector3 origin, Vector3 direction)
    {
        if (_hasCooldown)
            return;

        PlaySFX();
        this.StartCoroutine(FireCooldown());

        RaycastHit hit;

        if(Physics.Raycast(origin, direction, out hit, 1000f, LayerMask.NameToLayer("Player")))
            if(hit.collider.transform.root.GetComponentInChildren<Entity>() != null)
                hit.collider.transform.root.GetComponentInChildren<Entity>().UpdateHealth(_damage);
    }

    void PlaySFX()
    {
        _audio.pitch = Random.Range(.75f, 1.25f);
        _audio.clip = _gunSFX[Random.Range(0, _gunSFX.Length)];
        _audio.Play();
    }

    IEnumerator FireCooldown()
    {
        _hasCooldown = true;
        yield return new WaitForSeconds(_fireRate);
        _hasCooldown = false;
    }
}

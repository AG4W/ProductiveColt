using UnityEngine;

public class Entity : MonoBehaviour
{
    [Range(1f, 1000f)][SerializeField]float _maxHealth = 100f;
    float _currentHealth;

    //needs to unsub on death
    public LocalEventBus events { get; } = new LocalEventBus();

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void UpdateHealth(float value)
    {
        _currentHealth -= value;

        if (_currentHealth <= 0f)
            OnDeath();
        else if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    void OnDeath()
    {
        this.events.Clear();
        Destroy(this.gameObject);
    }
}
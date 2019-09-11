using UnityEngine;

public class Entity : MonoBehaviour
{
    //needs to unsub on death
    public LocalEventBus events { get; } = new LocalEventBus();
}
using UnityEngine;

public class Loader : MonoBehaviour
{
    void Awake()
    {
        GlobalEventBus.Initialize();
    }
}

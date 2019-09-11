using UnityEngine;

using System.Collections;

public class TimeManager : MonoBehaviour
{
    [Range(.01f, 1f)][SerializeField]float _slowMotionTimeScale = .1f;
    [Range(1, 10)][SerializeField]int _timeLerpMultiplier = 2;

    bool _inSlowMotion = false;

    void Start()
    {
        GlobalEventBus.Subscribe(GlobalEvent.ToggleSlowMotion, (object[] args) => Toggle());
    }
    void Toggle()
    {
        _inSlowMotion = !_inSlowMotion;

        this.StopAllCoroutines();
        this.StartCoroutine(Toggle(.5f));
    }

    IEnumerator Toggle(float duration)
    {
        float t = 0f;

        while (t <= duration)
        {
            t += Time.fixedDeltaTime;

            Time.timeScale = Mathf.Lerp(Time.timeScale, _inSlowMotion ? _slowMotionTimeScale : 1f, Mathf.Pow(t / duration, _timeLerpMultiplier));
            Time.fixedDeltaTime = .02f * Time.timeScale;

            yield return new WaitForFixedUpdate();
        }
    }
}

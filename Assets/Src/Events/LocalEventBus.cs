using System;
using System.Collections.Generic;

public class LocalEventBus
{
    List<Action<object[]>>[] _events;

    public LocalEventBus()
    {
        _events = new List<Action<object[]>>[Enum.GetNames(typeof(LocalEvent)).Length];

        for (int i = 0; i < _events.Length; i++)
            _events[i] = new List<Action<object[]>>();

        OnRaised += Raise;
    }

    public void Subscribe(LocalEvent e, Action<object[]> del)
    {
        _events[(int)e].Add(del);
    }
    public void Raise(LocalEvent e, params object[] args)
    {
        for (int i = 0; i < _events[(int)e].Count; i++)
            _events[(int)e][i]?.Invoke(args);
    }

    public static void RaiseAll(LocalEvent e, params object[] args)
    {
        OnRaised?.Invoke(e, args);
    }
    public delegate void OnRaisedEvent(LocalEvent e, object[] args);
    public static event OnRaisedEvent OnRaised;
}
public enum LocalEvent
{
    UpdateMouseInput,
    UpdateMovementInput,
    Jump,
}
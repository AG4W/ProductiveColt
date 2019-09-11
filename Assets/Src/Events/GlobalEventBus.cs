using System;
using System.Collections.Generic;

public static class GlobalEventBus
{
    static List<Action<object[]>>[] _events;

    public static void Initialize()
    {
        _events = new List<Action<object[]>>[Enum.GetNames(typeof(GlobalEvent)).Length];

        for (int i = 0; i < _events.Length; i++)
            _events[i] = new List<Action<object[]>>();
    }

    public static void Subscribe(GlobalEvent e, Action<object[]> del)
    {
        _events[(int)e].Add(del);
    }
    public static void Raise(GlobalEvent e, params object[] args)
    {
        for (int i = 0; i < _events[(int)e].Count; i++)
            _events[(int)e][i]?.Invoke(args);
    }
}
public enum GlobalEvent
{
    ToggleSlowMotion
}

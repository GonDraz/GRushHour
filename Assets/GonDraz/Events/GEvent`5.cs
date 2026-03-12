using System;
using System.Linq;
using UnityEngine;

namespace GonDraz.Events
{
    public sealed class GEvent<T1, T2, T3, T4, T5>
    {
        private readonly string _name;

        private Action<T1, T2, T3, T4, T5> _action;

        public GEvent()
        {
            _name = ToString();
        }

        public GEvent(Action<T1, T2, T3, T4, T5> action)
        {
            if (action == null) return;

            _name = action.Method.Name;
            _action = action;
        }

        public GEvent(string name)
        {
            _name = name;
        }

        public GEvent(string name, params Action<T1, T2, T3, T4, T5>[] actions)
        {
            _name = name;
            _action = null;
            foreach (var action in actions)
                if (!CheckForDuplicates(this, action))
                    _action += action;
        }

        public void Invoke(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5,
            Action onComplete = null)
        {
            if (_action == null) return;

            foreach (var d in _action.GetInvocationList())
                CallAction((Action<T1, T2, T3, T4, T5>)d, parameter1, parameter2, parameter3, parameter4, parameter5);

            onComplete?.Invoke();
        }

        private void CallAction(Action<T1, T2, T3, T4, T5> action, T1 parameter1, T2 parameter2, T3 parameter3,
            T4 parameter4, T5 parameter5)
        {
            if (action == null) return;
            try
            {
                action.Invoke(parameter1, parameter2, parameter3, parameter4, parameter5);
            }
            catch (Exception ex)
            {
                var real = ex.InnerException ?? ex.GetBaseException() ?? ex;

                Debug.LogError($"Event {_name} : has been infected : [{action.Method.Name}]");
                Debug.LogException(real);
            }
        }

        private static bool CheckForDuplicates(GEvent<T1, T2, T3, T4, T5> e, Action<T1, T2, T3, T4, T5> newAction)
        {
            if (e._action != null && newAction != null)
                if (e._action.GetInvocationList().Contains(newAction))
                {
                    Debug.LogError(
                        $"Event <color=yellow>[{e._name}]</color> : has been infected : [{newAction.Method.Name}]"
                    );
                    return true;
                }

            return false;
        }

        public static GEvent<T1, T2, T3, T4, T5> operator +(GEvent<T1, T2, T3, T4, T5> e,
            Action<T1, T2, T3, T4, T5> newAction)
        {
            e ??= new GEvent<T1, T2, T3, T4, T5>();

            if (CheckForDuplicates(e, newAction)) return e;

            e._action += newAction;
            return e;
        }

        public static GEvent<T1, T2, T3, T4, T5> operator -(GEvent<T1, T2, T3, T4, T5> e,
            Action<T1, T2, T3, T4, T5> action)
        {
            e ??= new GEvent<T1, T2, T3, T4, T5>();

            e._action -= action;
            return e;
        }

        public static implicit operator GEvent<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return action == null ? null : new GEvent<T1, T2, T3, T4, T5>(action);
        }

        public static implicit operator Action<T1, T2, T3, T4, T5>(GEvent<T1, T2, T3, T4, T5> e)
        {
            e ??= new GEvent<T1, T2, T3, T4, T5>();

            Action<T1, T2, T3, T4, T5> action = null;
            if (e._action != null) action += e._action;
            return action;
        }
    }
}
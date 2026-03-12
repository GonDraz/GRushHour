using System;
using System.Linq;
using UnityEngine;

namespace GonDraz.Events
{
    public sealed class GEvent<T>
    {
        private readonly string _name;

        private Action<T> _action;

        public GEvent()
        {
            _name = ToString();
        }

        public GEvent(Action<T> action)
        {
            if (action == null) return;

            _name = action.Method.Name;
            _action = action;
        }

        public GEvent(string name)
        {
            _name = name;
        }

        public GEvent(string name, params Action<T>[] actions)
        {
            _name = name;
            _action = null;
            foreach (var action in actions)
                if (!CheckForDuplicates(this, action))
                    _action += action;
        }

        public void Invoke(T parameter, Action onComplete = null)
        {
            if (_action == null) return;

            foreach (var d in _action.GetInvocationList())
                CallAction((Action<T>)d, parameter);

            onComplete?.Invoke();
        }

        private void CallAction(Action<T> action, T parameter)
        {
            if (action == null) return;
            try
            {
                action.Invoke(parameter);
            }
            catch (Exception ex)
            {
                var real = ex.InnerException ?? ex.GetBaseException() ?? ex;

                Debug.LogError($"Event {_name} : has been infected : [{action.Method.Name}]");
                Debug.LogException(real);
            }
        }

        private static bool CheckForDuplicates(GEvent<T> e, Action<T> newAction)
        {
            if (e._action == null || newAction == null) return false;
            if (!e._action.GetInvocationList().Contains(newAction)) return false;
            Debug.LogError(
                $"Event <color=yellow>[{e._name}]</color> : has been infected : [{newAction.Method.Name}]"
            );
            return true;
        }

        public static GEvent<T> operator +(GEvent<T> e, Action<T> newAction)
        {
            e ??= new GEvent<T>();

            if (CheckForDuplicates(e, newAction)) return e;

            e._action += newAction;
            return e;
        }

        public static GEvent<T> operator -(GEvent<T> e, Action<T> action)
        {
            e ??= new GEvent<T>();

            e._action -= action;
            return e;
        }

        public static implicit operator GEvent<T>(Action<T> action)
        {
            return action == null ? null : new GEvent<T>(action);
        }

        public static implicit operator Action<T>(GEvent<T> e)
        {
            e ??= new GEvent<T>();

            Action<T> action = null;
            if (e._action != null) action += e._action;
            return action;
        }
    }
}
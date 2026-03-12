using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GonDraz.Base;
using GonDraz.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GonDraz.StateMachine
{
    public abstract class BaseStateMachine<TMachine, TState> : BaseBehaviour
        where TMachine : BaseStateMachine<TMachine, TState>
        where TState : BaseState<TMachine, TState>
    {
        [SerializeField] [ReadOnly] private string currentStateName;
        [SerializeField] [ReadOnly] private string previousStateName;

        [SerializeField] private bool enableDebugLog; // Có thể bật/tắt log chuyển state

        private readonly object _lock = new();
        private TState _currentState;

        private Dictionary<Type, TState> _states;

        private Dictionary<Type, TState> States
        {
            get
            {
                lock (_lock)
                {
                    if (_states != null) return _states;
                    _states = new Dictionary<Type, TState>();

                    var types = typeof(TMachine).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(t => t.IsSubclassOf(typeof(TState)))
                        .ToArray();
                    foreach (var type in types)
                        try
                        {
                            var instance = (TState)Activator.CreateInstance(type, true);
                            instance.Instance((TMachine)this);
                            if (!_states.TryAdd(type, instance))
                                Debug.LogWarning($"Duplicate state type detected: {type.FullName}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Cannot create state {type.FullName}: {ex.Message}");
                        }

                    return _states;
                }
            }
        }

        protected virtual void Awake()
        {
            GetCurrentState();
        }

        private void Update()
        {
            GetCurrentState().OnUpdate();
        }

        private void FixedUpdate()
        {
            GetCurrentState().OnFixedUpdate();
        }

        private void LateUpdate()
        {
            GetCurrentState().OnLateUpdate();
        }

        protected TState GetCurrentState()
        {
            if (_currentState == null) ChangeState(InitialState(), false);
            return _currentState;
        }

        protected bool TryGetState<TType>(out TType state) where TType : TState
        {
            if (States.TryGetValue(typeof(TType), out var value))
            {
                state = (TType)value;
                return true;
            }

            state = null;
            return false;
        }

        protected abstract Type InitialState();

        protected void ChangeState(string stateName, bool canBack = true)
        {
            lock (_lock)
            {
                var stateType = States.Keys.FirstOrDefault(t => t.Name == stateName);
                if (stateType != null)
                    ChangeState(stateType, canBack);
                else
                    Debug.LogError($"State with name '{stateName}' not found.");
            }
        }

        public void ChangeState<T1>(bool canBack = true) where T1 : IState
        {
            lock (_lock)
            {
                ChangeState(typeof(T1), canBack);
            }
        }

        private void ChangeState(Type type, bool canBack = true)
        {
            lock (_lock)
            {
                if (!States.TryGetValue(type, out var state)) return;
                ChangeState(state, canBack);
                currentStateName = _currentState?.GetType().Name;
                previousStateName = _currentState?.PreviousState?.GetType().Name;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void ChangeState(TState state, bool canBack = true)
        {
            lock (_lock)
            {
                if (_currentState == null)
                {
                    _currentState = state;
                    if (enableDebugLog)
                        Debug.Log(
                            $"<color=blue>{name}</color> - Change state to <color=red>{_currentState.GetName()}</color>");
                    _currentState.OnEnter();
                    return;
                }

                if (state.GetType().FullName == _currentState.GetType().FullName)
                    return;

                if (enableDebugLog)
                    Debug.Log(
                        $"<color=blue>{name}</color> - Change state from <color=red>{_currentState.GetName()}</color> to <color=green>{state.GetType().Name}</color>");

                _currentState.OnExit();
                var previousState = _currentState;
                _currentState = state;
                if (!canBack)
                    previousState = null;
                else
                    previousState.PreviousState = null;
                _currentState.OnEnter(previousState);
            }
        }

        protected bool CanBack()
        {
            if (GetCurrentState().PreviousState == null) return false;

            return GetCurrentState().PreviousState.GetName() != GetCurrentState().GetName();
        }

        protected void BackToPreviousState()
        {
            if (!CanBack()) return;

            GetCurrentState().OnExit();

            // Debug.Log("Back state from <color=red>" + CurrentState.GetType().Name + "</color> to <color=green>" +
            //           CurrentState.PreviousState.GetType().Name + "</color>");

            _currentState = GetCurrentState().PreviousState;
            GetCurrentState().OnEnter();
        }


        public void RegisterEvent<Ts>(EventState eventState, Action action) where Ts : TState
        {
            RegisterEvent<Ts>(eventState, new GEvent(action));
        }

        public void RegisterEvent<Ts>(EventState eventState, GEvent action) where Ts : TState
        {
            lock (_lock)
            {
                if (States.TryGetValue(typeof(Ts), out var state))
                    switch (eventState)
                    {
                        case EventState.Enter:
                            state.Enter += action;
                            break;
                        case EventState.Exit:
                            state.Exit += action;
                            break;
                        case EventState.FixedUpdate:
                            state.FixedUpdate += action;
                            break;
                        case EventState.LateUpdate:
                            state.LateUpdate += action;
                            break;
                        case EventState.Update:
                            state.Update += action;
                            break;
                        default:
                            Debug.LogError($"Error RegisterEvent: {eventState} is not a valid EventState");
                            break;
                    }
                else
                    Debug.LogError($"Error RegisterEvent: {eventState} Can't register Action: {action}");
            }
        }

        public void UnregisterEvent<Ts>(EventState eventState, Action action) where Ts : TState
        {
            UnregisterEvent<Ts>(eventState, new GEvent(action));
        }

        public void UnregisterEvent<Ts>(EventState eventState, GEvent action) where Ts : TState
        {
            lock (_lock)
            {
                if (States.TryGetValue(typeof(Ts), out var state))
                    switch (eventState)
                    {
                        case EventState.Enter:
                            state.Enter -= action;
                            break;
                        case EventState.Exit:
                            state.Exit -= action;
                            break;
                        case EventState.FixedUpdate:
                            state.FixedUpdate -= action;
                            break;
                        case EventState.LateUpdate:
                            state.LateUpdate -= action;
                            break;
                        case EventState.Update:
                            state.Update -= action;
                            break;
                        default:
                            Debug.LogError($"Error UnregisterEvent: {eventState} is not a valid EventState");
                            break;
                    }
                else
                    Debug.LogError($"Error UnregisterEvent: {eventState} Can't unregister Action: {action}");
            }
        }

        public bool IsInState<Ts>() where Ts : TState
        {
            return GetCurrentState().GetType() == typeof(Ts);
        }
    }
}
using System;
using GonDraz.Extensions;
using UnityEngine;

namespace GonDraz.StateMachine
{
    public abstract class
        BaseGlobalStateMachine<TMachine> : BaseStateMachine<TMachine,
        BaseGlobalStateMachine<TMachine>.BaseGlobalState>
        where TMachine : BaseGlobalStateMachine<TMachine>
    {
        private static bool applicationIsQuitting;
        private static TMachine Instance { get; set; }

        protected override void Awake()
        {
            applicationIsQuitting = false;
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as TMachine;
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance != this) return;
            Instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        public static TMachine GetInstance()
        {
            if (applicationIsQuitting)
                return null;

            if (Instance) return Instance;
            var obj = new GameObject("(Singleton)" + typeof(TMachine).Name);
            Instance = obj.GetOrAddComponent<TMachine>();
            DontDestroyOnLoad(obj);
            return Instance;
        }

        public static void Register<Ts>(EventState eventState, Action action) where Ts : BaseGlobalState
        {
            var inst = GetInstance();
            if (!inst) return;
            inst.RegisterEvent<Ts>(eventState, action);
        }

        public static void Unregister<Ts>(EventState eventState, Action action) where Ts : BaseGlobalState
        {
            var inst = GetInstance();
            if (!inst) return;
            inst.UnregisterEvent<Ts>(eventState, action);
        }

        public static void Change<Ts>(bool canBack = true) where Ts : BaseGlobalState
        {
            var inst = GetInstance();
            if (!inst) return;
            inst.ChangeState<Ts>(canBack);
        }

        public abstract class BaseGlobalState : BaseState<TMachine, BaseGlobalState>
        {
        }
    }
}
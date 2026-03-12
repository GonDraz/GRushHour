using GonDraz.Events;

namespace GonDraz.StateMachine
{
    public class BaseState<TMachine, TState> : IState
    {
        public GEvent Enter;
        public GEvent Exit;
        public GEvent FixedUpdate;
        protected TMachine Host;
        public GEvent LateUpdate;
        public TState PreviousState;
        public GEvent Update;

        public virtual void OnEnter()
        {
            Enter?.Invoke();
        }

        public virtual void OnUpdate()
        {
            Update?.Invoke();
        }

        public virtual void OnLateUpdate()
        {
            LateUpdate?.Invoke();
        }

        public virtual void OnFixedUpdate()
        {
            FixedUpdate?.Invoke();
        }

        public virtual void OnExit()
        {
            Exit?.Invoke();
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void Instance(TMachine host)
        {
            Host = host;
        }

        public virtual void OnEnter(TState previousState)
        {
            SetPreviousState(previousState);
            OnEnter();
        }

        public void SetPreviousState(TState previousState)
        {
            PreviousState = previousState;
        }

        public override string ToString()
        {
            return $"{GetName()} (Host: {Host?.GetType().Name}, PreviousState: {PreviousState})";
        }

        public string GetName()
        {
            return GetType().Name;
        }
    }
}
namespace GonDraz.StateMachine
{
    public interface IState
    {
        public void OnEnter();
        public void OnUpdate();
        public void OnLateUpdate();
        public void OnFixedUpdate();
        public void OnExit();
    }
}
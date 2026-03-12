namespace GonDraz.Base
{
    public abstract class BaseBehaviourSingleton<T> : BaseBehaviour where T : BaseBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this as T;
            if (IsDontDestroyOnLoad()) DontDestroyOnLoad(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
            {
                Instance = null;
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
            Destroy(gameObject);
        }

        protected abstract bool IsDontDestroyOnLoad();
    }
}
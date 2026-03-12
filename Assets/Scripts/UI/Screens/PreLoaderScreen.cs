using GonDraz.Events;
using GonDraz.UI;

namespace UI.Screens
{
    public class PreLoaderScreen : Presentation
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }

        public override bool SubscribeUsingOnEnable()
        {
            return true;
        }

        public override bool UnsubscribeUsingOnDisable()
        {
            return true;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            BaseEventManager.ApplicationLoadFinished += ApplicationLoadFinished;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            BaseEventManager.ApplicationLoadFinished -= ApplicationLoadFinished;
        }

        private void ApplicationLoadFinished()
        {
            Hide();
        }
    }
}
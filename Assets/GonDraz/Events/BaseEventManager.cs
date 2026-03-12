namespace GonDraz.Events
{
    public static class BaseEventManager
    {
        public static GEvent ApplicationLoadFinished = new("ApplicationLoadFinished");
        public static GEvent<bool> ApplicationPause = new("ApplicationPause");
        public static GEvent GamePause = new("GamePause");
        public static GEvent UpdateNotification = new("UpdateNotification");
    }
}
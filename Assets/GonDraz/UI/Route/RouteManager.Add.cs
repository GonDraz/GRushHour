using UnityEngine;

namespace GonDraz.UI.Route
{
    public static partial class RouteManager
    {
        public static void AddWidgets(Presentation[] widgets)
        {
            foreach (var widget in widgets) AddWidget(widget);
        }

        private static void AddWidget(Presentation screen)
        {
            var type = screen.GetType();
            if (Presentations.TryGetValue(type, out var existing))
                Debug.LogWarning($"Screen [{type}] already exists. Replacing with new instance.", screen);
            Presentations[type] = screen;
        }
    }
}
using GonDraz.Base;

namespace GonDraz.UI.Route
{
    public class Route : BaseBehaviour
    {
        private bool _onPress;

        private void Awake()
        {
            var widgets = GetComponentsInChildren<Presentation>(true);
            RouteManager.AddWidgets(widgets);
            RouteManager.SetRouteParent(transform);
            Popup.OnBackButtonAction = RouteManager.Pop;
            DontDestroyOnLoad(this);
        }

        // private void LateUpdate()
        // {
        //     BackButtonPressed();
        // }
        //
        // private void BackButtonPressed()
        // {
        //     if (Input.GetKey(KeyCode.Escape))
        //     {
        //         if (!_onPress)
        //         {
        //             EventManager.GamePause.Invoke();
        //             RouteManager.Pop();
        //         }
        //
        //         _onPress = true;
        //     }
        //     else
        //     {
        //         _onPress = false;
        //     }
        // }
    }
}
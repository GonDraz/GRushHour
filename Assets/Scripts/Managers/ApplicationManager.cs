using System;
using System.Collections;
using System.Collections.Generic;
using GlobalState;
using GonDraz.Base;
using GonDraz.Events;
using UnityEngine;

namespace Managers
{
    public class ApplicationManager : BaseBehaviourSingleton<ApplicationManager>
    {
        private static bool _isApplicationLoadFinished;
#if UNITY_EDITOR
        [SerializeField] private bool pauseWhenUnFocus;
#else
        [SerializeField] private bool pauseWhenUnFocus = true;
#endif

        protected override void Awake()
        {
            base.Awake();
            foreach (var manager in ComponentInits())
            {
                Debug.Log("Init : <color=blue>" + manager.Name + "</color>");
                var managerObject = new GameObject { name = manager.Name };
                managerObject.AddComponent(manager);
            }
        }

        public void Start()
        {
            StartCoroutine(ApplicationLoadFinished());
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isApplicationLoadFinished && pauseWhenUnFocus)
                BaseEventManager.ApplicationPause.Invoke(!hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isApplicationLoadFinished && pauseWhenUnFocus)
                BaseEventManager.ApplicationPause.Invoke(pauseStatus);
        }

        protected override bool IsDontDestroyOnLoad()
        {
            return true;
        }

        private static List<Type> ComponentInits()
        {
            return new List<Type>
            {
                typeof(GlobalStateMachine)
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            var app = new GameObject { name = nameof(ApplicationManager) };
            app.AddComponent<ApplicationManager>();
        }

        private static IEnumerator ApplicationLoadFinished()
        {
            yield return new WaitForEndOfFrame();
            BaseEventManager.ApplicationLoadFinished.Invoke();
            _isApplicationLoadFinished = true;
            Debug.Log("Application Load Finished");
        }
    }
}
using System;
using GonDraz.Base;
using GonDraz.Events;
using GonDraz.Extensions;
using TMPro;
using UnityEngine;

namespace GonDraz.UI
{
    public class DotNotification : BaseBehaviour
    {
        [SerializeField] private bool showNumber;
        [SerializeField] private GameObject badge;
        [SerializeField] private TMP_Text number;

        private Func<int> _funcValue;

        private RectTransform _layout;

        private int _valueNumber;

        private RectTransform Layout
        {
            get
            {
                if (!_layout) _layout = GetComponent<RectTransform>();
                return _layout;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateValue();
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
            BaseEventManager.UpdateNotification += OnUpdateNotification;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            BaseEventManager.UpdateNotification -= OnUpdateNotification;
        }

        private void OnUpdateNotification()
        {
            UpdateValue();
        }

        public void SetFunc(Func<int> func)
        {
            _funcValue = func;
            UpdateValue();
        }

        public void SetValue(int value)
        {
            _valueNumber = value;
            UpdateValue();
        }

        public void SetFunc(Func<bool> func)
        {
            SetFunc(() => func() ? 1 : 0);
        }

        public void SetValue(bool value)
        {
            SetValue(value ? 1 : 0);
        }

        private string GetString(int value)
        {
            // if (value == 1 || !showNumber) return "!";
            if (!showNumber) return "!";
            return value > 99 ? "99+" : value.ToString();
        }

        private int GetValue()
        {
            if (_funcValue != null) _valueNumber = _funcValue.Invoke();
            return _valueNumber;
        }

        [ContextMenu("UpdateValue")]
        public void UpdateValue()
        {
            _valueNumber = GetValue();
            if (_valueNumber > 0)
            {
                number.text = GetString(_valueNumber);
                badge.gameObject.SetActive(true);
            }
            else
            {
                badge.gameObject.SetActive(false);
            }

            ReBuildLayout();
        }

        [ContextMenu("RebuildLayout")]
        private void ReBuildLayout()
        {
            Layout.UpdateLayout().Forget();
        }

        // private void UpdateNotificationCountByType(NotificationType obj)
        // {
        //     if (notificationType == NotificationType.Other)
        //     {
        //         UpdateValue();
        //         return;
        //     }
        //     if(notificationType != obj) return;
        //     UpdateValue();
        // }
    }
}
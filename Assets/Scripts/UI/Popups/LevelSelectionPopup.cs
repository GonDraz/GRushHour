using System;
using GonDraz.UI;
using UI.LevelSelectionView;
using UnityEngine;

namespace UI.Popups
{
    public class LevelSelectionPopup : Popup
    {
        [SerializeField] private LevelSelectionListView levelSelectionView;

        public override void Show(Action callback = null)
        {
            base.Show(callback + levelSelectionView.ScrollToCurrentLevel);
            levelSelectionView.Show();
        }
    }
}
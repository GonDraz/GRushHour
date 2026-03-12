using Mahas.ListView;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.LevelSelectionView
{
    public class LevelSelectionCardView : ListViewCard<LevelSelectionData>
    {
        [SerializeField] private TMP_Text levelNumberText;

        protected override void OnSpawn()
        {
            levelNumberText.text = Data.levelName;
        }

        public void OnCardClicked()
        {
            // Load current level của người chơi
            LevelManager.SetCurrentLevel(Data.levelIndex);

            LevelManager.PlayCurrentLevel();
        }
    }
}
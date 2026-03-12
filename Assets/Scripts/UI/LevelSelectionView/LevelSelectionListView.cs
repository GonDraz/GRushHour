using System.Collections.Generic;
using Mahas.ListView;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.LevelSelectionView
{
    public class LevelSelectionListView : MonoBehaviour
    {
        [SerializeField] private ListView listView;
        [SerializeField] private TMP_InputField searchInput;

        private readonly List<LevelSelectionData> _data = new();

        private bool _isInitialized;

        private void Insatialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _data.Clear();

            // Use GetTotalLevelCount instead of GetAllLevels (Addressables optimization)
            var totalLevels = LevelManager.GetTotalLevelCount();
            for (var index = 0; index < totalLevels; index++) _data.Add(new LevelSelectionData { levelIndex = index });

            listView.SetupData(_data);
        }

        public void Show()
        {
            Insatialize();
        }

        public void ScrollToCurrentLevel()
        {
            listView.Manipulator.ScrollTo(LevelManager.CurrentLevelIndex.Value, 0.5f).SetDelay(0.1f)
                .SetAlignment(AlignmentType.Center)
                .Play();
        }

        public void OnScrollToItem()
        {
            if (int.TryParse(searchInput.text, out var index))
                listView.Manipulator.ScrollTo(index, 0.5f).SetAlignment(AlignmentType.Center).Play();
        }
    }
}
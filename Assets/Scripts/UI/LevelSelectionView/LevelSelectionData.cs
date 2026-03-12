using Mahas.ListView;

namespace UI.LevelSelectionView
{
    public class LevelSelectionData : IListViewData, IHaveMessageForGizmo
    {
        public int levelIndex;

        public string levelName => "Level " + (levelIndex + 1);

        // public bool isLocked;
        public string GetMessage()
        {
            return levelName;
        }
    }
}
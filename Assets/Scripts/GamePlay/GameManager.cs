using Data;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bg;
        private GameplayLevel _currentLevel;

        private GameplayLevel _levelPrefab;
        private DiContainer _container;

        private LevelList _levelList;
        private StartUI _startUI;

        [Inject]
        private void Inject(LevelList levelList, GameplayLevel levelPrefab, StartUI startUI, DiContainer diContainer)
        {
            _startUI = startUI;
            _levelList = levelList;
            _levelPrefab = levelPrefab;
            _container = diContainer;
        }

        private void Awake()
        {
            _startUI.Show(true);
        }

        [Button]
        public void LoadCurrentLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }

            var index = PlayerPrefs.GetInt("LevelIndex", 0);
            index %= _levelList.Levels.Length;
            
            bg.color = _levelList.Levels[index].bgColor;
            _currentLevel = _container.InstantiatePrefab(_levelPrefab).GetComponent<GameplayLevel>();

            _currentLevel.Initialize(_levelList.Levels[index]);
        }

        [Button]
        public void LoadNextLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }

            var index = PlayerPrefs.GetInt("LevelIndex", 0);
            index++;
            PlayerPrefs.SetInt("LevelIndex", index);

            index %= _levelList.Levels.Length;
            
            bg.color = _levelList.Levels[index].bgColor;
            _currentLevel = _container.InstantiatePrefab(_levelPrefab).GetComponent<GameplayLevel>();
            _currentLevel.Initialize(_levelList.Levels[index]);
        }
    }
}
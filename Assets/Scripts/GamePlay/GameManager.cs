using Data;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        private GameplayLevel _currentLevel; 
        
        private GameplayLevel _levelPrefab;
        private DiContainer _container;
        
        private LevelList _levelList;
        
        [Inject]
        private void Inject(LevelList levelList,GameplayLevel levelPrefab, DiContainer diContainer)
        {
            _levelList = levelList;
            _levelPrefab = levelPrefab;
            _container = diContainer;
        }

        private void Awake()
        {
            _currentLevel = _container.InstantiatePrefab(_levelPrefab).GetComponent<GameplayLevel>();
            _currentLevel.Initialize(_levelList.Levels[0]);
        }
    }
}
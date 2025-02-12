using Cysharp.Threading.Tasks;
using Data;
using UI;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class GameplayLevel : MonoBehaviour
    {
        public static bool CanTakeInput;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private GameplayGrid grid;
        [SerializeField] private GameplayCamera gCamera;

        private GamePlayUI _gamePlayUI;
        private LevelData _levelData;
        private NextLevelUI _nextLevelUI;
        private ReloadUI _reloadUI;

        [Inject]
        private void Inject(GamePlayUI gamePlayUI, NextLevelUI nextLevelUI, ReloadUI reloadUI)
        {
            _gamePlayUI = gamePlayUI;
            _nextLevelUI = nextLevelUI;
            _reloadUI = reloadUI;
        }

        public void Initialize(LevelData levelData)
        {
            CanTakeInput = true;
            _levelData = levelData;
            grid.Initialize(levelData, this);
            gCamera.Initialize(levelData.GridSize);

            LoadDeck();
        }

        private void LoadDeck()
        {
            _gamePlayUI.Initialize(_levelData, grid);
        }

        public void OnLevelFinished()
        {
            CanTakeInput = false;
            AudioManager.instance.PlayAudio(successSound);
            _nextLevelUI.Show();
        }

        public async void OnLevelFailed()
        {
            CanTakeInput = false;
            await UniTask.Delay(250);
            AudioManager.instance.PlayAudio(failSound);
            _reloadUI.Show();
        }
    }
}
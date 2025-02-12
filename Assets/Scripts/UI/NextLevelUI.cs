using Cysharp.Threading.Tasks;
using DG.Tweening;
using MobileHapticsProFreeEdition;
using TMPro;
using UnityEngine;
using Zenject;
using GameManager = GamePlay.GameManager;

namespace UI
{
    public class NextLevelUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI completedText;
        private GameManager _gm;

        [Inject]
        private void Inject(GameManager gm)
        {
            _gm = gm;
        }

        public async void Show(bool instant = false)
        {
            completedText.text = "LEVEL COMPLETED \n" + (PlayerPrefs.GetInt("LevelIndex", 0) + 1);
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

            await UniTask.Delay(500);

            if (instant)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.2f);
            }
        }

        public void NextLevelClicked()
        {
            AndroidTapticWave.Haptic(HapticModes.Confirm);

            _gm.LoadNextLevel();
            canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
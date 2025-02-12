using Cysharp.Threading.Tasks;
using DG.Tweening;
using MobileHapticsProFreeEdition;
using TMPro;
using UnityEngine;
using Zenject;
using GameManager = GamePlay.GameManager;

namespace UI
{
    public class ReloadUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI failedText;
        [SerializeField] private TextMeshProUGUI noMoreMovesText;
        private GameManager _gm;

        [Inject]
        private void Inject(GameManager gm)
        {
            _gm = gm;
        }

        public async void Show(bool instant = false)
        {
            await UniTask.Delay(300);
            noMoreMovesText.transform.DOKill();
            noMoreMovesText.transform.DOScale(1f, 0.3f);
            await UniTask.Delay(800);
            noMoreMovesText.transform.DOScale(0f, 0.3f);
            await UniTask.Delay(500);

            failedText.text = "LEVEL FAILED \n" + (PlayerPrefs.GetInt("LevelIndex", 0) + 1);
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

        public async void ReloadClicked()
        {
            AndroidTapticWave.Haptic(HapticModes.Confirm);

            _gm.LoadCurrentLevel();
            canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
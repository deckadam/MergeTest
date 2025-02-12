using DG.Tweening;
using MobileHapticsProFreeEdition;
using UnityEngine;
using Zenject;
using GameManager = GamePlay.GameManager;

namespace UI
{
    public class StartUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        private GameManager _gm;

        [Inject]
        private void Inject(GameManager gm)
        {
            _gm = gm;
        }

        public void Show(bool instant = false)
        {
            gameObject.SetActive(true);
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

        public void StartClicked()
        {
            AndroidTapticWave.Haptic(HapticModes.Confirm);

            _gm.LoadCurrentLevel();
            canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
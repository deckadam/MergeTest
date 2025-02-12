using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;

        [SerializeField] private Image soundImage;
        [SerializeField] private Sprite soundOpen;
        [SerializeField] private Sprite soundClose;

        [SerializeField] private Image hapticImage;
        [SerializeField] private Sprite hapticOpen;
        [SerializeField] private Sprite hapticClose;

        [SerializeField] private RectTransform settingsGroup;

        [SerializeField] private Vector2 openSize;
        [SerializeField] private Vector2 closeSize;

        public static bool SoundOpen;
        public static bool HapticOpen;
        public static bool SettingsPanelOpen;
        private bool _shown;

        private void Awake()
        {
            SoundOpen = PlayerPrefs.GetInt("SoundOpen", 1) == 1;
            HapticOpen = PlayerPrefs.GetInt("HapticOpen", 1) == 1;

            soundImage.sprite = SoundOpen ? soundOpen : soundClose;
            hapticImage.sprite = hapticOpen ? hapticOpen : hapticClose;

            settingsGroup.sizeDelta = closeSize;
        }

        public void OnBgClicked()
        {
            if (!SettingsPanelOpen)
            {
                return;
            }
            settingsPanel.gameObject.SetActive(false);
            OnShowClicked();
        }

        public void OnShowClicked()
        {
            SettingsPanelOpen = !SettingsPanelOpen;
            settingsGroup.DOKill();

            if (SettingsPanelOpen)
            {
                settingsGroup.DOSizeDelta(openSize, 0.2f);
                settingsPanel.gameObject.SetActive(true);
            }
            else
            {
                settingsGroup.DOSizeDelta(closeSize, 0.2f);
                settingsPanel.gameObject.SetActive(false);
            }
        }

        public void OnSoundButtonClicked()
        {
            SoundOpen = !SoundOpen;
            soundImage.sprite = SoundOpen ? soundOpen : soundClose;
            PlayerPrefs.SetInt("SoundOpen", SoundOpen ? 1 : 0);
        }

        public void OnHapticButtonClicked()
        {
            HapticOpen = !HapticOpen;
            hapticImage.sprite = HapticOpen ? hapticOpen : hapticClose;
            PlayerPrefs.SetInt("HapticOpen", HapticOpen ? 1 : 0);
        }
    }
}
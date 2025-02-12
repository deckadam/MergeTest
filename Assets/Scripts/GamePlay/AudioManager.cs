using System.Collections.Generic;
using Common.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private List<AudioSource> _audioSources;

    private void Awake()
    {
        instance = this;
        _audioSources = new List<AudioSource>();
    }

    public async void PlayAudio(AudioClip audioClip, float delay = 0f)
    {
        if (!SettingsPanel.SoundOpen)
        {
            return;
        }

        await UniTask.Delay((int)(delay * 1000));

        if (_audioSources.Count > 0)
        {
            var source = _audioSources[0];
            _audioSources.RemoveAt(0);
            Play(audioClip, source);
        }
        else
        {
            var source = gameObject.AddComponent<AudioSource>();
            Play(audioClip, source);
        }
    }

    private async void Play(AudioClip audioClip, AudioSource audioSource)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        await UniTask.Delay((int)(audioClip.length * 1250));
        _audioSources.Add(audioSource);
    }
}
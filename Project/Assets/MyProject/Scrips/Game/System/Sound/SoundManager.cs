using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    [HideInInspector] public AudioSource bgmAudioSource;
    [HideInInspector] public List<AudioSource> seAudioSource = new List<AudioSource>();

    [SerializeField]
    private List<AudioClip> audioClips = new List<AudioClip>();
    private Coroutine bgmFadeCoroutine;

    public static SoundManager Instance { get; private set; }
    private int sePoolSize = 5;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;

        for(int i = 0; i < sePoolSize; i++)
        {
            AudioSource se = gameObject.AddComponent<AudioSource>();
            seAudioSource.Add(se);
        }
    }


    public void PlayBGM(string name, float volume = 0.6f, float fadeTime = 1.0f)
    {
        // 既に再生中なら再生しない
        if (bgmAudioSource.clip != null &&
            bgmAudioSource.clip.name == name) return;

        int length = audioClips.Count;
        for (int i = 0; i < length; i++)
        {
            if (audioClips[i].name == name)
            {
                // 既存のフェードがあれば停止
                if (bgmFadeCoroutine != null)
                {
                    StopCoroutine(bgmFadeCoroutine);
                    bgmFadeCoroutine = null;
                }

                bgmAudioSource.clip = audioClips[i];
                bgmAudioSource.volume = 0.0f;
                bgmAudioSource.Play();

                // フェードイン開始
                bgmFadeCoroutine = StartCoroutine(FadeBGM(volume, fadeTime));
            }
        }

    }

    public void StopBGM(float fadeTime = 1.0f)
    {
        if (!bgmAudioSource.isPlaying) return;

        // 既存のフェードがあれば停止
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        // フェードアウト開始
        bgmFadeCoroutine = StartCoroutine(FadeBGM(0.0f, fadeTime, stopFade: true));
    }

    // フェード機能
    private IEnumerator FadeBGM(float targetVolume, float duration, bool stopFade = false)
    {
        float startVolume = bgmAudioSource.volume;
        float time = 0.0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;

        // フェードアウト後に停止する場合
        if (stopFade)
        {
            bgmAudioSource.Stop();
        }

        bgmFadeCoroutine = null;
    }

    public void PlaySE(string name, float volume = 1.0f)
    {
        int length = audioClips.Count;
        for (int i = 0; i < length; i++)
        {
            if (audioClips[i].name == name)
            {
                foreach (var se in seAudioSource)
                {
                    if (!se.isPlaying)
                    {
                        se.clip = audioClips[i];
                        se.volume = volume;
                        se.Play();
                        return;
                    }
                }

                // 空きがなければ先頭を上書き
                seAudioSource[0].clip = audioClips[i];
                seAudioSource[0].volume = volume;
                seAudioSource[0].Play();
                return;
            }
        }
    }
}

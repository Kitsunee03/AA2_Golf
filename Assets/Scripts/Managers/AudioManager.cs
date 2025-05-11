using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer m_audioMixer;
    private AudioSource m_UIAudioSource;
    private AudioSource m_backgroundMusicAudioSource;
    private AudioSource m_generalAudioSource;

    protected override void Initialize()
    {
        m_UIAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        m_backgroundMusicAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        m_generalAudioSource = transform.GetChild(2).GetComponent<AudioSource>();
    }

    public void PlayAudioClipEffect(AudioClip p_audioClip)
    {
        m_generalAudioSource.PlayOneShot(p_audioClip);
    }

    public void SetMasterVolumeTo(float p_value)
    {
        m_masterVolume = p_value;
        float value = 20 * Mathf.Log10(p_value);
        m_audioMixer.SetFloat("MasterVolume", value);
    }

    float m_masterVolume = 1;
    float m_effectVolume = 1;
    float m_musicVolume = 1;

    public float GetMasterVolume()
    {
        return m_masterVolume;
    }

    public float GetEffectVolume()
    {
        return m_effectVolume;
    }

    public float GetMusicVolume()
    {
        return m_musicVolume;
    }

    public void SetEffectsVolumeTo(float p_value)
    {
        m_effectVolume = p_value;
        float value = 20 * Mathf.Log10(p_value);
        m_audioMixer.SetFloat("EffectsVolume", value);
    }

    public void SetMusicVolumeTo(float p_value)
    {
        m_musicVolume = p_value;
        float value = 20 * Mathf.Log10(p_value);
        m_audioMixer.SetFloat("MusicVolume", value);
    }

    public void PlayBackgroundMusic(AudioClip p_audioClip)
    {
        m_backgroundMusicAudioSource.clip = p_audioClip;
        m_backgroundMusicAudioSource.Play();
    }

    public AudioSource UIEffectsAudioSource { get { return m_UIAudioSource; } }
    public AudioSource BackgroundMusicAudioSource { get { return m_backgroundMusicAudioSource; } }

}

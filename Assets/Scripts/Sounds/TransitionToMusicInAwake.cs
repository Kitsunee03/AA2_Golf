using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToMusicInAwake : MonoBehaviour
{
    [SerializeField] AudioClip m_musicAudioClip;
    [SerializeField] float m_fadeInMusicDuration;
    [SerializeField] float m_fadeOutMusicDuration;
    [SerializeField] bool endArena = false;

    private void Start()
    {
        if(!endArena)
            TransitionMusic();
    }

    public void TransitionMusic()
    {
        Job job = new Job(true);

        Task task = new Task(FadeInMusic);
        task.AddListenerToEndOfTaskEvent(() =>
        {
            AudioManager.Instance.PlayBackgroundMusic(m_musicAudioClip);

        });
        job.AddTask(task);
        task = new Task(FadeOutMusic);
        job.AddListenerToEndOfJobEvent(() =>
        {
            CoroutineManager.Instance.RemoveJob(job);
        });
        job.AddTask(task);
        job.Start();
    }

    IEnumerator FadeInMusic()
    {
        float time = 0;

        if(m_fadeInMusicDuration == 0)
        {
            AudioManager.Instance.BackgroundMusicAudioSource.volume = 0;
        }
        else
        {
            while (time < m_fadeInMusicDuration)
            {
                time += Time.deltaTime;
                AudioManager.Instance.BackgroundMusicAudioSource.volume = 1 - time / m_fadeInMusicDuration;
                yield return null;
            }
        }
    }

    IEnumerator FadeOutMusic()
    {
        float time = 0;

        while (time < m_fadeOutMusicDuration)
        {
            time += Time.deltaTime;
            AudioManager.Instance.BackgroundMusicAudioSource.volume = time / m_fadeOutMusicDuration;
            yield return null;
        }
    }

    public void SetEndArena(bool end)
    {
        endArena = end;
    }

}

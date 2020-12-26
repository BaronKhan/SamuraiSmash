using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
  private AudioSource m_audio_source;

  private void Awake()
  {
    foreach (var obj in GameObject.FindGameObjectsWithTag("Music"))
    {
      if (obj != gameObject)
        Destroy(obj);
    }
    DontDestroyOnLoad(transform.gameObject);
    m_audio_source = GetComponent<AudioSource>();
  }

  public void PlayMusic()
  {
    if (m_audio_source.isPlaying) return;
    m_audio_source.Play();
  }

  public void StopMusic()
  {
    m_audio_source.Stop();
  }
}
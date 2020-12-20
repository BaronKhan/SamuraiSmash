using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
  public bool m_fade_in = false;
  public float m_fade_speed = 1f;
  public int m_scene_index = 0;

  private Image m_image = null;

  // Start is called before the first frame update
  void Start()
  {
    if (m_fade_in)
      SetFadeType(false);
  }

  IEnumerator FadeIn()
  {
    for (float ft = 1f; ft >= 0; ft -= 0.16f * m_fade_speed)
    {
      Color c = m_image.color;
      c.a = ft;
      m_image.color = c;
      yield return null;
    }

    Destroy(gameObject);
  }
  IEnumerator FadeOut()
  {
    for (float ft = 0f; ft < 1; ft += 0.16f * m_fade_speed)
    {
      Color c = m_image.color;
      c.a = ft;
      m_image.color = c;
      yield return null;
    }

    SceneManager.LoadScene(m_scene_index);
  }

  // Update is called once per frame
  void Update()
  {

  }

  public Fader SetFadeType(bool fade_out)
  {
    return SetFadeType(fade_out, m_fade_speed, m_scene_index);
  }

  public Fader SetFadeType(bool fade_out, float fade_speed, int scene_index)
  {
    m_fade_speed = fade_speed;
    m_scene_index = scene_index;
    m_image = GetComponent<Image>();
    m_image.color = new Color(0, 0, 0, fade_out ? 0 : 1);

    StartCoroutine(fade_out ? FadeOut() : FadeIn());
    return this;
  }
}

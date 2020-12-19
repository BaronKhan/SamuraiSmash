using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
  public int m_score = 0;
  public int m_max_score = 0;

  private TextMesh text_mesh = null;

  // Start is called before the first frame update
  void Start()
  {
    text_mesh = GetComponent<TextMesh>();
    if (PlayerPrefs.HasKey("Score"))
    {
      m_max_score = PlayerPrefs.GetInt("Score");
    }
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  private void SaveScore()
  {
    PlayerPrefs.SetInt("Score", m_score);
    PlayerPrefs.Save();
  }

  public void Increment()
  {
    if (++m_score >= m_max_score)
      SaveScore();
    text_mesh.text = m_score.ToString();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
  public int m_score = 0;

  private TextMesh text_mesh = null;

  // Start is called before the first frame update
  void Start()
  {
    text_mesh = GetComponent<TextMesh>();
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  private void SaveScore()
  {

  }

  public void Increment()
  {
    ++m_score;
    SaveScore();
    text_mesh.text = m_score.ToString();
  }
}

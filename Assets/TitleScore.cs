using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScore : MonoBehaviour
{
  Text m_text = null;

  // Start is called before the first frame update
  void Start()
  {
    m_text = GetComponent<Text>();
    if (PlayerPrefs.HasKey("Score"))
      m_text.text = PlayerPrefs.GetInt("Score").ToString();
    else
      m_text.text = "";
  }

  // Update is called once per frame
  void Update()
  {
        
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
  private bool started = false;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if (!started && Input.GetMouseButtonDown(0))
    {
      // TODO: fade in and out
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
      started = true;
    }
  }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour, IPointerClickHandler
{
  private bool started = false;
  private Fader fader = null;

  // Start is called before the first frame update
  void Start()
  {
    fader = GetComponentInChildren<Fader>();
    fader.gameObject.SetActive(false);
  }

  // Update is called once per frame
  void Update()
  {
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Debug.Log("Play button clicked");
    if (!started)
    {
      fader.gameObject.SetActive(true);
      if (!PlayerPrefs.HasKey("Watched"))
      {
        fader.SetFadeType(true, 0.25f, SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("Watched", 1);
        PlayerPrefs.Save();
      }
      else
        fader.SetFadeType(true, 0.25f, SceneManager.GetActiveScene().buildIndex + 2);

      started = true;
    }
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    Debug.Log("Entered");
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    Debug.Log("Exited");
  }
}

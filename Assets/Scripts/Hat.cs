using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hat : MonoBehaviour
{
  public bool m_change_hat_disable = true;
  public Camera m_camera = null;

  private MeshRenderer[] m_hats;
  private int m_hats_count = 0;
  private int m_enabled_hat = 0;

  private void UpdateHat()
  {
    for (int i = 0; i < m_hats_count; ++i)
    {
      m_hats[i].forceRenderingOff = (i != m_enabled_hat);
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    if (PlayerPrefs.HasKey("HatIndex"))
      m_enabled_hat = PlayerPrefs.GetInt("HatIndex");
    else if (PlayerPrefs.HasKey("Score") && PlayerPrefs.GetInt("Score") > 6)
      m_enabled_hat = Random.Range(0, m_hats_count);
    m_hats = GetComponentsInChildren<MeshRenderer>();
    m_hats_count = m_hats.Length;
    Debug.Log("Got " + m_hats_count.ToString() + " hats");
    UpdateHat();
  }

  // Update is called once per frame
  void Update()
  {
    if (m_change_hat_disable || !m_camera)
      return;

    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit_info;
      if (Physics.Raycast(ray.origin, ray.direction, out hit_info))
      {
        if (hit_info.collider.tag == "Hat")
        {
          m_enabled_hat = (m_enabled_hat + 1) % m_hats_count;
          PlayerPrefs.SetInt("HatIndex", m_enabled_hat);
          PlayerPrefs.Save();
          UpdateHat();
        }
      }
    }
  }
}

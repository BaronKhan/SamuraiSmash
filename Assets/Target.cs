using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
  public float speed = 5;
  public float scale = 0.2f;

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    float t = Time.time;
    float s = 0.1f + (scale * Mathf.Cos(speed * t));
    transform.localScale = new Vector3(s, s, s);
  }
}

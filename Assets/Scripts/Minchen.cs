using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public enum MinchenState
{
  Idle,
  Stance,
  Dash,
  Slash
}*/

public class Minchen : MonoBehaviour
{
  private float x = 0;
  private float z = 0;
  //Queue<>

  // Start is called before the first frame update
  void Start()
  {
    x = transform.position.x;
    z = transform.position.z;
  }

  // Update is called once per frame
  void Update()
  {
    //if (Input.GetTouch(0))
  }
}

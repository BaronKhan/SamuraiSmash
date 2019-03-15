using UnityEngine;

public class Rope : MonoBehaviour
{
  public Rigidbody hook;

  public GameObject linkPrefab;

  public int links = 14;
  
  void Start()
  {
    GenerateRope();
  }

  void GenerateRope()
  {
    Rigidbody previousRB = hook;
    for (int i = 0; i < links; ++i)
    {
      GameObject link = Instantiate(linkPrefab, transform);
      HingeJoint joint = link.GetComponent<HingeJoint>();
      joint.connectedBody = previousRB;
      previousRB = link.GetComponent<Rigidbody>();
    }
  }


}

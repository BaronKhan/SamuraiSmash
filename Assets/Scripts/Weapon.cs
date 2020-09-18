using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
  public float hit_sound_delay = 0f;

  private bool m_attacking = false;
  private AudioSource hit_sound = null;

  // Start is called before the first frame update
  void Start()
  {
    hit_sound = GetComponent<AudioSource>();
  }

  // Update is called once per frame
  void Update()
  {
        
  }

  public void SetAttack(bool attacking)
  {
    Debug.Log("Weapon attacking = " + attacking);
    if (!m_attacking && attacking)
    {
      hit_sound.PlayDelayed(hit_sound_delay);
    }
    m_attacking = attacking;
  }

  public bool IsAttacking()
  {
    return m_attacking;
  }
}

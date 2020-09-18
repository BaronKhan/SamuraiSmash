using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------

public enum MinchenState
{
  Idle,
  Stance,
  Dash,
  Slash
}

//-----------------------------------------------------------------------------

public class Minchen : MonoBehaviour
{

  //---------------------------------------------------------------------------

  public float movement_speed = 1f;
  public float rotate_speed = 10f;

  private float x = 0;
  private float z = 0;
  private GameObject target_enemy = null;
  private Animator animator = null;
  private MinchenState state = MinchenState.Stance;
  private GameObject weapon = null;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    x = transform.position.x;
    z = transform.position.z;
    animator = GetComponent<Animator>();
    weapon = FindChildWithTag(transform, "Weapon");
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    ProcessTouch();
    UpdateState();
  }

  //---------------------------------------------------------------------------

  void UpdateState()
  {
    MinchenState old_state = state;
    switch (state)
    {
      case MinchenState.Stance:
        {
          if (animator.GetCurrentAnimatorStateInfo(0).IsName("Stance"))
            SetWeaponAttack(false);
          if (target_enemy)
            state = MinchenState.Dash;
          break;
        }
      case MinchenState.Dash:
        {
          if (!MoveToEnemy())
            state = MinchenState.Slash;
          break;
        }
      case MinchenState.Slash:
        {
          SlashEnemy();
          if (animator.GetCurrentAnimatorStateInfo(0).IsName("Slash1") && !target_enemy)
          {
            state = MinchenState.Stance;
          }
          break;
        }
    }

    UpdateAnimation(old_state);
  }

  //-----------------------------------------------------------------------------

  void UpdateAnimation(MinchenState old_state)
  {
    animator.SetBool("isFighting", true);
    animator.SetBool("isWalking", state == MinchenState.Dash);
    animator.SetInteger("walkingSpeed", 6);

    if (old_state != state)
    {
      Debug.Log("State change from " + old_state + " to " + state);
      if (state == MinchenState.Slash)
        animator.SetTrigger("slash");
    }
  }

  //-----------------------------------------------------------------------------

  bool MoveToEnemy()
  {
    if (!target_enemy)
      return true;

    Vector3 v = (transform.position - target_enemy.transform.position);
    float distance = v.magnitude;
    Vector3 direction = v.normalized;
    if (distance < 2.5f)
    {
      return false;
    }
    else
    {
      float step = Time.deltaTime * movement_speed;
      transform.position = Vector3.MoveTowards(transform.position, target_enemy.transform.position, step);

      Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);
      transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotate_speed);
    }

    return true;
  }

  //-----------------------------------------------------------------------------

  void SlashEnemy()
  {
    target_enemy = null;
    SetWeaponAttack(true);
  }

  //-----------------------------------------------------------------------------

  private void SetWeaponAttack(bool attack)
  {
    ((Weapon)weapon.GetComponent(typeof(Weapon))).SetAttack(attack);
  }


  //-----------------------------------------------------------------------------

  void ProcessTouch()
  {
    if (Input.GetMouseButtonUp(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit_info;
      if (Physics.Raycast(ray.origin, ray.direction, out hit_info))
      {
        if (hit_info.collider.tag == "Enemy")
          target_enemy = hit_info.collider.gameObject;
      }
    }
  }

  //-----------------------------------------------------------------------------

  GameObject FindChildWithTag(Transform transform, string tag)
  {
    foreach (Transform child in transform)
    {
      if (child.tag == tag)
      {
        return child.gameObject;
      }
      else
      {
        GameObject child_child = FindChildWithTag(child, tag);
        if (child_child)
          return child_child;
      }
    }

    return null;
  }
}

﻿using System.Collections;
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
  private Vector3 target_pos;
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
    ResetTargetPosition();
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    ProcessTouch();
    UpdateState();
  }

  //---------------------------------------------------------------------------

  private void UpdateState()
  {
    MinchenState old_state = state;
    switch (state)
    {
      case MinchenState.Stance:
        {
          if (animator.GetCurrentAnimatorStateInfo(0).IsName("Stance"))
            SetWeaponAttack(false);
          if (target_enemy || target_pos != transform.position)
            state = MinchenState.Dash;
          break;
        }
      case MinchenState.Dash:
        {
          if (target_enemy)
          {
            if (!Move(target_enemy.transform.position))
              state = MinchenState.Slash;
          }
          else
          {
            if (!Move(target_pos))
              state = MinchenState.Stance;
          }
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

  private void UpdateAnimation(MinchenState old_state)
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

  private bool Move(Vector3 p)
  {
    Vector3 v = (transform.position - p);
    float distance = v.magnitude;
    Vector3 direction = v.normalized;
    Quaternion look_rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);
    if ((target_enemy && distance < 2.5f && Quaternion.Angle(transform.rotation, look_rotation) <= 5f) || (distance < 0.5f))
    {
      {
        ResetTargetPosition();
        return false;
      }
    }
    else
    {
      float step = Time.deltaTime * (movement_speed * (target_enemy ? 3 : 1));
      if (!target_enemy || distance >= 2.5f)
        transform.position = Vector3.MoveTowards(transform.position, p, step);
      transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * rotate_speed);
    }

    return true;
  }

  //-----------------------------------------------------------------------------

  private void SlashEnemy()
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

  private void ProcessTouch()
  {
    if (Input.GetMouseButtonUp(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit_info;
      if (Physics.Raycast(ray.origin, ray.direction, out hit_info))
      {
        if (hit_info.collider.tag == "Enemy")
          target_enemy = hit_info.collider.gameObject;
        else if (hit_info.collider.tag == "Floor")
        {
          Debug.Log("Clicked floor");
          target_pos = new Vector3(hit_info.point.x, transform.position.y, hit_info.point.z);
          /*RaycastHit hit_info_hori;
          if (Physics.Raycast(transform.position, (target_pos - transform.position), out hit_info_hori))
          {
            if (hit_info_hori.collider.tag == "Enemy")
            {
              Debug.Log("Enemy in the way");
              target_enemy = hit_info_hori.collider.gameObject;
              target_pos = transform.position;
            }
          }*/
        }
      }
    }
  }

  //-----------------------------------------------------------------------------

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Minchen OnTriggerEnter");
    if (other.tag == "Enemy")
    {
      Debug.Log("Collided with Enemy");
      ResetTargetPosition();
    }
  }

  //-----------------------------------------------------------------------------

  private void ResetTargetPosition()
  {
    target_pos = transform.position;
  }

  //-----------------------------------------------------------------------------

  private GameObject FindChildWithTag(Transform transform, string tag)
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

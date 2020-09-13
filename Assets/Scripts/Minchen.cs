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

  private Animator animator = null;

  private MinchenState state = MinchenState.Stance;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    x = transform.position.x;
    z = transform.position.z;
    animator = GetComponent<Animator>();
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    processTouch();
    step();
  }

  //---------------------------------------------------------------------------

  void step()
  {
    updateState();

    if (state == MinchenState.Dash)
      moveToEnemy();
  }

  //---------------------------------------------------------------------------

  void updateState()
  {
    MinchenState old_state = state;
    switch (state)
    {
      case MinchenState.Stance:
        {
          if (target_enemy)
            state = MinchenState.Dash;
          break;
        }
      case MinchenState.Dash:
        {
          if (!target_enemy)
            state = MinchenState.Stance;
          break;
        }
      case MinchenState.Slash:
        {
          state = MinchenState.Stance;
          break;
        }
    }
    if (old_state != state)
    {
      print(state);
    }
    updateAnimation();
  }

  //-----------------------------------------------------------------------------

  void updateAnimation()
  {
    animator.SetBool("isFighting", state != MinchenState.Idle);
    animator.SetBool("isWalking", state == MinchenState.Dash);
    animator.SetInteger("walkingSpeed", 6);
  }

  //-----------------------------------------------------------------------------

  void moveToEnemy()
  {
    if (!target_enemy)
      return;

    Vector3 v = (transform.position - target_enemy.transform.position);
    float distance = v.magnitude;
    Vector3 direction = v.normalized;
    if (distance < 2.5f)
    {
      target_enemy = null;
      state = MinchenState.Slash;
    }
    else
    {
      float step = Time.deltaTime * movement_speed;
      transform.position = Vector3.MoveTowards(transform.position, target_enemy.transform.position, step);

      Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);

      //rotate us over time according to speed until we are in the required rotation
      transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotate_speed);
    }
  }

  //-----------------------------------------------------------------------------

  void processTouch()
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

}

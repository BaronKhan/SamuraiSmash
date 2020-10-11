﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------

public enum EnemyState
{
  Idle,
  Stance,
  Walk,
  Slash
}

//-----------------------------------------------------------------------------

public class Enemy : MonoBehaviour
{
  public float hit_speed = 10.0f;
  public Vector3 target_pos = new Vector3(0, 0, 0);

  private bool hit = false;
  private Vector3 direction = new Vector3(0, 0, 0);
  private Animator animator = null;

  private GameObject head_text = null;
  private GameObject head = null;

  private bool is_lowest = true;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    animator = GetComponent<Animator>();

    if (target_pos.magnitude == 0f)
      target_pos = transform.position;

    head = FindChildWithTag(transform, "Head");
    head_text = FindChildWithTag(transform, "UI");
    UpdateHead();
  }

  //---------------------------------------------------------------------------

  private void UpdateHead()
  {
    head.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
    head_text.transform.rotation = Quaternion.Euler(new Vector3(45, 270, 0));
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    MoveEnemy();
    UpdateHead();
  }

  //---------------------------------------------------------------------------

  private void MoveEnemy()
  {
    if (hit)
    {
      float step = Time.deltaTime * hit_speed;
      transform.position += direction;
    }
    else
    {

    }
  }

  //---------------------------------------------------------------------------

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Enemy onTriggerEnter");
    if (hit)
      return;

    if (other.tag == "Weapon")
      CollisionWithWeapon(other);
  }

  //---------------------------------------------------------------------------

  private void CollisionWithWeapon(Collider other)
  {
    Debug.Log("Enemy collided with weapon");
    if (WeaponIsAttacking(other))
    {
      Debug.Log("Enemy hit with weapon");
      if (is_lowest)  // is_lowest
      {
        hit = true;
        direction = Quaternion.Euler(0, 90, 0) * (-transform.forward);
        if (animator)
        {
          animator.SetBool("isFighting", false);
          animator.SetBool("isWalking", false);
        }
      }
      else
      {
        Minchen player = (Minchen)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(Minchen));
        player.Die(); 
      }
    }
  }

  //---------------------------------------------------------------------------

  private bool WeaponIsAttacking(Collider other)
  {
    return ((Weapon)other.gameObject.GetComponent(typeof(Weapon))).IsAttacking();
  }

  //---------------------------------------------------------------------------

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

  //---------------------------------------------------------------------------
}

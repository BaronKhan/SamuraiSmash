using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------

public enum EnemyState
{
  Idle,
  Stance,
  Walk,
  Slash,
  Dead
}

//-----------------------------------------------------------------------------

public class Enemy : MonoBehaviour
{
  public float movement_speed = 1f;
  public float hit_speed = 10.0f;
  public float rotate_speed = 10f;
  public Vector3 target_pos = new Vector3(0, 0, 0);

  private Vector3 direction = new Vector3(0, 0, 0);
  private Animator animator = null;

  private GameObject head_text = null;
  private GameObject head = null;

  private bool is_lowest = true;
  private EnemyState state = EnemyState.Stance;
  private Minchen minchen = null;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    minchen = (Minchen)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(Minchen));
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
    EnemyState old_state = state;
    switch (state)
    {
      case EnemyState.Stance:
      {
        Quaternion look_rotation = Quaternion.LookRotation(minchen.transform.position - transform.position) * Quaternion.Euler(0, 270, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * rotate_speed);
        if (transform.position != target_pos)
          state = EnemyState.Walk;
        break;
      }
      case EnemyState.Dead:
      {
        float step = Time.deltaTime * hit_speed;
        transform.position += direction;
        break;
      }
      case EnemyState.Walk:
      {
        MoveEnemy(target_pos);
        break;
      }
    }
    UpdateAnimation(old_state);
    UpdateHead();
  }

  //---------------------------------------------------------------------------

  private void MoveEnemy(Vector3 p)
  {
    Vector3 v = (transform.position - p);
    float distance = v.magnitude;
    Vector3 direction = v.normalized;
    Quaternion look_rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);
    if (distance <= 0.01f)
    {
      {
        target_pos = transform.position;
        state = EnemyState.Stance;
        return;
      }
    }
    else
    {
      float step = Time.deltaTime * movement_speed;
      if (distance >= 2.5f)
        transform.position = Vector3.MoveTowards(transform.position, p, step);
      transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * rotate_speed);
    }
  }

  //---------------------------------------------------------------------------

  private void OnTriggerEnter(Collider other)
  {
    EnemyState old_state = state;
    Debug.Log("Enemy onTriggerEnter");
    if (state == EnemyState.Dead)
      return;

    if (other.tag == "Weapon")
      CollisionWithWeapon(other);
    UpdateAnimation(old_state);
    UpdateHead();
  }

  //---------------------------------------------------------------------------

  private void UpdateAnimation(EnemyState old_state)
  {
    animator.SetBool("isFighting", state != EnemyState.Dead);
    animator.SetBool("isWalking", state == EnemyState.Walk);
    animator.SetInteger("walkingSpeed", (state == EnemyState.Dead) ? 0 : 6);

    if (old_state != state)
    {
      Debug.Log("Enemy state change from " + old_state + " to " + state);
      if (state == EnemyState.Slash)
        animator.SetTrigger("slash");
    }
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
        state = EnemyState.Dead;
        direction = Quaternion.Euler(0, 90, 0) * (-transform.forward);
      }
      else
      {
        minchen.Die(); 
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

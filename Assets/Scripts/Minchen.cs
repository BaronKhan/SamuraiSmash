using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------

public enum MinchenState
{
  Idle,
  Stance,
  Dash,
  Slash,
  Dead
}

//-----------------------------------------------------------------------------

public class Minchen : MonoBehaviour
{

  //---------------------------------------------------------------------------

  public float movement_speed = 1f;
  public float rotate_speed = 10f;
  public float hit_speed = 10.0f;

  private float x = 0;
  private float z = 0;
  private GameObject target_enemy = null;
  private Vector3 target_pos;
  public GameObject old_target_enemy;
  private Animator animator = null;
  private MinchenState state = MinchenState.Stance;
  private GameObject weapon = null;
  private AudioSource dead_sound = null;

  private readonly static float enemy_min_dist = 2.5f;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    x = transform.position.x;
    z = transform.position.z;
    animator = GetComponent<Animator>();
    weapon = FindChildWithTag(transform, "Weapon");
    dead_sound = GetComponent<AudioSource>();
    ResetTargetPosition();
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    ProcessTouch();
    UpdateState();

    if (OutsideView())
    {
      Destroy(gameObject);
    }
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
          // If holding down, weapon sound only plays once, reset weapon
          SetWeaponAttack(false);
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
      case MinchenState.Dead:
        {
          float step = Time.deltaTime * hit_speed;
          transform.position += Quaternion.Euler(0, 90, 0) * (-transform.forward); ;
          break;
        }
    }

    UpdateAnimation(old_state);
  }

  //-----------------------------------------------------------------------------

  private void UpdateAnimation(MinchenState old_state)
  {
    animator.SetBool("isFighting", state != MinchenState.Dead);
    animator.SetBool("isWalking", state == MinchenState.Dash);
    animator.SetInteger("walkingSpeed", (state == MinchenState.Dead) ? 0 : 6);

    if (old_state != state)
    {
      Debug.Log("Minchen state change from " + old_state + " to " + state);
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
    if ((target_enemy && distance < enemy_min_dist && Quaternion.Angle(transform.rotation, look_rotation) <= 5f) || (distance < 0.5f))
    {
      {
        ResetTargetPosition();
        return false;
      }
    }
    else
    {
      float step = Time.deltaTime * (movement_speed * (target_enemy ? 3 : 1));
      if (!target_enemy || distance >= enemy_min_dist)
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
    if (state == MinchenState.Slash)
      return;

    if (Input.GetMouseButton(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit_info;
      if (Physics.Raycast(ray.origin, ray.direction, out hit_info))
      {
        if (hit_info.collider.tag == "Enemy")
        {
          target_enemy = hit_info.collider.gameObject;
          target_pos = transform.position;
        }
        else if (hit_info.collider.tag == "Floor")
        {
          // Debug.Log("Clicked floor");
          target_pos = new Vector3(hit_info.point.x, transform.position.y, hit_info.point.z);
          target_enemy = null;
        }

        // Ray cast from position to target and search for obstacles
        Vector3 raycast_pos = (target_enemy) ? target_enemy.transform.position : target_pos;
        RaycastHit hit_info_hori;
        if (Physics.Raycast(transform.position, (raycast_pos - transform.position), out hit_info_hori))
        {
          if (hit_info_hori.collider.tag == "Enemy")
          {
            GameObject raycast_obj = hit_info_hori.collider.gameObject;
            if ((raycast_obj.transform.position - transform.position).magnitude < (raycast_pos - transform.position).magnitude)
            {
              Debug.Log("Enemy in the way");
              Vector3 dist = (raycast_obj.transform.position - transform.position);
              if (dist.magnitude <= enemy_min_dist)
                target_pos = transform.position;
              else
                target_pos = raycast_obj.transform.position;
              target_enemy = null;
            }
          }
        }
      }
      old_target_enemy = target_enemy;
    }
  }

  //-----------------------------------------------------------------------------

  private void OnTriggerEnter(Collider other)
  {
    // Debug.Log("Minchen OnTriggerEnter");
    if (other.tag == "Enemy")
    {
      Debug.Log("Collided with Enemy");
      ResetTargetPosition();
    }
    else if (other.tag == "Weapon")
    {
      CollisionWithWeapon(other);
    }
  }

  //---------------------------------------------------------------------------

  private void CollisionWithWeapon(Collider other)
  {
    if (other.gameObject == weapon)  // ignore own weapon
      return;

    Debug.Log("Minchen collided with weapon");
    if (WeaponIsAttacking(other))
    {
      Debug.Log("Minchen hit by enemy weapon");
      Vector3 direction = (transform.position - other.transform.position).normalized;
      Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);
      transform.rotation = lookRotation;
      Die();
    }
  }

  //---------------------------------------------------------------------------

  private bool WeaponIsAttacking(Collider other)
  {
    Weapon w = ((Weapon)other.gameObject.GetComponent(typeof(Weapon)));
    return w.is_enemy_weapon && w.IsAttacking() && !target_enemy && state != MinchenState.Slash;
  }

  //-----------------------------------------------------------------------------

  private void ResetTargetPosition()
  {
    target_pos = transform.position;
  }

  //-----------------------------------------------------------------------------

  public void Die()
  {
    dead_sound.Play();
    state = MinchenState.Dead;
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

  //-----------------------------------------------------------------------------

  private bool OutsideView()
  {
    return transform.position.magnitude > 50;
  }

  //-----------------------------------------------------------------------------
}

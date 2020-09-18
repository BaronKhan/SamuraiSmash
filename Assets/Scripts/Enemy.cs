using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
  public float hit_speed = 10.0f;

  private bool hit = false;
  private Vector3 direction = new Vector3(0, 0, 0);

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    MoveEnemy();
  }

  private void MoveEnemy()
  {
    if (hit)
    {
      float step = Time.deltaTime * hit_speed;
      transform.position += direction;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Enemy onTriggerEnter");
    if (hit)
      return;

    if (other.tag == "Weapon")
      CollisionWithWeapon(other);
  }

  private void CollisionWithWeapon(Collider other)
  {
    Debug.Log("Enemy collided with weapon");
    if (WeaponIsAttacking(other))
    {
      Debug.Log("Enemy hit with weapon");
      hit = true;
      direction = Quaternion.Euler(0, 90, 0) * (-transform.forward);
    }
  }

  private bool WeaponIsAttacking(Collider other)
  {
    return ((Weapon)other.gameObject.GetComponent(typeof(Weapon))).IsAttacking();
  }
}

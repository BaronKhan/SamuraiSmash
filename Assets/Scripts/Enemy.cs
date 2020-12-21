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

public class Enemy : MonoBehaviour, System.IComparable<Enemy>
{
  public HeadText.TextType head_text_type = HeadText.TextType.Int;
  public int head_text_int = 0;
  public float head_text_float = 0.0f;
  public char head_text_symbol = 'e';
  public int head_text_num = 0;
  public int head_text_den = 0;
  public bool head_text_updated = false;

  public float movement_speed = 3f;
  public float hit_speed = 10.0f;
  public float rotate_speed = 10f;
  public Vector3 target_pos = new Vector3(0, 0, 0);
  public float move_distance = 3f;

  private Vector3 direction = new Vector3(0, 0, 0);
  private Animator animator = null;

  public GameObject head_text = null;
  private GameObject head = null;
  private GameObject weapon = null;
  private AudioSource dead_sound = null;

  private bool is_lowest = false;
  private EnemyState state = EnemyState.Stance;
  private Minchen minchen = null;

  private CTRL ctrl = null;

  private bool ready = false;

  private double move_time_ms = 0.0;

  private Renderer anger_symbol_renderer = null;
  public Renderer red_circle_renderer = null;
  private bool is_waiting = false;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    animator = GetComponent<Animator>();
    dead_sound = GetComponent<AudioSource>();

    if (target_pos.magnitude == 0f)
    {
      ready = true;
      target_pos = transform.position;
    }
    else
    {
      state = EnemyState.Walk;
    }

    head = FindChildWithTag(transform, "Head");
    head_text = FindChildWithTag(transform, "UI");
    UpdateHead();

    weapon = FindChildWithTag(transform, "Weapon");

    GameObject ctrl_obj = GameObject.FindWithTag("GameController");
    if (ctrl_obj)
      ctrl = ((CTRL)ctrl_obj.GetComponent(typeof(CTRL)));

    GameObject anger_symbol = FindChildWithTag(transform, "Symbol");
    anger_symbol_renderer = anger_symbol.GetComponent<Renderer>();
    anger_symbol_renderer.enabled = false;

    GameObject red_circle = FindChildWithTag(transform, "RedCircle");
    red_circle_renderer = red_circle.GetComponent<Renderer>();
    red_circle_renderer.enabled = false;
  }

  //---------------------------------------------------------------------------

  private void UpdateHead()
  {
    if (!head)
      return;
    head.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
    head_text.transform.rotation = Quaternion.Euler(new Vector3(45, 270, 0));
  }

  //---------------------------------------------------------------------------

  private void UpdateHeadText()
  {
    var ht = ((HeadText)head_text.GetComponent(typeof(HeadText)));
    switch (head_text_type)
    {
      case HeadText.TextType.Int:
        {
          ht.SetInteger(head_text_int);
          break;
        }
      case HeadText.TextType.Float:
        {
          ht.SetFloat(head_text_float);
          break;
        }
      case HeadText.TextType.Fraction:
        {
          ht.SetFraction(head_text_num, head_text_den);
          break;
        }
      case HeadText.TextType.Symbol:
        {
          ht.SetSymbol(head_text_symbol);
          break;
        }
    }

  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    if (minchen && minchen.Dead)
    {
      animator.SetBool("isWalking", false);
      return;
    }

    if (!head_text_updated && head_text)
      UpdateHeadText();

    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (!player)
    {
      minchen = null;
      return;
    }
    minchen = (Minchen)player.GetComponent(typeof(Minchen));

    EnemyState old_state = state;
    switch (state)
    {
      case EnemyState.Stance:
      {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Stance"))
          SetWeaponAttack(false);
        Quaternion look_rotation = Quaternion.LookRotation(minchen.transform.position - transform.position) * Quaternion.Euler(0, 270, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * rotate_speed);
        if (transform.position != target_pos)
          state = EnemyState.Walk;
        if (!is_waiting)
          StartCoroutine(WaitToMove());
        break;
      }
      case EnemyState.Dead:
      {
        transform.position += direction;
        break;
      }
      case EnemyState.Walk:
      {
        SetWeaponAttack(false);
        MoveEnemy(target_pos);
        anger_symbol_renderer.enabled = false;
        break;
      }
      case EnemyState.Slash:
      {
        SlashEnemy();
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Slash1"))
        {
          state = EnemyState.Stance;
        }
        break;
      }
    }

    UpdateAnimation(old_state);
    UpdateHead();

    if (OutsideView())
    {
      Destroy(gameObject);
    }
  }

  //-----------------------------------------------------------------------------

  private void SlashEnemy()
  {
    SetWeaponAttack(true);
  }

  //-----------------------------------------------------------------------------

  private void SetWeaponAttack(bool attack)
  {
    ((Weapon)weapon.GetComponent(typeof(Weapon))).SetAttack(attack);
  }

  //---------------------------------------------------------------------------

  IEnumerator WaitToMove()
  {
    if (is_waiting)
      yield break;

    is_waiting = true;

    yield return new WaitForSeconds(Random.Range(2, 5));

    anger_symbol_renderer.enabled = true;

    yield return new WaitForSeconds(1);

    anger_symbol_renderer.enabled = false;

    if (state == EnemyState.Stance)
    {
      SetNewTargetPos();
    }

    is_waiting = false;

    if (!head_text.activeSelf)
      head_text.SetActive(true);
  }

  //---------------------------------------------------------------------------

  void SetNewTargetPos()
  {
    if (!minchen)
      return;
    Vector3 v = (minchen.transform.position - transform.position).normalized;
    target_pos = transform.position + (v * move_distance);
    state = EnemyState.Walk;
    move_time_ms = System.DateTime.Now.Millisecond;
  }

  //---------------------------------------------------------------------------

  private void MoveEnemy(Vector3 p)
  {
    Vector3 v = (transform.position - p);
    float distance = v.magnitude;
    Vector3 direction = v.normalized;
    Quaternion look_rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);
    float player_distance = (transform.position - minchen.transform.position).magnitude;
    if (player_distance <= 2.5f)
    {
      target_pos = transform.position;
      state = EnemyState.Slash;
      return;
    }
    else if (distance <= 0.2f)
    {
      target_pos = transform.position;
      if (!ready)
        ready = true;
      state = EnemyState.Stance;
      return;
    }
    else
    {
      float step = Time.deltaTime * movement_speed;
      if (distance >= 0.1f)
        transform.position = Vector3.MoveTowards(transform.position, p, step * (ready ? 1 : movement_speed));
      transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * rotate_speed);
    }
  }

  //---------------------------------------------------------------------------

  private void OnTriggerEnter(Collider other)
  {
    EnemyState old_state = state;
    // Debug.Log("Enemy onTriggerEnter");
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
    if (!animator)
      return;
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
      bool player_target = (minchen.old_target_enemy == gameObject);
      if (player_target)
      {
        if (is_lowest)
        {
          dead_sound.Play();
          state = EnemyState.Dead;
          direction = Quaternion.Euler(0, 90, 0) * (-transform.forward);
          if (ctrl)
            ctrl.OnEnemyDead((Enemy)gameObject.GetComponent(typeof(Enemy)));
        }
        else
        {
          minchen.Die();
        }
      }
    }
  }

  //---------------------------------------------------------------------------

  private bool WeaponIsAttacking(Collider other)
  {
    Weapon w = ((Weapon)other.gameObject.GetComponent(typeof(Weapon)));
    return !w.is_enemy_weapon && w.IsAttacking();
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

  private bool OutsideView()
  {
    return transform.position.magnitude > 50;
  }

  //---------------------------------------------------------------------------

  public double GetValue()
  {
    switch (head_text_type)
    {
      case HeadText.TextType.Int:
        return head_text_int;
      case HeadText.TextType.Float:
        return head_text_float;
      case HeadText.TextType.Fraction:
        return (head_text_den == 0) ? 0 : ((double)head_text_num) / ((double)head_text_den);
      case HeadText.TextType.Symbol:
        return GetSymbolValue(head_text_symbol);
      default:
        return 0;
    }
  }

  //---------------------------------------------------------------------------

  private double GetSymbolValue(char symbol)
  {
    switch (symbol)
    {
      case 'e': return Mathf.Exp(1);
      case 'π': return Mathf.PI;
      case 'φ': return 1.61803398875;
    }

    Debug.LogWarning("Unknown symbol " + symbol);
    return 0.0;
  }

  //---------------------------------------------------------------------------

  int System.IComparable<Enemy>.CompareTo(Enemy other)
  {
    return GetValue().CompareTo(GetValue());
  }

  //---------------------------------------------------------------------------

  public void SetLowest(bool lowest)
  {
    is_lowest = lowest;
  }

  //---------------------------------------------------------------------------

  void OnApplicationFocus(bool hasFocus)
  {
    head_text.SetActive(hasFocus);
  }

  //---------------------------------------------------------------------------

  void OnApplicationPause(bool pauseStatus)
  {
    head_text.SetActive(!pauseStatus);
  }
}

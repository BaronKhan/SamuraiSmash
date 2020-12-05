using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GroupType
{
  Range,
  Negative,
  /*
   Rearrange,
   Decimal,
   Fractions,
   Constants,
   Randomise */
}

public class CTRL : MonoBehaviour
{
  private static readonly int s_max_enemy_count = 6;
  private static readonly float s_enemy_y = 0.4154629f;  // TODO: calibrate this
  private static readonly float s_enemy_below_offset = 12f;
  private static readonly float s_enemy_above_offset = 12f;

  public Enemy enemy_prefab = null;

  private SortedList<double, Enemy> m_enemies = new SortedList<double, Enemy>();
  private Minchen minchen = null;

  /*private int m_level = 1;*/

  public int m_score = 0;

  private bool flip_enemies = false;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (!player)
    {
      minchen = null;
      return;
    }
    minchen = (Minchen)player.GetComponent(typeof(Minchen));

    AddEnemies(3);
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {

  }

  private GroupType GetRandomGroupType()
  {
    float prob = UnityEngine.Random.Range(0.0f, 1.0f);
    float n = Enum.GetNames(typeof(GroupType)).Length;
    for (int i = 1; i <= n; ++i)
    {
      if (prob < (i / n))
        return (GroupType)(i - 1);
    }

    Debug.LogWarning("Returning default group type");
    return GroupType.Range;
  }

  //---------------------------------------------------------------------------

  private void AddEnemies(int enemy_count = 3)
  {
    (float, float, bool)[] positions_x_z_below  = {
      (3.7f, 2.4f, true),
      (3.7f, -2.4f, true),
      (5.297773f, 0, true),
      (-6.297773f, 0, false),
      (-4.7f, 2.4f, false),
      (-4.7f, -2.4f, false),
    };

    if (flip_enemies)
      Array.Reverse(positions_x_z_below);
    flip_enemies = !flip_enemies;

    for (int i = 0; i < enemy_count; ++i)
    {
      var p = positions_x_z_below[i];
      Enemy new_enemy = AddEnemy(new Vector3(p.Item1, s_enemy_y, p.Item2), p.Item3);

      // get uniform distribution of group types, but only activate later ones based on level
      GroupType group_type = GetRandomGroupType();

      SetEnemyValue(new_enemy, group_type);

      m_enemies.Add(new_enemy.GetValue(), new_enemy);
    }

    m_enemies.Values.First().SetLowest(true);
  }

  //---------------------------------------------------------------------------

  private void SetRangeValue(Enemy new_enemy)
  {
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(1, 10);
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetNegativeValue(Enemy new_enemy)
  {
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(-10, 10);
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetEnemyValue(Enemy new_enemy, GroupType type)
  {
    switch(type)
    {
      case GroupType.Range:
        {
          SetRangeValue(new_enemy);
          break;
        }
       case GroupType.Negative:
        {
          SetNegativeValue(new_enemy);
          break;
        }
    }
  }

  //---------------------------------------------------------------------------

  private Enemy AddEnemy(Vector3 target_pos, bool is_below = true)
  {
    if (is_below)
    {
      Enemy enemy = Instantiate(enemy_prefab, target_pos + new Vector3(s_enemy_below_offset, 0), Quaternion.Euler(0, 180, 0));
      enemy.target_pos = enemy.transform.position - new Vector3(s_enemy_below_offset, 0);
      return enemy;
    }
    else
    {
      Enemy enemy = Instantiate(enemy_prefab, target_pos - new Vector3(s_enemy_above_offset, 0), Quaternion.Euler(0, 0, 0));
      enemy.target_pos = enemy.transform.position + new Vector3(s_enemy_above_offset, 0);
      return enemy;
    }
  }

  IEnumerator AddEnemiesAfterTime(float time)
  {
    yield return new WaitForSeconds(time);
    AddEnemies();
  }

  //---------------------------------------------------------------------------

  private void ResetState()
  {
    minchen.Reset();
    StartCoroutine(AddEnemiesAfterTime(1.0f));
  }

  //---------------------------------------------------------------------------

  public void OnEnemyDead(Enemy enemy)
  {
    Debug.Log("OnEnemyDead");

    ++m_score;

    if (enemy != m_enemies.Values.First())
      Debug.LogWarning("Enemy was defeated but isn't at the front of the list");
    m_enemies.RemoveAt(0);
    if (m_enemies.Count == 0)
      ResetState();

    else
      m_enemies.Values.First().SetLowest(true);
  }
}

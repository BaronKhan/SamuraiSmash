using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GroupType
{
  Range,
  Rearrange,
  Negative,
  Decimal,
  Fractions,
  Constants,
  Randomise
}

public class CTRL : MonoBehaviour
{
  private static readonly int s_max_enemy_count = 6;
  private static readonly float s_enemy_y = 0.4154629f;  // TODO: calibrate this
  private static readonly float s_enemy_below_offset = 10f;

  public Enemy enemy_prefab = null;

  private GameObject m_player;
  private SortedList<double, Enemy> m_enemies = new SortedList<double, Enemy>();

  // private int m_level = 1;

  public int m_score = 0;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    AddEnemies(3);
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {

  }

  //---------------------------------------------------------------------------

  private void AddEnemies(int enemy_count = 6)
  {
    (float, float, bool)[] positions_x_z_below  = {
      (5.297773f, 0, true),
      (3.7f, 2.4f, true),
      (3.7f, -2.4f, true),
    };

    foreach (var p in positions_x_z_below)
    {
      Enemy new_enemy = AddEnemy(new Vector3(p.Item1, s_enemy_y, p.Item2), p.Item3);
      SetEnemyValue(new_enemy, GroupType.Range);
      m_enemies.Add(new_enemy.GetValue(), new_enemy);
    }

    m_enemies.Values.First().SetLowest(true);
  }

  //---------------------------------------------------------------------------

  private void SetEnemyValue(Enemy new_enemy, GroupType type)
  {
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(1, 10);
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private Enemy AddEnemy(Vector3 target_pos, bool is_below = true)
  {
    Enemy enemy = Instantiate(enemy_prefab, target_pos + new Vector3(s_enemy_below_offset, 0), Quaternion.Euler(0, 180, 0));
    enemy.target_pos = enemy.transform.position - new Vector3(s_enemy_below_offset, 0);
    return enemy;
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
      AddEnemies();
    else
      m_enemies.Values.First().SetLowest(true);
  }
}

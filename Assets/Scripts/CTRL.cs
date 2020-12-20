﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GroupType
{
  Range,
  Negative,
  Rearrange,
  Decimal,
  Fractions,
  /*Constants,
  Randomise*/
}

public class CTRL : MonoBehaviour
{
  public GameObject restart = null;
  public GameObject home = null;

  private static readonly float s_enemy_y = 0.4154629f;  // TODO: calibrate this
  private static readonly float s_enemy_below_offset = 11f;
  private static readonly float s_enemy_above_offset = 11f;

  public Enemy enemy_prefab = null;

  private SortedList<double, Enemy> m_enemies = new SortedList<double, Enemy>();
  private Minchen minchen = null;

  /*private int m_level = 1;*/

  private Score m_score = null;

  private bool flip_enemies = false;
  private int[] rearrange_digits = { 0, 0, 0, 0, 0 };

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    if (restart)
      restart.SetActive(false);
    if (home)
      home.SetActive(false);
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (!player)
    {
      minchen = null;
      return;
    }
    minchen = (Minchen)player.GetComponent(typeof(Minchen));

    m_score = GetComponentInChildren<Score>();

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
      (2.7f, 2.4f, true),
      (2.7f, -2.4f, true),
      (5.797773f, 0, true),
      (-6.797773f, 0, false),
      (-3.7f, 2.4f, false),
      (-3.7f, -2.4f, false),
    };

    if (flip_enemies)
      Array.Reverse(positions_x_z_below);
    flip_enemies = !flip_enemies;

    // get uniform distribution of group types, but only activate later ones based on level
    GroupType group_type = GetRandomGroupType();

    for (int i = 0; i < enemy_count; ++i)
    {
      var p = positions_x_z_below[i];
      Enemy new_enemy = AddEnemy(new Vector3(p.Item1, s_enemy_y, p.Item2), p.Item3);

      SetEnemyValue(new_enemy, group_type);

      m_enemies.Add(new_enemy.GetValue(), new_enemy);
    }

    m_enemies.Values.First().SetLowest(true);

    Debug.Log($"Smallest: {m_enemies.Keys.First()}, {m_enemies.Values.First()}");
  }

  //---------------------------------------------------------------------------

  private void SetRangeValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Int;
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(1, 10);
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetNegativeValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Int;
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(-10, 10);
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetRearrangeValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Int;
    int multiplier = 0;
    // group into 3, if new group, generate new 5 digit array, choose random rearrangement otherwise
    if ((m_enemies.Count() % 3) == 0)
    {
      int val = UnityEngine.Random.Range(100 * (int)Math.Pow(10, multiplier), 1000 * (int)Math.Pow(10, multiplier));
      new_enemy.head_text_int = val;
      for (int i = 0; i < 3 + multiplier; ++i)
      {
        rearrange_digits[i] = val % 10;
        // avoid 0 digits for now...
        if (rearrange_digits[i] == 0)
          rearrange_digits[i] = UnityEngine.Random.Range(1, 10);
        val /= 10;
      }
    }
    else
    {
      do
      {
        // Fisher-Yates shuffle algorithm
        for (int i = 2 + multiplier; i >= 0; --i)
        {
          int j = UnityEngine.Random.Range(0, i);
          int x = rearrange_digits[i] ^ rearrange_digits[j];
          rearrange_digits[i] = rearrange_digits[i] ^ x;
          rearrange_digits[j] = rearrange_digits[i] ^ x;
        }
        // convert rearrange_digits
        int val = 0;
        for (int i = 0; i < 3 + multiplier; ++i)
        {
          val += (int)(rearrange_digits[i] * (Math.Pow(10, i)));
        }
        new_enemy.head_text_int = val;
      }
      while (m_enemies.ContainsKey(new_enemy.GetValue()));
    }
  }

  //---------------------------------------------------------------------------

  private void SetDecimalValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Float;
    do
    {
      new_enemy.head_text_float = UnityEngine.Random.Range(-10.0f, 10.0f);
    }
    while (EnemiesHaveValueWithinRange(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------
  private void SetFractionalValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Fraction;
    do
    {
      int num = UnityEngine.Random.Range(1, 5);
      int den = UnityEngine.Random.Range(num+1, 10);
      new_enemy.head_text_num = num;
      new_enemy.head_text_den = den;
    }
    while (EnemiesHaveValueWithinRange(new_enemy.GetValue()));
  }


  //---------------------------------------------------------------------------

  private bool EnemiesHaveValueWithinRange(double value)
  {
    foreach (var enemy in m_enemies)
    {
      if (Math.Abs(enemy.Key - value) < 0.1f)
        return true;
    }

    return false;
  }

  //---------------------------------------------------------------------------

  private void SetEnemyValue(Enemy new_enemy, GroupType type)
  {
    // TODO: map of range type to callback
    switch (type)
    {
      case GroupType.Range    : { SetRangeValue(new_enemy)     ; break; }
      case GroupType.Negative : { SetNegativeValue(new_enemy)  ; break; }
      case GroupType.Rearrange: { SetRearrangeValue(new_enemy) ; break; }
      case GroupType.Decimal  : { SetDecimalValue(new_enemy)   ; break; }
      case GroupType.Fractions: { SetFractionalValue(new_enemy); break; }
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

    m_score.Increment();

    if (enemy != m_enemies.Values.First())
      Debug.LogWarning("Enemy was defeated but isn't at the front of the list");
    m_enemies.RemoveAt(0);
    if (m_enemies.Count == 0)
      ResetState();

    else
      m_enemies.Values.First().SetLowest(true);
  }

  //---------------------------------------------------------------------------

  public void OnGameOver(bool show_symbol = true)
  {
    Debug.Log("Game Over");
    if (show_symbol)
      m_enemies.ElementAt(0).Value.red_circle_renderer.enabled = true;
  }

  //---------------------------------------------------------------------------

  public void OnMinchenDestroyed()
  {
    Debug.Log("Minchen Destroyed");
    if (restart)
      restart.SetActive(true);
    if (home)
      home.SetActive(true);
  }
}

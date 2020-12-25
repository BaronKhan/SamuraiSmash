using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GroupType
{
  Range,
  Negative,
  Rearrange,
  Decimal,
  Constants,
  Fractions,
  /*Randomise*/
}

public class CTRL : MonoBehaviour
{
  public GameObject restart = null;
  public GameObject home = null;
  public GameObject high_score_text = null;
  public Text dead_high_score_text = null;

  private static readonly float s_enemy_y = 0.4154629f;  // TODO: calibrate this
  private static readonly float s_enemy_below_offset = 21f;
  private static readonly float s_enemy_above_offset = 21f;

  public Enemy enemy_prefab = null;

  private SortedList<double, Enemy> m_enemies = new SortedList<double, Enemy>();
  private Minchen minchen = null;

  private int m_level = 1;
  private float m_multiplier_scaler = 1.5f;

  private Score m_score = null;

  private bool flip_enemies = false;
  private int[] rearrange_digits = { 0, 0, 0, 0, 0 };

  private bool slow_update = false;

  //---------------------------------------------------------------------------

  // Start is called before the first frame update
  void Start()
  {
    if (restart)
      restart.SetActive(false);
    if (home)
      home.SetActive(false);
    if (high_score_text)
      high_score_text.SetActive(false);
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (!player)
    {
      minchen = null;
      return;
    }
    minchen = (Minchen)player.GetComponent(typeof(Minchen));

    m_score = GetComponentInChildren<Score>();

    AddEnemies(6);
  }

  //---------------------------------------------------------------------------

  // Update is called once per frame
  void Update()
  {
    if (!slow_update)
      StartCoroutine(SlowUpdate());
  }

  //---------------------------------------------------------------------------

  private IEnumerator SlowUpdate()
  {
    slow_update = true;

    yield return new WaitForSeconds(1);

    ShowFtue();

    // paranoia
    if (m_enemies.Values.Count > 0)
      m_enemies.Values.First().SetLowest(true);

    slow_update = false;
  }

  //---------------------------------------------------------------------------

  private GroupType GetRandomGroupType()
  {
    float bias = UnityEngine.Random.Range(0.0f, Mathf.Min(0.05f * m_level, 0.5f));
    float prob = UnityEngine.Random.Range(0.0f, 1.0f);
    float n = Enum.GetNames(typeof(GroupType)).Length;
    for (int i = 1; i <= Math.Max(1, Math.Min(m_level, n)); ++i)
    {
      if (Math.Min(prob + bias, 1) <= (i / n))
        return (GroupType)(i - 1);
    }

    Debug.LogWarning("Returning default group type");
    return GroupType.Range;
  }

  //---------------------------------------------------------------------------

  private void ShowFtue()
  {
    if (m_enemies.Keys.Count > 0 && m_score.m_max_score < 7 && m_score.m_score < 6)
      m_enemies.Values.First().red_circle_renderer.enabled = true;
  }

  //---------------------------------------------------------------------------

  private void AddEnemies(int enemy_count = 2)
  {
    /*(float, float, bool)[] positions_x_z_below  = {
      (2.7f, 2.4f, true),
      (2.7f, -2.4f, true),
      (5.797773f, 0, true),
      (-6.797773f, 0, false),
      (-3.7f, 2.4f, false),
      (-3.7f, -2.4f, false),
    };*/
    (float, float, bool)[] positions_x_z_below = {
      (11.7f, 1.4f, true),
      (11.7f, -1.4f, true),
      (9.1f, 4.0f, true),
      (9.1f, -4.0f, true),
      (5, 4.9f, true),
      (5, -4.9f, true),
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

    ShowFtue();

  }

  //---------------------------------------------------------------------------

  private void SetRangeValue(Enemy new_enemy)
  {
    int multiplier = Math.Max(1, Math.Min((int)((m_level) / m_multiplier_scaler), 5));
    new_enemy.head_text_type = HeadText.TextType.Int;
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(1, (int)Math.Pow(10, multiplier));
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetNegativeValue(Enemy new_enemy)
  {
    int multiplier = Math.Max(1, Math.Min((int)((m_level - 1) / m_multiplier_scaler), 5));
    new_enemy.head_text_type = HeadText.TextType.Int;
    do
    {
      new_enemy.head_text_int = UnityEngine.Random.Range(-(int)Math.Pow(10, Math.Max(1, multiplier-1)), (int)Math.Pow(10, multiplier));
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetRearrangeValue(Enemy new_enemy)
  {
    new_enemy.head_text_type = HeadText.TextType.Int;
    int multiplier = Math.Max(0, Math.Min((int)((m_level - 2) / m_multiplier_scaler) - 1, 1));
    // group into 4, if new group, generate new 5 digit array, choose random rearrangement otherwise
    if ((m_enemies.Count() % 4) == 0)
    {
      int val = UnityEngine.Random.Range(1000 * (int)Math.Pow(10, multiplier), 10000 * (int)Math.Pow(10, multiplier));
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
    int multiplier = Math.Max(1, Math.Min((int)((m_level - 2) / m_multiplier_scaler), 2));
    new_enemy.head_text_type = HeadText.TextType.Float;
    do
    {
      new_enemy.head_text_float = UnityEngine.Random.Range(-(float)Math.Pow(10, multiplier - 1), (float)Math.Pow(10, multiplier));
    }
    while (EnemiesHaveValueWithinRange(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------

  private void SetSymbolValue(Enemy new_enemy)
  {
    do
    {
      float prob = UnityEngine.Random.Range(0.0f, 1.0f);
      float threshold = 0.25f * Math.Min(Math.Max(1, (int)((m_level - 3) / m_multiplier_scaler)), 3);
      if (prob < threshold)
      {
        char[] symbols = { 'e', 'π', 'φ' };
        new_enemy.head_text_type = HeadText.TextType.Symbol;
        new_enemy.head_text_symbol = symbols[UnityEngine.Random.Range(0, symbols.Length)];
      }
      else
      {
        new_enemy.head_text_type = HeadText.TextType.Int;
        new_enemy.head_text_int = UnityEngine.Random.Range(0, 4);
      }
    }
    while (m_enemies.ContainsKey(new_enemy.GetValue()));
  }

  //---------------------------------------------------------------------------
  private void SetFractionalValue(Enemy new_enemy)
  {
    int multiplier = Math.Max(1, Math.Min((int)((m_level - 3) / m_multiplier_scaler), 3));
    new_enemy.head_text_type = HeadText.TextType.Fraction;
    do
    {
      int num = UnityEngine.Random.Range(1, (int)Math.Pow(10, multiplier-1));
      int den = UnityEngine.Random.Range(num+1, (int)Math.Pow(10, multiplier));
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
      {
        Debug.LogWarning("Got close values " + enemy.Key + " and " + value);
        return true;
      }
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
      case GroupType.Constants: { SetSymbolValue(new_enemy)    ; break; }
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
    if (m_score.m_score >= 30)
      AddEnemies(6);
    else if (m_score.m_score > 5)
      AddEnemies(4);
    else
      AddEnemies(2);
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
    if (m_score.m_score % 3 == 0)
    {
      ++m_level;
       Debug.Log("level = " + m_level);
    }

    if (enemy != m_enemies.Values.First())
      Debug.LogWarning("Enemy was defeated but isn't at the front of the list");
    m_enemies.RemoveAt(0);
    if (m_enemies.Count == 0)
      ResetState();
    else
      m_enemies.Values.First().SetLowest(true);

    ShowFtue();
  }

  //---------------------------------------------------------------------------

  public void OnGameOver(bool show_symbol = true)
  {
    Debug.Log("Game Over");
    if (show_symbol)
      m_enemies.Values.First().red_circle_renderer.enabled = true;

    foreach (Enemy enemy in m_enemies.Values)
    {
      if (enemy.head_text_type == HeadText.TextType.Symbol)
      {
        enemy.head_text_float = (float)enemy.GetValue();
        enemy.head_text_type = HeadText.TextType.Float;
        enemy.head_text_updated = false;
      }
    }
  }

  //---------------------------------------------------------------------------

  public void OnMinchenDestroyed()
  {
    Debug.Log("Minchen Destroyed");
    if (restart)
      restart.SetActive(true);
    if (home)
      home.SetActive(true);

    if (m_score.m_score > m_score.m_max_score && high_score_text)
      high_score_text.SetActive(true);
    else if (dead_high_score_text && m_score.m_max_score > 0)
      dead_high_score_text.text = m_score.m_max_score.ToString();
  }
}

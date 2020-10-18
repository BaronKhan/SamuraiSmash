using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTRL : MonoBehaviour
{
  private static readonly int s_max_enemy_count = 6;
  private static readonly float s_enemy_y = 0.4154629f;  // TODO: calibrate this
  private static readonly float s_enemy_below_offset = 10f;

  public Enemy enemy_prefab = null;

  private GameObject m_player;
  private Enemy[] m_enemy_heap = new Enemy[s_max_enemy_count];
 
  // private int level = 1;

  // Start is called before the first frame update
  void Start()
  {
    AddEnemies(3);
  }

  // Update is called once per frame
  void Update()
  {

  }

  private void AddEnemies(int enemy_count = 6)
  {
    /*Vector3 target_pos = new Vector3(5.297773f, s_enemy_y, 0);
    Enemy new_enemy = AddEnemy(target_pos, true);*/

    Vector3 target_pos2 = new Vector3(3.7f, s_enemy_y, 2.4f);
    Enemy new_enemy2 = AddEnemy(target_pos2, true);

    Vector3 target_pos3 = new Vector3(3.7f, s_enemy_y, -2.4f);
    Enemy new_enemy3 = AddEnemy(target_pos3, true);
  }

  private Enemy AddEnemy(Vector3 target_pos, bool is_below = true)
  {
    Enemy enemy = Instantiate(enemy_prefab, target_pos + new Vector3(s_enemy_below_offset, 0), Quaternion.Euler(0, 180, 0));
    enemy.target_pos = enemy.transform.position - new Vector3(s_enemy_below_offset, 0);
    enemy.is_lowest = false;
    // SetValue(enemy);
    return enemy;
  }

  private void SetValue(Enemy enemy)
  {
    HeadText head_text = ((HeadText)enemy.head_text.GetComponent(typeof(HeadText)));
    head_text.SetInteger(Random.Range(1, 999));
  }

  public void OnEnemyDestroyed(Enemy enemy)
  {
    
  }
}

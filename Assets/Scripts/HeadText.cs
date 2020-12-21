using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadText : MonoBehaviour
{
  public enum TextType
  {
    Int,
    Float,
    Symbol,
    Fraction,
  };

  public TextMesh text_mesh_frac = null;

  private TextMesh text_mesh = null;
  private TextType text_type = TextType.Int;
  private readonly int text_max_len = 5;
  private readonly int text_frac_max_len = 3;

  // Start is called before the first frame update
  void Start()
  {
    text_mesh = GetComponent<TextMesh>();
    if (!text_mesh_frac)
    {
      Debug.LogError("Destroying Enemy as TextMeshFrac not found");
      Destroy(gameObject);
      return;
    }

    SetInteger(1);
  }

  // Update is called once per frame
  void Update()
  {
        
  }

  private bool SetText(string s)
  {
    text_mesh.fontStyle = (text_type == TextType.Symbol) ? FontStyle.Italic : FontStyle.Normal;
    int len = text_mesh.text.Length;
    if (len > text_max_len)
    {
      Debug.LogWarning($"Input \"{s}\" is greater than {text_max_len} ({len})");
      return false;
    }

    text_mesh.text = s;
    text_mesh_frac.text = "";
    text_mesh.characterSize = Mathf.Max(1.0f, 2.0f - (0.25f * (len - 1)));

    return true;
  }

  public bool SetInteger(int i)
  {
    text_type = TextType.Int;
    return SetText(i.ToString());
  }

  public bool SetFloat(float f)
  {
    text_type = TextType.Float;
    /*int n = Math.Floor(Math.Log10(n) + 1);*/
    return SetText(f.ToString("0.00"));
  }
  public bool SetSymbol(char c)
  {
    text_type = TextType.Symbol;
    return SetText(c.ToString());
  }
  public bool SetFraction(int num, int den)
  {
    text_type = TextType.Fraction;
    string num_s = num.ToString();
    string den_s = den.ToString();
    int frac_length = Math.Max(num_s.Length, den_s.Length);
    if (frac_length > text_frac_max_len)
    {
      Debug.LogWarning($"Fraction length \"{num_s}/{den_s}\" is greater than {text_frac_max_len} ({frac_length})");
      return false;
    }

    text_mesh.text = $"{num_s}\n{den_s}";
    text_mesh_frac.text = "";
    for (int i = 0; i <= frac_length; ++i) { text_mesh_frac.text += '_'; }
    text_mesh.characterSize = 1.0f;

    return true;
  }
}

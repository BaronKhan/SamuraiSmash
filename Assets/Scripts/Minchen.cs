﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minchen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DebugMove();
    }

    void DebugMove()
    {
      transform.Translate((Input.GetAxis("Vertical")) * 1000, 0, 0);
    }
}

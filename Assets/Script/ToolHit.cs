using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolHit : MonoBehaviour
{
    public virtual void Hit()   // class parent — override di TreeCuttable
    {
        Debug.Log("Hit");
    }
}

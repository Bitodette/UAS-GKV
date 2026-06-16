using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCuttable : ToolHit
{
<<<<<<< HEAD
    
    public override void Hit()
    {
        Destroy(gameObject);
    }
}
=======
    [SerializeField] GameObject pickupDrop;
    [SerializeField] int dropCount = 5;
    [SerializeField] float spread = 1f;

    public override void Hit()
    {
        if (pickupDrop == null)
        {
            Debug.LogError("Pickup Drop belum diassign!");
            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 pos = transform.position;

            pos.x += Random.Range(-spread / 2f, spread / 2f);
            pos.y += Random.Range(-spread / 2f, spread / 2f);

            Instantiate(pickupDrop, pos, Quaternion.identity);

        Destroy(gameObject);
        }   
    }
}
>>>>>>> bfd6c88cc71a9593bde31b79406590fe3a5f9bc5

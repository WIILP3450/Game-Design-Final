using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZones : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.collider.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.Die();
            //Debug.Log("death triggered");
        }
    }
}

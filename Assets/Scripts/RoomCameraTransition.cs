using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCameraTransition : MonoBehaviour
{
    public GameObject VirtualCamera;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            VirtualCamera.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            VirtualCamera.SetActive(false);
        }
    }
}

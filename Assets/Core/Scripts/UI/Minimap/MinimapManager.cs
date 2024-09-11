using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public float minimapCamHeight = 9f;
    private GameObject playerObject;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (playerObject != null)
        {
            // Set this object's position to the player object's position
            transform.position = playerObject.transform.position + new Vector3(0, minimapCamHeight,0);

        }
    }

    private void FindPlayer()
    {
        Player playerScript = FindObjectOfType<Player>();
        if (playerScript != null && playerScript.gameObject.activeInHierarchy)
        {
            playerObject = playerScript.gameObject;
        }
        else
        {
            Debug.LogWarning("No active GameObject with Player script found in the scene.");
        }
    }
}

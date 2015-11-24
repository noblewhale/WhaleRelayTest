using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour 
{
	
	void Update() 
    {
        if (networkView.isMine)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.position += Vector3.up;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                transform.position += Vector3.down;
            }
        }
	}
}

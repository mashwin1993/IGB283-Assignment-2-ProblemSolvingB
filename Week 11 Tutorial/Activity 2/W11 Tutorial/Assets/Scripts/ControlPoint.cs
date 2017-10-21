using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour {

    public bool isMoving = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(isMoving)
        {
            Vector3 position = new Vector3();
            //Find Mous Pos
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Find new postion of GameObject
            position.x = mousePosition.x;
            position.y = mousePosition.y;
            position.z = transform.position.z;

            transform.position = position;
        }
		
	}
}

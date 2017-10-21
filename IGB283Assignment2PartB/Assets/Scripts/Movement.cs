using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    //Importing IGB283Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    //Public Variables
    public Vector3 offset;

    // Translate the mesh
    public static Matrix3x3 Translate(Vector3 offset)
    {
        // Create a new matrix
        Matrix3x3 matrix = new Matrix3x3();

        // Set the rows of the matrix
        matrix.SetRow(0, new Vector3(1.0f, 0.0f, offset.x));
        matrix.SetRow(1, new Vector3(0.0f, 1.0f, offset.y));
        matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

        // Return the matrix
        return matrix;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.E) == true)
        {
            Matrix3x3 M =  Translate(offset);
            Debug.Log("Move");
        }
	}
}

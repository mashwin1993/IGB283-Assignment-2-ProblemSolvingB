using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Limb : MonoBehaviour {
    //Importing IGB283Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    //Arm Sliders
    public GameObject child;
	public GameObject control;

    //Settable Start Vector3
    public Vector3 StartLocation;

    public Vector3 jointLocation;
	public Vector3 jointOffset;


	public float angle;

	public float lastAngle;

	public Vector3[] limbVertexLocations;

    //Materials
    public Material material;
    //Public MeshRenderer meshRenderer;
    public Mesh mesh;

    //Public colour choice
    public float[] spriteColor;

    //Public Colour Setting
    new Vector3 DrawColor;

    //Is head Check
    public bool isHead;

    //Methods//

    // Draw the limb 
    private void DrawLimb()
    {
        // Add a MeshFilter and MeshRenderer to the Empty GameObject
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>().material = material;

        // Get the Mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        // Clear all vertex and index data from the mesh
        mesh.Clear();


        // Create a rectangle with supplied vertices
        mesh.vertices = new Vector3[] {
            limbVertexLocations[0],
            limbVertexLocations[1],
            limbVertexLocations[2],
            limbVertexLocations[3]
        };

        // Set the colour of the rectangle
        mesh.colors = new Color[] {
            new Color(spriteColor[0], spriteColor[1], spriteColor[2], spriteColor[3]),
            new Color(spriteColor[0], spriteColor[1], spriteColor[2], spriteColor[3]),
            new Color(spriteColor[0], spriteColor[1], spriteColor[2], spriteColor[3]),
            new Color(spriteColor[0], spriteColor[1], spriteColor[2], spriteColor[3])
        };

        // Set vertex indicies
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
    }

    // Move the joint to its starting position
    public void MoveByOffset(Vector3 offset)
    {
        // Find the translation Matrix
        Matrix3x3 T = Translate(offset);

        // Move the mesh
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = T.MultiplyPoint(vertices[i]);
        }

        mesh.vertices = vertices;

        // Apply the transform to the joint
        jointLocation = T.MultiplyPoint(jointLocation);

        // Apply the same operation to the children
        if (child != null)
        {
            child.GetComponent<Limb>().MoveByOffset(offset);
        }

    }
    
    // Rotate the limb around a point
    public void RotateAroundPoint(Vector3 point, float angle, float lastAngle)
    {
        // Move the point to the origin
        Matrix3x3 T1 = Translate(-point);

        // Undo the last rotation
        Matrix3x3 R1 = Rotate(-lastAngle);

        // Move the point back to the oritinal position
        Matrix3x3 T2 = Translate(point);

        // Perform the new rotation
        Matrix3x3 R2 = Rotate(angle);

        // The final translation matrix
        Matrix3x3 M = T2 * R2 * R1 * T1;

        // Move the mesh
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = M.MultiplyPoint(vertices[i]);
        }

        mesh.vertices = vertices;

        // Apply the transformation to the joint
        jointLocation = M.MultiplyPoint(jointLocation);

        // Apply the transformation to the children
        if (child != null)
        {
            child.GetComponent<Limb>().RotateAroundPoint(point, angle, lastAngle);
        }
    }

    // Rotate a vertex around the origin
    public static Matrix3x3 Rotate(float angle)
    {
        // Create a new matrix
        Matrix3x3 matrix = new Matrix3x3();

        // Set the rows of the matrix
        matrix.SetRow(0, new Vector3(Mathf.Cos(angle), -Mathf.Sin(angle), 0.0f));
        matrix.SetRow(1, new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0.0f));
        matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

        // Return the matrix
        return matrix;
    }

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

    //

    // This will run before Start
    void Awake () {
    	// Draw the limb 
    	DrawLimb();

        
    }

	// Use this for initialization
	void Start () {
		// Move the child to the joint location
		if (child != null) {
			child.GetComponent<Limb>(). MoveByOffset(jointOffset);
		}
    }
	
	// Update is called once per frame
	void Update () {
		lastAngle = angle;
		if (control != null) {
			angle = control.GetComponent<Slider>().value;
		}

		if (child != null) {
			child.GetComponent<Limb>().RotateAroundPoint( jointLocation, angle, lastAngle);
		}	

		// Recalculate the bounds of the mesh
		mesh.RecalculateBounds();
	}

}

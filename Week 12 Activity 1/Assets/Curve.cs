using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve : MonoBehaviour
{

    // Find the Y value for a given X
    private float FindY(float x)
    {
        return -Mathf.Pow(x / 3, 2) + 4;
    }

    // Curve variables
    public int numberDivisions = 31;
    public float domainMin = -7.0f;
    public float domainMax = 7.0f;
    public float width = 0.1f;

    // Area variables
    public Material fillMaterial;
    public Material rectangleMaterial;
    public int numberRectangles = 5;
    public int numberFillVertices = 7;
    public float x1 = -2.0f;
    public float x2 = 3.0f;
    public Color areaFillColor = new Color(0.8f, 0.3f, 0.3f, 1.0f);
    public Color rectangleColor = new Color(0.2f, 0.6f, 0.3f, 0.5f);
    public GameObject areaFill;
    public GameObject rectangles;
    private float areaUnderCurve = 0.0f;

    //Methods
    private void DrawCurve()
    {
        //Set up the Line Renderer
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = numberDivisions;
        lr.widthMultiplier = width;

        //Set the position of the linerender
        float incrementSize = (Mathf.Abs(domainMin) + Mathf.Abs(domainMax)) / (numberDivisions - 1);
        for (int i = 0; i < numberDivisions; i++)
        {
            Vector3 position = new Vector3();
            position.x = ((float)i * incrementSize) + domainMin;
            position.y = FindY(position.x);
            position.z = 0.0f;
            lr.SetPosition(i, position);
        }

    }

    private void FillArea()
    {
        // Add a MeshFilter and MeshRenderer to the Empty GameObject 
        areaFill.AddComponent<MeshFilter>();
        areaFill.AddComponent<MeshRenderer>();

        // Get the Mesh from the MeshFilter
        Mesh mesh = areaFill.GetComponent<MeshFilter>().mesh;

        // Set the material to the material we have selected
        areaFill.GetComponent<MeshRenderer>().material = fillMaterial;

        // Clear all vertex and index data from the mesh
        mesh.Clear();

        // Find the higher y value
        float lowerY = 0;
        float y1 = FindY(x1);
        float y2 = FindY(x2);
        if (y1 < y2)
        {
            lowerY = y1;
        }
        else
        {
            lowerY = y2;
        }

        // Throw an error if the implementation would fail
        if (lowerY < 0)
        {
            throw new System.Exception("This implementation does not support finding the area above a curve");
        }

        // Create a rectangle that fills as much space as possible between x1 and x2
        List<Vector3> vertices = new List<Vector3>()
        {
            new Vector3(x1, 0, 0),
            new Vector3(x1, lowerY, 0),
            new Vector3(x2, 0, 0),
            new Vector3(x2, lowerY, 0)
        };

        // Set the colour of the rectangle
        List<Color> colors = new List<Color>()
        {
            areaFillColor,
            areaFillColor,
            areaFillColor,
            areaFillColor
        };

        // Set vertex indicies
        List<int> triangles = new List<int>() { 0, 1, 2, 1, 2, 3 };

        // Find the remaining area
        float incrementSize = (Mathf.Abs(x2) + Mathf.Abs(x1)) / (numberFillVertices - 1);
        int currentIndex = vertices.Count;

        // Add the first triangle
        triangles.Add(1);
        triangles.Add(4);
        triangles.Add(5);
        for (int i = 0; i < numberFillVertices; i++)
        {
            // Find the vertices of the rectangle
            Vector3 v1 = new Vector3();

            v1.x = ((float)i * incrementSize) + x1;
            v1.y = FindY(v1.x);
            v1.z = 0.0f;

            Vector3 v2 = new Vector3();

            v2.x = v2.x;
            v2.y = lowerY;
            v2.z = 0.0f;

            vertices.Add(v1);
            vertices.Add(v2);

            // Set the colour at the vertices
            colors.Add(areaFillColor);
            colors.Add(areaFillColor);

            // Find the indices of the rectangle
            triangles.Add(currentIndex);
            triangles.Add(currentIndex + 1);
            triangles.Add(currentIndex + 2);
            triangles.Add(currentIndex + 1);
            triangles.Add(currentIndex + 2);
            triangles.Add(currentIndex + 3);

            // Increment the vertex counter
            currentIndex += 2;
        }

        // Add the last two vertices
        Vector3 lv1 = new Vector3();

        lv1.x = x2;
        lv1.y = FindY(lv1.x);
        lv1.z = 0.0f;

        Vector3 lv2 = new Vector3();

        lv2.x = x2;
        lv2.y = lowerY;
        lv2.z = 0.0f;

        vertices.Add(lv1);
        vertices.Add(lv2);

        // Add the last two colors
        colors.Add(areaFillColor);
        colors.Add(areaFillColor);

        // Set the vertices, colors and triangles of the mesh to be equal to our lists
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();

    }

    public void DrawRectangles()
    {
        // Add a MeshFilter and MeshRenderer to the Empty GameObject
        rectangles.AddComponent<MeshFilter>();
        rectangles.AddComponent<MeshRenderer>();

        // Get the Mesh from the MeshFilter
        Mesh mesh = rectangles.GetComponent<MeshFilter>().mesh;

        // Set the material to the material we have selected
        rectangles.GetComponent<MeshRenderer>().material = rectangleMaterial;

        // Clear all vertex and index data from the mesh
        mesh.Clear();

        // Create empty lists to store the rectangle data
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        // Find all of the rectangles
        float incrementSize = (Mathf.Abs(x2) + Mathf.Abs(x1)) / (numberRectangles);

        for (int i = 0; i < numberRectangles; i++)
        {
            // Find the vertices of the rectangle
            Vector3 v1 = new Vector3();
            v1.x = ((float)i * incrementSize) + x1;
            v1.y = 0.0f;
            v1.z = -0.1f;

            float y1 = FindY(v1.x);
            float y2 = FindY(v1.x + incrementSize);

            float smallerY = y1 < y2 ? y1 : y2;

            Vector3 v2 = new Vector3();

            v2.x = v1.x;
            v2.y = smallerY;
            v2.z = -0.1f;

            Vector3 v3 = new Vector3();

            v3.x = v1.x + incrementSize;
            v3.y = 0.0f;
            v3.z = -0.1f;

            Vector3 v4 = new Vector3();

            v4.x = v1.x + incrementSize;
            v4.y = smallerY;
            v4.z = -0.1f;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            // Set the colour at the vertices
            colors.Add(rectangleColor);
            colors.Add(rectangleColor);
            colors.Add(rectangleColor);
            colors.Add(rectangleColor);

            // Set the indices of the triangles
            triangles.Add(i * 4);
            triangles.Add(i * 4 + 1);
            triangles.Add(i * 4 + 2);
            triangles.Add(i * 4 + 1);
            triangles.Add(i * 4 + 2);
            triangles.Add(i * 4 + 3);

            // Add the area of the rectangle to the total
            areaUnderCurve += incrementSize * smallerY;
        }

        // Draw the rectangles
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();

    }


    // Use this for initialization
    void Start()
    {
        // Draw the Curve
        DrawCurve();

        //Fill the area
        FillArea();

        // Show the area under the curve
        Debug.Log(areaUnderCurve);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

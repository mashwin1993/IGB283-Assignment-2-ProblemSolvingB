using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSpline : MonoBehaviour
{

    //Public Variables
    // Spline variables
    public int numControlPoints = 9;
    public int numLinePositionsMultiplier = 10;
    public int k = 4;

    private int numLinePositions;
    private int m;
    private int n;
    private List<float> knots = new List<float>();

    public GameObject controlPoint;

    private List<GameObject> controlPoints;

    // Linerenderer variables
    public float lineWidth = 0.05f;

    private Vector3 FindBspline(float t)
    {
        int i = k - 1;
        while (knots[i + 1] < t)
        {
            i++;
        }
        if (i > m)
        {
            i = m;
        }
        float x = BsplineBasis(i - 3, k, t) * controlPoints[i- 3].transform.position.x
                + BsplineBasis(i - 2, k, t) * controlPoints[i - 2].transform.position.x
                + BsplineBasis(i - 1, k, t) * controlPoints[i - 1].transform.position.x 
                + BsplineBasis(i, k, t) * controlPoints[i].transform.position.x;

        float y = BsplineBasis(i - 3, k, t) * controlPoints[i - 3].transform.position.y 
                + BsplineBasis(i - 2, k, t) * controlPoints[i - 2].transform.position.y
                + BsplineBasis(i - 1, k, t) * controlPoints[i - 1].transform.position.y 
                + BsplineBasis(i, k, t) * controlPoints[i].transform.position.y;

        return new Vector3(x, y, 1.0f);
    }

    private float BsplineBasis(int i, int k, float t)
    {
        if (k == 1)
        {
            if ((knots[i] <= t) && (t < knots[i + 1]))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return BsplineBasis(i, k - 1, t) * (t - knots[i]) / (knots[i + k - 1] - knots[i])
                + BsplineBasis(i + 1, k - 1, t) * (knots[i + k] - t) / (knots[i + k] - knots[i + 1]);
        }
    }

    //Methods

    //Set the bspline variables
    void SetupBspline()
    {
        m = numControlPoints - 1;
        n = m + k;

        numLinePositions = numControlPoints * numLinePositionsMultiplier;

        if (knots == null)
        {
            knots = new List<float>();
        } else
        {
            knots.Clear();
        }

        for (int i = 0; i < n + 1; i++)
        {
            knots.Add(i);
        }
    }

    //Move control points
    void MoveControlPoint()
    {
        //Hold the point
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
            if (hitCollider && hitCollider.transform.tag == "ControlPoint")
            {
                hitCollider.transform.gameObject.GetComponent<ControlPoint>().isMoving = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Drop all points 
            foreach (GameObject controlPoint in controlPoints)
            {
                controlPoint.GetComponent<ControlPoint>().isMoving = false;
            }
        }
    }

    // Add new controlPoint
    void AddOrRemoveControlPoint()
    {
        if (Input.GetMouseButtonDown(1)) //RightClick
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            if (hitCollider && hitCollider.transform.tag == "ControlPoint")
            {
                numControlPoints -= 1;
                controlPoints.Remove(hitCollider.transform.gameObject);
                Destroy(hitCollider.transform.gameObject);
            }
            else
            {
                GameObject newControlPoint = Instantiate(controlPoint, this.transform);

                //Set the positoom of new controlPoint
                Vector3 position = new Vector3();
                position.x = mousePosition.x;
                position.y = mousePosition.y;
                position.z = transform.position.z;

                newControlPoint.transform.position = position;

                numControlPoints += 1;
                controlPoints.Add(newControlPoint);
            }
        }

        SetupBspline();
    }

    // Draw debug lines between control points
    void DrawDebugLines()
    {
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Debug.DrawLine(controlPoints[i].transform.position, controlPoints[i + 1].transform.position, Color.green);
        }
    }

    //Draw the b-splie
    void DrawCurve()
    {
        LineRenderer curve = GetComponent<LineRenderer>();
        curve.widthMultiplier = lineWidth;
        curve.positionCount = numLinePositions;
        for (int i = 0; i < numLinePositions; i++)
        {
            curve.SetPosition(i, FindBspline(knots[k - 1] + (knots[m + 1] - knots[k - 1]) * i / (numLinePositions - 1)));
        }
    }


    // Use this for initialization
    void Start()
    {
        SetupBspline();

        // Spawn the control points
        GameObject[] initialControlPoints = new GameObject[numControlPoints];

        float intervalSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0.0f)).x / numControlPoints;
        for (int i = 0; i < numControlPoints; i++)
        {
            initialControlPoints[i] = Instantiate(controlPoint, this.transform);

            // Set initial position
            float startPosition = Camera.main.ScreenToWorldPoint(new Vector2(0.0f, 0.0f)).x / 2;
            startPosition += intervalSize / 2;
            float xPosition = startPosition + (intervalSize * i);
            float yPosition = Mathf.Sin(xPosition);
            initialControlPoints[i].transform.position = new Vector3(xPosition, yPosition, 1.0f);

            controlPoints = new List<GameObject>(initialControlPoints);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Move the control points
        MoveControlPoint();

        // Add or remove control points
        AddOrRemoveControlPoint();

        // Draw debug lines between control points
        DrawDebugLines();

        // Recalculate the curve
        if (numControlPoints >= k)
        {
            DrawCurve();

        }
    }
}

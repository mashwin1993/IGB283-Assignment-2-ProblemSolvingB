using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour {
    public Vector2 scale = Vector2.one;

    public Vector2 pos {
        get {
            if (mesh && mesh.vertices.Length > 0) {
                return mesh.vertices[0];
            } else {
                return Vector2.zero;
            }
        }
        
    }

    //Importing IGB283Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    //Arm Sliders
    public GameObject child;
    public GameObject follower;

    //Settable Start Vector3
    public Vector3 StartLocation;

    public Vector3 jointLocation;
	public Vector3 jointOffset;
    public Vector2 jointVert {
        get { if (mesh && mesh.vertices.Length > 1) {
                return mesh.vertices[1];
            } else {
                return Vector2.zero;
            }
        }
    }

	public float angle;

	//public float lastAngle;

	public Vector3[] limbVertexLocations;

    //Starting Angle Adjustment
    public float startingAngle;
    //Materials
    public Material material;
    //Public MeshRenderer meshRenderer;
    public Mesh mesh;

    //Public colour choice
    public Color spriteColour = Color.white;

    //Is head Check
    public bool isHead;

    //Is root Check
    public bool isRoot;

    //Is follower Check
    public bool isFollower;

    //Border variables
    [Header("Border")]
    float minX = -4;
    float maxX = 4;

    //Nod modifiers
    [Header("Nod modifiers")]
    public float nodSpeed = 1;
    public float nodMedian = 35;
    public float nodRange = 10;

    //Dynamic jump variables
    [Header("Dynamic jump variables")]
    
    //Positioning Vars
    public Vector2 dynJumpVars;
    public Vector2 startPos;

    //Timing Vars
    public float startTime;
    public float duration;
    public float t;

    //Movement locks
    public bool movingRight = true;
    public bool wasMovingRight = true;
    public bool moving;
    public bool collapsing;

    int rightModifier {
        get { if (wasMovingRight) {
                return 1;
            } else {
                return -1;
            } }
    }

    //Step variables
    [Header("Step variables")]
    public Vector2 stepVars = new Vector2(1, 0.5f);
    public float stepDuration = 0.5f;

    //Leap variables
    [Header("Leap variables")]
    public Vector2 leapVars = new Vector2(3, 1);
    public float leapDuration = 1;

    //Jump variables
    [Header("Jump variables")]
    public Vector2 jumpVars = new Vector2(0, 4);
    public float jumpDuration = 2;

    //Fall variables
    [Header("Fall variables")]
    public Vector2 fallVars = new Vector2(0, 5);
    public float fallDuration = 1;

    //Move type variables
    public enum Movement {
        step,
        leap,
        jump,
        fall
    }

    public Movement currentMovement;
    float resetTimer;
    float resetLength = 0.5f;

    //Limb bob variables
    [Header("Limb bob variables")]
    public float bobMedian;
    public float bobRange = 10;

    //Collapse variables
    [Header("Collapse variables")]
    public float collapseMedian;
    public float collapseRange;
    public float collapseDuration;
    public float collapseOffset;

    [Header("Follower Vars")]
    public List<KeyValuePair<Movement, bool>> recieveQueue = new List<KeyValuePair<Movement, bool>>();
    public List<KeyValuePair<Movement, bool>> sendQueue = new List<KeyValuePair<Movement, bool>>();
    public int delay = 1;

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

        List<Vector3> vertexList = new List<Vector3> {
            Vector3.zero, jointLocation
        };

        vertexList.AddRange(limbVertexLocations);

        mesh.vertices = vertexList.ToArray();

        List<Color> colourList = new List<Color>();
        for (int i = 0; i < mesh.vertices.Length; i++) {
            colourList.Add(spriteColour);
        }
        mesh.colors = colourList.ToArray();

        // Set vertex indicies
        mesh.triangles = new int[] { 2, 3, 4, 2, 4, 5 };
    }

    // Move the joint to its starting position
    public void MoveByOffset(Vector3 offset) {
        meshTransform.MoveByOffset(offset);

        // Apply the same operation to the children
        if (child != null) {
            child.GetComponent<Limb>().MoveByOffset(offset);
        }

    }

    // Rotate the limb around a point
    public void RotateAroundPoint(Vector3 point, float angle, float lastAngle)
    {
        angle *= rightModifier;
        meshTransform.RotateAroundPoint(point, angle, lastAngle);

        // Apply the transformation to the children
        if (child != null)
        {
            child.GetComponent<Limb>().RotateAroundPoint(point, angle, lastAngle);
        }

        this.angle += angle - lastAngle;

    }


    //HeadNod
    //Rotates the head + and - set degrees
    private void HeadNod()
    {
        float nodAngle = nodMedian + (Mathf.Sin(nodSpeed * Time.time) * nodRange);

        RotateAroundPoint(jointVert, nodAngle, angle);
    }

    public void MoveTo(Vector2 location) {
        meshTransform.Translate(location - pos);
    }

    public void Reverse(Vector2 location) {
        meshTransform.ScaleUnrestricted(new Vector2(-1, 1));
        MoveTo(location);

        if (child) {
            child.GetComponent<Limb>().Reverse(jointVert);
        }
    }

    void UpdateJump() {
        t = (Time.time - startTime) / duration;
        t = Mathf.Clamp(t, 0, 1);

        Vector2 newPos;

        newPos.x = Mathf.Lerp(startPos.x, dynJumpVars.x, t);

        newPos.y = startPos.y + (Mathf.Sin(Mathf.Clamp(t, 0, 1) * Mathf.PI) * dynJumpVars.y);

        //meshTransform.SetPosition(newPos);
        MoveByOffset(newPos - pos);

        UpdateBob(t);

        if (t >= 1) {
            moving = false;
        }
    }

    void SetupJump(Vector2 vars, float duration, float tOverride = 0) {
        if (movingRight != wasMovingRight) {
            Reverse(pos);

            wasMovingRight = movingRight;
        }

        startTime = Time.time;
        if (tOverride != 0) {
            tOverride = Mathf.Clamp(tOverride, 0, 1);
            startTime -= tOverride * duration;
        }

        this.duration = duration;
        dynJumpVars = vars;
        dynJumpVars.x += pos.x;
        startPos = pos;

        moving = true;
    }

    void DoStep() {
        Vector2 tempVar = stepVars;

        //Direction correction
        if (!movingRight) {
            tempVar.x = -tempVar.x;
        }
        
        //Auto-reverse
        float endX = tempVar.x + pos.x;
        if (endX < minX || endX > maxX) {
            movingRight = !movingRight;
            return;
        }

        SetupJump(tempVar, stepDuration);
        sendQueue.Add(new KeyValuePair<Movement, bool>(Movement.step, movingRight));
    }

    void DoLeap() {
        Vector2 tempVar = leapVars;

        if (!movingRight) {
            tempVar.x = -tempVar.x;
        }

        float endX = tempVar.x + pos.x;
        if (endX < minX) {
            tempVar.x = minX - pos.x;
        } else if (endX > maxX) {
            tempVar.x = maxX - pos.x;
        }

        SetupJump(tempVar, leapDuration);
        sendQueue.Add(new KeyValuePair<Movement, bool>(Movement.leap, movingRight));
    }

    void DoJump() {
        SetupJump(jumpVars, jumpDuration);
        sendQueue.Add(new KeyValuePair<Movement, bool>(Movement.jump, movingRight));
    }

    void DoFall() {
        SetupJump(fallVars, fallDuration * 2, 0.5f);
        collapsing = true;
        sendQueue.Add(new KeyValuePair<Movement, bool>(Movement.fall, movingRight));
    }

    // This will run before Start
    void Awake () {
    	// Draw the limb 
    	DrawLimb();
        meshTransform.Initialise(mesh);
        meshTransform.Scale(scale);
        MoveTo(StartLocation);
    }

	// Use this for initialization
	void Start () {
        MoveExtension();

        //Set Starting angles - redundant
        if (child != null) {
            child.GetComponent<Limb>().RotateAroundPoint(jointVert, startingAngle, angle);
        }
    }

    public void MoveExtension() {
        // Move the child to the joint location
        if (child != null) {
            child.GetComponent<Limb>().MoveTo(jointVert);
            child.GetComponent<Limb>().MoveExtension();
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (isHead == true)
        {
            HeadNod();
        }



        if (isRoot && !isFollower) {
            //L/R inputs
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                movingRight = true;
            } else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                movingRight = false;
            }

            //Movement logic
            if (Input.GetKey(KeyCode.Z)) {
                currentMovement = Movement.fall;
                resetTimer = Time.time + resetLength;
            } else if (Input.GetKey(KeyCode.W)) {
                currentMovement = Movement.jump;
                resetTimer = Time.time + resetLength;
            } else if (Input.GetKey(KeyCode.S)) {
                currentMovement = Movement.leap;
                resetTimer = Time.time + resetLength;
            } else if (Time.time > resetTimer) {
                currentMovement = Movement.step;
            }

            if (moving) {
                UpdateJump();
            } else if (collapsing) {
                if (t >= 1) {
                    SetCollapse();
                } else {
                    UpdateCollapse(t);
                }
            } else {
                if (currentMovement == Movement.fall) {
                    DoFall();
                } else if (currentMovement == Movement.jump) {
                    DoJump();
                } else if (currentMovement == Movement.leap) {
                    DoLeap();
                } else {
                    DoStep();
                }
            }
        } else if (isFollower) {
            if (moving) {
                UpdateJump();
            } else if (collapsing) {
                if (t >= 1) {
                    SetCollapse();
                } else {
                    UpdateCollapse(t);
                }
            } else if (recieveQueue.Count > 0) {
                movingRight = recieveQueue[0].Value;

                Movement choice = recieveQueue[0].Key;
                if (choice == Movement.step) {
                    DoStep();
                } else if (choice == Movement.leap) {
                    DoLeap();
                } else if (choice == Movement.jump) {
                    DoJump();
                } else if (choice == Movement.fall) {
                    DoFall();
                }

                recieveQueue.RemoveAt(0);
            }
        }

        // Recalculate the bounds of the mesh
        mesh.RecalculateBounds();

        if (sendQueue.Count > delay) {
            if (follower) {
                follower.GetComponent<Limb>().recieveQueue.Add(sendQueue[0]);
                sendQueue.RemoveAt(0);
            }
        }
    }

    void SetCollapse() {
        startTime = Time.time - (duration * collapseOffset);
        duration = collapseDuration;
        t = 0;

        UpdateCollapse(t);
    }

    public void UpdateCollapse(float t) {
        if (isRoot) {
            t = (Time.time - startTime) / duration;
            this.t = Mathf.Clamp(t, 0, 1);
            t = this.t;

            if (t >= 1) {
                collapsing = false;
            }
        }

        float collapseAngle = collapseMedian + (Mathf.Sin(t * Mathf.PI) * collapseRange);

        RotateAroundPoint(pos, collapseAngle, angle);

        if (child) {
            child.GetComponent<Limb>().UpdateCollapse(t);
        }


    }

    public void UpdateBob(float t) {
        if (isHead) {
            return;
        }

        float bobAngle = bobMedian + (Mathf.Sin(t * Mathf.PI) * bobRange);

        RotateAroundPoint(pos, bobAngle, angle);

        if (child) {
            child.GetComponent<Limb>().UpdateBob(t);
        }
    }
}
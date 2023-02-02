using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyStats
{
    public Vector3 velocity;
    public Vector3 position;
    public float mass;
    public GameObject self;
    public int id;

    public BodyStats(Vector3 v, Vector3 p, float m, int simID, GameObject s = null)
    {
        this.velocity = v;
        this.position = p;
        this.mass = m;
        this.self = s;
    }

    public override string ToString()
    {
        return "BodyStats: {\nVelocity: " + velocity + "\nPosition: " + position + "\n}";
    }
}

[System.Serializable]
public class CelestialBody : MonoBehaviour
{
    public static float massMultiplier = 500000f;
    public float mass = 1;
    public static double GRAVITATIONAL_CONSTANT = 6.67 * Mathf.Pow(10, -8);
    public const float INIT_VEL_MULT = 0.05f;
    public const int INIT_LR_VEL_MULT = 20;
    public LineRenderer lr;
    public float radius = 1.5f;
    public bool isLocked;
    public float intensity { get => radius * 0.14f; }
    public int simID;
    LineDrawer lineDrawer;

    private void OnValidate()
    {
        transform.localScale = radius * Vector3.one;
        this.mass = transform.lossyScale.x * massMultiplier;

        GameController gc = FindObjectOfType<GameController>();
        if (gc.autoUpdateSim)
            gc.PredictPositions();

    }
    public Vector3 velocity;
    public Vector3 initVel;
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lineDrawer = GetComponent<LineDrawer>();
    }
    public void Init()
    {
        Debug.Log("Initialised");
        this.velocity = initVel * INIT_VEL_MULT;
    }

    public void Simulate()
    {
        Vector3 force = CalculateForces();
        Vector3 acceleration = force / this.mass;
        if (!isLocked)
        {
            velocity += acceleration / Time.fixedDeltaTime;
            transform.position += velocity / Time.fixedDeltaTime;
        }
    }

    Vector3 CalculateForces()
    {
        var celestialBodies = GameObject.FindObjectsOfType<CelestialBody>();
        Vector3 Force = Vector3.zero;

        foreach (var otherBody in celestialBodies)
        {
            if (otherBody == this) continue;

            double currentForce = GRAVITATIONAL_CONSTANT * this.mass * otherBody.mass;
            float distanceSquared = Mathf.Pow(Vector3.Distance(transform.position, otherBody.transform.position), 2);
            currentForce /= distanceSquared;

            Force += (float)currentForce * (otherBody.transform.position - transform.position).normalized;
        }

        return Force;
    }

    public void DrawInitVel()
    {
        Vector3[] lrPoints = new Vector3[2] { transform.position + transform.lossyScale.x * initVel.normalized, transform.position + transform.lossyScale.x * initVel.normalized + INIT_LR_VEL_MULT * initVel };
        lr.SetPositions(lrPoints);
    }

    public BodyStats PredictPosition(BodyStats[] celestialBodies, BodyStats body, int relativeBody, bool debug, float deltaTime)
    {
        Vector3 Force = Vector3.zero;

        foreach (var otherBody in celestialBodies)
        {
            if (otherBody.position == body.position) continue;

            double currentForce = GRAVITATIONAL_CONSTANT * body.mass * otherBody.mass;
            var dist = Vector3.Distance(body.position, otherBody.position);
            if (dist == 0) continue;
            float distanceSquared = Mathf.Pow(dist, 2);
            currentForce /= distanceSquared;

            Force += (float)currentForce * (otherBody.position - body.position).normalized;
        }

        Vector3 acceleration = Force / this.mass;

        body.velocity = body.velocity + acceleration / deltaTime;
        body.position = body.position + body.velocity / deltaTime;

        Vector3 lrPoint = body.position;
        if (relativeBody != -1 && body.position != transform.position)
            lrPoint -= (celestialBodies[relativeBody].position - celestialBodies[relativeBody].self.transform.position);

        if (debug)
            lineDrawer.line.Add(lrPoint);

        return body;
    }
}

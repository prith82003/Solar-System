using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using System.Threading;

public class GameController : MonoBehaviour
{
    public bool init = true;
    public bool isPaused = true;

    [Header("Projection")]
    public int numSteps = 1;
    public bool relativeTo;
    public GameObject relativeBody;
    public bool autoUpdateSim;
    public bool updatePredictions;

    [Header("Focus")]
    public bool focus;
    public GameObject focusObject;
    public static event Action ClearPoints;

    private void Start()
    {
        ClearPoints();
        LineDrawer.DrawLine = false;
        init = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<UIController>().TogglePause();
        }

        if (!isPaused && updatePredictions)
            PredictPositions();

        if (focus)
            Camera.main.transform.position = new Vector3(focusObject.transform.position.x, Camera.main.transform.position.y, focusObject.transform.position.z);
    }

    void FixedUpdate()
    {
        var celestialBodies = GameObject.FindObjectsOfType<CelestialBody>();

        foreach (var body in celestialBodies)
        {
            if (!isPaused)
            {
                // LineDrawer.DrawLine = false;
                if (init)
                    body.Init();

                // ! Instead of recalculating use the stored values in prediction
                // ! OR
                // ! Use multithreading to calculate each body
                // ! AND
                // ! Multithread the prediction

                body.Simulate();
                // ThreadStart bodySim = delegate
                // {
                //     body.Simulate();
                // };
                // bodySim.Invoke();
            }
            else
            {
                // body.DrawInitVel();
            }
        }
        if (!isPaused)
            init = false;
    }

    public static BodyStats[] celestialBodies;

    // Loop Through Each planet
    // Store projected velocity
    // Store projected positions
    public void PredictPositions()
    {
        if (!LineDrawer.DrawLine) return;
        UnityEngine.Debug.Log("Predicting");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        ClearPoints?.Invoke();
        // LineDrawer.DrawLine = true;
        CelestialBody[] bodies = GameObject.FindObjectsOfType<CelestialBody>();
        celestialBodies = new BodyStats[bodies.Length];
        int relBody = -1;

        for (int i = 0; i < bodies.Length; i++)
        {
            Vector3 initVel = bodies[i].initVel * CelestialBody.INIT_VEL_MULT;

            if (!isPaused)
                initVel = bodies[i].velocity;

            bodies[i].lr.positionCount = 0;
            if (relativeTo)
            {
                if (bodies[i].transform.position == this.relativeBody.transform.position)
                    relBody = i;
                celestialBodies[i] = new BodyStats(initVel, bodies[i].transform.position, bodies[i].mass, i, bodies[i].gameObject);
            }
            else
            {
                celestialBodies[i] = new BodyStats(initVel, bodies[i].transform.position, bodies[i].mass, i);
            }
        }

        for (int x = 0; x < numSteps; x++)
        {
            // Method 1:
            //ThreadStart planetSim = delegate
            //{
            //    DebugSimulate(bodies, relBody);
            //};

            //planetSim.Invoke();
            
            // Method 2:
            DebugSimulate(bodies, relBody, Time.fixedDeltaTime);

            // Method 3:
            //var fxd = Time.fixedDeltaTime;
            //Thread _thread = new Thread(() => DebugSimulate(bodies, relBody, fxd));
            //_thread.Start();

        }
        sw.Stop();

        WriteLine("Time Taken: " + sw.ElapsedMilliseconds + "ms");
    }

    void DebugSimulate(CelestialBody[] bodies, int relBody, float timeStep)
    {
        for (int debugIndex = 0; debugIndex < bodies.Length; debugIndex++)
        {
            // Debug.Log("Celestial Body: " + bodies[i].name + "\nBefore Sim: " + celestialBodies[i].ToString());
            celestialBodies[debugIndex] = bodies[debugIndex].PredictPosition(celestialBodies, celestialBodies[debugIndex], relBody, true, timeStep);
            // Debug.Log("After Sim: " + celestialBodies[i].ToString());
        }
    }

    void OnValidate()
    {
        // LineDrawer.DrawLine = false;
        if (writer != null && !Application.isPlaying)
            writer.Close();

        if (!Application.isPlaying)
            return;

        // if (isPaused && init && autoUpdateSim)
        // {
        //     PredictPositions();
        // }
    }

    private void Awake()
    {
        InitFile();
        EditorApplication.playModeStateChanged += CloseFile;
    }

    static StreamWriter writer;
    static void InitFile()
    {
        string pathPre = "./Assets/Logs/Log";
        string pathPost = ".txt";
        string pathName = pathPre + pathPost;
        int counter = 0;

        while (File.Exists(pathName))
        {
            pathName = pathPre + counter + pathPost;
            counter++;
        }

        writer = File.CreateText(pathName);
        writer.WriteLine("Log Started");
    }

    static void WriteLine(string arg)
    {
        if (writer == null)
            return;
        if (!writer.BaseStream.CanWrite)
            return;
        writer.WriteLine(arg);
    }

    static void CloseFile(PlayModeStateChange pms)
    {
        ClearPoints?.Invoke();
        if (pms == PlayModeStateChange.ExitingPlayMode)
        {
            writer.WriteLine("Log Closed");
            writer.Close();
        }
    }
}
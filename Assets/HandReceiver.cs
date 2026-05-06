using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class Landmark { public float x, y, z; }

[System.Serializable]
public class HandData { public Landmark[] landmarks; }

public class HandReceiver : MonoBehaviour
{
    public GameObject[] landmarkSpheres;
    private UdpClient udpClient;
    private Thread receiveThread;
    private HandData latestData;
    private bool newDataAvailable = false;
    private readonly object dataLock = new object();

    public float scaleX = 10f;
    public float scaleY = -8f;
    public float scaleZ = 5f;

    private static readonly int[,] connections = new int[,]
    {
        {0,1},{1,2},{2,3},{3,4},
        {0,5},{5,6},{6,7},{7,8},
        {5,9},{9,10},{10,11},{11,12},
        {9,13},{13,14},{14,15},{15,16},
        {13,17},{17,18},{18,19},{19,20},
        {0,17}
    };

    private LineRenderer[] lines;

    void Start()
    {
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;

        int lineCount = connections.GetLength(0);
        lines = new LineRenderer[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            GameObject go = new GameObject($"Line_{i}");
            go.transform.SetParent(transform);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.green;
            lr.endColor = Color.green;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.useWorldSpace = true;
            lr.enabled = false;
            lines[i] = lr;
        }

        udpClient = new UdpClient(5052);
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref endPoint);
                string json = Encoding.UTF8.GetString(data);
                HandData parsed = JsonUtility.FromJson<HandData>(json);
                lock (dataLock) { latestData = parsed; newDataAvailable = true; }
            }
            catch (Exception) { }
        }
    }

    void Update()
    {
        lock (dataLock)
        {
            if (!newDataAvailable || latestData == null) return;
            newDataAvailable = false;

            for (int i = 0; i < latestData.landmarks.Length && i < landmarkSpheres.Length; i++)
            {
                var lm = latestData.landmarks[i];
                landmarkSpheres[i].transform.localPosition = new Vector3(
                    lm.x * scaleX - scaleX / 2,
                    lm.y * scaleY + scaleY / -2,
                    lm.z * scaleZ
                );
            }

            int lineCount = connections.GetLength(0);
            for (int i = 0; i < lineCount; i++)
            {
                int a = connections[i, 0];
                int b = connections[i, 1];
                lines[i].SetPosition(0, landmarkSpheres[a].transform.position);
                lines[i].SetPosition(1, landmarkSpheres[b].transform.position);
                lines[i].enabled = true;
            }
        }
    }

    void OnDestroy()
    {
        receiveThread?.Abort();
        udpClient?.Close();
    }
}

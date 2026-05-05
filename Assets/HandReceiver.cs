using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

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

    void Start()
    {
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
        }
    }

    void OnDestroy()
    {
        receiveThread?.Abort();
        udpClient?.Close();
    }
}
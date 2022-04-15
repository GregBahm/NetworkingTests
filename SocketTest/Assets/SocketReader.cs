using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public class SocketWriter : MonoBehaviour
{
    [SerializeField]
    private string IpAddress = "127.0.0.1";

    [SerializeField]
    private int Port = 1990;

    private TcpClient client;

    private void Start()
    {
        client = new TcpClient();
        client.Connect(IpAddress, Port);
    }

    private void Update()
    {
        
    }

    private void WriteNetworkData()
    {
        while (true)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IpAddress, Port);

                using (NetworkStream stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        int offset = 0;
                        while (offset < DataSize)
                        {
                            offset += stream.Read(transformData, offset, transformData.Length - offset);
                        }

                        lock (transformDataSwapper)
                        {
                            transformDataSwapper = transformData;
                        }

                        stream.WriteByte(0);
                    }
                }
            }
        }
    }
}
public class SocketReader : MonoBehaviour
{
    [SerializeField]
    private string IpAddress = "127.0.0.1";

    [SerializeField]
    private int Port = 1990;

    private const int DataSize = sizeof(float) * 3 * 2// Two Wrist Positions
        + sizeof(float) * 4 * 24 * 4; // 24 hand joint rotations Rotation

    private Thread thread;

    private byte[] transformData;
    private byte[] transformDataSwapper;
    private BinaryFormatter formatter = new BinaryFormatter();

    private void Start()
    {
        transformData = new byte[DataSize];
        transformDataSwapper = new byte[DataSize];

        thread = new Thread(() => ReadNetworkData());
        thread.IsBackground = true;
        thread.Start();
    }

    private void Update()
    {
        NetworkData data = GetNetworkData();
        // Apply to hands
    }

    private NetworkData GetNetworkData()
    {
        lock (transformDataSwapper)
        {
            string asString = System.Text.Encoding.Default.GetString(transformDataSwapper);
            return JsonUtility.FromJson<NetworkData>(asString);
        }
    }

    public T Deserialize<T>(byte[] array)
        where T : struct
    {
        using (MemoryStream stream = new MemoryStream(array))
        {
            return (T)formatter.Deserialize(stream);
        }
    }

    private void OnDestroy()
    {
        thread.Abort();
    }

    private void ReadNetworkData()
    {
        while (true)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IpAddress, Port);

                using (NetworkStream stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        int offset = 0;
                        while (offset < DataSize)
                        {
                            offset += stream.Read(transformData, offset, transformData.Length - offset);
                        }

                        lock (transformDataSwapper)
                        {
                            transformDataSwapper = transformData;
                        }

                        stream.WriteByte(0);
                    }
                }
            }
        }
    }
}

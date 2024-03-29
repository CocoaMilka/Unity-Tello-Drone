using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TelloController : MonoBehaviour
{
    // Given via TellSDK
    private string droneIP = "192.168.10.1";
    private int commandPort = 8889;
    private UdpClient udpClient;

    TcpListener server;
    Thread serverThread;
    List<TcpClient> clients = new List<TcpClient>();

    // Holds user Input, updates in GetInput()
    float turnAxis = 0;
    float heightAxis = 0;
    float forwardAxis = 0;
    float sideAxis = 0;

    void Start()
    {
        // Attempt to connect to drone
        try
        {
            udpClient = new UdpClient();
            udpClient.Connect(droneIP, commandPort);
        }
        catch (SocketException e)
        {
            Debug.LogError(e.ToString());
        }

        // Initiate SDK mode to send flight commands
        SendCommand("command");
        
    }

    public void startServer()
    {
        // Start server on a separate thread to not block main
        serverThread = new Thread(InitializeServer);
        serverThread.Start();
    }

    void InitializeServer()
    {
        Int32 port = 7777;
        // Local IP for machine
        IPAddress localAddr = IPAddress.Parse("131.151.8.167");

        TcpListener server = new TcpListener(localAddr, port);

        server.Start();

        Debug.Log("Server running...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Debug.Log($"New client connected: {clientEndPoint.Address}:{clientEndPoint.Port}");

            Thread clientThread = new Thread(() => ServeClient(client));
            clientThread.Start();
        }
    }

    void ServeClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string clientData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        Debug.Log($"Received: {clientData}");
    }



    void Update()
    {
        // Check user input then apply to drone
        //GetInput();
        //UpdateDrone();

        // Debugging
        if(Input.GetKeyDown(KeyCode.A))
        {
            SendCommand("takeoff");
            Debug.Log("Command sent: Takeoff");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SendCommand("land");
            Debug.Log("Command sent: Land");
        }
    }

    void GetInput()
    {
        // Startup
        if (Input.GetButtonDown("Takeoff"))
        {
            SendCommand("takeoff");
            Debug.Log("Button pressed: Takeoff");
        }

        if (Input.GetButtonDown("Land"))
        {
            SendCommand("land");
            Debug.Log("Button pressed: Land");
        }

        // User Input - Checking if any relevant button is being pressed
        if (Input.GetButton("Turn") || Input.GetButton("Vertical") || Input.GetButton("Forward") || Input.GetButton("Side"))
        {
            turnAxis = Input.GetAxis("Turn");
            heightAxis = Input.GetAxis("Vertical");
            forwardAxis = Input.GetAxis("Forward");
            sideAxis = Input.GetAxis("Side");

            Debug.Log("Joystick Pressed");
        }
        else
        {
            turnAxis = 0;
            heightAxis = 0;
            forwardAxis = 0;
            sideAxis = 0;
        }
    }

    void UpdateDrone()
    {
        if (turnAxis > 0)
        {
            SendCommand("cw " + Mathf.RoundToInt(turnAxis * 360));  // Turning clockwise
            Debug.Log("Joystick turned clockwise: " + turnAxis);
        }
        else if (turnAxis < 0)
        {
            SendCommand("ccw " + Mathf.RoundToInt(-turnAxis * 360));  // Turning counterclockwise
            Debug.Log("Joystick turned counterclockwise: " + turnAxis);
        }

        if (heightAxis > 0)
        {
            SendCommand("up " + Mathf.RoundToInt(heightAxis * 100));  // Moving up
            Debug.Log("Joystick moved up: " + heightAxis);
        }
        else if (heightAxis < 0)
        {
            SendCommand("down " + Mathf.RoundToInt(-heightAxis * 100));  // Moving down
            Debug.Log("Joystick moved down: " + heightAxis);
        }

        if (forwardAxis > 0)
        {
            SendCommand("forward " + Mathf.RoundToInt(forwardAxis * 100));  // Moving forward
            Debug.Log("Joystick moved forward: " + forwardAxis);
        }
        else if (forwardAxis < 0)
        {
            SendCommand("back " + Mathf.RoundToInt(-forwardAxis * 100));  // Moving backward
            Debug.Log("Joystick moved backward: " + forwardAxis);
        }

        if (sideAxis > 0)
        {
            SendCommand("right " + Mathf.RoundToInt(sideAxis * 100));  // Moving right
            Debug.Log("Joystick moved right: " + sideAxis);
        }
        else if (sideAxis < 0)
        {
            SendCommand("left " + Mathf.RoundToInt(-sideAxis * 100));  // Moving left
            Debug.Log("Joystick moved left: " + sideAxis);
        }
    }

    // Sends a UDP packet containing the movement instruction to the drone
    void SendCommand(string command)
    {
        try
        {
            byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(command);
            udpClient.Send(commandBytes, commandBytes.Length);
            Debug.Log("Command sent: " + command);
        }
        catch (SocketException e)
        {
            Debug.LogWarning(e.ToString());
        }
    }

    void OnDisable()
    {
        udpClient.Close();

        // Close all clients
        foreach (TcpClient client in clients)
        {
            client.Close();
        }

        // Stop the server
        if (server != null)
        {
            server.Stop();
        }

        // Abort the server thread
        if (serverThread != null)
        {
            serverThread.Abort();
        }

        Debug.Log("Server and clients closed!");
    }
}

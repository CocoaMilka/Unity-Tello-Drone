using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TelloController : MonoBehaviour
{
    // Given via TellSDK
    private string droneIP = "192.168.10.1";
    private int commandPort = 8889;
    private UdpClient udpClient;

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

    void Update()
    {
        // Check user input then apply to drone
        GetInput();
        UpdateDrone();
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
    }
}

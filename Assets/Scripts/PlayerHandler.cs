using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> command = new NetworkVariable<FixedString32Bytes>("blank", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            command.Value = "takeoff";
        }
    }
}

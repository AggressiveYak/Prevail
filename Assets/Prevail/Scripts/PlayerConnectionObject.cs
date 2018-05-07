using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnectionObject : NetworkBehaviour
{
    public GameObject playerUnitPrefab;

    // SyncVars are variables where if their value changes on the SERVER
    [SyncVar(hook = "OnPlayerNameChanged")]
    public string playerName = "Anonymous";

	// Use this for initialization
	void Start ()
    {
        //Is this actually my own local PlayerConnectionObject?
        if (isLocalPlayer == false)
        {
            //This object belongs to another player.
            return;
        }

        //Since PlayerObject is invisible and not part of the world,
        //give me something physical to move around

        Debug.Log("PlayerObject::Start -- Spawning my own personal unit.");

        //Instantiate only creates an object on the local computer
        //even if it has a NetworkIdentity it still will not exist on
        //the network (and therefore not on any other client) unless
        // NetworkServer.Spawn() is called on this object.

        //Instantiate(playerUnitPrefab);

        // Command the server to Spawn our unit
        CmdSpawnMyUnit();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Remember: Update runs on everyone's computer whether or not they own this 
        // particular object

        if (isLocalPlayer == false)
        {
            return;
           
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            string n = "Miguel" + Random.Range(1, 100);

            Debug.Log("Sending the server a request to change our name to: " + n);
            CmdChangePlayerName(n);
        }
	}


    
    void OnPlayerNameChanged(string newName)
    {
        Debug.Log("OnPlayerNameChanged: OldName: " + playerName + "NewName: "+ newName);

        // WARNING: If you use a hook on a SyncVar, then our local value does not get automatically
        // updated.
        playerName = newName;

        gameObject.name = "PlayerConnectionObject [" + newName + "]";
    }

    // Commands ----------------------------------------------------------
    // Commands are special finctions that only get executed on the server
    // --------

    [Command]
    void CmdSpawnMyUnit()
    {
        // we are guaranteed to be on the server right now
        GameObject playerGO = Instantiate(playerUnitPrefab);
        
        //go.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

        // Now that the object exists on the server, propagate it to all
        // the clients (and also wire up the NetworkIdentity)

        NetworkServer.SpawnWithClientAuthority(playerGO, connectionToClient);
        
    }


    [Command]
    void CmdChangePlayerName(string n)
    {
        Debug.Log("CmdChangePlayerName: " + n);

        // Maybe we should check that the name doesn't have any blacklisted words
        // If there is a bad word in the name, do we just ignore this request and do nothing?
        // ...or do we still call the Rpc but with the original name?

        playerName = n;

        // Tell all clients what this player's name now is.
        //RpcChangePlayerName(playerName);
    }

    // RPC -------------------------------------------------------------
    // RPCs are special functions that only get executed on the clients.

    //[ClientRpc]
    //void RpcChangePlayerName(string n)
    //{
    //    Debug.Log("RPcChangePlayerName: We were asked to change the player name on a particular PlayerConnectionObject : " + n);
    //    playerName = n;
    //}
}

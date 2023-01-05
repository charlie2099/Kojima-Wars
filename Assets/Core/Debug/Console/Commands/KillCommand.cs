using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Console.Commands
{
    public class KillCommand : IConsoleCommand
    {
        public string Name() => "kill";

        public string Description() => "kills player when a reset is needed";

        public bool IsHidden() => false;

        public void Execute(string[] args)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
                {
                    GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<CombatComponent>().PlayerDeath();
                }
            }     
        }
    }
}

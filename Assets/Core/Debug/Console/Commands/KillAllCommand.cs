using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Console.Commands
{
    public class KillAllCommand : IConsoleCommand
    {
        public string Name() => "killall";

        public string Description() => "kills all players";

        public bool IsHidden() => false;

        public void Execute(string[] args)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<CombatComponent>().PlayerDeath();
            }
        }
    }
}
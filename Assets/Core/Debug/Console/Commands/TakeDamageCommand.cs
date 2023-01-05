using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class TakeDamageCommand : IConsoleCommand
    {
        private int _damage = 100;
        
        public string Name() => "ouch";

        public string Description() => "Inflicts a pre-set amount of damage onto the player";

        public bool IsHidden() => false;

        public void Execute(string[] args)
        {
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameScene"))
            {
                for (int i = 0; i < GameObject.FindGameObjectsWithTag("Player").Length; i++)
                {
                    var player = GameObject.FindGameObjectsWithTag("Player")[i];
                    if (player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        CombatComponent combatComponent = player.GetComponent<CombatComponent>();
                        combatComponent.TakeDamage(_damage); 

                        // ISSUE: Client player doesn't refill health and shield on respawn after using this command.
                        // However they do when are shot by another player.
                    }
                }
            }
        }
    }
}
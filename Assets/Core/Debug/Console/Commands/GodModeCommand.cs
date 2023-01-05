using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class GodModeCommand : IConsoleCommand
    {
        /*private float _lastHealth;
        private float _lastShield;*/
        public bool _isGodModeActive = false;
        public string Name() => "god";

        public string Description() => "toggles god mode, giving the player infinite health";

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
                        _isGodModeActive = !_isGodModeActive;
                        player.GetComponent<CombatComponent>().SetGod(_isGodModeActive);
                        DeveloperConsole.Print($"GOD MODE [{_isGodModeActive}]");
                    }
                }
            }
            else
            {
                DeveloperConsole.Print("He is the messiah!");
            }
        }
    }
}
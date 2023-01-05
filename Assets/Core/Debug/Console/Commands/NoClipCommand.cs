using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Networking;

namespace Console.Commands
{

    public class NoClipCommand : IConsoleCommand
    {
        bool isNoClip = false;
        ControlType previousInput;

        public string Name() => "noclip";

        public string Description() => "allows for spectating map and flying around";

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
                        isNoClip = !isNoClip;
                        
                        DeveloperConsole.Print($"{isNoClip}");
                        if (isNoClip)
                        {
                            previousInput = InputManager.GetCurrentControlType();
                            if (InputManager.GetCurrentControlType() == ControlType.VTOL)
                            {
                                player.GetComponent<NetworkTransformComponent>().ForceSwitchMechMode();
                                var playerMech = GameObject.FindGameObjectsWithTag("Player")[i];
                                if (playerMech.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
                                {
                                    player = playerMech;
                                }
                            }
                            player.GetComponent<Collider>().enabled = false;
                            InputManager.SetInputType(ControlType.NOCLIP);
                        }
                        else
                        {
                            InputManager.SetInputType(previousInput);
                            if(InputManager.GetCurrentControlType() == ControlType.VTOL)
                            {
                                player.GetComponent<NetworkTransformComponent>().SwitchModeServerRPC(player.GetComponent<NetworkObject>().OwnerClientId);
                            }
                            player.GetComponent<Collider>().enabled = true;
                        }
                    }
                }
            }
        }
    }
}


using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Console.Commands
{
    public class InstantCooldownCommand : IConsoleCommand
    {
        private bool _noCooldownActive = false;
        private float[] _previousCooldownTime = new float[3];
        
        public string Name() => "nocool";

        public string Description() => "Sets cooldown time to zero for all abilities";

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
                        var playerClass = player.GetComponent<CS_UseAbilities>().GetPlayerClass();

                        if (!_noCooldownActive)
                        {
                            DeveloperConsole.Print("[NO COOLDOWN MODE] = TRUE");
                            for (int j = 0; j < _previousCooldownTime.Length; j++)
                            {
                                _previousCooldownTime[j] = playerClass.GetAbility(j).GetCooldownTime();
                                playerClass.GetAbility(j).CooldownTime = 0f;
                                DeveloperConsole.Print("Ability cooldown 1: " + playerClass.GetAbility(j).GetCooldownTime());
                                
                                // ISSUE: Payload issue client side
                            }
                        }
                        else
                        {
                            DeveloperConsole.Print("[NO COOLDOWN MODE] = FALSE");
                            for (int j = 0; j < _previousCooldownTime.Length; j++)
                            {
                                playerClass.GetAbility(j).CooldownTime = _previousCooldownTime[j];
                                DeveloperConsole.Print("Ability cooldown 1: " + playerClass.GetAbility(j).GetCooldownTime());
                            }
                        }

                        _noCooldownActive = !_noCooldownActive;
                    }
                }
            }
        }
    }
}
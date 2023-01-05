using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Console.Commands
{

    public class HideUICommand : IConsoleCommand
    {
        public bool isHidden = false;
        public string Name() => "uglify";

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
                        isHidden = !isHidden;
                        GameObject ui = GameObject.Find("Canvas");
                        if (isHidden)
                        {
                            ui.GetComponent<Canvas>().enabled = false;
                            if (player.name == "TemplatePlayerMech(Clone)")
                            {
                                GameObject weapon = player.transform.GetChild(player.transform.childCount - 1).GetChild(0).GetChild(1).GetChild(0).gameObject;
                                weapon.SetActive(false);
                            }
                            
                            //find all ui elements
                            //disable
                        }
                        else
                        {
                            ui.GetComponent<Canvas>().enabled = true;
                            if (player.name == "TemplatePlayerMech(Clone)")
                            {
                                GameObject weapon = player.transform.GetChild(player.transform.childCount - 1).GetChild(0).GetChild(1).GetChild(0).gameObject;
                                weapon.SetActive(true);
                            }
                            
                            //find all ui elements
                            //enable
                        }
                    }
                }
            }
        }
    }
}
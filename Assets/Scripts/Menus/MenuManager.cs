using UnityEngine;
using UnityEngine.SceneManagement;

public static class MenuManager
{
    /// <summary>
    /// Goes to the menu with the given name
    /// </summary>
    /// <param name="name">name of the menu to go</param>
   public static void GoToMenu(MenuName name)
    {
        switch (name)
        {
            case MenuName.Main:
                // go to main scene
                SceneManager.LoadScene(GameInitializer.MainMenuScene);
                break;
            case MenuName.Pause:
                //Instantiate prefab
                Object.Instantiate(Resources.Load(GameInitializer.PauseMenuPrefab));
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public GameObject CustomGridPanel;
    public void OnStartClicked()
    {
        PlayerPrefs.SetInt("rows", 3);
        PlayerPrefs.SetInt("cols", 4);
        SceneManager.LoadScene(1);
    }

    public void OnCustomClicked()
    {
        CustomGridPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}

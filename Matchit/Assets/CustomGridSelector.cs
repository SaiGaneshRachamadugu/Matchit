using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;


public class CustomGridSelector : MonoBehaviour
{
    public GameObject CustomGridPanel;

    public void SetGrid_2x2() => SetGrid(2, 2);
    public void SetGrid_2x3() => SetGrid(2, 3);
    public void SetGrid_5x6() => SetGrid(5, 6);
    public void SetGrid_5x5() => SetGrid(5, 5);
    public void SetGrid_3x3() => SetGrid(3, 3);
    public void SetGrid_4x4() => SetGrid(4, 4);


    private void SetGrid(int rows, int cols)
    {
        PlayerPrefs.SetInt("rows", rows);
        PlayerPrefs.SetInt("cols", cols);
        SceneManager.LoadScene(1);
        CustomGridPanel.SetActive(false);
    }
}

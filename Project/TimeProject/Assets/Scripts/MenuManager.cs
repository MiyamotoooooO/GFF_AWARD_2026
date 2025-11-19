using UnityEngine.UI;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("ÉÅÉjÉÖÅ[ê›íË")]
    [SerializeField]private GameObject backGroundUI;
    private int count = 0;
    private bool isMenu = true;


    private void Start()
    {
        //backGroundUI = GameObject.Find("MenuManager");
        backGroundUI.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isMenu)
        {
            isMenu = true;
            Time.timeScale = 1f;
            backGroundUI.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isMenu)
        {
            isMenu = false;
            Time.timeScale = 0f;
            backGroundUI.SetActive(true);
        }
    }
    public void menuButton1()
    {
        Debug.Log("is Button down");
        //isMenu = true;
        if (!isMenu)
        {
            Debug.Log($"isMenu : {isMenu}");
            Time.timeScale = 1f;
            backGroundUI.SetActive(false);
            isMenu = true;
        }
        else
        {
            Debug.Log($"isMenu : {isMenu}");
            Time.timeScale = 0f;
            backGroundUI.SetActive(true);
            isMenu = false;
        }

        //count++;
        //if (count % 2 == 1)
        //{
        //    Time.timeScale = 0f;
        //    backGroundUI.SetActive(true);
        //}
        //else if (count % 2 == 0)
        //{
        //    Time.timeScale = 1f;
        //    backGroundUI.SetActive(false);
        //}

        Debug.Log("menu");
    }

    public void menuButton2()
    {
        Debug.Log("is Button down");
        //isMenu = true;
        if (isMenu)
        {
            Debug.Log($"isMenu : {isMenu}");
            Time.timeScale = 1f;
            backGroundUI.SetActive(false);
            isMenu = true;
        }
        //else
        //{
        //    Debug.Log($"isMenu : {isMenu}");
        //    Time.timeScale = 0f;
        //    backGroundUI.SetActive(true);
        //    isMenu = false;
        //}
    }
}

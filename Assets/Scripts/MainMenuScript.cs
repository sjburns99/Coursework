using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public TMP_Text title;
    public RawImage bgHourglass;
    public Button settingsButton;
    public TMP_Dropdown levelSelectDD;
    public Button quitButton;
    public TMP_Text settingsLabel;
    public Slider bonusTimeSlider;
    public TMP_Text bonusTimeSliderLabel;
    public Button backButton;

    //Level Completion
    public static bool level1Complete = false;
    public static bool level2Complete = false;
    public static bool level3Complete = false;

    //User options
    public static float bonusTime = 0f; 

    // Start is called before the first frame update
    void Start()
    {
        //Enable Cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        bgHourglass.transform.rotation = new Quaternion(0, 0, 0, 0);
        title.text = "Time Loop";
        bonusTimeSliderLabel.text = "Bonus Time - " + bonusTime.ToString() + " Seconds";
        Debug.Log(level1Complete.ToString() + level2Complete.ToString() + level3Complete.ToString());

        //Hide settings submenu options
        settingsLabel.gameObject.SetActive(false);
        bonusTimeSlider.gameObject.SetActive(false);
        bonusTimeSliderLabel.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        //Check level completion
        foreach (TMP_Dropdown.OptionData option in levelSelectDD.options)
            Debug.Log(option.text);

        if(level1Complete)
        {
            levelSelectDD.options[1].text += " *";
        }
        if (level2Complete)
        {
            levelSelectDD.options[2].text += " *";
        }
        if (level3Complete)
        {
            levelSelectDD.options[3].text += " *";
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Slowly rotate hourglass in background
        bgHourglass.transform.Rotate(0, 0, 0.05f, 0);

        
    }

    public void valueChanged()
    {
        switch(levelSelectDD.value)
        {
            case 1:
                SceneManager.LoadScene("ButtonPush");
                break;
            case 2:
                SceneManager.LoadScene("BasicCube");
                break;
            case 3:
                SceneManager.LoadScene("ButtonAndCube");
                break;
        }
    }

    public void toggleSettings()
    {
        //Toggles whether the settings menu or main menu is currently active
        bool settingsOpen;

        //Determines if the settings menu is currently open by checking if the settings label is active
        settingsOpen = settingsLabel.gameObject.activeSelf;

        //Toggle main menu components
        title.gameObject.SetActive(settingsOpen);
        levelSelectDD.gameObject.SetActive(settingsOpen);
        settingsButton.gameObject.SetActive(settingsOpen);
        quitButton.gameObject.SetActive(settingsOpen);

        //Toggle settings menu components
        settingsLabel.gameObject.SetActive(!settingsOpen);
        bonusTimeSlider.gameObject.SetActive(!settingsOpen);
        bonusTimeSliderLabel.gameObject.SetActive(!settingsOpen);
        backButton.gameObject.SetActive(!settingsOpen);
    }

    public void bonusTimeSliderChanged()
    {
        bonusTime = bonusTimeSlider.value;
        bonusTimeSliderLabel.text = "Bonus Time - " + bonusTime.ToString() + " Seconds";
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
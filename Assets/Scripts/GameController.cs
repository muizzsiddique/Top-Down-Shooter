using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text gameOverText;
    public Text objectivesText;
    public Text interactText;
    public int totalEnemies;
    
    GameObject player;
    int score;
    bool intelCollected = false;
    bool canInteract = false;

    void Start()
    {
        player = GameObject.Find("Player");
        gameOverText.text = "";
        interactText.text = "";
        SetObjectivesText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Level 1 Prototype");
        }

        if (score >= totalEnemies && player.activeSelf)
        {
            gameOverText.text = "Level Complete!";
        }

        if (!player.activeSelf)
        {
            gameOverText.text = "Game Over!";
        }

        interactText.text = "";
        if (canInteract)
            interactText.text = "Press [SPACE] to Interact";

        SetObjectivesText();
    }

    public void UpdateScore(int score)
    {
        this.score = score;
    }

    public void UpdateIntelCollected(bool collected)
    {
        intelCollected = collected;
    }

    public void UpdateCanInteract(bool interact)
    {
        canInteract = interact;
    }

    void SetObjectivesText()
    {
        string objectiveString = "Current Objectives\n";

        objectiveString += "- Enemies killed (" + score + "/" + totalEnemies + ")\n";

        if (intelCollected)
            objectiveString += "- Optional: Acquire intel (Complete)";
        else
            objectiveString += "- Optional: Acquire intel";

        objectivesText.text = objectiveString;
    }
}

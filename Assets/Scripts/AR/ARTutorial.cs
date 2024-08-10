using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the AR tutorial by handling next, previous, and close actions.
/// </summary>
public class ARTutorial : MonoBehaviour
{
    /// <summary>
    /// List of tutorial objects to be displayed in sequence.
    /// </summary>
    public List<GameObject> arTutorialObjects = new List<GameObject>();

    /// <summary>
    /// Radial selection for main menu activation.
    /// </summary>
    public RadialSelection radialSelection;

    /// <summary>
    /// Button to spawn main menu.
    /// </summary>
    public OVRInput.Button spawnButton;

    /// <summary>
    /// UI Buttons for navigation and closing the tutorial.
    /// </summary>
    public Button nextButton;
    public Button previousButton;
    public Button closeButton;

    public GameObject canvas;

    /// <summary>
    /// Current index of the active tutorial object.
    /// </summary>
    private int _index = 0;

    /// <summary>
    /// Initializes button states on start.
    /// </summary>
    private void Start()
    {
        previousButton.interactable = false;
        previousButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Displays the next tutorial object and updates button states.
    /// </summary>
    public void Next()
    {
        Debug.Log("Next");
        if (_index < arTutorialObjects.Count - 1)
        {
            arTutorialObjects[_index].SetActive(false);
            _index++;
            arTutorialObjects[_index].SetActive(true);
            previousButton.interactable = true;
            previousButton.gameObject.SetActive(true);
        }

        if (_index == arTutorialObjects.Count - 1)
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);
            closeButton.GetComponentInChildren<TMP_Text>().text = "Close";
        }
    }

    /// <summary>
    /// Displays the previous tutorial object and updates button states.
    /// </summary>
    public void Previous()
    {
        if (_index > 0)
        {
            arTutorialObjects[_index].SetActive(false);
            _index--;
            arTutorialObjects[_index].SetActive(true);
            nextButton.interactable = true;
            nextButton.gameObject.SetActive(true);
        }

        if (_index == 0)
        {
            previousButton.interactable = false;
            previousButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Closes the tutorial and resets the index.
    /// </summary>
    public void Close()
    {
        // Set all tutorial objects to inactive
        foreach (var obj in arTutorialObjects)
        {
            obj.SetActive(false);
        }

        // Close Tutorial
        canvas.SetActive(false);
        // Activate the main menu using the radial selection
        radialSelection.SetSpawnButton(spawnButton);
    }

    /// <summary>
    /// Opens the tutorial and initializes the first object and button states.
    /// </summary>
    public void OpenTutorial()
    {
        canvas.SetActive(true);
        _index = 0;
        if (arTutorialObjects.Count > 0)
        {
            arTutorialObjects[_index].SetActive(true);
        }
        nextButton.interactable = true;
        nextButton.gameObject.SetActive(true);
        previousButton.interactable = false;
        previousButton.gameObject.SetActive(false);
    }
}

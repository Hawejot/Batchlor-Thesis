using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlArMenu : MonoBehaviour
{
    // Public GameObjects for the different menu sections
    public GameObject tutorial;
    public GameObject what;
    public GameObject types;
    public GameObject activationFunctions;

    // Private boolean variables to track the active state of each menu section
    private bool _tutorialActive = false;
    private bool _whatActive = false;
    private bool _typesActive = false;
    private bool _activationFunctionsActive = false;

    // Method called when a button is clicked
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Tutorial":
                if (!_tutorialActive)
                {
                    DeactivateAll(); // Deactivate other sections
                    _tutorialActive = ToggleGameObject(tutorial, _tutorialActive); // Activate the selected section
                }
                else
                {
                    _tutorialActive = ToggleGameObject(tutorial, _tutorialActive); // Deactivate the selected section
                }
                break;
            case "What":
                if (!_whatActive)
                {
                    DeactivateAll(); // Deactivate other sections
                    _whatActive = ToggleGameObject(what, _whatActive); // Activate the selected section
                }
                else
                {
                    _whatActive = ToggleGameObject(what, _whatActive); // Deactivate the selected section
                }
                break;
            case "Types":
                if (!_typesActive)
                {
                    DeactivateAll(); // Deactivate other sections
                    _typesActive = ToggleGameObject(types, _typesActive); // Activate the selected section
                }
                else
                {
                    _typesActive = ToggleGameObject(types, _typesActive); // Deactivate the selected section
                }
                break;
            case "ActivationFunctions":
                if (!_activationFunctionsActive)
                {
                    DeactivateAll(); // Deactivate other sections
                    _activationFunctionsActive = ToggleGameObject(activationFunctions, _activationFunctionsActive); // Activate the selected section
                }
                else
                {
                    _activationFunctionsActive = ToggleGameObject(activationFunctions, _activationFunctionsActive); // Deactivate the selected section
                }
                break;
            default:
                Debug.LogWarning("Unknown button name: " + buttonName); // Handle unknown button names
                break;
        }
    }

    // Private method to toggle the active state of a GameObject and return the new state
    private bool ToggleGameObject(GameObject obj, bool isActive)
    {
        obj.SetActive(!isActive); // Set the GameObject's active state to the opposite of its current state
        return !isActive; // Return the new state
    }

    // Private method to deactivate all menu sections and reset their active states
    private void DeactivateAll()
    {
        tutorial.SetActive(false); // Deactivate the tutorial section
        what.SetActive(false); // Deactivate the what section
        types.SetActive(false); // Deactivate the types section
        activationFunctions.SetActive(false); // Deactivate the activation functions section

        // Reset all active state variables to false
        _tutorialActive = false;
        _whatActive = false;
        _typesActive = false;
        _activationFunctionsActive = false;
    }
}

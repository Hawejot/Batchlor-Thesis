using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class AnnData : NetworkBehaviour
{
    #region Variables
    // Network configuration variables
    private int inputLayerSize = 2;
    private int hiddenLayerOneSize = 1;
    private int hiddenLayerTwoSize = 3;
    private int outputLayerSize = 1;

    // ANN parameters
    private float[] inputs = new float[3];
    private float[][] weights = new float[3][];
    private float[][] biases = new float[3][];

    // Activation function flag
    private bool reluOrSigmoid = false; // Set to false for ReLU and true for sigmoid

    // UI references
    public AnnCanvas annCanvas;
    public GameObject inputFieldOne;
    public GameObject inputFieldTwo;
    public GameObject inputFieldThree;
    public GameObject layerTextOne;
    public GameObject layerTextTwo;
    public GameObject layerTextThree;
    public GameObject layerTextFour;
    public GameObject activationText;


    // Activation text prefix
    private string activationPretext = "Current Activation = ";

    // Random number generator
    private System.Random rand = new System.Random();
    #endregion

    #region Start
    /// <summary>
    /// Initializes the ANN parameters and UI at the start.
    /// </summary>
    void Start()
    {
        InitializeWeightsAndBiases();
        PopulateWeightsAndBiases();
        InitializeInputs();
        initiliseLayerInputText();
        DrawAnn();
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Placeholder for forward propagation logic.
    /// </summary>
    public void Forward()
    {
        ShowSolutionServerRpc();
    }

    /// <summary>
    /// Switches the activation function between ReLU and sigmoid.
    /// </summary>
    public void SwitchActivation()
    {
        ReluSigmoidSwitch();
    }

    /// <summary>
    /// Increases the size of the specified layer.
    /// </summary>
    /// <param name="layerIndex">Index of the layer to increase.</param>
    public void IncreaseLayer(int layerIndex)
    {
        ChangeLayers(layerIndex, 1);
    }

    /// <summary>
    /// Decreases the size of the specified layer.
    /// </summary>
    /// <param name="layerIndex">Index of the layer to decrease.</param>
    public void DecreaseLayer(int layerIndex)
    {
        ChangeLayers(layerIndex, -1);
    }

    /// <summary>
    /// Randomizes the input at the specified index.
    /// </summary>
    /// <param name="index">Index of the input to randomize.</param>
    public void AdaptInput(int index)
    {
        ChangeInput(index);
    }

    /// <summary>
    /// Randomizes the weights and biases.
    /// </summary>
    public void RandomiseWeightsAndBiases()
    {
        PopulateWeightsAndBiases();
    }

    /// <summary>
    /// Triggers the drawing of the ANN and synchronizes the configuration across clients.
    /// </summary>
    public void TriggerDrawAnn()
    {

        if (IsOwner)
        {
            UpdateAnnConfigServerRpc(inputLayerSize, hiddenLayerOneSize, hiddenLayerTwoSize, outputLayerSize, inputs, weights, biases, reluOrSigmoid);
        }
    }
    #endregion

  #region Private Functions
    /// <summary>
    /// Draws the ANN using the current configuration.
    /// </summary>
    private void DrawAnn()
    {
        annCanvas.DrawAnn(
            inputLayerSize,
            hiddenLayerOneSize,
            hiddenLayerTwoSize,
            outputLayerSize,
            inputs,
            weights,
            biases,
            reluOrSigmoid
        );
    }

    /// <summary>
    /// Switches the activation function and updates the UI text.
    /// </summary>
    private void ReluSigmoidSwitch()
    {
        reluOrSigmoid = !reluOrSigmoid;
        activationText.GetComponent<TextMeshProUGUI>().text = activationPretext + (reluOrSigmoid ? "Sigmoid" : "ReLU");
    }

    /// <summary>
    /// Initializes the weights and biases arrays.
    /// </summary>
    private void InitializeWeightsAndBiases()
    {
        weights[0] = new float[9]; // Max 3 inputs * 3 hidden layer one neurons
        weights[1] = new float[9]; // Max 3 hidden layer one neurons * 3 hidden layer two neurons
        weights[2] = new float[9]; // Max 3 hidden layer two neurons * 3 output neurons

        biases[0] = new float[3]; // Max 3 hidden layer one neurons
        biases[1] = new float[3]; // Max 3 hidden layer two neurons
        biases[2] = new float[3]; // Max 3 output neurons
    }

    private void InitializeInputs()
    {
        inputs[0] = RandomFloat();
        inputs[1] = RandomFloat();
        inputs[2] = RandomFloat();
    }

    /// <summary>
    /// Populates the weights and biases with random values.
    /// </summary>
    private void PopulateWeightsAndBiases()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                weights[i][j] = RandomFloat();
            }
        }

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = RandomFloat();
            }
        }
    }

    /// <summary>
    /// Changes the size of the specified layer.
    /// </summary>
    /// <param name="layerIndex">Index of the layer to change.</param>
    /// <param name="changeBy">Amount to change the layer size by.</param>
    private void ChangeLayers(int layerIndex, int changeBy)
    {
        switch (layerIndex)
        {
            case 0:
                inputLayerSize = Math.Max(1, Math.Min(3, inputLayerSize + changeBy));//Changes Layer size for input
                layerTextOne.GetComponent<TextMeshProUGUI>().text = inputLayerSize.ToString();// Changes Layer text for input
                ChangeInputUi();
                break;
            case 1:
                hiddenLayerOneSize = Math.Max(1, Math.Min(3, hiddenLayerOneSize + changeBy));//Changes Layer size for hl1
                layerTextTwo.GetComponent<TextMeshProUGUI>().text = hiddenLayerOneSize.ToString();//Changes Layer text for hl1
                break;
            case 2:
                hiddenLayerTwoSize = Math.Max(1, Math.Min(3, hiddenLayerTwoSize + changeBy)); //Changes Layer size for hl2
                layerTextThree.GetComponent<TextMeshProUGUI>().text = hiddenLayerTwoSize.ToString(); //Changes Layer text for hl2
                break;
            case 3:
                outputLayerSize = Math.Max(1, Math.Min(3, outputLayerSize + changeBy)); //Changes Layer size for output
                layerTextFour.GetComponent<TextMeshProUGUI>().text = outputLayerSize.ToString();//Changes Layer text for output
                break;
            default:
                // Handle unexpected layerIndex values if needed
                break;
        }
    }


    /// <summary>
    /// Initializes the layer input text.
    /// </summary>
    private void initiliseLayerInputText()
    {
        layerTextOne.GetComponent<TextMeshProUGUI>().text = inputLayerSize.ToString();
        layerTextTwo.GetComponent<TextMeshProUGUI>().text = hiddenLayerOneSize.ToString();
        layerTextThree.GetComponent<TextMeshProUGUI>().text = hiddenLayerTwoSize.ToString();
        layerTextFour.GetComponent<TextMeshProUGUI>().text = outputLayerSize.ToString();

        ChangeInputUi();
    }

    /// <summary>
    /// Changes the input value at the specified index.
    /// </summary>
    /// <param name="index">Index of the input to change.</param>
    private void ChangeInput(int index)
    {
        inputs[index] = RandomFloat();
        ChangeInputUi();
    }

    /// <summary>
    /// Updates the input UI based on the current input layer size.
    /// </summary>
    private void ChangeInputUi()
    {
        inputFieldOne.SetActive(inputLayerSize >= 1);
        inputFieldTwo.SetActive(inputLayerSize >= 2);
        inputFieldThree.SetActive(inputLayerSize == 3);

        if (inputLayerSize >= 1)
            inputFieldOne.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = inputs[0].ToString();
        if (inputLayerSize >= 2)
            inputFieldTwo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = inputs[1].ToString();
        if (inputLayerSize == 3)
            inputFieldThree.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = inputs[2].ToString();
    }

    /// <summary>
    /// Generates a random float between -1 and 1.
    /// </summary>
    /// <returns>A random float value.</returns>
    private float RandomFloat()
    {
        return (float)Math.Round(rand.NextDouble() * 2 - 1, 2);
    }

    /// <summary>
    /// Updates the ANN configuration on the server and synchronizes it with clients.
    /// </summary>
    /// <param name="inputLayerSize">Input layer size.</param>
    /// <param name="hiddenLayerOneSize">Hidden layer one size.</param>
    /// <param name="hiddenLayerTwoSize">Hidden layer two size.</param>
    /// <param name="outputLayerSize">Output layer size.</param>
    /// <param name="inputs">Input values.</param>
    /// <param name="weights">Weights values.</param>
    /// <param name="biases">Biases values.</param>
    /// <param name="activationFunction">Activation function flag.</param>
    [ServerRpc(RequireOwnership = false)]
    private void UpdateAnnConfigServerRpc(int inputLayerSize, int hiddenLayerOneSize, int hiddenLayerTwoSize, int outputLayerSize, float[] inputs, float[][] weights, float[][] biases, bool activationFunction)
    {
        this.inputLayerSize = inputLayerSize;
        this.hiddenLayerOneSize = hiddenLayerOneSize;
        this.hiddenLayerTwoSize = hiddenLayerTwoSize;
        this.outputLayerSize = outputLayerSize;
        this.inputs = inputs;
        this.weights = weights;
        this.biases = biases;
        this.reluOrSigmoid = activationFunction;

        UpdateAnnConfigClientRpc(this.inputLayerSize, this.hiddenLayerOneSize, this.hiddenLayerTwoSize, this.outputLayerSize, this.inputs, weights, biases, this.reluOrSigmoid);
    }

    /// <summary>
    /// Updates the ANN configuration on the clients and draws the ANN.
    /// </summary>
    /// <param name="inputLayerSize">Input layer size.</param>
    /// <param name="hiddenLayerOneSize">Hidden layer one size.</param>
    /// <param name="hiddenLayerTwoSize">Hidden layer two size.</param>
    /// <param name="outputLayerSize">Output layer size.</param>
    /// <param name="inputs">Input values.</param>
    /// <param name="weights">Weights values.</param>
    /// <param name="biases">Biases values.</param>
    /// <param name="activationFunction">Activation function flag.</param>
    [ClientRpc]
    private void UpdateAnnConfigClientRpc(int inputLayerSize, int hiddenLayerOneSize, int hiddenLayerTwoSize, int outputLayerSize, float[] inputs, float[][] weights, float[][] biases, bool activationFunction)
    {
        this.inputLayerSize = inputLayerSize;
        this.hiddenLayerOneSize = hiddenLayerOneSize;
        this.hiddenLayerTwoSize = hiddenLayerTwoSize;
        this.outputLayerSize = outputLayerSize;
        this.inputs = inputs;
        this.weights = weights;
        this.biases = biases;
        this.reluOrSigmoid = activationFunction;

        DrawAnn();
    }

    /// <summary>
    /// Shows the solution on the server and synchronizes it with clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void ShowSolutionServerRpc()
    {
        ShowSolutionClientRpc();
    }

    /// <summary>
    /// Shows the solution on the clients.
    /// </summary>
    [ClientRpc]
    private void ShowSolutionClientRpc()
    {
        annCanvas.DisplayResults();
    }
    #endregion
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnCanvas : MonoBehaviour
{
    #region Variables
    // Variables needed to draw the ANN
    private const float MaxY = 9.5f;
    private const float InputX = -10f;
    private const float HiddenLayerOneX = -4f;
    private const float HiddenLayerTwoX = 4f;
    private const float OutputX = 10f;
    private const float textSize = 0.5f;

    private const float ConnectionThickness = 2f; // Thickness for UI lines
    private  Color connectionColor = new Color(0.3f, 0.75f, 1f, 0.5f); //Color for UI lines

    private readonly List<GameObject> _neurons = new List<GameObject>();
    private readonly List<GameObject> _connections = new List<GameObject>();
    private readonly List<GameObject> _textObjects = new List<GameObject>();

    private string solutionTextOne;
    private string solutionTextTwo;
    private string solutionTextOutput;
    private int SolutionCount = 0;

    //Variables needed for network
    private bool ActivationSigmoid = false;
    private float[] inputs;
    private float[][] weights;
    private float[][] biases;

    private int InputLayerSize;
    private int HiddenLayerOneSize;
    private int HiddenLayerTwoSize;
    private int OutputLayerSize;


    // Prefabs
    public GameObject NeuronPrefab;
    public Material LineMaterial;
    public GameObject TextPrefab; // Prefab for displaying text
    public Transform ParentPanel; // The panel to which the ANN elements will be parented
    public GameObject ResultTextHl1;
    public GameObject ResultTextHl2;
    public GameObject ResultTextOutput;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Initializes the ANN when the script starts.
    /// </summary>
    void Start()
    {
        // Initialization code if needed
    }

    /// <summary>
    /// Handles updates if necessary.
    /// </summary>
    void Update()
    {
        // Handle updates if necessary
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Draws the ANN using the provided configuration.
    /// </summary>
    /// <param name="inputLayerSize">Size of the input layer.</param>
    /// <param name="hiddenLayerOneSize">Size of the first hidden layer.</param>
    /// <param name="hiddenLayerTwoSize">Size of the second hidden layer.</param>
    /// <param name="outputLayerSize">Size of the output layer.</param>
    /// <param name="inputs">Input values.</param>
    /// <param name="weights">Weight values.</param>
    /// <param name="biases">Bias values.</param>
    /// <param name="activationSigmoid">Activation function flag.</param>
    public void DrawAnn(int inputLayerSize, int hiddenLayerOneSize, int hiddenLayerTwoSize, int outputLayerSize, float[] inputs, float[][] weights, float[][] biases, bool activationSigmoid)
    {
        // Despawn previous ANN
        DespawnAnn();

        // Draw new ANN
        float[] inputYs = CalculateYs(inputLayerSize);
        float[] hiddenLayerOneYs = CalculateYs(hiddenLayerOneSize);
        float[] hiddenLayerTwoYs = CalculateYs(hiddenLayerTwoSize);
        float[] outputYs = CalculateYs(outputLayerSize);

        List<GameObject> inputNeurons = DrawLayer(inputYs, InputX, inputs);
        List<GameObject> hiddenLayerOneNeurons = DrawLayer(hiddenLayerOneYs, HiddenLayerOneX, biases[0]);
        List<GameObject> hiddenLayerTwoNeurons = DrawLayer(hiddenLayerTwoYs, HiddenLayerTwoX, biases[1]);
        List<GameObject> outputNeurons = DrawLayer(outputYs, OutputX, biases[2]);

        DrawAllConnections(inputNeurons, hiddenLayerOneNeurons, hiddenLayerTwoNeurons, outputNeurons, weights);

        this.InputLayerSize = inputLayerSize;
        this.HiddenLayerOneSize = hiddenLayerOneSize;
        this.HiddenLayerTwoSize = hiddenLayerTwoSize;
        this.OutputLayerSize = outputLayerSize;
        this.ActivationSigmoid = activationSigmoid;
        this.inputs = inputs;
        this.weights = weights;
        this.biases = biases;

        SolutionCount = 0;
        DisplayResults(true);

        Forward();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Despawns the ANN by clearing all neurons, connections, and text objects.
    /// </summary>
    private void DespawnAnn()
    {
        ClearListAndDeleteGameObjects(_neurons);
        ClearListAndDeleteGameObjects(_connections);
        ClearListAndDeleteGameObjects(_textObjects);

        ResultTextHl1.transform.parent.gameObject.SetActive(false);
        ResultTextHl2.transform.parent.gameObject.SetActive(false);
        ResultTextOutput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Helper method to clear and delete GameObjects from a list.
    /// </summary>
    /// <param name="gameObjects">List of GameObjects to clear and delete.</param>
    private void ClearListAndDeleteGameObjects(List<GameObject> gameObjects)
    {
        if (gameObjects != null && gameObjects.Count > 0)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            gameObjects.Clear();
        }
    }

    /// <summary>
    /// Draws a single layer of neurons.
    /// </summary>
    /// <param name="yPositions">Y positions for the neurons in the layer.</param>
    /// <param name="xPosition">X position for the neurons in the layer.</param>
    /// <param name="layerBiases">Bias values for the neurons in the layer.</param>
    /// <returns>List of drawn neuron GameObjects.</returns>
    private List<GameObject> DrawLayer(float[] yPositions, float xPosition, float[] layerBiases)
    {
        List<GameObject> layerNeurons = new List<GameObject>();
        for (int i = 0; i < yPositions.Length; i++)
        {
            float y = yPositions[i];
            Debug.Log("Instantiated at: " + xPosition + ", " + y);

            GameObject neuron = Instantiate(NeuronPrefab, ParentPanel);
            neuron.transform.localPosition = new Vector3(xPosition, y, 0);
            neuron.transform.localRotation = Quaternion.identity;
            neuron.transform.localScale = new Vector3(0.06f, 0.25f, 1f);

            if (layerBiases != null && layerBiases.Length > i)
            {
                if(xPosition == InputX)
                {
                    DrawBiasOrInput(neuron.transform.localPosition, layerBiases[i], Color.magenta);
                }
                else
                {
                    DrawBiasOrInput(neuron.transform.localPosition, layerBiases[i], Color.green);
                }
            }
            layerNeurons.Add(neuron);
            _neurons.Add(neuron);
        }
        return layerNeurons;
    }

    /// <summary>
    /// Draws the bias or input value.
    /// </summary>
    /// <param name="position">Position to draw the text at.</param>
    /// <param name="bias">Bias value to display.</param>
    /// <param name="textColor">Color of the text.</param>
    private void DrawBiasOrInput(Vector3 position, float bias, Color textColor)
    {
        if (TextPrefab == null)
        {
            Debug.LogError("TextPrefab is not assigned in the inspector.");
            return;
        }

        Vector3 biasPosition = position;
        GameObject biasText = Instantiate(TextPrefab, ParentPanel);
        biasText.name = "Bias Text";
        biasText.transform.localPosition = biasPosition;

        TextMeshProUGUI textComponent = biasText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = bias.ToString("F2"); // Display bias with two decimal places
            textComponent.color = textColor; // Set text color to the specified color
            textComponent.enableWordWrapping = false; // Disable word wrapping
            textComponent.fontSize = textSize*4; // Set font size to 0.5
        }
        else
        {
            Debug.LogError("The instantiated TextPrefab does not have a TextMeshProUGUI component.");
        }
        _textObjects.Add(biasText);
    }

    /// <summary>
    /// Draws all connections between layers.
    /// </summary>
    /// <param name="inputNeurons">List of input neurons.</param>
    /// <param name="hiddenLayerOneNeurons">List of first hidden layer neurons.</param>
    /// <param name="hiddenLayerTwoNeurons">List of second hidden layer neurons.</param>
    /// <param name="outputNeurons">List of output neurons.</param>
    /// <param name="weights">Weight values for connections.</param>
    private void DrawAllConnections(List<GameObject> inputNeurons, List<GameObject> hiddenLayerOneNeurons, List<GameObject> hiddenLayerTwoNeurons, List<GameObject> outputNeurons, float[][] weights)
    {
        DrawConnections(inputNeurons, hiddenLayerOneNeurons, weights[0]);
        DrawConnections(hiddenLayerOneNeurons, hiddenLayerTwoNeurons, weights[1]);
        DrawConnections(hiddenLayerTwoNeurons, outputNeurons, weights[2]);
    }

    /// <summary>
    /// Draws connections between two layers.
    /// </summary>
    /// <param name="layer1">List of neurons in the first layer.</param>
    /// <param name="layer2">List of neurons in the second layer.</param>
    /// <param name="layerWeights">Weight values for the connections.</param>
    private void DrawConnections(List<GameObject> layer1, List<GameObject> layer2, float[] layerWeights)
    {
        int weightIndex = 0;
        foreach (GameObject neuron1 in layer1)
        {
            foreach (GameObject neuron2 in layer2)
            {
                DrawUiLine(neuron1.transform.localPosition, neuron2.transform.localPosition, connectionColor);
                DrawWeight(neuron1.transform.localPosition, neuron2.transform.localPosition, layerWeights[weightIndex]);
                weightIndex++;
            }
        }
    }

    /// <summary>
    /// Draws a UI line between two points.
    /// </summary>
    /// <param name="start">Start position of the line.</param>
    /// <param name="end">End position of the line.</param>
    /// <param name="color">Color of the line.</param>
    private void DrawUiLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("UILine");
        line.transform.SetParent(ParentPanel, false);

        Image lineImage = line.AddComponent<Image>();
        lineImage.color = color;

        RectTransform rectTransform = line.GetComponent<RectTransform>();
        Vector3 differenceVector = end - start;
        rectTransform.sizeDelta = new Vector2(differenceVector.magnitude, ConnectionThickness); // Set the width and height
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.localPosition = start;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        line.transform.SetSiblingIndex(0);

        _connections.Add(line);
    }

    /// <summary>
    /// Draws the weight between two points.
    /// </summary>
    /// <param name="start">Start position of the weight text.</param>
    /// <param name="end">End position of the weight text.</param>
    /// <param name="weight">Weight value to display.</param>
    private void DrawWeight(Vector3 start, Vector3 end, float weight)
    {
        Vector3 midPoint = (start + end) / 2; // Find the midpoint of the line
        GameObject weightText = Instantiate(TextPrefab, ParentPanel);
        weightText.transform.localPosition = midPoint;
        weightText.transform.localRotation = Quaternion.identity;
        TextMeshProUGUI textComponent = weightText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = weight.ToString("F2"); // Display weight with two decimal places
            textComponent.color = Color.black; // Set text color to black for better readability
            textComponent.enableWordWrapping = false; // Disable word wrapping
            textComponent.fontSize = textSize;
        }
        else
        {
            Debug.LogError("The instantiated TextPrefab does not have a TextMeshProUGUI component.");
        }
        _textObjects.Add(weightText);
    }

    /// <summary>
    /// Calculates Y positions for neurons in a layer.
    /// </summary>
    /// <param name="amounts">Number of neurons in the layer.</param>
    /// <returns>Array of Y positions for the neurons.</returns>
    private float[] CalculateYs(int amounts)
    {
        if (amounts == 1)
        {
            return new float[] { 0 };
        }
        if (amounts == 2)
        {
            return new float[] { MaxY / 2, -MaxY / 2 };
        }
        if (amounts == 3)
        {
            return new float[] { MaxY, 0, -MaxY };
        }

        float[] ys = new float[amounts];
        float step = MaxY * 2 / (amounts - 1);
        for (int i = 0; i < amounts; i++)
        {
            ys[i] = MaxY - i * step;
        }

        return ys;
    }


    /// <summary>
    /// Computes the forward propagation of the neural network,
    /// calculates the values for each neuron in the hidden and output layers,
    /// and logs the operations performed.
    /// </summary>
    private void Forward()
    {
        List<float> ResultsHiddenLayerOne = new List<float>();
        List<float> ResultsHiddenLayerTwo = new List<float>();
        List<float> ResultsOutput = new List<float>();

        string solutionTextOne = "";
        string solutionTextTwo = "";
        string solutionTextOutput = "";

        // Calculate values for the first hidden layer
        for (int i = 0; i < HiddenLayerOneSize; i++)
        {
            float sum = 0;
            string operation = "Activation(";
            string realNumbersOperation = "Activation(";
            for (int j = 0; j < InputLayerSize; j++)
            {
                float weight = weights[0][i * InputLayerSize + j];
                sum += inputs[j] * weight;
                operation += $"input{j + 1} * w{j + 1}{i + 1} + ";
                realNumbersOperation += $"{inputs[j]} * {weight} + ";
            }
            float bias = biases[0][i];
            sum += bias; // Add bias for the first hidden layer neuron

            operation += $"bias) = ";
            realNumbersOperation += $"{bias}) = ";

            float result = Activate(sum);

            ResultsHiddenLayerOne.Add(result);
            operation += realNumbersOperation + "Activation(" + sum.ToString() + ") = " + result.ToString("F2");
            solutionTextOne += operation + "\n";
        }

        // Calculate values for the second hidden layer
        for (int i = 0; i < HiddenLayerTwoSize; i++)
        {
            float sum = 0;
            string operation = "Activation(";
            string realNumbersOperation = "Activation(";
            for (int j = 0; j < HiddenLayerOneSize; j++)
            {
                float weight = weights[1][i * HiddenLayerOneSize + j];
                sum += ResultsHiddenLayerOne[j] * weight;
                operation += $"hidden1_{j + 1} * w{j + 1}{i + 1} + ";
                realNumbersOperation += $"{ResultsHiddenLayerOne[j]} * {weight} + ";
            }
            float bias = biases[1][i];
            sum += bias; // Add bias for the second hidden layer neuron

            operation += $"bias) = ";
            realNumbersOperation += $"{bias}) = ";

            float result = Activate(sum);

            ResultsHiddenLayerTwo.Add(result);
            operation += realNumbersOperation + "Activation(" + sum.ToString() + ") = " + result.ToString("F2");
            solutionTextTwo += operation + "\n";
        }

        // Calculate values for the output layer
        for (int i = 0; i < OutputLayerSize; i++)
        {
            float sum = 0.0f;
            string operation = "Activation(";
            string realNumbersOperation = "Activation(";
            for (int j = 0; j < HiddenLayerTwoSize; j++)
            {
                float weight = weights[2][i * HiddenLayerTwoSize + j];
                sum += ResultsHiddenLayerTwo[j] * weight;
                operation += $"hidden2_{j + 1} * w{j + 1}{i + 1} + ";
                realNumbersOperation += $"{ResultsHiddenLayerTwo[j]} * {weight} + ";
            }
            float bias = biases[2][i];
            sum += bias; // Add bias for the output layer neuron
            operation += $"bias) = ";
            realNumbersOperation += $"{bias}) = ";
            float result = Activate(sum);
            ResultsOutput.Add(result);
            operation += realNumbersOperation + "Activation(" + sum.ToString() + ") = " + result.ToString("F2");
            solutionTextOutput += operation + "\n";
        }

        ResultTextHl1.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = solutionTextOne;
        ResultTextHl2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = solutionTextTwo;
        ResultTextOutput.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = solutionTextOutput;

    }

    /// <summary>
    /// Applies the chosen activation function to a value.
    /// </summary>
    /// <param name="value">The value to apply the activation function to.</param>
    /// <returns>The activated value.</returns>
    private float Activate(float value)
    {
        return ActivationSigmoid ? 1 / (1 + Mathf.Exp(-value)) : Mathf.Max(0, value);
    }

    /// <summary>
    /// Displays the results of the neural network computation.
    /// </summary>
    /// <param name="rest">The rest of the results.</param>
    /// <summary>
    /// Displays the results of the neural network computation.
    /// </summary>
    /// <param name="reset">Whether to reset the solution count.</param>
    public void DisplayResults(bool reset = false)
    {
        //Setting the canvas foe the math to active
        ResultTextHl1.transform.parent.gameObject.SetActive(true);


        if (reset)
        {
            SolutionCount = 0;
        }
        else
        {
            SolutionCount++;
        }

        switch (SolutionCount)
        {
            case 0:
                ResultTextHl1.SetActive(false);
                ResultTextHl2.SetActive(false);
                ResultTextOutput.SetActive(false);
                break;

            case 1:
                ResultTextHl1.SetActive(true);
                ResultTextHl2.SetActive(false);
                ResultTextOutput.SetActive(false);
                break;

            case 2:
                ResultTextHl1.SetActive(true);
                ResultTextHl2.SetActive(true);
                ResultTextOutput.SetActive(false);
                break;

            default: // case 3 or more
                ResultTextHl1.SetActive(true);
                ResultTextHl2.SetActive(true);
                ResultTextOutput.SetActive(true);
                break;
        }
    }


    #endregion
}
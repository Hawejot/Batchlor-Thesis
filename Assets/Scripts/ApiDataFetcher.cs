using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;

public class ApiDataFetcher : MonoBehaviour
{
    public string apiUrl;
    public float timeout = 10f; // Set a timeout duration in seconds
    private bool requestSucceeded = false; // Boolean to indicate the request result

    [Serializable]
    public class LayerInfo
    {
        public string activation = null;
        public string class_name;
        public int index;
        public string name;
        public int[] output_shape;
        public int parameters;
    }

    [Serializable]
    public class LayerInfoList : INetworkSerializable
    {
        public LayerInfo[] layers;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int layerCount = layers != null ? layers.Length : 0;
            serializer.SerializeValue(ref layerCount);

            if (serializer.IsReader)
            {
                layers = new LayerInfo[layerCount];
            }

            for (int i = 0; i < layerCount; i++)
            {
                if (serializer.IsReader)
                {
                    layers[i] = new LayerInfo();
                }

                serializer.SerializeValue(ref layers[i].activation);
                serializer.SerializeValue(ref layers[i].class_name);
                serializer.SerializeValue(ref layers[i].index);
                serializer.SerializeValue(ref layers[i].name);

                int outputShapeLength = layers[i].output_shape != null ? layers[i].output_shape.Length : 0;
                serializer.SerializeValue(ref outputShapeLength);
                if (serializer.IsReader)
                {
                    layers[i].output_shape = new int[outputShapeLength];
                }
                for (int j = 0; j < outputShapeLength; j++)
                {
                    serializer.SerializeValue(ref layers[i].output_shape[j]);
                }

                serializer.SerializeValue(ref layers[i].parameters);
            }
        }
    }

    public IEnumerator GetLayerInfo(Action<LayerInfoList> onSuccess, Action<string> onError)
    {
        Debug.Log("Starting GetLayerInfo coroutine");
        requestSucceeded = false; // Reset the boolean before starting the request

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            Debug.Log("Sending web request to: " + apiUrl);

            bool isRequestTimeout = false;

            IEnumerator requestCoroutine = SendWebRequestWithTimeout(webRequest, timeout, () => isRequestTimeout = true);

            yield return StartCoroutine(requestCoroutine);

            if (isRequestTimeout)
            {
                string errorMessage = "Web request timed out.";
                Debug.LogError(errorMessage);
                onError?.Invoke(errorMessage);
                requestSucceeded = false;
            }
            else if (webRequest.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = "Web request error: " + webRequest.error;
                Debug.LogError(errorMessage);
                onError?.Invoke(errorMessage);
                requestSucceeded = false;
            }
            else
            {
                string jsonText = webRequest.downloadHandler.text;

                try
                {
                    LayerInfoList layerInfoList = JsonUtility.FromJson<LayerInfoList>("{\"layers\":" + jsonText + "}");

                    if (layerInfoList != null && layerInfoList.layers != null)
                    {
                        Debug.Log("Successfully parsed JSON response");
                        onSuccess?.Invoke(layerInfoList);
                        requestSucceeded = true;
                    }
                    else
                    {
                        throw new Exception("Invalid JSON response.");
                    }
                }
                catch (Exception e)
                {
                    string parseError = "Error parsing JSON: " + e.Message;
                    Debug.LogError(parseError);
                    onError?.Invoke(parseError);
                    requestSucceeded = false;
                }
            }
        }
    }

    private IEnumerator SendWebRequestWithTimeout(UnityWebRequest webRequest, float timeout, Action onTimeout)
    {
        float requestStartTime = Time.time;

        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            if (Time.time - requestStartTime > timeout)
            {
                onTimeout.Invoke();
                webRequest.Abort();
                yield break;
            }
            yield return null;
        }
    }

    public bool IsRequestSucceeded()
    {
        return requestSucceeded;
    }
}

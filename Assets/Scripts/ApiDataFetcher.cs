using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Responsible for fetching data from the API
public class ApiDataFetcher : MonoBehaviour
{

    public string apiUrl;

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
    public class LayerInfoList
    {
        public LayerInfo[] layers;
    }

    // Coroutine to fetch layer information from API
    public IEnumerator GetLayerInfo(Action<LayerInfoList> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(webRequest.error);
            }
            else
            {
                string jsonText = webRequest.downloadHandler.text;
                LayerInfoList layerInfoList = JsonUtility.FromJson<LayerInfoList>("{\"layers\":" + jsonText + "}");

                if (layerInfoList != null && layerInfoList.layers != null)
                {
                    onSuccess?.Invoke(layerInfoList);
                }
                else
                {
                    onError?.Invoke("Invalid JSON response.");
                }
            }
        }
    }
}

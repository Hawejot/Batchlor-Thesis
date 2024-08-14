using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RadialSelection : MonoBehaviour
{
    public OVRInput.Button spawnbutton;
    public OVRInput.Button despawnButton;
    public OVRInput.Button selectButton;

    public RadialSelection secondaryMenue = null;

    [Range(2, 10)]
    public int numberofRadialParts;
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public float angleBetweenParts;
    public Transform handtransform;
    public float scale = 0.3f;
    public bool debugBool = false;

    public UnityEvent<int> OnRadialPartSelected; // <int> is the argument type>

    public List<Sprite> radialPartSprites; // List of sprites to be shown on radial parts

    private List<GameObject> spawnedRadialParts = new List<GameObject>();
    private List<GameObject> icons = new List<GameObject>();

    private int currentPart = 0;

    public bool spawned = false;

    public void SetSpawnButton(OVRInput.Button spawnbutton)
    {
        this.spawnbutton = spawnbutton;
    }

    // Start is called before the first frame update
    void Start()
    {
            SpawnRadialPart();

            HideSelector();


    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(spawnbutton))
        {
            if (!spawned && spawnbutton != OVRInput.Button.None)
            {
                if(secondaryMenue != null)
                {
                    if (!secondaryMenue.spawned)
                    {
                        SpawnRadialPart();
                    }
                }
                else
                {
                    SpawnRadialPart();
                }
                
            }
}
        if (OVRInput.GetDown(despawnButton))
        {
            if (spawned)
            {
                HideSelector();
            }

        }

        //update which sclice is selected
        if (spawned)
        {
            GetSelectedRadialPart();
                radialPartCanvas.gameObject.SetActive(true);
        }

        //Select current scie
        if (OVRInput.GetDown(selectButton))
        {
            if (spawned)
            {
                HideAndTriggerSelector();
            }
            
        }

    }

    public void HideAndTriggerSelector()
    {
        OnRadialPartSelected.Invoke(currentPart);
        radialPartCanvas.gameObject.SetActive(false);
        spawned = false;
    }

    public void HideSelector()
    {
        radialPartCanvas.gameObject.SetActive(false);
        spawned = false;
    }

    public void GetSelectedRadialPart()
    {
        Vector3 centerToHand = handtransform.position - radialPartCanvas.position;

        Vector3 centertoHandProjection = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward);

        float angle = Vector3.SignedAngle(radialPartCanvas.up, centertoHandProjection, -radialPartCanvas.forward);


        if (angle < 0)
        {
            angle += 360;
        }

        currentPart = (int)angle * numberofRadialParts / 360;


        for (int i = 0; i < numberofRadialParts; i++)
        {
            if (i == currentPart)
            {
                spawnedRadialParts[i].GetComponent<Image>().color = Color.yellow;
                spawnedRadialParts[i].transform.localScale = 1.1f * Vector3.one;
                icons[i].transform.localScale = (scale+0.1f) * Vector3.one;
            }
            else
            {
                spawnedRadialParts[i].GetComponent<Image>().color = Color.white;
                spawnedRadialParts[i].transform.localScale = Vector3.one;
                icons[i].transform.localScale = scale * Vector3.one;
            }
        }
    }

    public void SpawnRadialPart()
    {
        radialPartCanvas.gameObject.SetActive(true);

        radialPartCanvas.position = handtransform.position;
        radialPartCanvas.rotation = handtransform.rotation;

        foreach (var item in spawnedRadialParts)
        {
            Destroy(item);
        }

        foreach (var item in icons)
        {
            Destroy(item);
        }

        icons.Clear();
        spawnedRadialParts.Clear();


        for (int i = 0; i < numberofRadialParts; i++)
        {
            float angle = -i * 360 / numberofRadialParts - angleBetweenParts / 2 + 180;
            Vector3 radialPartEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialPart = Instantiate(radialPartPrefab, radialPartCanvas);
            spawnedRadialPart.transform.position = radialPartCanvas.position;
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;
            spawnedRadialPart.GetComponent<Image>().fillAmount = (1 / (float)numberofRadialParts) - (angleBetweenParts / 360);

            // Calculate the position for the new sprite
            float radius = 80; // Adjust the radius as needed
            float nextAngle = -(i+1) * 360 / numberofRadialParts - angleBetweenParts / 2 +180;
            float middel = (nextAngle - angle) / 2 * 1.2f;
            angle = (angle+middel) * Mathf.Deg2Rad;
            Vector3 spritePosition = new Vector3(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle),
                0
            );


            // Create a new GameObject for the sprite
            GameObject spriteObject = new GameObject("Sprite");
            spriteObject.transform.SetParent(radialPartCanvas);
            spriteObject.transform.localPosition = spritePosition;
            spriteObject.transform.localScale = scale * Vector3.one; // Scale down

            // Add an Image component to the new GameObject and assign the sprite
            Image spriteImage = spriteObject.AddComponent<Image>();
            if (i < radialPartSprites.Count)
            {
                spriteImage.sprite = radialPartSprites[i];
            }

            // Ensure the sprite object is rendered on top
            spawnedRadialPart.transform.SetSiblingIndex(0);

            spawnedRadialParts.Add(spawnedRadialPart);
            icons.Add(spriteObject);
        }

        spawned = true;

    }

    public void setSpawned(bool value)
    {
        spawned = value;
    }
}

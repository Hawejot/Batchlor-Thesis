using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class RoomMeshManager : MonoBehaviour
{
    public MRUK MrukInstance;

    public bool GenerateFloor = true;
    public bool GenerateWalls = true;
    public bool GenerateObjects = true;

    public bool meshrendererEnabled = false;

    private FloorMeshGenerator floorMeshGenerator;
    private WallMeshGenerator wallMeshGenerator;
    private ObjectMeshGenerator objectMeshGenerator;

    private void Start()
    {
        if (MrukInstance == null)
        {
            Debug.LogError("MRUK instance is not assigned.");
            return;
        }

        // Initialize mesh generators
        floorMeshGenerator = gameObject.AddComponent<FloorMeshGenerator>();
        wallMeshGenerator = gameObject.AddComponent<WallMeshGenerator>();
        objectMeshGenerator = gameObject.AddComponent<ObjectMeshGenerator>();

        // Register a callback to generate the room meshes once the scene is loaded
        MrukInstance.RegisterSceneLoadedCallback(GenerateRoomMeshes);
    }

    private void GenerateRoomMeshes()
    {
        var currentRoom = MrukInstance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("Current room not found.");
            return;
        }

        if (GenerateFloor)
        {
            floorMeshGenerator.GenerateFloorMesh(currentRoom);
        }

        if (GenerateWalls)
        {
            wallMeshGenerator.GenerateWallsMeshes(currentRoom);
        }

        if (GenerateObjects)
        {
            objectMeshGenerator.GenerateObjectsMeshes(currentRoom);
        }

        if (meshrendererEnabled)
        {
            objectMeshGenerator.setMeshRendererEnabled(true);
        }
    }

    public GameObject GetFloorMesh()
    {
        return floorMeshGenerator.GetFloorMesh();
    }

    public List<GameObject> GetWallMeshes()
    {
        return wallMeshGenerator.GetWallMeshes();
    }

    public List<GameObject> GetObjectMeshes()
    {
        return objectMeshGenerator.GetObjectMeshes();
    }
}

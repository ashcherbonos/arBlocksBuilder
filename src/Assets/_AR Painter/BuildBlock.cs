using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SaveData = System.Collections.Generic.Dictionary<string, BlockParams>;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;
using UnityEngine.PostProcessing;
using System.Linq;

public class BuildBlock : MonoBehaviour
{

    public static BuildBlock instance;

    public static int modelId
    {
        get
        {
            return PlayerPrefs.GetInt("carentmodelid", 0);
        }
        set
        {
            PlayerPrefs.SetInt("carentmodelid", value);
        }
    }


    public GameObject newBlock;
    public static bool addBlock = true;
    public static bool colorZond = false;
    public static bool delBlock = false;
    public static bool placing = false;

    private SaveData blocksData;
    public Transform hitZoomParent;
    public PostProcessingProfile postProcessingProfile;

    public static float zoom
    {
        get
        {
            return PlayerPrefs.GetFloat("zoom" + modelId.ToString(), 0.1f);
        }
        set
        {
            PlayerPrefs.SetFloat("zoom" + modelId.ToString(), value);
            ResetZoom();
        }
    }

    private static void ResetZoom()
    {
        instance.hitZoomParent.transform.localScale = zoom * Vector3.one;
        ResetElevationOverFloor();

        AmbientOcclusionModel.Settings setings = instance.postProcessingProfile.ambientOcclusion.settings;
        setings.radius = 0.2f * zoom;
        instance.postProcessingProfile.ambientOcclusion.settings = setings;
    }

    private static float floorY = 0;

    private static void ResetElevationOverFloor()
    {

        Vector3 pos = instance.hitZoomParent.transform.position;
        pos.y = floorY + 0.5f * instance.hitZoomParent.localScale.y;
        instance.hitZoomParent.transform.position = pos;

    }

    void Awake()
    {
        instance = this;

    }

    void Start()
    {
        blocksData = new Dictionary<string, BlockParams>();
        Load(modelId);
        hitZoomParent.localScale = Vector3.zero;
        Invoke("UnparentTheHitZoomParent", 1f);
    }

    void UnparentTheHitZoomParent()
    {
        hitZoomParent.parent = null;
        hitZoomParent.rotation = Quaternion.identity;
        floorY = hitZoomParent.position.y - 0.5f * hitZoomParent.localScale.y;
        ResetZoom();
        PostProcessingBehaviour postProcessingBehaviour = Camera.main.gameObject.AddComponent<PostProcessingBehaviour>();
        postProcessingBehaviour.profile = postProcessingProfile;
    }

    void Update()
    {

        if (placing)
        {
            Placing();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    if (hit.collider.tag == "Block")
                    {
                        if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                        {
                            if (addBlock)
                            {
                                // generate new block

                                GameObject block = Instantiate(newBlock);
                                block.transform.SetParent(this.transform);
                                block.transform.localPosition = ToGrid(hit.point + hit.normal * zoom / 2f);
                                block.transform.rotation = this.transform.rotation;

                                block.transform.localScale = Vector3.one;
                                SetVertexesColor(block);
                                AddToBloksData(block.transform.localPosition);
                                Combine(block);
                                ResetMeshCollider();
                            }
                            else if (delBlock)
                            {
                                DeleteBlock(hit.triangleIndex, (ToGrid(hit.point - hit.normal * zoom / 2f)));
                            }
                            else if (colorZond)
                            {
                                ColorZond(hit.triangleIndex);
                            }

                        }
                    }
                }
            }
        }

    }


    private bool modelMovingByCam = false;

    private void Placing()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            // move model by camera movment
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    if (hit.collider.tag == "Block")
                    {
                        modelMovingByCam = true;
                        hitZoomParent.SetParent(Camera.main.transform);
                    }
                }
            }
            if (modelMovingByCam)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    modelMovingByCam = false;
                    hitZoomParent.SetParent(null);
                }
            }
            else
            // lace on AR plane
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                    ARPoint point = new ARPoint
                    {
                        x = screenPosition.x,
                        y = screenPosition.y
                    };

                    ARHitTestResultType[] resultTypes = {
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                        ARHitTestResultType.ARHitTestResultTypeHorizontalPlane,
                        ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    };

                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType(point, resultType))
                        {
                            return;
                        }
                    }
                }
            }
        }
    }


    bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
    {
        List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
        if (hitResults.Count > 0)
        {
            foreach (var hitResult in hitResults)
            {
                Debug.Log("Got hit!");
                Vector3 pos = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                floorY = pos.y;
                hitZoomParent.position = pos;
                ResetElevationOverFloor();

                hitZoomParent.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                return true;
            }
        }
        return false;
    }


    private Vector3 ToGrid(Vector3 pos)
    {

        pos = transform.worldToLocalMatrix.MultiplyPoint(pos);
        pos.x = (float)Math.Round(pos.x, MidpointRounding.AwayFromZero);
        pos.y = (float)Math.Round(pos.y, MidpointRounding.AwayFromZero);
        pos.z = (float)Math.Round(pos.z, MidpointRounding.AwayFromZero);
        return pos;
    }

    void Combine(GameObject block, bool addToDictionary = true)
    {

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;

        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = hitZoomParent.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.CombineMeshes(combine, true, true);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.SetActive(true);

        Destroy(block.gameObject);

    }

    void ResetMeshCollider()
    {
        Destroy(this.gameObject.GetComponent<MeshCollider>());
        gameObject.AddComponent<MeshCollider>();
    }

    void SetVertexesColor(GameObject block)
    {
        Mesh mesh = block.GetComponent<MeshFilter>().mesh;
        Array.Fill(mesh.colors32, UiController.color);
    }

    void DeleteBlock(int hitTriIndex, Vector3 pos)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        if (mesh.triangles.Length > 36)
        {
            int[] oldTriangles = mesh.triangles;
            int[] newTriangles = new int[mesh.triangles.Length - 36];

            int newBlockIndex = 0;
            int oldBlockIndex = 0;
            int oldBlocksAmaunt = mesh.triangles.Length / 36;
            int blockHitIndex = (int)(hitTriIndex / 12);
            int oldTrisIndexShift = 0;

            while (oldBlockIndex < oldBlocksAmaunt)
            {
                if (oldBlockIndex != blockHitIndex)
                {
                    for (int i = 0; i < 36; i++)
                    {
                        newTriangles[newBlockIndex * 36 + i] = oldTriangles[oldBlockIndex * 36 + i];
                    }
                    newBlockIndex++;
                }
                oldBlockIndex++;
            }
            mesh.triangles = newTriangles;

            ResetMeshCollider();
        }

        blocksData.Remove(DictionaryKey(pos));
    }

    void DeleteMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = new int[0];
    }

    void ColorZond(int hitTriIndex)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        UiController.instance.SetColor(mesh.colors32[mesh.triangles[hitTriIndex * 3] - 2]);
    }

    void AddToBloksData(Vector3 pos)
    {
        BlockParams blockParams = new BlockParams();
        blockParams.position = pos;
        blockParams.color32 = UiController.color;
        blocksData.Add(DictionaryKey(pos), blockParams);
    }

    public static string DictionaryKey(Vector3 pos)
    {
        return pos.x + "_" + pos.y + "_" + pos.z;
    }

    public void Save()
    {
        SaveController.Save(modelId, blocksData);
    }

    public void Load(int id)
    {
        DeleteMesh();
        modelId = id;
        blocksData = SaveController.Load(modelId);
        if (blocksData.Count == 0)
        {
            GameObject block = Instantiate(newBlock);
            block.transform.SetParent(this.transform);
            block.transform.localPosition = Vector3.zero;
            block.transform.rotation = this.transform.rotation;
            block.transform.localScale = Vector3.one;
            UiController.color = new Color32(255, 255, 255, 255);
            SetVertexesColor(block);
            AddToBloksData(block.transform.localPosition);
            Combine(block);
        }
        else
        {
            foreach (BlockParams blockParams in blocksData.Values)
            {
                GameObject block = Instantiate(newBlock);
                block.transform.SetParent(this.transform);
                block.transform.localPosition = blockParams.position;
                block.transform.rotation = this.transform.rotation;
                block.transform.localScale = Vector3.one;
                UiController.color = blockParams.color32;
                SetVertexesColor(block);
                Combine(block);
            }
        }

        UiController.instance.SetColor(UiController.color);
        ResetMeshCollider();
        ResetZoom();
    }
}

[System.Serializable]
public struct BlockParams
{
    private int x, y, z;
    private byte r, g, b;

    public Vector3 position
    {
        get
        {
            return new Vector3(x, y, z);
        }
        set
        {
            x = (int)value.x;
            y = (int)value.y;
            z = (int)value.z;
        }
    }

    public Color32 color32
    {
        get
        {
            return new Color32(r, g, b, 255);
        }
        set
        {
            r = value.r;
            g = value.g;
            b = value.b;
        }
    }
}

public class SaveController
{
    private static readonly int SCREN_TO_HISTORY_ICON_SIZE = 2;

    private static string FileName(int id)
    {
        return Application.persistentDataPath + "/save" + id;
    }

    public static string ScreenFileName(int id)
    {
        return FileName(id) + "screen";
    }

    public static void Save(int id, SaveData saveData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(FileName(id));
        formatter.Serialize(file, saveData);
        file.Close();
        SaveScreen(id);
    }

    public static SaveData Load(int id)
    {

        SaveData saveData = new SaveData();

        if (File.Exists(FileName(id)))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream file = File.Open(FileName(id), FileMode.Open);
            try
            {
                saveData = (SaveData)formatter.Deserialize(file);
            }
            catch
            {
                Debug.LogWarning("Couldn't deserialize SaveGame. Creating new!");
            }
            file.Close();
        }
        return saveData;
    }

    public static int iconWidth
    {
        get
        {
            return iconHeight;
        }
    }

    public static int iconHeight
    {
        get
        {
            return (int)(Mathf.Min(Screen.height, Screen.width) / SCREN_TO_HISTORY_ICON_SIZE);
        }
    }

    private static void SaveScreen(int id)
    {

        Camera camera = UiController.instance.cameraForScreenshots;
        camera.aspect = 1;
        camera.gameObject.SetActive(true);
        RenderTexture rt = new RenderTexture(iconWidth, iconHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(iconWidth, iconHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, iconWidth, iconHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // added to avoid errors
        MonoBehaviour.Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(ScreenFileName(id), bytes);
        camera.gameObject.SetActive(false);
    }
}

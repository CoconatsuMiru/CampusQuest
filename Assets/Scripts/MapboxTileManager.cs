using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapboxTileManager : MonoBehaviour
{
    [Header("Mapbox Settings")]
    public string mapboxToken = "sk.eyJ1Ijoia29oZmkiLCJhIjoiY21hbjJuc2F4MGRudzJtcXczZzRiNmJ0OSJ9.-DZWq9OZNwdRBu0r_UEocA";
    public int zoom = 18;
    public int tileSize = 256;

    [Header("Tile Settings")]
    public GameObject tilePrefab; // Prefab with a SpriteRenderer
    public Transform mapParent;

    private Dictionary<string, GameObject> tiles = new Dictionary<string, GameObject>();

    public void UpdateMap(float latitude, float longitude)
    {
        Vector2Int tileCoord = LatLonToTile(latitude, longitude, zoom);
        StartCoroutine(DownloadTile(tileCoord.x, tileCoord.y, zoom));
    }

    private IEnumerator DownloadTile(int x, int y, int z)
    {
        string tileKey = $"{x}_{y}_{z}";
        if (tiles.ContainsKey(tileKey)) yield break;

        string url = $"https://api.mapbox.com/styles/v1/mapbox/streets-v11/tiles/{tileSize}/{z}/{x}/{y}@2x?access_token={mapboxToken}";
        Debug.Log($"Fetching tile: {url}");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Tile download failed: {www.error}");
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(www);

        GameObject tileGO = Instantiate(tilePrefab, mapParent);
        tileGO.name = tileKey;

        SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        // Position the tile at world origin, can be offset for dynamic positioning
        tileGO.transform.position = Vector3.zero;
        sr.sortingLayerName = "Background";

        tiles[tileKey] = tileGO;
    }

    public Vector2Int LatLonToTile(float lat, float lon, int zoom)
    {
        int x = Mathf.FloorToInt((lon + 180f) / 360f * (1 << zoom));
        float latRad = lat * Mathf.Deg2Rad;
        int y = Mathf.FloorToInt((1f - Mathf.Log(Mathf.Tan(latRad) + 1f / Mathf.Cos(latRad)) / Mathf.PI) / 2f * (1 << zoom));
        return new Vector2Int(x, y);
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    public TextMeshProUGUI gpsText;
    public Button getCoordinatesButton;
    public Button setCoordinatesButton;
    public Button calculateDistanceButton;
    public GPSToUnity gpsToUnityConverter;
    public Transform objectToMove;

    private bool isLocationEnabled;
    private Vector3 savedCoordinates;

    void Start()
    {
        // Assign button listeners
        getCoordinatesButton.onClick.AddListener(GetCoordinates);
        setCoordinatesButton.onClick.AddListener(SetCoordinates);
        calculateDistanceButton.onClick.AddListener(CalculateDistance);

        // Handle Android location permission
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
        }

        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            gpsText.text = "Location Services Disabled!";
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            gpsText.text = "Timed out";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            gpsText.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            isLocationEnabled = true;
        }
    }

    void Update()
    {
        if (isLocationEnabled)
        {
            gpsText.text = $"Latitude: {Input.location.lastData.latitude} \n" +
                           $"Longitude: {Input.location.lastData.longitude} \n" +
                           $"Altitude: {Input.location.lastData.altitude}";
        }
    }

    void GetCoordinates()
    {
        if (isLocationEnabled)
        {
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;
            float altitude = Input.location.lastData.altitude;

            gpsText.text = $"Current Coordinates:\nLat: {latitude}, Long: {longitude}, Alt: {altitude}";
        }
        else
        {
            gpsText.text = "Location not available!";
        }
    }

    void SetCoordinates()
    {
        if (isLocationEnabled)
        {
            savedCoordinates = new Vector3(
                Input.location.lastData.latitude,
                Input.location.lastData.longitude,
                Input.location.lastData.altitude);

            gpsText.text = $"Coordinates Set:\nLat: {savedCoordinates.x}, Long: {savedCoordinates.y}, Alt: {savedCoordinates.z}";
        }
        else
        {
            gpsText.text = "Cannot set coordinates. Location not available!";
        }
    }

    void CalculateDistance()
    {
        if (isLocationEnabled && savedCoordinates != Vector3.zero)
        {
            float currentLatitude = Input.location.lastData.latitude;
            float currentLongitude = Input.location.lastData.longitude;

            float distance = HaversineDistance(savedCoordinates.x, savedCoordinates.y, currentLatitude, currentLongitude);

            gpsText.text = $"Distance to saved location: {distance} meters";

            // Convert GPS to Unity position and move the object
            Vector3 unityPosition = gpsToUnityConverter.GPS2UnityPosition(currentLatitude, currentLongitude, Input.location.lastData.altitude);
            objectToMove.position = unityPosition;
        }
        else
        {
            gpsText.text = "Cannot calculate distance. Either location is unavailable or coordinates are not set.";
        }
    }

    float HaversineDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000;
        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }
}

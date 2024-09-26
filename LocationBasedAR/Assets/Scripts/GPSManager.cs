using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    public TextMeshProUGUI gpsText; // Displays GPS info
    public Button getCoordinatesButton; // Button to get current coordinates
    public Button setCoordinatesButton; // Button to set (save) coordinates
    public Button calculateDistanceButton; // Button to calculate distance
    public GPSToUnity gpsToUnityConverter; // Optional: for converting GPS coordinates to Unity positions
    public Transform objectToMove; // Optional: move object based on GPS position

    private bool isLocationEnabled;
    private Vector3 savedCoordinates;

    void Start()
    {
        // Add button listeners for OnClick
        getCoordinatesButton.onClick.AddListener(GetCoordinates);
        setCoordinatesButton.onClick.AddListener(SetCoordinates);
        calculateDistanceButton.onClick.AddListener(CalculateDistance);

        // Handle Android location permissions
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
        }

        StartCoroutine(StartLocationService());
    }

    // Start location services, request user permission if not granted, and wait for initialization
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
            isLocationEnabled = true; // Location is enabled successfully
        }
    }

    // Automatically update the GPS info on the UI
    void Update()
    {
        if (isLocationEnabled)
        {
            gpsText.text = $"Lat: {Input.location.lastData.latitude} \n" +
                           $"Long: {Input.location.lastData.longitude} \n" +
                           $"Alt: {Input.location.lastData.altitude}";
        }
    }

    // This function will be called when the "Get Coordinates" button is clicked
    public void GetCoordinates()
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

    // This function will be called when the "Set Coordinates" button is clicked
    public void SetCoordinates()
    {
        if (isLocationEnabled)
        {
            // Save current coordinates in the savedCoordinates variable
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

    // This function will be called when the "Calculate Distance" button is clicked
    public void CalculateDistance()
    {
        if (isLocationEnabled && savedCoordinates != Vector3.zero)
        {
            float currentLatitude = Input.location.lastData.latitude;
            float currentLongitude = Input.location.lastData.longitude;

            // Calculate the distance between the saved coordinates and the current coordinates
            float distance = HaversineDistance(savedCoordinates.x, savedCoordinates.y, currentLatitude, currentLongitude);

            gpsText.text = $"Distance to saved location: {distance} meters";

            // Optional: Convert GPS to Unity position and move the object
            Vector3 unityPosition = gpsToUnityConverter.GPS2UnityPosition(currentLatitude, currentLongitude, Input.location.lastData.altitude);
            objectToMove.position = unityPosition;
        }
        else
        {
            gpsText.text = "Cannot calculate distance. Either location is unavailable or coordinates are not set.";
        }
    }

    // Haversine formula to calculate the distance between two GPS points (latitude/longitude)
    float HaversineDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000; // Radius of the Earth in meters
        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }
}

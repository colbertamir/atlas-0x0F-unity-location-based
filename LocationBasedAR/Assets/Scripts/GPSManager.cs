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
    private bool isLocationEnabled;
    private Vector3 savedCoordinates;

    void Start()
    {
        // Assign button listeners
        getCoordinatesButton.onClick.AddListener(GetCoordinates);
        setCoordinatesButton.onClick.AddListener(SetCoordinates);

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
        // Check if the user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            gpsText.text = "Location Services Disabled!";
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until the service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait <= 0)
        {
            gpsText.text = "Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            gpsText.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            isLocationEnabled = true;
        }
    }

    void Update()
    {
        // Continuously update the coordinates
        if (isLocationEnabled)
        {
            gpsText.text = $"Latitude: {Input.location.lastData.latitude} \n" +
                           $"Longitude: {Input.location.lastData.longitude} \n" +
                           $"Altitude: {Input.location.lastData.altitude}";
        }
    }

    // Button click method to get and display coordinates
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

    // Button click method to set & store current coordinates
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
}

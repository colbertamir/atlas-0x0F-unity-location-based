using UnityEngine;
using TMPro;
using System.Collections;

public class GPSManager : MonoBehaviour
{
    public TextMeshProUGUI gpsText;
    private bool isLocationEnabled;

    void Start()
    {
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
        if (isLocationEnabled)
        {
            // Fetch and display GPS coordinates
            gpsText.text = $"Latitude: {Input.location.lastData.latitude} \n" +
                           $"Longitude: {Input.location.lastData.longitude} \n" +
                           $"Altitude: {Input.location.lastData.altitude}";
        }
    }
}

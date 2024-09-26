using UnityEngine;

public class GPSToUnity : MonoBehaviour
{
    // Reference GPS point
    public double originLatitude = 0;
    public double originLongitude = 0;
    public double originAltitude = 0;

    // Earth's radius in meters
    private const float EarthRadius = 6371000f;

    // Converts GPS coordinates to Unity world position
    public Vector3 GPS2UnityPosition(float latitude, float longitude, float altitude)
    {
        // Calculate latitude/longitude in radians
        double latOriginRad = originLatitude * Mathf.Deg2Rad;
        double lonOriginRad = originLongitude * Mathf.Deg2Rad;
        double latTargetRad = latitude * Mathf.Deg2Rad;
        double lonTargetRad = longitude * Mathf.Deg2Rad;

        // Calculate the differences in latitude/longitude
        double deltaLat = latTargetRad - latOriginRad;
        double deltaLon = lonTargetRad - lonOriginRad;

        // Calculate distance using Haversine formula
        float x = (float)(deltaLon * Mathf.Cos((float)latOriginRad) * EarthRadius);
        float z = (float)(deltaLat * EarthRadius);

        // Adjust altitude as y position
        float y = altitude - (float)originAltitude;

        // Return as Unity's world position
        return new Vector3(x, y, z);
    }
}

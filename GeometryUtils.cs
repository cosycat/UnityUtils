using UnityEngine;

namespace UnityUtils
{
    public static class GeometryUtils
    {
        public static void CalculateWorldAlignedBoundingBox(Vector3 dimensions, Vector3 center, Quaternion rotation, out Vector3 minWorldPos, out Vector3 maxWorldPos)
        {
            var minPos = Vector3.positiveInfinity;
            var maxPos = Vector3.negativeInfinity;
            // check every corner of the box, whether with the current rotation it is the new min or max
            for (var i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? dimensions.x / 2 : -dimensions.x / 2,
                    (i & 2) == 0 ? dimensions.y / 2 : -dimensions.y / 2,
                    (i & 4) == 0 ? dimensions.z / 2 : -dimensions.z / 2
                );
                corner = rotation * corner + center;
                minPos = Vector3.Min(minPos, corner);
                maxPos = Vector3.Max(maxPos, corner);
            }
            minWorldPos = minPos;
            maxWorldPos = maxPos;
            if (minWorldPos == Vector3.positiveInfinity || maxWorldPos == Vector3.negativeInfinity)
            {
                Debug.LogWarning($"Something went wrong with the calculation of the min and max of the ship: min: {minWorldPos}, max: {maxWorldPos}");
            }
        }
    }
}
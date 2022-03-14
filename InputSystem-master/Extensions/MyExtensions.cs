using UnityEngine;
using System.Collections;

public static class MyExtensions
{
    public static bool ExistAndNotEmpty(this ICollection collection) => collection != null && collection.Count != 0;

    public static Vector3 Set_X(this Vector3 pos, float value) => new Vector3(value, pos.y, pos.z);
    public static Vector3 Offset_X(this Vector3 pos, float value) => new Vector3(pos.x + value, pos.y, pos.z);
    public static Vector3 Set_Y(this Vector3 pos, float value) => new Vector3(pos.x, value, pos.z);
    public static Vector3 Offset_Y(this Vector3 pos, float value) => new Vector3(pos.x, pos.y + value, pos.z);
    public static Vector3 Set_Z(this Vector3 pos, float value) => new Vector3(pos.x, pos.y, value);
    public static Vector3 Offset_Z(this Vector3 pos, float value) => new Vector3(pos.x, pos.y, pos.z + value);
    public static Vector3 Offset(this Vector3 pos, float x, float y, float z) => new Vector3(pos.x + x, pos.y + y, pos.z + z);
}
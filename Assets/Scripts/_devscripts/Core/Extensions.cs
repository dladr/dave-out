using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Collection of useful extension methods for Unity development.
/// Provides convenient utilities for Color, Vector3, Collections, Transform, and more.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Modifies color alpha value. Automatically clamps to valid range (0-1).
    /// </summary>
    /// <param name="color">Color to modify</param>
    /// <param name="alpha">Alpha value to set (0-1)</param>
    /// <returns>New color with modified alpha</returns>
    public static Color SetAlpha(this Color color, float alpha)
    {
        if (alpha < 0)
        {
            Debug.LogWarning($"SetAlpha was set to {alpha}. Hoisting to 0");
            alpha = 0;
        }
        else if (alpha > 1)
        {
            Debug.LogWarning($"SetAlpha was set to {alpha}. Lowering to 1");
            alpha = 1;
        }

        return new Color(color.r, color.g, color.b, alpha);
    }

    /// <summary>
    /// Creates a new color with selectively modified components.
    /// </summary>
    /// <param name="color">Base color to modify</param>
    /// <param name="r">New red value (null to keep existing)</param>
    /// <param name="g">New green value (null to keep existing)</param>
    /// <param name="b">New blue value (null to keep existing)</param>
    /// <param name="a">New alpha value (null to keep existing)</param>
    /// <returns>New color with modified components</returns>
    public static Color Set(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        a ??= color.a;
        r ??= color.r;
        g ??= color.g;
        b ??= color.b;

        return new Color(r.Value, g.Value, b.Value, a.Value);
    }

    /// <summary>
    /// Creates a new Vector3 by adding values to existing components.
    /// </summary>
    /// <param name="v">Base vector</param>
    /// <param name="x">Value to add to X component</param>
    /// <param name="y">Value to add to Y component</param>
    /// <param name="z">Value to add to Z component</param>
    /// <returns>New vector with added values</returns>
    public static Vector3 Add(this Vector3 v, float x = 0, float y = 0, float z = 0)
    {
        v = new Vector3(v.x + x, v.y + y, v.z + z);
        return v;
    }

    /// <summary>
    /// Creates a new Vector3 with selectively modified components.
    /// </summary>
    /// <param name="v">Base vector</param>
    /// <param name="x">New X value (null to keep existing)</param>
    /// <param name="y">New Y value (null to keep existing)</param>
    /// <param name="z">New Z value (null to keep existing)</param>
    /// <returns>New vector with modified components</returns>
    public static Vector3 Modify(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        v = new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        return v;
    }

    /// <summary>
    /// Gets a random element from any enumerable collection.
    /// Optimized to enumerate the collection only once.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection</typeparam>
    /// <param name="list">Collection to pick from</param>
    /// <returns>Random element from the collection</returns>
    /// <exception cref="Exception">Thrown if collection is empty</exception>
    public static T GetRandom<T>(this IEnumerable<T> list)
    {
        var listArray = list as T[] ?? list.ToArray(); // Enumerate once
        var count = listArray.Length;
        
        if (count == 1)
            return listArray[0];
        if (count == 0)
        {
            throw new Exception("Hey, this random list doesn't have any elements!");
        }
        var random = Random.Range(0, count);
        return listArray[random];
    }

    public static float RandomRange(this float f)
    {
        return Random.Range(-f, f);
    }

    public static float RandomRange(this int num)
    {
        return Random.Range(-num, num + 1);
    }

    public static Vector3 RandomRange(this Vector3 v)
    {
        return new Vector3(
            v.x.RandomRange(),
            v.y.RandomRange(),
            v.z.RandomRange()
        );
    }

    public static void ShuffleArray<T>(this T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    public static string ToLowerRemoveSpaces(this string s)
    {
        return s?.ToLower().Replace(" ", "");
    }

    public static Transform FindRecursive(this Transform t, string name)
    {
        foreach (Transform transform in t)
        {
            if (transform.name == name)
            {
                return transform;
            }
            var deepResult = FindRecursive(transform, name);
            if (deepResult != null)
                return deepResult;
        }
        return null;
    }

    public static GameObject[] GetAllDirectChildren(this Transform t)
    {
        var children = new List<GameObject>();
        foreach (Transform child in t)
        {
            children.Add(child.gameObject);
        }
        return children.ToArray();
    }

    public static Transform[] GetAllDirectChildrenTransforms(this Transform t)
    {
        var children = new List<Transform>();
        foreach (Transform child in t)
        {
            children.Add(child);
        }
        return children.ToArray();
    }
}



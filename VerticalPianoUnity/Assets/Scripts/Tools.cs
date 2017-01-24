using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public static class Tools
{
    // Trig
    public static Vector2 Perpendicular(Vector2 v)
    {
        return new Vector2(v.y, -v.x);
    }
    public static float PosifyRotation(float rotation)
    {
        rotation = rotation % (Mathf.PI * 2f);

        return rotation >= 0 ? rotation : rotation + Mathf.PI * 2f;
    }
    /// <summary>
    /// 0 is north, 1 is north west etc.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static int AngleToEightDirInt(float angle)
    {
        float a = PosifyRotation(angle) - (Mathf.PI * (2 / 16f));
        int dir = (int)(a / (Mathf.PI / 4f)) - 1;
        if (dir == -1) dir = 7;

        return dir;
    }
    public static float AngleBetweenVectors(Vector2 p1, Vector2 p2)
    {
        float theta = Mathf.Atan2(Mathf.Abs(p2.y - p1.y), Mathf.Abs(p2.x - p1.x));
        //Debug.Log("Theta:" + "(" + (p2.y - p1.y) + ") / (" + (p2.x - p1.x) + ")");
        if (p2.y > p1.y)
        {
            if (p2.x > p1.x)
            {
                return theta;
            }
            else
            {
                return Mathf.PI - theta;
            }
        }
        else
        {
            if (p2.x > p1.x)
            {
                return Mathf.PI * 2 - theta;
            }
            else
            {
                return Mathf.PI + theta;
            }
        }
    }


    // Random
    public static Vector2 RandomDirection2D()
    {
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    public static Vector2 RandomPosInCircle(float radius)
    {
        float t = 2f * Mathf.PI * UnityEngine.Random.value;
        float u = UnityEngine.Random.value + UnityEngine.Random.value;
        float r = (u > 1 ? 2 - u : u) * radius;
        return new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
    }
    public static int RandomSign()
    {
        return UnityEngine.Random.value < 0.5f ? -1 : 1;
    }
    public static float RandNeg()
    {
        return (UnityEngine.Random.value - 0.5f) * 2f;
    }
    public static T RandomEnum<T>()
    {
        Array v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(UnityEngine.Random.Range(0, v.Length));
    }
    /// <summary>
    /// If all weights are 0, treats all weights as 1
    /// </summary>
    /// <param name="weights"></param>
    /// <returns></returns>
    public static int WeightedChoice(float[] weights)
    {
        float total = 0;
        foreach (float w in weights) total += w;

        if (total == 0) return UnityEngine.Random.Range(0, weights.Length);

        float rand = UnityEngine.Random.value * total;
        for (int i = 0; i < weights.Length; ++i)
        {
            rand -= weights[i];
            if (rand <= 0) return i;
        }
        return -1; // never get here
    }


    // Math
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    public static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }

    // Text
    public static string ColorRichTxt(string s, Color color)
    {
        return ColorRichTxt(s, 0, s.Length, color);
    }
    public static string ColorRichTxt(string s, int start_i, int n, Color color)
    {
        s = s.Insert(start_i + n, "</color>");
        s = s.Insert(start_i, "<color=#" + ColorToHex(color) + ">");
        return s;
    }

    // Other
    public static T[] ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            int r = UnityEngine.Random.Range(0, array.Length - 1);
            T temp = array[r];
            array[r] = array[i];
            array[i] = temp;
        }
        return array;
    }
    public static bool ArrayContains<T>(T[] array, T value)
    {
        foreach (T v in array)
        {
            if (v.Equals(value)) return true;
        }
        return false;
    }

    public static T NextEnumValue<T>(T value)
    {
        Array v = Enum.GetValues(typeof(T));
        return (T)(object)(((int)(object)value + 1) % v.Length);
    }
    public static int EnumLength(Type enum_type)
    {
        return Enum.GetValues(enum_type).Length;
    }
    public static Array EnumValues(Type enum_type)
    {
        return Enum.GetValues(enum_type);
    }

    /// <summary>
    /// Finds the index of the max value in array. If several max values exist, one of their indices is chosen at random.
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static int MaxIndex(float[] array)
    {
        float max = float.MinValue;
        int i_initial = UnityEngine.Random.Range(0, array.Length);
        int max_i = i_initial;

        int i = i_initial;
        while (true)
        {
            if (array[i] > max)
            {
                max = array[i];
                max_i = i;
            }
            i = (i + 1) % array.Length;
            if (i == i_initial) break;
        }

        return max_i;
    }
    public static int IndexOfMax(int[] array)
    {
        int index = 0;
        int max = int.MinValue;
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i] > max)
            {
                index = i;
                max = array[i];
            }
        }

        return index;
    }

    public static KeyCode NumericalKey(int num)
    {
        switch (num)
        {
            case 0: return KeyCode.Alpha0;
            case 1: return KeyCode.Alpha1;
            case 2: return KeyCode.Alpha2;
            case 3: return KeyCode.Alpha3;
            case 4: return KeyCode.Alpha4;
            case 5: return KeyCode.Alpha5;
            case 6: return KeyCode.Alpha6;
            case 7: return KeyCode.Alpha7;
            case 8: return KeyCode.Alpha8;
            case 9: return KeyCode.Alpha9;
            default: return KeyCode.None;
        }
    }
    public static string FormatTimeAsMinSec(float total_seconds)
    {
        float total_minutes = total_seconds / 60f;
        float minute = (int)total_minutes;
        float second = (int)((total_minutes - minute) * 60);
        return minute + ":" + (second < 10 ? "0" + second : second.ToString());
    }

    /// <summary>
    /// Calls GameObject.Destroy on all children of transform. and immediately detaches the children
    /// from transform so after this call tranform.childCount is zero.
    /// </summary>
    public static void DestroyChildren(Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
        transform.DetachChildren();
    }

    public static void Populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    // Color
    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
    public static Color SetColorAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }


    // Debug
    public static void DebugDrawPlus(Vector2 pos, Color color, float size = 0.1f, float duration = 0)
    {
        float s = size / 2f;
        Debug.DrawLine(pos - Vector2.right * s, pos + Vector2.right * s, color, duration);
        Debug.DrawLine(pos - Vector2.up * s, pos + Vector2.up * s, color, duration);
    }
    public static void DebugDrawX(Vector2 pos, Color color, float size = 0.1f, float duration = 0)
    {
        float s = size / 2f;
        Debug.DrawLine(pos - new Vector2(1, 1) * s, pos + new Vector2(1, 1) * s, color, duration);
        Debug.DrawLine(pos - new Vector2(1, -1) * s, pos + new Vector2(1, -1) * s, color, duration);
    }
    public static void Log(object obj)
    {
        Debug.Log(obj.ToString() + "\n");
    }
    public static void Log(object obj, Color color)
    {
        Debug.Log("<color=#" + ColorToHex(color) + ">" + obj.ToString() + "</color>\n");
    }
}

public static class CoroutineUtil
{
    public static IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }
    public static IEnumerator ResetTrailRenderer(TrailRenderer tr)
    {
        float trailTime = tr.time;
        tr.time = 0;
        yield return new WaitForSeconds(0.5f);
        tr.time = trailTime;
    }
    public static IEnumerator DoNextFrame(Action func)
    {
        yield return null;
        func();
    }
    public static IEnumerator DoAtEndOfFrame(Action func)
    {
        yield return new WaitForEndOfFrame();
        func();
    }
    public static IEnumerator DoAfterDelay(Action func, float seconds, bool real_seconds = false)
    {
        if (real_seconds)
        {
            IEnumerator delay = WaitForRealSeconds(seconds);
            while (delay.MoveNext()) yield return delay.Current;
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
        func();
    }
    public static IEnumerator WaitForEvent(Action action)
    {
        bool flag = false;
        Action a = () => flag = true;
        action += a;
        while (flag == false) yield return null;
        action -= a;
    }
}

public class EventArgs<T> : EventArgs
{
    public T Value { get; private set; }

    public EventArgs(T val)
    {
        Value = val;
    }
}

public class IVector2
{
    public int x;
    public int y;

    public IVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public IVector2(Vector2 pos)
    {
        this.x = (int)pos.x;
        this.y = (int)pos.y;
    }
    public float Magnitude()
    {
        return Mathf.Sqrt(x * x + y * y);
    }
    public Vector2 AsVector2()
    {
        return new Vector2(x, y);
    }

    public static float Distance(IVector2 v1, IVector2 v2)
    {
        int dx = v2.x - v1.x;
        int dy = v2.y - v1.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
    public static int DistanceManhatten(IVector2 v1, IVector2 v2)
    {
        int dx = Mathf.Abs(v2.x - v1.x);
        int dy = Mathf.Abs(v2.y - v1.y);
        return dx + dy;
    }
    public static IVector2 operator +(IVector2 v1, IVector2 v2)
    {
        return new IVector2(v1.x + v2.x, v1.y + v2.y);
    }
    public static IVector2 operator -(IVector2 v1, IVector2 v2)
    {
        return new IVector2(v1.x - v2.x, v1.y - v2.y);
    }
    public static IVector2 operator *(IVector2 v1, int s)
    {
        return new IVector2(v1.x * s, v1.y * s);
    }
    public override bool Equals(object obj)
    {
        // If parameter is null return false
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to IVector2 return false
        IVector2 v = obj as IVector2;
        if ((System.Object)v == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (x == v.x) && (y == v.y);
    }
    public override int GetHashCode()
    {
        return x ^ y;
    }
    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}

public class Pair<T, U>
{
    public Pair()
    {
    }

    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }

    public T First { get; set; }
    public U Second { get; set; }
};

/// <summary>
/// Comparer for comparing two keys, handling equality as beeing greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater
        else
            return result;
    }
}

public class ModStr
{
    private string str;
    private SortedList<int, LinkedList<string>> mods;

    public ModStr(string str)
    {
        this.str = str;
        mods = new SortedList<int, LinkedList<string>>();
    }
    public void Insert(int startindex, string value, bool after_other_mods=false)
    {
        LinkedList<string> vals;
        if (mods.TryGetValue(startindex, out vals))
        {
            if (after_other_mods) vals.AddLast(value);
            else vals.AddFirst(value);
        }
        else
        {
            LinkedList<string> new_vals = new LinkedList<string>();
            new_vals.AddLast(value);
            mods.Add(startindex, new_vals);
        }
    }
    public void Surround(int startindex, int length, string left, string right)
    {
        Insert(startindex, left);
        Insert(startindex + length, right, true);
    }
    public void ColorRichTxt(int startindex, int length, Color color)
    {
        Surround(startindex, length, "<color=#" + Tools.ColorToHex(color) + ">", "</color>");
    }

    public string GetOriginal()
    {
        return str;
    }
    public string Get()
    {
        string modded_str = "";
        int str_i = 0;

        foreach (KeyValuePair<int, LinkedList<string>> mod in mods)
        {
            modded_str += str.Substring(str_i, mod.Key - str_i);
            foreach (string s in mod.Value) modded_str += s;
            str_i = mod.Key;
        }
        modded_str += str.Substring(str_i, str.Length - str_i);

        return modded_str;
    }
}
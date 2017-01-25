using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLineDrawer : MonoBehaviour
{
    public int initial_pool_size = 0;
    private int max_lines = 20;
    private int active_count = 0;

    private static DebugLineDrawer _instance;
    public static DebugLineDrawer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<DebugLineDrawer>();

                if (_instance == null) Debug.LogError("Missing DebugLineDrawer");
                else
                {
                    DontDestroyOnLoad(_instance);
                }
            }
            return _instance;
        }
    }

    private LinkedList<Line> lines;
    private Material mat;


	public static LineRenderer Draw(Vector3 p0, Vector3 p1, Color color, float seconds=0, float width = 0.1f)
    {
        DebugLineDrawer I = Instance;
        if (I.active_count >= I.max_lines) return null;

        LinkedListNode<Line> node = I.lines.First;
        Line newline = null;

        while (node != null)
        {
            Line line = node.Value;

            if (line.IsFree())
            {
                I.lines.Remove(node);
                newline = line;
                break;
            }

            node = node.Next;
        }

        if (newline == null)
        {
            newline = I.CreateNewLine();
        }

        newline.SetParams(p0, p1, color, seconds, width);
        newline.liner.gameObject.SetActive(true);
        I.AddLineToList(newline);
        ++I.active_count;

        return newline.liner;
    }
    

    private void Awake()
    {
        // if this is the first instance, make this the singleton
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(_instance);
            Initialize();
        }
        else
        {
            // destroy other instances that are not the already existing singleton
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }
    private void Initialize()
    {
        lines = new LinkedList<Line>();
        mat = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < initial_pool_size; ++i)
        {
            AddLineToList(CreateNewLine());
        }
    }
    private void Update()
    {
        LinkedListNode<Line> node = lines.First;

        while (node != null)
        {
            Line line = node.Value;

            if (!line.IsFree())
            {
                if (Time.time >= line.killtime)
                {
                    // Kill line
                    line.liner.gameObject.SetActive(false);
                    lines.Remove(node);
                    lines.AddFirst(line);
                    --active_count;
                }
                else
                {
                    break; // no more lines to kill
                }
            }
            node = node.Next;
        }   
    }
    private void AddLineToList(Line line)
    {
        LinkedListNode<Line> node = lines.Last;

        while (node != null)
        {
            if (node.Value.killtime <= line.killtime)
            {
                // Insert after this node
                lines.AddAfter(node, line);
                return;
            }

            node = node.Previous;
        }

        // Insert at head
        lines.AddFirst(line);
    }
    private Line CreateNewLine()
    {
        GameObject go = new GameObject("line");
        go.transform.SetParent(transform);
        go.SetActive(false);

        Line line = new Line();
        line.liner = go.AddComponent<LineRenderer>();
        line.liner.material = mat;

        return line;
    }


    private class Line
    {
        public LineRenderer liner;
        public float killtime;

        public bool IsFree()
        {
            return !liner.gameObject.activeInHierarchy;
        }
        public void SetParams(Vector3 p0, Vector3 p1, Color color, float seconds = 0, float width = 0.1f)
        {
            liner.SetPosition(0, p0);
            liner.SetPosition(1, p1);
            liner.startColor = color;
            liner.endColor = color;
            liner.startWidth = width;
            liner.endWidth = width;
            killtime = Time.time + seconds;
            if (seconds == 0) killtime += Time.deltaTime / 1000f;
        }
    }
}


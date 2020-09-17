using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TerminalManager : MonoBehaviour
{
    public static TerminalManager Instance;

    [Header("Settings")]
    [SerializeField]
    int maxLines = 300;

    [Header("Refereces")]
    [SerializeField]
    GameObject holder;

    [SerializeField]
    TMP_Text viewArea;

    [SerializeField]
    TMP_InputField inputField;

    Dictionary<string, System.Action<List<string>>> commands = new Dictionary<string, System.Action<List<string>>>();

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        this.RegisterCommands();

        this.viewArea.text = string.Empty;
        this.holder.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            this.Toggle();
        }
    }

    void Toggle()
    {
        this.holder.SetActive(!this.holder.activeSelf);

        if(this.holder.activeSelf)
        {
            this.inputField.text = string.Empty;
        }
    }

    public void Command()
    {
        TerminalManager.Log(this.inputField.text);

        string cmd;
        List<string> args;

        this.Parse(this.inputField.text, out cmd, out args);

        if(!this.commands.ContainsKey(cmd))
        {
            TerminalManager.LogError("Command " + cmd + " not found!");
            return;
        }

        this.commands[cmd](args);

        this.inputField.text = string.Empty;
    }

    public void AddLine(string _line)
    {
        this.viewArea.text += "\n"+_line;

        List<string> lines = new List<string>(this.viewArea.text.Split('\n'));

        while(lines.Count > this.maxLines)
        {
            lines.RemoveAt(0);
        }

        this.viewArea.text = string.Join("\n", lines);
    }

    public static void Log(string _text)
    {
        TerminalManager.Instance.AddLine(_text);
        Debug.Log(_text);
    }

    public static void LogWarning(string _text)
    {
        TerminalManager.Instance.AddLine("<color=yellow>" + _text + "</color>");
        Debug.LogWarning(_text);
    }

    public static void LogError(string _text)
    {
        TerminalManager.Instance.AddLine("<color=red>" + _text + "</color>");
    }

    void Parse(string _line, out string _cmd, out List<string> _args)
    {
        List<string> parsed = new List<string>(_line.Split(' '));

        _cmd = parsed[0];
        parsed.RemoveAt(0);

        _args = parsed;
    }
    public void RegisterCommands()
    {
        var rejected_commands = new Dictionary<string, TerminalCommand>();
        var method_flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

        foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var method in type.GetMethods(method_flags))
            {
                var attribute = System.Attribute.GetCustomAttribute(
                    method, typeof(TerminalCommand)) as TerminalCommand;

                if (attribute == null)
                {
                    continue;
                }

                var methods_params = method.GetParameters();

                string command_name = attribute.Cmd;
                System.Action<List<string>> proc;


                if (methods_params.Length != 1 || methods_params[0].ParameterType != typeof(List<string>))
                {
                    continue;
                }

                // Convert MethodInfo to Action.
                // This is essentially allows us to store a reference to the method,
                // which makes calling the method significantly more performant than using MethodInfo.Invoke().
                proc = (System.Action<List<string>>)System.Delegate.CreateDelegate(typeof(System.Action<List<string>>), method);
                this.AddCommand(attribute.Cmd, proc, attribute.Help);
            }
        }
    }

    public void AddCommand(string _cmd, System.Action<List<string>> _action, string _help)
    {
        this.commands.Add(_cmd, _action);
    }
}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class TerminalCommand : System.Attribute
{
    public string Cmd
    {
        get;
        private set;
    }

    public string Help
    {
        get;
        private set;
    }

    public TerminalCommand(string _cmd, string _help)
    {
        this.Cmd = _cmd;
        this.Help = _help;
    }
}
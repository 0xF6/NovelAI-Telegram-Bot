using Python.Runtime;

namespace nai.nai;

public static class PythonBridge
{
    public static void Init()
    {
        Runtime.PythonDLL = Config.PythonDllPath;
        Console.WriteLine($"Selected Runtime.PythonDLL: {Runtime.PythonDLL}");
    }


    public static PythonNovelAI Create() => new();
}


public class PythonNovelAI : IDisposable
{
    private nint thP;

    public PythonNovelAI()
    {
        PythonEngine.Initialize();
        thP = PythonEngine.BeginAllowThreads();
    }


    public void FoBar()
    {
        dynamic np = Py.Import("numpy");
    }

    public void Dispose() => PythonEngine.Shutdown();
}
using IngameDebugConsole;
using System.Reflection;
using CMD = IngameDebugConsole.ConsoleMethodAttribute;

public static class DebugCommand
{
    public static void CommandRegist()
    {
        if (DebugLogManager.Instance == null) return;

        var methods = typeof(DebugCommand).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        foreach (var method in methods)
        {
            var cma = method.GetCustomAttribute<CMD>();
            if (cma != null)
            {
                DebugLogConsole.AddCommand(cma.Command, cma.Description, method, null, cma.ParameterNames);
            }
        }
    }

    [CMD("gm.iostep", "AxiIO步进", "counter")]
    public static void SetAxiIODebugStep(int p1)
    {
        AxiIO.AxiIO.SetDebugStep(p1);
    }

    [CMD("gm.iocl", "AxiIO步进")]
    public static void SetAxiIODebugClear()
    {
        AxiIO.AxiIO.ClearDbgStep();
    }

    [CMD("gm.nscl", "AxiNS Dir步进")]
    public static void SetAxiNSIODebugStep_Clear()
    {
        AxiNSIO.ClearDbgStep();
    }

    [CMD("gm.nsdir", "AxiNS Dir步进", "idx")]
    public static void SetAxiNSIODebugStep_dir(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.Dir, p1);
    }

    [CMD("gm.nsloadf", "AxiNS loadfile步进", "idx")]
    public static void SetAxiNSIODebugStep_loadfile(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.LoadFile, p1);
    }

    [CMD("gm.nssavef", "AxiNS savefile步进", "idx")]
    public static void SetAxiNSIODebugStep_savefile(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.SaveFile, p1);
    }
}

using AxibugEmuOnline.Client.ClientCore;
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


    [CMD("dbgall", "開啓所有IOdebug")]
    public static void SetAxiIODebugAllOpen()
    {
        SetAxiIODebugStep(99999999);
        SetAxiNSIODebugStep_dir(99999999);
        SetAxiNSIODebugStep_loadfile(99999999);
        SetAxiNSIODebugStep_savefile(99999999);
    }

    [CMD("iostep", "AxiIO步进", "counter")]
    public static void SetAxiIODebugStep(int p1)
    {
        AxiIO.AxiIO.SetDebugStep(p1);
    }


    [CMD("iocl", "AxiIO步进")]
    public static void SetAxiIODebugClear()
    {
        AxiIO.AxiIO.ClearDbgStep();
    }

    [CMD("nscl", "AxiNS Dir步进")]
    public static void SetAxiNSIODebugStep_Clear()
    {
        AxiNSIO.ClearDbgStep();
    }

    [CMD("nsdir", "AxiNS Dir步进", "idx")]
    public static void SetAxiNSIODebugStep_dir(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.Dir, p1);
    }

    [CMD("nsloadf", "AxiNS loadfile步进", "idx")]
    public static void SetAxiNSIODebugStep_loadfile(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.LoadFile, p1);
    }

    [CMD("nssavef", "AxiNS savefile步进", "idx")]
    public static void SetAxiNSIODebugStep_savefile(int p1)
    {
        AxiNSIO.SetDebugStep(AxiNSIO.E_AxiNS_dgbBk.SaveFile, p1);
    }

    [CMD("sgstep", "USG步进", "counter")]
    public static void SetUSGDebugStep(int p1)
    {
        UStoicGoose.SetDebugStep(p1);
    }

    [CMD("sgcl", "USG步进清空")]
    public static void SetUSGDebugStep()
    {
        UStoicGoose.ClearDbgStep();
    }
    [CMD("hot", "NS预热")]
    public static void hot()
    {
#if UNITY_SWITCH
        App.coRunner.StartCoroutine(App.SwitchHotstorage());
#endif
    }
}

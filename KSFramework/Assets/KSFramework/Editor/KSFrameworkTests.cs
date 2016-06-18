#if UNITY_5
using System.IO;
using KEngine;
using KSFramework;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class KSFrameworkTests
{
    private static LuaModule _testLuaModule;
    public static LuaModule GetLuaModule()
    {
        if (_testLuaModule != null) return _testLuaModule;

	    var luaModule = new LuaModule();
	    var em = luaModule.Init();
	    while (em.MoveNext())
	    {
	    }
        _testLuaModule = luaModule;
        return luaModule;
    }

	[Test]
	public void AllLuaTests()
	{
	    var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath, "LuaTests");
        foreach(var filepath in Directory.GetFiles(editorLuaScriptPath, "*.lua", SearchOption.AllDirectories))
        {
            Log.Warning("Test lua: {0}", filepath);
            var script = File.ReadAllBytes(filepath);
            GetLuaModule().ExecuteScript(script);
        }

	}
}
#endif

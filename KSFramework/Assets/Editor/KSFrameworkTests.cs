#if UNITY_5 || UNITY_2017_1_OR_NEWER
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

	    var luaModule = LuaModule.Instance;
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

            object ret;
            var result = GetLuaModule().ExecuteScript(script, out ret);
            Assert.IsTrue(result, "Lua Tests Failed: {0}", filepath);
        }

	}
}
#endif

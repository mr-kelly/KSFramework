if not Log then
    Log = CS.KEngine.Log
end

if not Time then 
	Time = CS.UnityEngine.Time
end

TestLuaBehaivour = {}

function TestLuaBehaivour:Awake()
    Log.Info("Test Lua Behaivour Awake!")
end
function TestLuaBehaivour:Update()

    if Time.frameCount % 100 == 0 then
        Log.Info("Test Lua Behaivour Update!")
    end
end

function TestLuaBehaivour:Start()
    Log.Info("Test Lua Behaivour start!")
end

return TestLuaBehaivour



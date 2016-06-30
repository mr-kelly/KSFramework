if not Log then
    Log = import_type('KEngine.Log')
end

TestLuaBehaivour = {}

function TestLuaBehaivour:Awake()
    Log.Info("Test Lua Behaivour Awake!")
end

function TestLuaBehaivour:Start()
    Log.Info("Test Lua Behaivour start!")
end

return TestLuaBehaivour



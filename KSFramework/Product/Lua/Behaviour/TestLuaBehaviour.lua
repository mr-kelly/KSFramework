Time  = CS.UnityEngine.Time

local TestLuaBehaivour = {}

function TestLuaBehaivour:Awake()
    self.A="test"

    print("Test Lua Behaivour Awake!")
end



function TestLuaBehaivour:Start()
    print("Test Lua Behaivour start!")
end

function TestLuaBehaivour:Update()

    if Time.frameCount % 100 == 0 then
        --print("Test Lua Behaivour Update!")
    end
end

return TestLuaBehaivour



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

/// <summary>
/// xlua中require第三方库，在lua中使用请参考Login.lua
/// </summary>
public class XLuaRequireTest : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        LuaEnv luaenv = new LuaEnv();
        luaenv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
        luaenv.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
        luaenv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);
        luaenv.AddBuildin("ffi", XLua.LuaDLL.Lua.LoadFFI);
        luaenv.DoString(@"
        ------------------------------------
        local rapidjson = require 'rapidjson' 
        local t = rapidjson.decode('{""a"":123}')
        print(t.a)
        t.a = 456
        local s = rapidjson.encode(t)
        print('json', s)
        ------------------------------------
        local lpeg = require 'lpeg'
        print(lpeg.match(lpeg.R '09','123'))
        ------------------------------------        
        local pb = require 'pb'
        local protoc = require 'protoc'

        assert(protoc:load [[
        message Phone {
            optional string name        = 1;
            optional int64  phonenumber = 2;
        }
        message Person {
            optional string name     = 1;
            optional int32  age      = 2;
            optional string address  = 3;
            repeated Phone  contacts = 4;
        } ]])

        local data = {
        name = 'ilse',
        age  = 18,
            contacts = {
                { name = 'alice', phonenumber = 12312341234 },
                { name = 'bob',   phonenumber = 45645674567 }
            }
        }

        local bytes = assert(pb.encode('Person', data))
        print(pb.tohex(bytes))

        local data2 = assert(pb.decode('Person', bytes))
        print(data2.name)
        print(data2.age)
        print(data2.address)
        print(data2.contacts[1].name)
        print(data2.contacts[1].phonenumber)
        print(data2.contacts[2].name)
        print(data2.contacts[2].phonenumber)
        ---------------------------------
        local ffi = require('ffi')
        ffi.cdef [[
            typedef struct {int fake_id;unsigned int len;} CSSHeader;
        ]]
        ffi.cdef [[
            typedef struct {
                CSSHeader header;
                float x;
                float y;
                float z;
            } Vector3;
        ]]

        local Vector3Native = ffi.typeof('Vector3 *')
        local v = CS.UnityEngine.Vector3(1, 2, 3)
        local vn = ffi.cast(Vector3Native, v)
        print(vn)
        if vn.header.fake_id == -1 then
            print('vector { ', vn.x, vn.y, vn.z, '}')
        else
            print('please gen code')
        end
       "
        );
        luaenv.Dispose();
    }
}
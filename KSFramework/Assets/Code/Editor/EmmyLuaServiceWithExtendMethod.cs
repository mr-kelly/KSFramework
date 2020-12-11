using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace EmmyLua
{
	enum TypeKind
	{
		Class,
		Array,
	}

	enum Proto
	{
		Lib,
		Ping
	}

	[InitializeOnLoad]
	class EmmyLuaService
	{
		private static Socket socket;

		private static Thread receiveThread;

		private static int PORT = 996;

		private static bool doTryLater;
		
		private static DateTime lastTime;

		private static bool connected;

		private static bool running;

		private const string KEY_EMMY_SERVICE_ENABLE = "emmy.service.enable";
		
		private static Dictionary<Type, List<MethodInfo>> type2ExtendMethod = new Dictionary<Type, List<MethodInfo>>();
		private static bool IsEnable
		{
			get { return EditorPrefs.GetBool(KEY_EMMY_SERVICE_ENABLE); }
			set { EditorPrefs.SetBool(KEY_EMMY_SERVICE_ENABLE, value); }
		}

		[MenuItem("KEngine/EmmyLua/Enable UnityAPI提示")]
		static void EnableService()
		{
			IsEnable = true;
			StartConnect();
		}
		
		[MenuItem("KEngine/EmmyLua/Enable UnityAPI提示", true)]
		static bool EnableServiceCheck()
		{
			return !IsEnable;
		}
		
		[MenuItem("KEngine/EmmyLua/Disable UnityAPI提示")]
		static void DisableService()
		{
			IsEnable = false;
			Stop();
		}
		
		[MenuItem("KEngine/EmmyLua/Disable UnityAPI提示", true)]
		static bool DisableServiceCheck()
		{
			return IsEnable;
		}

		static EmmyLuaService()
		{
			EditorApplication.update += Update;
			StartConnect();
		}

		static void StartConnect()
		{
			if (running || !IsEnable)
				return;
			running = true;
			connected = false;
			doTryLater = false;
			try
			{
				if (socket != null)
					socket.Close();
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect("127.0.0.1", PORT, OnConnect, socket);
			}
			catch (Exception )
			{
				TryLater();
			}
		}

		private static void OnConnect(IAsyncResult ar)
		{
			try
			{
				socket.EndConnect(ar);
				connected = true;
				SendData(socket);
			}
			catch (Exception )
			{
				TryLater();
			}
		}

		private static void TryLater()
		{
			running = false;
			connected = false;
			doTryLater = true;
			lastTime = DateTime.Now;
		}

		private static void Stop()
		{
			if (running)
			{
				running = false;
				connected = false;
				doTryLater = false;
				
				if (socket != null)
					socket.Close();
			}
		}

		private static void Update()
		{
			if (!IsEnable)
				return;
			var sp = DateTime.Now - lastTime;
			if (sp.TotalSeconds > 5)
			{
				if (connected)
				{
					Ping();
				}
				else if (doTryLater)
				{
					StartConnect();
				}
			}
		}

		[DidReloadScripts]
		static void UpdateScripts()
		{
			StartConnect();
		}

		private static void WriteString(BinaryWriter writer, string value)
		{
			var encoding = Encoding.UTF8;
			var chars = encoding.GetBytes(value);
			writer.Write(chars.Length);
			writer.Write(chars);
		}

		private static void WriteType(BinaryWriter write, Type type)
		{
			if (type.IsArray)
			{
				write.Write((byte) TypeKind.Array);
				WriteType(write, type.GetElementType());
			}
			else
			{
				write.Write((byte) TypeKind.Class);
				WriteString(write, type.FullName ?? "any");
			}
		}

		private static void Ping()
		{
			using (var buf = new MemoryStream())
			{
				var writer = new BinaryWriter(buf);
				writer.Write(8);
				writer.Write((int) Proto.Ping);
				try
				{
					var bytes = buf.GetBuffer();
					socket.Send(bytes, 8, SocketFlags.None);
				}
				catch (Exception )
				{
					TryLater();
				}
			}
		}
		
		private static void SendData(Socket client)
		{
			var buf = new MemoryStream();
			var writer = new BinaryWriter(buf);
			writer.Seek(8, SeekOrigin.Begin);
			var types = GetTypes();

			ParseExtendMethod(types);
		
			foreach (var type in types)
			{
				WriteTypeData(type, writer);
			}
			type2ExtendMethod.Clear();

				
			writer.Flush();
			// write size and proto
			var len = (int) buf.Length;
			writer.Seek(0, SeekOrigin.Begin);
			writer.Write(len);
			writer.Write((int) Proto.Lib);
			writer.Flush();
			// send
			client.Send(buf.GetBuffer(), len, SocketFlags.None);
			writer.Close();
			
		}

		private static void ParseExtendMethod(Type[] types)
		{
			type2ExtendMethod.Clear();
			try
			{

				foreach (var type in types)
				{
					var extendsMethods = (
						from method in type.GetMethods(BindingFlags.Static
						                               | BindingFlags.Public | BindingFlags.NonPublic)
						where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
						select method
					);
					foreach (var extendMethod in extendsMethods)
					{
						var thisType = extendMethod.GetParameters()[0].ParameterType;
						if (type2ExtendMethod.ContainsKey(thisType))
						{
							var methods = type2ExtendMethod[thisType];
							methods.Add(extendMethod);
						}
						else
						{
							var methods = new List<MethodInfo>();
							methods.Add(extendMethod);
							type2ExtendMethod.Add(thisType, methods);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}
		}
		private static void WriteTypeData(Type type, BinaryWriter writer)
		{
			var fullName = type.FullName;
			if (!string.IsNullOrEmpty(fullName))
			{
				// full name
				WriteString(writer, fullName);

				// base type full name
				{
					string baseTypeFullName = null;
					if (type.BaseType != null)
						baseTypeFullName = type.BaseType.FullName;
					writer.Write(baseTypeFullName != null);
					if (baseTypeFullName != null)
						WriteString(writer, baseTypeFullName);
				}

				// fields
				var fields =
					type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
				writer.Write(fields.Length);
				foreach (var fi in fields)
				{
					WriteString(writer, fi.Name);
					WriteType(writer, fi.FieldType);
				}

				// properties
				var properties =
					type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static| BindingFlags.DeclaredOnly);
				writer.Write(properties.Length);
				foreach (var pi in properties)
				{
					WriteString(writer, pi.Name);
					WriteType(writer, pi.PropertyType);
				}

				// methods
				var methods =
					(from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance| BindingFlags.Static | BindingFlags.DeclaredOnly)
						where !mi.Name.StartsWith("get_") && !mi.Name.StartsWith("set_")
						select mi).ToArray();
				List<MethodInfo> extendMethods = null;
				if (!type2ExtendMethod.TryGetValue(type, out extendMethods))
				{
					extendMethods=new List<MethodInfo>();
				}
				
				writer.Write(methods.Count()+ extendMethods.Count());
				foreach (var mi in methods)
				{
					// name
					WriteString(writer, mi.Name);
					
					// is static
					writer.Write(mi.IsStatic);
					
					// parameters
					var parameterInfos = mi.GetParameters();
					writer.Write(parameterInfos.Length);
					foreach (var pi in parameterInfos)
					{
						WriteString(writer, pi.Name);
                        WriteType(writer, pi.ParameterType);						
					}

					// returns
					WriteType(writer, mi.ReturnType);
				}
				foreach (var mi in extendMethods)
				{
					// name
					WriteString(writer, mi.Name);
					
					// 必然视为方法
					writer.Write(false);
					
					// parameters 扩展方法忽略第一个参数
					var parameterInfos = mi.GetParameters().Skip(1).ToArray();
					writer.Write(parameterInfos.Length);
					foreach (var pi in parameterInfos)
					{
						WriteString(writer, pi.Name);
						WriteType(writer, pi.ParameterType);						
					}

					// returns
					WriteType(writer, mi.ReturnType);
				}
			}
		}

		private static Type[] GetTypes()
		{
			var unityTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where !(assembly.ManifestModule is ModuleBuilder)
				      && assembly.FullName != "UnityEditor"
				from type in assembly.GetExportedTypes()
				where type.BaseType != typeof(MulticastDelegate)
				      && !type.IsInterface 
				      && !type.IsEnum
				      //&& !type.IsGenericType
				      //&& !type.IsGenericTypeDefinition
				      && !type.IsNested
				      && !IsExcluded(type)
				select type;
			var arr = unityTypes.ToArray();

			return arr;
		}

		private static bool IsExcluded(Type type)
		{
			return false;
		}
	}
}
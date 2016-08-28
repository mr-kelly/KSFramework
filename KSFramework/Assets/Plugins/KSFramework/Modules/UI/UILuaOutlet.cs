using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KSFramework
{
	[RequireComponent(typeof(KEngine.UI.UIWindowAsset))]
	[DisallowMultipleComponent]
	public class UILuaOutlet : MonoBehaviour
	{
		/// Outlet info, serialize
		/// </summary>
		[System.Serializable]
		public class OutletInfo
		{
			/// <summary>
			/// Lua Property Name
			/// </summary>
			public string Name;

			/// <summary>
			/// Component type 's full name (with namespace)
			/// </summary>
			public string ComponentType;

			/// <summary>
			/// UI Control Object
			/// </summary>
			public UnityEngine.Object Object;
		}

		public List<OutletInfo> OutletInfos = new List<OutletInfo>();
	}
}

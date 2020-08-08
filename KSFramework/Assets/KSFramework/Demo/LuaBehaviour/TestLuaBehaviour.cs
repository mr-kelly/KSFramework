using System.Collections;
using System.Collections.Generic;
using KSFramework;
using UnityEngine;

public class TestLuaBehaviour : MonoBehaviour {

	void Awake ()
	{
	    LuaBehaviour.Create(gameObject, "Behaviour/TestLuaBehaviour");
	}
	
}

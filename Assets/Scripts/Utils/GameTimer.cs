using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A timer handles game time
/// </summary>
public class GameTimer : MonoBehaviour
{
	#region Fields
	
	// timer execution
	private float elapsedSeconds = 0;
	
	#endregion
	
	#region Properties

	public float ElapsedSeconds
    {
		get { return elapsedSeconds; }
    }

	#endregion

	#region Methods

	void Update()
    {	
		elapsedSeconds += Time.deltaTime;
	}
	
	/// <summary>
	/// Runs the timer
	/// </summary>
	public void Run()
    {
        elapsedSeconds = 0;
	}

	#endregion
}

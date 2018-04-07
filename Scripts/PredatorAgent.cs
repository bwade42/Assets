using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : Agent {

	Rigidbody rBody; //controls physics of Predator Agent

	void Start () 
	{
		rBody = GetComponent<Rigidbody>();
	}

	public Transform Target; //controls orientation,positon and scale of agent



	//When the agent reaches its target, it marks itself done and its agent reset function 
	//moves the target to a random location. In addition, if the agent rolls off the platform, 
	//the reset function puts it back onto the floor.

	public override void AgentReset()
	{
		
		//Agent go outside bounds of arena
		if (this.transform.position.y < 0)
		{  
			// The agent fell

			this.transform.position = Vector3.zero;
			this.rBody.angularVelocity = Vector3.zero;
			this.rBody.velocity = Vector3.zero;
		}
		else
		{ 
			// Move the prey to a new spot
			Target.position = new Vector3(Random.value * 8 - 4,
				0.5f,
				Random.value * 8 - 4);
		}
	}

	//Oberserving the enviorment
	/** All the values are divided by 5 to normalize the inputs to the neural network to the range [-1,1]. **/
	/** (The number five is used because the platform is 10 units across.) **/

	public override void CollectObservations()
	{
		/**Information that we want to send to the brain**/
		Debug.Log (this.transform.position.y);
		// Relative Position Of Prey 
		Vector3 relativePosition = Target.position - this.transform.position;

		AddVectorObs(relativePosition.x / 5);
		AddVectorObs(relativePosition.z / 5);

		/** Position of the agent itself within the confines of the floor. **/
		/** This data is collected as the agent's distance from each edge of the floor. **/

		//Distance to edges of platform 
		AddVectorObs((this.transform.position.x + 5) / 5);
		AddVectorObs((this.transform.position.x - 5) / 5);
		AddVectorObs((this.transform.position.z + 5) / 5);
		AddVectorObs((this.transform.position.z - 5) / 5);


		/** The velocity of the agent. This helps the agent learn to control its speed so it doesn't **/
		/** overshoot the target and roll off the platform. **/

		// Agent velocity
		AddVectorObs(rBody.velocity.x / 5);
		AddVectorObs(rBody.velocity.z / 5);


	}

	/** Decisions of the brain are passed to this function **/
	public float speed = 10;
	private float previousDistance = float.MaxValue;

	public override void AgentAction(float[] vectorAction, string textAction)
	{
		// Rewards
		float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

		// Reached target
		if (distanceToTarget < 1.42f)
		{
			Done();
			AddReward(1.0f);
		}

		// Getting closer
		if (distanceToTarget < previousDistance)
		{
			AddReward(0.1f);
		}

		// Time penalty
		AddReward(-0.05f);

		// Fell off platform
		if (this.transform.position.y < -1.0)
		{
			Done();
			AddReward(-1.0f);
		}
		previousDistance = distanceToTarget;

		// Here we can add a negative awards for not reaching the target in a certain amount of time

		// Actions, size = 2
		// If we wanted to move in three dimensions size would be 3
		//agent has no idea what these values, they are used  by the 
		//training process to see what kind of rewards it will see

		Vector3 controlSignal = Vector3.zero;

		/** Using values in the range of [-1, 1] is good for two reasons **
		 * 1. The learning algorithm has less incentive to try very large values
		 * 2. limit large values
		 **/
		controlSignal.x = Mathf.Clamp(vectorAction[0], -1, 1);
		controlSignal.z = Mathf.Clamp(vectorAction[1], -1, 1);
		rBody.AddForce(controlSignal * speed); // apply values from array to rigid body component
	}

}

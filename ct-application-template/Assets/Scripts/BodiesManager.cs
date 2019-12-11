using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BodiesManager : MonoBehaviour {

	private Dictionary<string, Human> _humans; //

    [Header("Local Human:")]
    public Transform humanTransform;  //Transform type: Component of GameObject that stores info about position and rotation f.ex
    [Space(5)]  //espaçamento de 5mm no display no Unity
    public Transform head;
    public Transform neck;
    public Transform spineShoulder;
    public Transform spineMid;
    public Transform spineBase;
    [Space(5)]
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightWrist;
    public Transform rightHand;
    public Transform rightHandTip;
    public Transform rightThumb;
    [Space(5)]
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftWrist;
    public Transform leftHand;
    public Transform leftHandTip;
    public Transform leftThumb;
    [Space(5)]
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightAnkle;
    public Transform rightFoot;
    [Space(5)]
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftAnkle;
    public Transform leftFoot;


    public Human human; //Class Human : has an id, a body and a time update

    [Space(20)]
    public Transform ObjectOI;
    public BodyJointType JointToChooseHumans;

    [Space(20)]
    public bool removeHead;
    [Range(0, 0.5f)]
    public float headSize;
    public float Y_HeadOffset;



    void Start () {
		_humans = new Dictionary<string, Human>(); //Inicialization of dictionary
        human = null; 
        _assembleHierarchy();
    }

    void Update () {

        //Example of how to iterate the humans.
		foreach (Human h in _humans.Values) {
			// get human properties:
			string id = h.id;

            // Inventar o metodo de escolher o gajo
            human = h;

            //just because it's an example :)
            break;
        }

        float dist = 12000.0f;
        foreach (Human h in _humans.Values)
        {
            float newDist = Vector3.Distance(h.body.Joints[JointToChooseHumans], ObjectOI.position);
            if ( newDist < dist)
            {
                human = h;
                dist = newDist;
            }
        }




        if (human != null) {          
            _disassembleHierarchy();
            _updateHumanJoints(human.body.Joints);
            _assembleHierarchy();
        }
        


		// finally
		_cleanDeadHumans();

        //if 
	}

	public void setNewFrame (Body[] bodies) // Adds new human if it isn't in the list
    {
        foreach (Body b in bodies)  
		{
            try //Try, catch and finally : exception handling
            {
			string bodyID = b.Properties[BodyPropertiesType.UID];
			if (!_humans.Keys.Contains(bodyID))
			{
				_humans.Add(bodyID, new Human());
			}
			_humans[bodyID].Update(b);
			}
			catch (Exception) { }
		}
	}

	void _cleanDeadHumans () //If human info hasn't been updated in more than 1 second, deadhumans() removes the human id from list of humans
    {
		List<Human> deadhumans = new List<Human>();

		foreach (Human h in _humans.Values)
		{
			if (DateTime.Now > h.lastUpdated.AddMilliseconds(1000))
				deadhumans.Add(h); 
		}

		foreach (Human h in deadhumans)
		{
			_humans.Remove(h.id);
		}
	}



    void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 200, 35), "Number of users: " + _humans.Count);
	}


    private void _disassembleHierarchy()
    {
        Transform human = head.parent;

        rightShoulder.parent = human;
        rightElbow.parent = human;
        rightWrist.parent = human;
        rightHand.parent = human;
        rightHandTip.parent = human;
        rightThumb.parent = rightHandTip.parent;

        leftShoulder.parent = human;
        leftElbow.parent = human;
        leftWrist.parent = human;
        leftHand.parent = human;
        leftHandTip.parent = human;
        leftThumb.parent = leftHandTip.parent;
    }

    private void _assembleHierarchy()
    {
        rightThumb.parent = rightHand;
        rightHandTip.parent = rightHand;
        rightHand.parent = rightWrist;
        rightWrist.parent = rightElbow;
        rightElbow.parent = rightShoulder;

        leftThumb.parent = leftHand;
        leftHandTip.parent = leftHand;
        leftHand.parent = leftWrist;
        leftWrist.parent = leftElbow;
        leftElbow.parent = leftShoulder;
    }
    

    private void _updateHumanJoints(Dictionary<BodyJointType, Vector3> joints)
    {
        //atribuição de um valor a todas as transform variables definidas no inicio. Valores vindos do CreepyTracker

        head.localPosition =            human.body.Joints[BodyJointType.head];
        neck.localPosition =            human.body.Joints[BodyJointType.neck];
        spineBase.localPosition =       human.body.Joints[BodyJointType.spineBase];        
        spineShoulder.localPosition =   human.body.Joints[BodyJointType.spineShoulder];
        spineMid.localPosition =        human.body.Joints[BodyJointType.spineMid];
                                        
        rightShoulder.localPosition =   human.body.Joints[BodyJointType.rightShoulder];
        rightElbow.localPosition =      human.body.Joints[BodyJointType.rightElbow];
        rightWrist.localPosition =      human.body.Joints[BodyJointType.rightWrist];
        rightHand.localPosition =       human.body.Joints[BodyJointType.rightHand];
        rightHandTip.localPosition =    human.body.Joints[BodyJointType.rightHandTip];
        rightThumb.localPosition =      human.body.Joints[BodyJointType.rightThumb];
                                        
        leftShoulder.localPosition =    human.body.Joints[BodyJointType.leftShoulder];
        leftElbow.localPosition =       human.body.Joints[BodyJointType.leftElbow];
        leftWrist.localPosition =       human.body.Joints[BodyJointType.leftWrist];
        leftHand.localPosition =        human.body.Joints[BodyJointType.leftHand];
        leftHandTip.localPosition =     human.body.Joints[BodyJointType.leftHandTip];
        leftThumb.localPosition =       human.body.Joints[BodyJointType.leftThumb];
                                        
        rightHip.localPosition =        human.body.Joints[BodyJointType.rightHip];
        rightKnee.localPosition =       human.body.Joints[BodyJointType.rightKnee];
        rightAnkle.localPosition =      human.body.Joints[BodyJointType.rightKnee];
        rightFoot.localPosition =       human.body.Joints[BodyJointType.rightFoot];
 
        leftHip.localPosition =         human.body.Joints[BodyJointType.leftHip];
        leftKnee.localPosition =        human.body.Joints[BodyJointType.leftKnee];
        leftAnkle.localPosition =       human.body.Joints[BodyJointType.leftKnee];
        leftFoot.localPosition =        human.body.Joints[BodyJointType.leftFoot];
    }



    [Space(20)]
    public Transform headPivot;
    public void calibrateHuman()
    {
        if (human != null)
        {
            UnityEngine.XR.InputTracking.Recenter();
            print("HUMAN RECENTER DONE");
        }
        else
        {
            Debug.LogError("No human to calibrate");
        }
    }

}

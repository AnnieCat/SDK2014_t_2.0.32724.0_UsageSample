using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

public class CubeMovement : MonoBehaviour
{
	private PXCMPipelineUtil pp=null;
	private PXCMHandAnalysis hand;

    public GameObject cube;
    public GameObject arrowVis;

    private bool handOpen;
    private float handOrient;

    private List<Vector3> distanceCheck;
    private List<float> xPosCheck;

    // Instantiate different versions of the smoothing script with different latency rates
    Smoothing sm1 = new Smoothing(4);
    Smoothing sm2 = new Smoothing(6);
    Smoothing sm3 = new Smoothing(8);
    Smoothing sm4 = new Smoothing(10);

    void Start () {

		if (PXCMPipelineUtil.CreateInstance(out pp)<pxcmStatus.PXCM_STATUS_NO_ERROR) {
			print("Unable to create the pipeline instance");
			return;
		}


		pp.EnableHandAnalysis();
		
		if (pp.Init()<pxcmStatus.PXCM_STATUS_NO_ERROR) {
			print("Unable to initialize all modalities");
			return;
		}

        distanceCheck = new List<Vector3>(0);
        xPosCheck = new List<float>(0);
    }
    
    void OnDisable() {
		if (pp==null) return;
		pp.Dispose();
		pp=null;
    }

    void Update () {
		if (pp==null) return;
		if (pp.AcquireFrame(false,0)<pxcmStatus.PXCM_STATUS_NO_ERROR) return;

		PXCMHandAnalysis.HandData hdata=null;
		hand=pp.QueryHandAnalysis();
		if (hand!=null) {
            //if(hand.QueryHandData(PXCMHandAnalysis.AccessOrder.AccessOrder_RightHands,0,out hdata)>=pxcmStatus.PXCM_STATUS_NO_ERROR)  to specify which hand
			if (hand.QueryHandData(PXCMHandAnalysis.AccessOrder.AccessOrder_NearToFar,0,out hdata)>=pxcmStatus.PXCM_STATUS_NO_ERROR)
			{
                //get hand position and smooth the data
				float xPos = Remap(hdata.massCenterImage.x,30f,288f,-3.5f,3.5f);
				float yPos = Remap (hdata.massCenterImage.y,226f,36f,0.4f,4.6f);
				float zPos = Remap (hdata.massCenterWorld.z,68f,0.5f,3.4f,-2.0f);

                List<double> myList = new List<double>() {xPos,yPos,zPos};
                myList = sm1.SmoothList(myList);
                cube.transform.position = new Vector3((float)myList[0], (float)myList[1], (float)myList[2]);

                //Get the amount of "fold" of all fingers, which gives a buggy hand open close, and smooth the data
                int closed = 0;
                for (int i = 0; i<hdata.fingersData.Length; i++)
                {
                    if (hdata.fingersData[i].foldedness < 100)
                        closed++;
                }
                var openness = sm2.SmoothValue(closed);



                //print(openness);
                if (openness >= 0.8f){
                    handOpen = false;
                    cube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }
                if (openness < 0.8f) {
                    handOpen = true;
                    cube.transform.localScale = new Vector3(1f,1f,1f);
                }



                // Check for hand orientation, works best when hand is open

                var handOrient = hdata.palmOrientation.z;

                if (handOrient > 0.3f)
                    arrowVis.transform.rotation = Quaternion.Euler(new Vector3(0f, 30f, 0f));
                if (handOrient < -0.2f)
                    arrowVis.transform.rotation = Quaternion.Euler(new Vector3(0f, -30f, 0f));
                if (handOrient > -0.2f && handOrient < 0.3f)
                    arrowVis.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));



                // Use the hand width when hand is closed to approximate which direction the puppet is "looking"

                float myZ = Remap(hdata.massCenterWorld.z, 15f, 80f, 80f, 15f);
                float myWidth = (30f * hdata.boundingBoxImage.w) / myZ;
                float myHeight = (30f * hdata.boundingBoxImage.h) / myZ;

                if (myWidth < 30f)
                    print("probably Right:     " + myWidth);
                if (myWidth > 30f && myWidth < 38f)
                    print("probably Forward:     " + myWidth);
                if (myWidth > 38f)
                    print("probably Left:     " + myWidth);



                // Determine hand speed

                float lastDistance = HandSpeedCalc(new Vector3(hdata.massCenterWorld.x, hdata.massCenterWorld.y, hdata.massCenterWorld.z));
                double handSpeed = sm2.SmoothValue((double)lastDistance);
                //print(handSpeed);



                // Was there a sudden movement to the left or right?
                float handXchange = HandJerkinessCalc(hdata.massCenterWorld.x);
                double fastXposMovement = sm3.SmoothValue(handXchange);

                if(fastXposMovement<0.5f&&fastXposMovement>0.09f)
                    print("looked left");
                if (fastXposMovement > -0.5f && fastXposMovement < -0.09f)
                    print("looked right");
            } 
		}

		
		
		pp.ReleaseFrame();
    }

    // Changes values from one range to another
    float Remap(float handLoc, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (handLoc - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }

    float HandSpeedCalc(Vector3 myLoc)
    {
        distanceCheck.Add(myLoc);

        float myDist = 0f;
        if (distanceCheck.Count > 2)
        {
            distanceCheck.RemoveAt(0);
            myDist = Vector3.Distance(distanceCheck[0], distanceCheck[1]);
        }

        return myDist;
    }

    float HandJerkinessCalc(float myPos)
    {
        xPosCheck.Add(myPos);

        float myMotion = 0f;
        if (xPosCheck.Capacity > 2)
        {
            myMotion = xPosCheck[2] - xPosCheck[1];
            xPosCheck.RemoveAt(0);
           
        }

        return myMotion;
    }
}

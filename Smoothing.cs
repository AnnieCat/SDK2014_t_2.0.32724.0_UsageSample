using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

// Ratko Jagodic
// 
// How to use:
// Smooth sm = new Smooth();
// myVal = sm.smoothValue(myVal);

//List<double> myList = new List<double>(){hand.x, hand.y, hand.z};
// myList = sm.smoothList(myList);
// myList[0]  // x
// myList[1]  // y
// myList[2]  // z


public class Smoothing : MonoBehaviour {

    int smoothing;
    // internal buffers for storing the history of values
    List<double> smoothingBuffer;
    List<List<double>> smoothingListBuffer;
    //List<cv::Rect> smoothingRectBuffer;


    public Smoothing(int smoothAmt = 4)
    {
        smoothing = smoothAmt;
        smoothingBuffer = new List<double>();
        smoothingListBuffer = new List<List<double>>();
        //smoothingRectBuffer = List<cv::Rect>();

        // initialize the smoothing buffer
        Clear();
    }

    void Clear()
    {
        // clear the smoothing buffers
        smoothingBuffer.Clear();
        smoothingListBuffer.Clear();
        //smoothingRectBuffer.Clear();
    }

    public double SmoothValue(double newVal)
    {
        smoothingBuffer.Add(newVal);
        if (smoothingBuffer.Count > smoothing)
            smoothingBuffer.RemoveAt(0); 

        // return average
        double tot = 0;
        int c = 0;
        for (int i = 0; i < smoothingBuffer.Count; i++)
        {
            tot += smoothingBuffer[i];
            c++;
        }
        
        return tot / c;

    }


    public List<double> SmoothList(List<double> newVal)
    {
        smoothingListBuffer.Add(newVal);
	    if (smoothingListBuffer.Count > smoothing)
		    smoothingListBuffer.RemoveAt(0); 

	    // return average
	    List<double> total = new List<double>(){0.0, 0.0, 0.0};
        List<double> ave = new List<double>(){0.0, 0.0, 0.0};

	    int c = 0;
	    for(int i=0; i<smoothingListBuffer.Count; i++)
	    {
		    total[0] += smoothingListBuffer[i][0];
		    total[1] += smoothingListBuffer[i][1];
		    total[2] += smoothingListBuffer[i][2];	
		    c++;
	    }

	    ave[0] = total[0]/c;
	    ave[1] = total[1]/c;
	    ave[2] = total[2]/c;

	    return ave;
    }

    /*
    cv::Rect Smoothing::smoothRect(cv::Rect newVal)
    {
        smoothingRectBuffer.push_back(newVal);
        if (smoothingRectBuffer.size() > smoothing)
            smoothingRectBuffer.erase(smoothingRectBuffer.begin());

        // return average
        cv::Rect total(0.0, 0.0, 0.0, 0.0);
        cv::Rect ave(0.0, 0.0, 0.0, 0.0);

        int c = 0;
        for(int i=0; i<smoothingRectBuffer.size(); i++)
        {
            total.x += smoothingRectBuffer[i].x;
            total.y += smoothingRectBuffer[i].y;
            total.height += smoothingRectBuffer[i].height;	
            total.width += smoothingRectBuffer[i].width;	
            c++;
        }

        ave.x = total.x/c;
        ave.y = total.y/c;
        ave.width = total.width/c;
        ave.height = total.height/c;

        return ave;
    }
        */


}

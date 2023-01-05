using System;
using System.Collections.Generic;

public static class EasingHelper
{
    public static float easeInSine(float _x) => (float)(1-Math.Cos((double)(_x*Math.PI)/2)); 
    public static float easeOutSine(float _x) => (float)(1-Math.Sin((double)(_x*Math.PI)/2));
    public static float easeInOutSine(float _x) => (float)((Math.Cos(Math.PI*((double)_x)))/2)*-1;
}
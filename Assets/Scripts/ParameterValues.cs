using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParameterValues
{
    public static float streetWidth, shoulderWidth, option3, option4, option5;


    public static float StreetWidth {			//note caps på func namn
	get{return streetWidth;}
	set{streetWidth=value;}
    }
    public static float ShoulderWidth {
	get{return shoulderWidth;}
	set{shoulderWidth=value;}
    }
    public static float Option3 {
	get{return option3;}
	set{option3=value;}
    }
    public static float Option4 {
	get{return option4;}
	set{option4=value;}
    }
    public static float Option5 {
	get{return option5;}
	set{option5=value;}
    }
}

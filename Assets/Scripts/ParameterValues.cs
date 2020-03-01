using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParameterValues
{
    public static float option1, option2, option3, option4, option5;


    public static float Option1 {			//note caps på func namn
	get{return option1;}
	set{option1=value;}
    }
    public static float Option2 {
	get{return option2;}
	set{option2=value;}
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSBackendApp
{
   /*
    *    Preprocessed weights for prediction model 
    * 
    *    88.53202305    - night_minutes
    *   -3484.70738165  - temp
    *   -45668.83834707 - newYear
    *   -19263.36088888 - holliday
    *   -19911.37125412 - sunday
    *   -14261.48527985 - saturday
    *   -41043.45609299 - covid_cases
    *
    *   656918.3379204228 - bies 
    */

    public static class PredictionModelConfiguration
    {
       public static double NightDurationWeight    = 88.53202305f;
       public static double WeatherWeight          = -3484.70738165f;
       public static double NewYearWeight          = -45668.8383470f;
       public static double HolidayWeight          = -19263.36088888f;
       public static double SundayWeight           = -19911.37125412f;
       public static double SaturdayWeight         = -14261.48527985f;
       public static double CovidCasesWeight       = -41043.45609299f;
                     
       public static double Bies                   = 656918.3379204228f;
    }
}

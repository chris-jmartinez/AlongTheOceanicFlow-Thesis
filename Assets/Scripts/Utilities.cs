using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public static class Utilities {

    public static void Shuffle<T>(this IList<T> list)
    {
        Random random = new Random();
        int n = list.Count;

        for (int i = list.Count - 1; i > 1; i--)
        {
            int k = random.Next(i + 1);

            T value = list[k];
            list[k] = list[i];
            list[i] = value;
        }
    }

    public static List<T> ShuffleLinq<T>(List<T> myList)
    {
        //shuffle
        var rnd = new Random();
        List<T> result = myList.OrderBy(item => rnd.Next()) as List<T>;
        return result;
    }


    public static float TruncateFloatTwoDecimals(float numberToTruncate)
    {
        float truncatedNumber = (float)(Math.Truncate((double)numberToTruncate * 100.0) / 100.0);

        return truncatedNumber;
    }

    //Code to convert from string to int and viceversa
    /*
    int labelIntCountdown = int.Parse(labelCountdownTextMesh.text);
    labelIntCountdown--;
    labelCountdownTextMesh.text = labelIntCountdown.ToString();
    */

}


using Newtonsoft.Json.Linq;

public class JsonParser
{
    private string jsonString;


	public JsonParser()
	{
	}

    public static EventObject Parse(string jsonString)
    {


        JObject jsonObject = JObject.Parse(jsonString);
        JArray jArrayEvents= (JArray)jsonObject["events"];
        EventObject eventObject = null ;
        
        // If arrive more events in a unique call
        //List<EventObject> eventObjectsList = new List<EventObject>();

        foreach (JObject jObject in jArrayEvents)
        {
            string type = (string)jObject.SelectToken("typ");
            string id = (string)jObject.SelectToken("val");
            string active = (string)jObject.SelectToken("act");
            if (jObject.SelectToken("dur") != null)
            {
                string duration = (string)jObject.SelectToken("dur");
                eventObject = new EventObject(type, id, active, duration);
            } else
            {
                eventObject = new EventObject(type, id, active, null);
            }
            
        }
        return eventObject;
    }



}

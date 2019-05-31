using System;

public class EventObject
{
    string type;
    string id;
    string active;
    string duration;
    public EventObject(string type, string id, string active, string duration)
	{
        this.type = type;
        this.id = id;
        this.active = active;
        this.duration = duration;

	}

    public string getType()
    {
        return type;
    }

    public string getDuration()
    {
        return duration;
    }

    public string getID()
    {
        return id;
    }

    public string getActive()
    {
        return active;
    }
}

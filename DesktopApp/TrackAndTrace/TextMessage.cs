using System;

namespace TrackAndTrace
{
    public class TextMessage
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public DateTime Time { get; set; }

        public TextMessage(string id, string body, string from, DateTime time)
        {
            Id = id;
            Body = body;
            From = from;
            Time = time;
        }

        public string GetOutputFileLine()
        { 
            return Body + "," + From + "," + Time.ToString("HH") + "," +Time.ToString("mm");
        }
    }
}
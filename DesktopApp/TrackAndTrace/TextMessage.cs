using System;

namespace TrackAndTrace
{
    /// <summary>
    /// Simple class for storing a text message.
    /// </summary>
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

        /// <summary>
        /// Creates the line for the file as it should be for the CSV output files.
        /// </summary>
        /// <returns>The file contents formatted for CSV.</returns>
        public string GetOutputFileLine()
        { 
            return Body + "," + From + "," + Time.ToString("HH") + "," +Time.ToString("mm");
        }
    }
}
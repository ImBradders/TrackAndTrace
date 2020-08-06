using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using TrackAndTrace;

namespace SearchApp
{
    public partial class SearchWindow : Form
    {
        private const string VIEWING_FILES_PATH = @"C:\TrackAndTrace\ViewingFiles";
        public SearchWindow()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Trim().Equals(""))
            {
                MessageBox.Show("Please ensure there is text in search field!", "Error - PEBCAK");
                return;
            }
            
            if (!filesExist())
            {
                MessageBox.Show("There are no files in the system to search through!\n" +
                                                "If you were expecting files to be there and they aren't, contact the developer.",
                                    "Error - No Files");
                return;
            }

            List<string> filesContainingCriteria = new List<string>();
            List<TextMessage> messages;
            string[] filesToSearch = Directory.GetFiles(VIEWING_FILES_PATH);

            foreach (string file in filesToSearch)
            {
                string fileShort = file.Substring(VIEWING_FILES_PATH.Length + 1);
                messages = processFile(file);

                foreach (TextMessage message in messages)
                {
                    if (message.Body.ToLower().Contains(txtName.Text.ToLower()))
                    {
                        if (!filesContainingCriteria.Contains(fileShort))
                            filesContainingCriteria.Add(fileShort);
                    }
                }
            }

            string output = "";

            if (filesContainingCriteria.Count == 0)
                output = "No files";
            else
            {
                int i;
                for (i = 0; i < filesContainingCriteria.Count - 1; i++)
                {
                    output += filesContainingCriteria[i] + ", ";
                }

                if (output.Equals(""))
                    output = filesContainingCriteria[i] + " contains ";
                else
                    output += "and " + filesContainingCriteria[i] + " contain ";
            }
            output += "\"" + txtName.Text + "\".";

            MessageBox.Show(output, "Search Results");
        }

        private bool filesExist()
        {
            return Directory.Exists(VIEWING_FILES_PATH);
        }

        private List<TextMessage> processFile(string filePath)
        {
            List<TextMessage> fileContents = new List<TextMessage>();
            int length = filePath.Length - ".csv".Length - (VIEWING_FILES_PATH.Length + 1);
            string fileDate = filePath.Substring(VIEWING_FILES_PATH.Length + 1, length);
            string[] lines = ReadFile(filePath);

            foreach (string line in lines)
            {
                string[] splitLine = line.Split(',');
                /* 0 = body
                 * 1 = number
                 * 2 = hour
                 * 3 = minute */
                DateTime messageDate = DateTime.Now;
                try
                {
                    messageDate = DateTime.ParseExact(fileDate + " " + splitLine[2] + splitLine[3], "yyyy.MM.dd HHmm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                catch (Exception exception)
                {
                    //ingore this as we dont really need the date here but this is done for completion.
                }
                
                fileContents.Add(new TextMessage("0", splitLine[0], splitLine[1], messageDate));
            }

            return fileContents;
        }
        
        private string[] ReadFile(string filePath)
        {
            StreamReader reader = null;
            List<string> lines = new List<string>();
            try
            {
                reader = new StreamReader(filePath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            catch (IOException ioException)
            {
                MessageBox.Show("Issue while reading from file.\n\n" + ioException.Message, 
                    "Big Scary Error Message");
                return null;
            }
            finally
            {
                reader?.Close();
            }

            return lines.ToArray();
        }
    }
}

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;

namespace RunnerTracker
{
    public class DataFile
    {
        // This class is to handle storing and retrieving the runner data from an XML file

        #region Variables and Declarations
        int Load_Runner_Station_Count;
        #endregion

        public DataFile()
        {
        }

        public void Load_Runner_Data(string path)
        {
            // proceed if the path is good
            if ((path != null) && (path != ""))
            {
                // test the size of the file to see if it is empty
                FileInfo FI = new FileInfo(path);
                if (FI.Length == 0)
                {
                    MessageBox.Show("The Runner Data File:\n\n" + path + "\n\nis empty!   No runner data can be retrieved","Empty File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // now look at the XML file
                XmlDocument doc = new XmlDocument();

                // now attempt to load the XML file
                try
                {
                    doc.Load(path);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Root element is missing.")
                    {   // file exists but is not an XML file
                        MessageBox.Show("The Runner Data file:\n\n" + path + "\n\nis not in the proper data format (XML)!   No runner data can be retrieved", "Improper format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                doc.Load(path);

                // clear the count
                Load_Runner_Station_Count = 0;

                // get the NodeList of all the runners
                XmlNodeList RunnersList = doc.SelectNodes("/Race/RUNNER");
                foreach (XmlNode xn in RunnersList)
                {
                    string BibNumber = string.Empty;

                    // for each runner, get the Bib #, name, gender
                    XmlNode Personal = xn.SelectSingleNode("Personal");
                    if (Personal != null)
                    {
                        Form1.RunnersList runner = new Form1.RunnersList();
                        BibNumber = Personal["Bib_Number"].InnerText;
                        runner.BibNumber = BibNumber;
// 8/11/16 - may not exist                        runner.Name = Personal["Name"].InnerText;
// 8/11/16 - may not exist                        runner.Gender = Personal["Gender"].InnerText;
                        // 8/10/16 No                    Form1.RunnerList.Add(runner);
                    }

                    // now get all the station reports for this runner
                    XmlNode Stations = xn.SelectSingleNode("Stations");
                    foreach (XmlElement station in Stations)
                    {
                        if (station.Name != "comment")
                        {
                            Form1.DB_Runner runner = new Form1.DB_Runner();
                            runner.BibNumber = BibNumber;
                            string new_name = station.Name.Replace('_', ' ');
//                            runner.Station = station.Name;
                            runner.Station = new_name;
                            foreach (XmlNode child in station)
                            {
                                switch (child.Name)
                                {
                                    case "Departed":
                                        //if (child.Name == "Departed")
                                        //{
                                        runner.TimeOut = child.InnerText;
                                        Load_Runner_Station_Count++;

                                        // this may take too much time to load in all the runners data
                                        lock (Form1.RunnerOutQue)
                                        {// lock
                                            Form1.RunnerOutQue.Enqueue(runner);
                                            Form1.New_RunnerOutQue_entry = true;
                                        }// unlock
                                         //}
                                        break;
                                    case "Arrived":
                                        //if (child.Name == "Arrived")
                                        //{
                                        runner.TimeIn = child.InnerText;
                                        Load_Runner_Station_Count++;

                                        // this may take too much time to load in all the runners data
                                        lock (Form1.RunnerInQue)
                                        {// lock
                                            Form1.RunnerInQue.Enqueue(runner);
                                            Form1.New_RunnerInQue_entry = true;
                                        }// unlock
                                         //}
                                        break;
                                }
                            }
                        }
                    }

                    // now get any status reports for this runner
                }
            }
        }

        public Form1.Race_Info Load_Info(string path)
        {
            // The Race_Info class looks like this:
                //public class Race_Info
                //{
                //    public string Name { get; set; }
                //    public string Location { get; set; }
                //    public string Sponsor { get; set; }
                //    public string Date { get; set; }
                //    public string Time { get; set; }
                //    public string Count { get; set; }
                //    public string Contact_Name { get; set; }
                //    public string Contact_Phone { get; set; }
                //    public string Packet { get; set; }
                //}
            Form1.Race_Info info = new Form1.Race_Info();
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList xnList = doc.SelectNodes("/Race");
            foreach (XmlNode xn in xnList)
            {
                XmlNode Info = xn.SelectSingleNode("INFO");
                if (Info != null)
                {
                    info.Name = Info["Name"].InnerText;
                    info.Location = Info["Location"].InnerText;
                    info.Sponsor = Info["Sponsor"].InnerText;
                    info.Date = Info["Date"].InnerText;
                    info.Time = Info["Time"].InnerText;
                    info.Count = Info["Count"].InnerText;
                    info.Contact_Name = Info["Contact_Name"].InnerText;
                    info.Contact_Phone = Info["Contact_Phone"].InnerText;
                    info.Packet = Info["Packet"].InnerText;
                }
            }
            return info;
        }

        public bool Save_Info(string path, Form1.Race_Info info)
        {
            // proceed if the path is good
            if ((path != null) && (path != ""))
            {
                // create the document instance
                XmlDocument doc = new XmlDocument();

                // now look at the XML file
                try
                {
                    doc.Load(path);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Root element is missing.")
                    {   // file exists but is empty, start by creating the root element
                        XmlWriter writer = null;
                        try
                        {
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Indent = true;
                            writer = XmlWriter.Create(path, settings);
                            writer.WriteStartElement("Race");
                            writer.WriteEndElement();
                            writer.Flush();
                            writer.Close();
                        }
                        finally
                        {
                            if (writer != null)
                                writer.Close();
                        }

                        Create_INFO(path, info);
                    }
                }

                // now can save the Info data
                XmlNode InfoNode = doc.SelectSingleNode("/Race/INFO");
                if (InfoNode != null)       // verify there is an INFO section already
                {       // INFO section exists
                    InfoNode["Name"].InnerText = info.Name;
                    InfoNode["Location"].InnerText = info.Location;
                    InfoNode["Sponsor"].InnerText = info.Sponsor;
                    InfoNode["Date"].InnerText = info.Date;
                    InfoNode["Time"].InnerText = info.Time;
                    InfoNode["Count"].InnerText = info.Count;
                    InfoNode["Contact_Name"].InnerText = info.Contact_Name;
                    InfoNode["Contact_Phone"].InnerText = info.Contact_Phone;
                    InfoNode["Packet"].InnerText = info.Packet;
                    doc.Save(path);
                }
                else
                {       // INFO section does not yet exist - create it
                    Create_INFO(path, info);
                }

                return true;
            }
            else
                return false;
        }

        void Create_INFO(string path, Form1.Race_Info info)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode rootNode = doc.SelectSingleNode("Race");

            // create the Info section
            XmlNode InfoNode = doc.CreateElement("INFO");
            rootNode.AppendChild(InfoNode);

            // now save all the data
            XmlNode userNode = doc.CreateElement("Name");
            userNode.InnerText = info.Name;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Location");
            userNode.InnerText = info.Location;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Sponsor");
            userNode.InnerText = info.Sponsor;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Date");
            userNode.InnerText = info.Date;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Time");
            userNode.InnerText = info.Time;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Count");
            userNode.InnerText = info.Count;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Contact_Name");
            userNode.InnerText = info.Contact_Name;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Contact_Phone");
            userNode.InnerText = info.Contact_Phone;
            InfoNode.AppendChild(userNode);
            userNode = doc.CreateElement("Packet");
            userNode.InnerText = info.Packet;
            InfoNode.AppendChild(userNode);
            doc.Save(path);
        }

//        public void Add_Runner_Time(string path, string BibNumber, string Station)
        public void Add_Runner_Time(string path, Form1.DB_Runner runner)
        {
            // ignore this call if we are still loading the previous XML file
            if (Load_Runner_Station_Count != 0)
            {
                Load_Runner_Station_Count--;    // just decrement it
            }
            else
            {     // not still loading - add to the XML file
                // proceed if the path is good
                if ((path != null) && (path != ""))
                {
                    // first change the station name to one usable in XML (cannot have spaces)
                    string Valid_Station_Name = runner.Station.Replace(' ', '_');

                    // now look at the XML file
                    XmlDocument doc = new XmlDocument();
                    //                    doc.Load(path);

                    // now look at the XML file
                    try
                    {
                        doc.Load(path);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Root element is missing.")
                        {   // file exists but is empty, start by creating the root element
                            XmlWriter writer = null;
                            try
                            {
                                XmlWriterSettings settings = new XmlWriterSettings();
                                settings.Indent = true;
                                writer = XmlWriter.Create(path, settings);
                                writer.WriteStartElement("Race");
                                writer.WriteEndElement();
                                writer.Flush();
                                writer.Close();
                            }
                            finally
                            {
                                if (writer != null)
                                    writer.Close();
                            }
                            //
                            //                            Create_INFO(path, info);
                            doc.Load(path);
                        }
                    }

                    // get the NodeList of all the runners
                    XmlNodeList RunnersList = doc.SelectNodes("/Race/RUNNER");
                    bool updated = false;
                    foreach (XmlNode xn in RunnersList)
                    {
                        // find the entry for this runner
                        XmlNode Bib = xn.SelectSingleNode("Personal/Bib_Number");
                        if (Bib != null)
                        {
                            if (Bib.InnerText == runner.BibNumber)
                            {
                                // find the station
                                XmlNode station = xn.SelectSingleNode("Stations/" + Valid_Station_Name);
                                if (station != null)
                                {       // already have a station entry
                                    if ((runner.TimeIn != null) && (runner.TimeIn != ""))
                                    {
                                        XmlNode arrive = doc.CreateElement("Arrived");
                                        arrive.InnerText = runner.TimeIn;
                                        station.AppendChild(arrive);
                                    }
                                    if ((runner.TimeOut != null) && (runner.TimeOut != ""))
                                    {
                                        XmlNode depart = doc.CreateElement("Departed");
                                        depart.InnerText = runner.TimeOut;
                                        station.AppendChild(depart);
                                    }
                                }
                                else
                                {       // this station has not been entered yet
                                    // check if there ia a Stations section already
                                    XmlNode StationsNode = xn.SelectSingleNode("Stations");
                                    if (StationsNode == null)
                                    {       // no Stations section - create it first
                                        StationsNode = doc.CreateElement("Stations");
                                        xn.AppendChild(StationsNode);
                                    }

                                    // now add new station
                                    XmlNode userNode = doc.CreateElement(Valid_Station_Name);
                                    if ((runner.TimeIn != null) && (runner.TimeIn != ""))
                                    {
                                        XmlNode arrive = doc.CreateElement("Arrived");
                                        arrive.InnerText = runner.TimeIn;
                                        userNode.AppendChild(arrive);
                                    }
                                    if ((runner.TimeOut != null) && (runner.TimeOut != ""))
                                    {
                                        XmlNode depart = doc.CreateElement("Departed");
                                        depart.InnerText = runner.TimeOut;
                                        userNode.AppendChild(depart);
                                    }
                                    StationsNode.AppendChild(userNode);
//                                    doc.Save(path);
                                }
                                doc.Save(path);
                                updated = true;
                            }
                        }
                    }
                    if (!updated)
                    {       // if it did not update, then it was not found in the runner list - need to add him
                        XmlNode rootNode = doc.SelectSingleNode("Race");
                        XmlNode RunnerNode = doc.CreateElement("RUNNER");
                        rootNode.AppendChild(RunnerNode);
                        XmlNode PersonalNode = doc.CreateElement("Personal");
                        RunnerNode.AppendChild(PersonalNode);
                        XmlNode userNode = doc.CreateElement("Bib_Number");
                        userNode.InnerText = runner.BibNumber;
                        PersonalNode.AppendChild(userNode);
                        XmlNode StationsNode = doc.CreateElement("Stations");
                        RunnerNode.AppendChild(StationsNode);
                        userNode = doc.CreateElement(Valid_Station_Name);
                        if ((runner.TimeIn != null) && (runner.TimeIn != ""))
                        {
                            XmlNode arrive = doc.CreateElement("Arrived");
                            arrive.InnerText = runner.TimeIn;
                            userNode.AppendChild(arrive);
                        }
                        if ((runner.TimeOut != null) && (runner.TimeOut != ""))
                        {
                            XmlNode depart = doc.CreateElement("Departed");
                            depart.InnerText = runner.TimeOut;
                            userNode.AppendChild(depart);
                        }
                        StationsNode.AppendChild(userNode);
                        doc.Save(path);
                    }
                }
            }
        }

        public void Add_Runner_DNF(string path)
        {
        }

        public void Add_Runner_Watch(string path)
        {
        }
    }
}

/*     An example XML file is shown here:

<?xml version="1.0" encoding="utf-8"?>
<Race>
  <!-- 
    Runner Tracker Runners Data 
    
    Version 1.0
       
    The INFO section of this file should match the Race Info file associated with the race.
    Three sections of data are maintained for each Runner:  Personal info, Station times, Status

   -->

  <INFO>
    <Name>A Track Race</Name>
    <Location>The Track</Location>
    <Sponsor>Runners Den</Sponsor>
    <Date>Saturday</Date>
    <Time>sunrise</Time>
    <Count>many</Count>
    <Contact_Name>Mr. Contact</Contact_Name>
    <Contact_Phone>800-234-5678</Contact_Phone>
    <Packet>145.71</Packet>
  </INFO>

  <RUNNER>
    <Personal><comment>This section is not optional: Bib Number, Name and Gender must be entered</comment>
      <Bib_Number>8888</Bib_Number>
      <Name>Joe Blow</Name>
      <Gender>male</Gender>
    </Personal>
    <Stations><comment>This section is optional. There will be a station entry for each station that the runner enters or leaves</comment>
      <Start>
        <Departed>05:02</Departed>
      </Start>
      <Finish>
        <Arrived>10:41</Arrived>
      </Finish>
    </Stations>
    <Status><comment>this section is optional</comment>
      <DNF><comment>this section is optional</comment>
      </DNF>
      <Watch><comment>this section is optional</comment>
      </Watch>
    </Status>
  </RUNNER>

  <RUNNER>
    <Personal><comment>This section is not optional: Bib Number, Name and Gender must be entered</comment>
      <Bib_Number>1234</Bib_Number>
      <Name>Mary</Name>
      <Gender>female</Gender>
    </Personal>
    <Stations><comment>This section is optional. There will be a station entry for each station that the runner enters or leaves</comment>
      <Start>
        <Departed>06:02</Departed>
      </Start>
      <Finish>
        <Arrived>11:41</Arrived>
      </Finish>
    </Stations>
    <Status><comment>this section is optional</comment>
      <DNF><comment>this section is optional</comment>
      </DNF>
      <Watch><comment>this section is optional</comment>
      </Watch>
    </Status>
  </RUNNER>

</Race>
*/
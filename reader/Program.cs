using System;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Mail;

namespace reader
{
    class RSSReader
    {
        private String link;
        private List<String> messages;
        private MailMessage digestMessage;
        private SmtpClient ourClient;
        private String toAddress;
        private String fromAddress;
        private int serverPort;
        private String serverHost;
        private String senderUserName;
        private String senderPassword;

        public RSSReader(String feedLink, String toAddress, String fromAddress, String senderUserName, String senderPassword)
        {
            this.link = feedLink;
            this.messages = new List<String>(); //Will contain all of our individual messages for the email
            this.digestMessage = new MailMessage(); //Object we'll pass our messages to
            this.ourClient = new SmtpClient(); //Sending mail client
            this.toAddress = toAddress; //Who we want the mail to go to 
            this.fromAddress = fromAddress; //Who we want the mail to be from
            this.serverPort = 587; //Default mail port
            this.serverHost = "INSERT CENTRE SERVER NAME HERE"; //Centre's SMTP server name
            this.senderUserName = senderUserName; //The username for the credentials. Does not have to be the same as the fromAddress
            this.senderPassword = senderPassword; //The password associated with the above username
        }

        public void readFeed()
        {
            String m_strFilePath = this.link;
            string xmlStr;
            using (var wc = new WebClient())
            {
                wc.Headers.Add("User-Agent: Other");
                xmlStr = wc.DownloadString(m_strFilePath);
            }

            XmlReader reader = XmlReader.Create(new StringReader(xmlStr));
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            foreach (SyndicationItem item in feed.Items)
            {
                String subject = item.Title.Text;
                String description = item.Summary.Text;
                description = Regex.Replace(description, "<p>", String.Empty);
                description = Regex.Replace(description, "</p>", String.Empty); //Removing paragraph tags from output
                String datePublished = item.PublishDate.ToString();
                Console.WriteLine("New item!");
                Console.WriteLine("Title reads:");
                Console.WriteLine(subject);
                Console.WriteLine("Description reads:");
                Console.WriteLine(description);
                Console.WriteLine("Date made reads:");
                Console.WriteLine(datePublished);
                String fullMessage = subject + "\n"+ datePublished + "\n" + description;
                this.messages.Add(fullMessage);
            }

        }

        public void sendMail(String subject)
        {
            String fullMessage = "Centre Email Digest\n";
            foreach (String item in this.messages)
            {
                fullMessage = fullMessage + item;
            }
            Console.WriteLine("Sending function goes here");
            this.digestMessage.From = new MailAddress(this.fromAddress);
            this.digestMessage.To.Add(new MailAddress(this.toAddress));
            this.digestMessage.Subject = subject;
            this.digestMessage.Body = fullMessage;
            this.ourClient.Port = this.serverPort;
            this.ourClient.Host = this.serverHost; 
            this.ourClient.EnableSsl = true;
            this.ourClient.UseDefaultCredentials = false;
            this.ourClient.Credentials = new NetworkCredential(this.senderUserName, this.senderPassword);
            this.ourClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            this.ourClient.Send(this.digestMessage);
            
        }

        
    }


    class ReadandSend
    {
        public static void Main(string[] args)
        {
            RSSReader myFeedReader = new RSSReader("placeholderURL","placehold","placehold","user","pass");
            myFeedReader.readFeed();
            myFeedReader.sendMail("Centre College Weekly Digest");

        }
    }
}

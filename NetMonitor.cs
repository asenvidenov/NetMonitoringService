using System.Net;
using System.Net.NetworkInformation;
using System.Net.Mail;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;


namespace NetMonitor
{
    public class PingAPs
    {
        private static string targetIP;
        private static string serverName ;
        private static int serverPort;
        private static int ssl;
        private static MailAddress sender;
        private static List<MailAddress> recipients = new List<MailAddress>();
        private static string password;
        public static int interval;
        public static void PingAP()
        {

            using (var sqliteConnection = new SqliteConnection("Data Source = ipsdb.db"))
            {
                sqliteConnection.Open();
                var command = sqliteConnection.CreateCommand();
                command.CommandText = @"select servername, serverport, ssl, sender, password";
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    serverName = reader.GetString(0);
                    serverPort = reader.GetInt32(1);
                    ssl = reader.GetInt32(2);
                    sender = new MailAddress(reader.GetString(3));
                    password = reader.GetString(4);
                }

                command.CommandText = @"select recipient from recipients";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recipients.Clear();
                        recipients.Add(new MailAddress(reader.GetString(0)));
                    }
                }

                command.CommandText = @"select milliseconds from interval";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        interval = reader.GetInt32(0);
                    }
                    else
                    { interval = 600000; }
                }

                command.CommandText = @"select ip from ips";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        targetIP = reader.GetString(0);
                        Ping pinger = new Ping();
                        if (pinger.Send(targetIP).Status != IPStatus.Success)
                        {
                            SmtpClient smtpClient = new SmtpClient
                            {
                                Host = serverName,
                                Port = serverPort,
                                Credentials = new NetworkCredential(sender.ToString(), password),
                                EnableSsl = ssl != 0
                            };
                            foreach (MailAddress recipient in recipients)
                            { 
                                smtpClient.SendMailAsync(recipient.ToString(), sender.ToString(), "Ping Report", targetIP + " Not Reachable"); 
                            }
                        }

                    }
                }
                sqliteConnection.Close();
            }

        }
    }
}
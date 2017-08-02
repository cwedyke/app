using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ZparseEmail.Helpers;
using Zillow.Models;
using Zillow.Services;
using Zillow.Services.Schema;
using LiteDB;


namespace ZparseEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create Gmail API service.
            var service = GetService();

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

            var msgs = ListMessages(service, "me", "from:no-reply@mail.zillow.com");

            Console.WriteLine(string.Format("Processing {0} emails.", msgs.Count));
            foreach (var m in msgs)
            {
                var emailBodyString = DecodeEmailBody(GetMessage(service, "me", m.Id));

                try
                {
                    var entity = ParseEmailToZillowEntity(emailBodyString);

                    if (entity.zpid > 0)
                        // Upload home details to local database for easy querying from webpage.
                        UpsertZillowEntity(entity);
                    else
                        // Unable to find property so log email for reference (debug, error, info, ect.)
                        LogEmail(emailBodyString);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                //******* Only send message for especially good deals? Send TXT MSG??
                //SendMessage(service, "me", CreateEmail("test results"));
                Console.Write("|");

                // Finally clean up email
                DeleteMessage(service, "me", m.Id);
            }
            Console.WriteLine();
            Console.WriteLine("Complete. (Press any key to close)");
            Console.ReadKey();
        }

        #region GoogleAPI
        public static GmailService GetService()
        {
            UserCredential credential;
            string[] Scopes = { GmailService.Scope.MailGoogleCom };
            string ApplicationName = "ZparseEmail";

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/zparse_email.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                //Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        public static Google.Apis.Gmail.v1.Data.Message GetMessage(GmailService service, String userId, String messageId)
        {
            UsersResource.MessagesResource.GetRequest getReq = new UsersResource.MessagesResource.GetRequest(service, "me", messageId);
            getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;

            return getReq.Execute();
        }

        public static string DecodeEmailBody(Google.Apis.Gmail.v1.Data.Message m)
        {
            string codedBody = m.Raw.Replace("-", "+");
            codedBody = codedBody.Replace("_", "/");
            return Encoding.UTF8.GetString(Convert.FromBase64String(codedBody));
        }

        public static List<Google.Apis.Gmail.v1.Data.Message> ListMessages(GmailService service, String userId, String query)
        {
            List<Google.Apis.Gmail.v1.Data.Message> result = new List<Google.Apis.Gmail.v1.Data.Message>();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.Q = query;

            do
            {
                try
                {
                    ListMessagesResponse response = request.Execute();
                    request.PageToken = response.NextPageToken;
                    result.AddRange(response.Messages);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));


            return result;
        }

        public static void SendMessage(GmailService service, String userId, Google.Apis.Gmail.v1.Data.Message email)
        {
            //return service.Users.Messages.Send(email, userId).Execute();
        }

        public static void DeleteMessage(GmailService service, String userId, String messageId)
        {
            service.Users.Messages.Delete(userId, messageId).Execute();
        }

        public static Google.Apis.Gmail.v1.Data.Message CreateEmail(string emailBody)
        {
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.From = new System.Net.Mail.MailAddress("zparse4@gmail.com");
            mailMessage.To.Add("cwedyke@yahoo.com");
            mailMessage.ReplyToList.Add("zparse4@gmail.com");
            mailMessage.Subject = "Parsed Homes";
            mailMessage.Body = emailBody;
            mailMessage.IsBodyHtml = false;

            //foreach (System.Net.Mail.Attachment attachment in email.Attachments)
            //{
            //    mailMessage.Attachments.Add(attachment);
            //}

            var mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(mailMessage);

            var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Encode(mimeMessage.ToString())
            };

            return gmailMessage;
        }

        public static string Encode(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
        #endregion

        public static ZillowEntityModel ParseEmailToZillowEntity(string value)
        {
            var entity = new ZillowEntityModel();
            string askingPrice = string.Empty;
            DateTime? priceDropOn = null;

            // remove all extra whitespace
            value = Regex.Replace(value, @"\s\s+", " ");


            int end = value.ToLower().IndexOf(". your '");
            int start = value.ToLower().IndexOf("new listing:");
            if (start == -1)
            {
                start = value.ToLower().IndexOf("price drop:");
                priceDropOn = DateTime.Now.Date;
            }
            else
            {
                // if this is a 'new listing' get askingprice.
                int startIndex = value.IndexOf("$");
                int endIndex = value.Substring(startIndex).IndexOf(".");

                askingPrice = value.Substring(startIndex + 1, endIndex - 1).Trim().Replace(",", "");
            }

            if (start != -1 && end != -1)
            {
                int length = (end - 12) - start;

                string address = value.Substring(start + 12, length).Trim();

                //now that i have address I need to seperate street from city,st
                int endOfStreetComma = address.IndexOf(',');
                int endOfStreetCondoNum = address.IndexOf('#');
                string street = string.Empty;
                string cityStZip = address.Substring(endOfStreetComma + 1).Trim();

                if (endOfStreetCondoNum != -1)
                    street = address.Substring(0, endOfStreetCondoNum).Trim();
                //use condo num to set 'street' instead of endOfStreetComma
                else
                    street = address.Substring(0, endOfStreetComma).Trim();

                var key = ZillowClientHelper.GetAvailableClientKey();
                ZillowClient client = new ZillowClient(key);
                ZillowClientHelper.IncrementKeyCount(key);

                searchresults result = new searchresults();
                try
                {
                    result = client.GetSearchResults(street, cityStZip);
                }
                catch { }

                if (result.response != null)
                {
                    foreach (SimpleProperty prop in result.response.results)
                    {
                        // annoying GetUpdatedPropertyDetails never works - 501 error, protected data
                        //key = ZillowClientHelper.GetAvailableClientKey();
                        //ZillowClient client1 = new ZillowClient(key);
                        //ZillowClientHelper.IncrementKeyCount(key);
                        //var propDets = client1.GetUpdatedPropertyDetails(prop.zpid);

                        int intAskingPrice = 0;
                        if (askingPrice != string.Empty)
                            intAskingPrice = Convert.ToInt32(askingPrice);

                        int intRentZest = 0;
                        if (prop.rentzestimate != null)
                            intRentZest = Convert.ToInt32(prop.rentzestimate.amount.Value);

                        int intZest = 0;
                        if (prop.zestimate != null)
                            intZest = Convert.ToInt32(prop.zestimate.amount.Value);

                        entity = new ZillowEntityModel()
                        {
                            zpid = prop.zpid,
                            address = street + " " + cityStZip, // keep for sake of seeing what I'm parsing above.
                            street = prop.address.street,
                            city = prop.address.city,
                            state = prop.address.state,
                            zipcode = prop.address.zipcode,
                            urlHome = prop.links.homedetails,
                            rentZestimate = intRentZest,
                            zestimate = intZest,
                            askingPrice = intAskingPrice,
                            isFavorite = false,
                            priceDropOn = priceDropOn,
                            isDeleted = false
                        };
                    }
                }
                else
                    entity = new ZillowEntityModel()
                    {
                        address = street + " " + cityStZip, // keep for sake of seeing what I'm parsing above.
                        isDeleted = false
                    };
            }

            return entity;
        }

        public static void UpsertZillowEntity(ZillowEntityModel z)
        {
            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                // Insert new entity (Id will be auto-incremented)
                var exists = table.FindOne(x => x.zpid == z.zpid);
                if (exists == null)
                {
                    z.createdOn = DateTime.Now;
                    table.Insert(z);
                }
                else
                {
                    z._id = exists._id;
                    z.modifiedOn = DateTime.Now;
                    table.Update(z);
                }
            }

        }

        public static void LogEmail(string emailMsg)
        {
            using (var db = Zillow.Database.ZparseDBHelper.GetZparseEmailMsgsDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseEmailMsgsTable(db);

                table.Insert(new ZillowEmailMsgModel() { EmailMsg = emailMsg });
            }
        }
    }
}

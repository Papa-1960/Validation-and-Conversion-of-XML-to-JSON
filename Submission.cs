using System;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConsoleApp1
{
    public class Program
    {
        // Corrected raw GitHub URLs for hosted files
        public static string xmlURL = "https://raw.githubusercontent.com/AlshehhiSultan/CSE-445_ASS4/main/Hotels.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/AlshehhiSultan/CSE-445_ASS4/main/HotelsErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/AlshehhiSultan/CSE-445_ASS4/main/Hotels.xsd";

        public static void Main(string[] args)
        {
            Console.WriteLine("Valid XML Verification:");
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            Console.WriteLine("\nError XML Verification:");
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            Console.WriteLine("\nConverted JSON from valid XML:");
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);

            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            List<string> errorMessages = new List<string>();
            try
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", xsdUrl);

                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = schemas,
                    ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
                    DtdProcessing = DtdProcessing.Prohibit
                };

                settings.ValidationEventHandler += (sender, e) =>
                {
                    string message = $"Line {e.Exception?.LineNumber}, Position {e.Exception?.LinePosition}: {e.Message}";
                    errorMessages.Add(message);
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }

                return errorMessages.Count == 0 ? "No Error" : string.Join("\n", errorMessages);
            }
            catch (XmlSchemaException ex)
            {
                return $"Schema error: {ex.Message} at Line {ex.LineNumber}, Position {ex.LinePosition}";
            }
            catch (XmlException ex)
            {
                return $"XML error: {ex.Message} at Line {ex.LineNumber}, Position {ex.LinePosition}";
            }
            catch (Exception ex)
            {
                return $"Validation exception: {ex.Message} (URL: {xmlUrl})";
            }
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit
                };

                XmlDocument doc = new XmlDocument();
                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    doc.Load(reader);
                }

                XmlNodeList hotelNodes = doc.SelectNodes("/Hotels/Hotel");
                if (hotelNodes == null || hotelNodes.Count == 0)
                {
                    return "Error: No Hotel elements found in the XML.";
                }

                List<Dictionary<string, object>> hotels = new List<Dictionary<string, object>>();

                foreach (XmlNode hotelNode in hotelNodes)
                {
                    var hotel = new Dictionary<string, object>();

                    XmlNode nameNode = hotelNode.SelectSingleNode("Name");
                    hotel["Name"] = nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText) ? nameNode.InnerText : "Unknown";

                    var phones = new List<string>();
                    XmlNodeList phoneNodes = hotelNode.SelectNodes("Phone");
                    if (phoneNodes != null)
                    {
                        foreach (XmlNode phone in phoneNodes)
                        {
                            if (phone != null && !string.IsNullOrEmpty(phone.InnerText))
                            {
                                phones.Add(phone.InnerText);
                            }
                        }
                    }
                    hotel["Phone"] = phones.Count > 0 ? phones : new List<string> { "N/A" };

                    XmlNode addressNode = hotelNode.SelectSingleNode("Address");
                    var address = new Dictionary<string, string>();
                    if (addressNode != null)
                    {
                        XmlNode numberNode = addressNode.SelectSingleNode("Number");
                        address["Number"] = numberNode != null && !string.IsNullOrEmpty(numberNode.InnerText) ? numberNode.InnerText : "";

                        XmlNode streetNode = addressNode.SelectSingleNode("Street");
                        address["Street"] = streetNode != null && !string.IsNullOrEmpty(streetNode.InnerText) ? streetNode.InnerText : "";

                        XmlNode cityNode = addressNode.SelectSingleNode("City");
                        address["City"] = cityNode != null && !string.IsNullOrEmpty(cityNode.InnerText) ? cityNode.InnerText : "";

                        XmlNode stateNode = addressNode.SelectSingleNode("State");
                        address["State"] = stateNode != null && !string.IsNullOrEmpty(stateNode.InnerText) ? stateNode.InnerText : "";

                        XmlNode zipNode = addressNode.SelectSingleNode("Zip");
                        address["Zip"] = zipNode != null && !string.IsNullOrEmpty(zipNode.InnerText) ? zipNode.InnerText : "";

                        if (addressNode.Attributes != null)
                        {
                            XmlAttribute nearestAirport = addressNode.Attributes["NearestAirport"];
                            if (nearestAirport != null && !string.IsNullOrEmpty(nearestAirport.Value))
                            {
                                address["_NearestAirport"] = nearestAirport.Value;
                            }
                        }
                    }
                    hotel["Address"] = address;

                    if (hotelNode.Attributes != null)
                    {
                        XmlAttribute rating = hotelNode.Attributes["Rating"];
                        if (rating != null && !string.IsNullOrEmpty(rating.Value))
                        {
                            hotel["_Rating"] = rating.Value;
                        }
                    }

                    hotels.Add(hotel);
                }

                var finalResult = new { Hotels = new { Hotel = hotels } };
                string jsonText = JsonConvert.SerializeObject(finalResult, Newtonsoft.Json.Formatting.Indented);
                return jsonText;
            }
            catch (XmlException ex)
            {
                return $"XML parsing error: {ex.Message} at Line {ex.LineNumber}, Position {ex.LinePosition}";
            }
            catch (Exception ex)
            {
                return $"JSON conversion error: {ex.Message} (URL: {xmlUrl})";
            }
        }
    }
}
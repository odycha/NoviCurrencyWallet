﻿using System.Xml.Serialization;

namespace NoviCurrencyWallet.Gateway.Models
{
	[XmlRoot(ElementName = "Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
	public class EcbEnvelope
	{
		[XmlElement(ElementName = "subject", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
		public string Subject { get; set; }

		[XmlElement(ElementName = "Sender", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
		public EcbSender Sender { get; set; }

		[XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
		public EcbCubeContainer CubeContainer { get; set; }
	}

	public class EcbSender
	{
		[XmlElement(ElementName = "name", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
		public string Name { get; set; }
	}

	public class EcbCubeContainer
	{
		[XmlElement("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
		public EcbCube DateCube { get; set; }
	}

	public class EcbCube
	{
		[XmlAttribute("time")]
		public string Date { get; set; }

		[XmlElement("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
		public List<EcbCurrencyRateDto> Rates { get; set; }
	}

	public class EcbCurrencyRateDto
	{
		[XmlAttribute("currency")]
		public string Currency { get; set; }

		[XmlAttribute("rate")]
		public decimal Rate { get; set; }
	}
}



//Ensures inner <Cube> elements are correctly mapped by using Namespace = "" to ignore any nested namespaces.

//Why do we need so many classes?
//Your EcbCurrencyRateDto is not sufficient on its own because the XML structure is deeply nested within multiple <Cube> 
//elements. The ECB response is not a flat format; it's hierarchical, meaning you need additional DTOs to match the nesting.


//What is a <Cube> in the ECB XML?
//The<Cube> elements act as containers for exchange rate data in a 
//hierarchical structure.They represent different levels of grouping:There’s no official reason from the ECB on why they chose the name "Cube",


//Why Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref" in XmlRoot?
//The reason for explicitly specifying the namespace in your C# DTO (EcbEnvelope) is because the XML 
//defines a default namespace for all its elements. If we do not handle namespaces correctly, the 
//deserialization process will fail or return null.


//❌ What Happens If We Omit the Namespace?
//🚨 Problem:
//XmlSerializer will look for <Envelope> without any namespace, but the ECB XML has a namespace.
//Since the XML root does have a namespace, it won't match, and deserialization will return null or fail.


//xmlns stands for "XML Namespace", and it is used to define a unique identifier for elements in an XML document.


//Why use string Date?
//The value of the time attribute is "2025-03-10", which is:
//A valid date format (yyyy-MM-dd).
//Not in a full DateTime format (e.g., missing time component).
//XmlSerializer might not always recognize it unless explicitly formatted.
//By using string, we avoid potential deserialization errors and have full control over how to parse the date.


using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GasBestPrice.Model
{
    [DataContract]
    public class Trigger : Document
    {
        public const string MIME_TYPE = "application/vnd.my.trigger+json";

        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public Trigger()
            : base(MediaType)
        {
            Payload = new JsonDocument();
        }

        [DataMember]
        public string StateId { get; set; }

        [DataMember]
        public JsonDocument Payload { get; set; }
    }
}

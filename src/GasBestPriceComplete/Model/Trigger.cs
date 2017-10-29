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
        }

        [DataMember]
        public string StateId { get; set; }

        [DataMember]
        public string Payload { get; set; }
    }
}

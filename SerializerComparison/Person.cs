using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ZeroFormatter;

namespace SerializerComparison
{
    public interface IPerson
    {

    }

    public class DocumentWithoutAttributes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class PersonWithoutAttributes : IPerson
    {
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public string Name { get; set; }
        public List<DocumentWithoutAttributes> Documents { get; set; }
    }

    [DataContract]
    [Serializable]
    public class Document
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember(Name = "ExpirationDate")]
        private string FormattedExpirationDate { get; set; }

        [IgnoreDataMember]
        public DateTime ExpirationDate
        {
            get
            {
                long.TryParse(FormattedExpirationDate.Replace("/Date(", string.Empty).Replace(")/", string.Empty), out long millisec);
                return UnixEpoch.AddMilliseconds(millisec);
            }
            set
            {
                FormattedExpirationDate = $"/Date({(value - UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond})/";
            }
        }
    }

    [DataContract]
    [Serializable]
    public class Person : IPerson
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        [DataMember]
        public int Age { get; set; }

        [DataMember(Name = "Birthday")]
        private string FormattedBirthday { get; set; }

        [IgnoreDataMember]
        public DateTime Birthday
        {
            get
            {
                long.TryParse(FormattedBirthday.Replace("/Date(", string.Empty).Replace(")/", string.Empty), out long millisec);
                return UnixEpoch.AddMilliseconds(millisec);
            }
            set
            {
                FormattedBirthday = $"/Date({(value - UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond})/";
            }
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Document> Documents { get; set; }
    }

    [ProtoContract]
    public class DocumentProtobuf
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Content { get; set; }

        [ProtoMember(4)]
        public DateTime ExpirationDate { get; set; }
    }

    [ProtoContract]
    public class PersonProtobuf : IPerson
    {
        [ProtoMember(1)]
        public int Age { get; set; }

        [ProtoMember(2)]
        public DateTime Birthday { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; }

        [ProtoMember(4)]
        public List<DocumentProtobuf> Documents { get; set; }
    }

    [ZeroFormattable]
    public class DocumentZeroFormatter
    {
        [Index(0)]
        public virtual int Id { get; set; }

        [Index(1)]
        public virtual string Name { get; set; }

        [Index(2)]
        public virtual string Content { get; set; }

        [Index(3)]
        public virtual DateTime ExpirationDate { get; set; }
    }

    [ZeroFormattable]
    public class PersonZeroFormatter : IPerson
    {
        [Index(0)]
        public virtual int Age { get; set; }

        [Index(1)]
        public virtual DateTime Birthday { get; set; }

        [Index(2)]
        public virtual string Name { get; set; }

        [Index(3)]
        public virtual List<DocumentZeroFormatter> Documents { get; set; }
    }
}

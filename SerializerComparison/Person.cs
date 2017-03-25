using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

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
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember(Name = "ExpirationDate")]
        private string FormattedExpirationDate { get; set; }

        // see http://stackoverflow.com/questions/10945825/datacontractjsonserializer-parsing-iso-8601-date
        [IgnoreDataMember]
        public DateTime ExpirationDate
        {
            get
            {
                return DateTime.ParseExact(FormattedExpirationDate, "o", CultureInfo.InvariantCulture);
            }
            set
            {
                FormattedExpirationDate = value.ToString("o");
            }
        }
    }

    [DataContract]
    [Serializable]
    public class Person : IPerson
    {
        [DataMember]
        public int Age { get; set; }

        [DataMember( Name = "Birthday")]
        private string FormattedBirthday { get; set; }

        [IgnoreDataMember]
        public DateTime Birthday {
            get
            {
                return DateTime.ParseExact(FormattedBirthday, "o", CultureInfo.InvariantCulture);
            }
            set
            {
                FormattedBirthday = value.ToString("o");
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
}

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace SerializerComparison
{
    public class CorrectnessTests
    {
        private static Person CreatePerson()
        {
            return new Person
            {
                Age = 1,
                Name = "j",
                Birthday = new DateTime(2000, 1, 1),
                Documents = new List<Document>
                {
                    new Document
                    {
                        Id = 2,
                        Name = "n",
                        Content = ",",
                        ExpirationDate = new DateTime(2001, 1, 1)
                    }
                }
            };
        }

        private static PersonWithoutAttributes CreatePersonWithoutAttributes()
        {
            return new PersonWithoutAttributes
            {
                Age = 1,
                Name = "j",
                Birthday = new DateTime(2000, 1, 1),
                Documents = new List<DocumentWithoutAttributes>
                {
                    new DocumentWithoutAttributes
                    {
                        Id = 2,
                        Name = "n",
                        Content = ",",
                        ExpirationDate = new DateTime(2001, 1, 1)
                    }
                }
            };
        }

        private static PersonProtobuf CreatePersonProtobuf()
        {
            return new PersonProtobuf
            {
                Age = 1,
                Name = "j",
                Birthday = new DateTime(2000, 1, 1),
                Documents = new List<DocumentProtobuf>
                {
                    new DocumentProtobuf
                    {
                        Id = 2,
                        Name = "n",
                        Content = ",",
                        ExpirationDate = new DateTime(2001, 1, 1)
                    }
                }
            };
        }

        [Fact]
        public void JilSerializesCorrectly()
        {
            Jil.JSON.SetDefaultOptions(new Jil.Options(unspecifiedDateTimeKindBehavior: Jil.UnspecifiedDateTimeKindBehavior.IsUTC));
            var json = Jil.JSON.Serialize(CreatePersonWithoutAttributes());

            json.Should().Contain("Age\":1");
            json.Should().Contain("Name\":\"j");
            json.Should().Contain("Id\":2");
            json.Should().Contain("Name\":\"n");
            json.Should().Contain("Content\":\",");

            var person = Jil.JSON.Deserialize<PersonWithoutAttributes>(json);
            person.Age.Should().Be(1);
            person.Name.Should().Be("j");
            person.Birthday.Should().Be(new DateTime(2000, 1, 1));
            person.Documents.Should().NotBeNull();
            person.Documents.Should().NotBeEmpty();
            person.Documents.First().Id.Should().Be(2);
            person.Documents.First().Name.Should().Be("n");
            person.Documents.First().Content.Should().Be(",");
            person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
        }

        [Fact]
        public void NetJSONSerializesCorrectly()
        {
            var json = NetJSON.NetJSON.Serialize(CreatePersonWithoutAttributes());

            json.Should().Contain("Age\":1");
            json.Should().Contain("Name\":\"j");
            json.Should().Contain("Id\":2");
            json.Should().Contain("Name\":\"n");
            json.Should().Contain("Content\":\",");

            var person = NetJSON.NetJSON.Deserialize<PersonWithoutAttributes>(json);
            person.Age.Should().Be(1);
            person.Name.Should().Be("j");
            person.Birthday.Should().Be(new DateTime(2000, 1, 1));
            person.Documents.Should().NotBeNull();
            person.Documents.Should().NotBeEmpty();
            person.Documents.First().Id.Should().Be(2);
            person.Documents.First().Name.Should().Be("n");
            person.Documents.First().Content.Should().Be(",");
            person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
        }

        [Fact]
        public void DataContractSerializerSerializesCorrectly()
        {
            DataContractSerializer s = new DataContractSerializer(typeof(PersonWithoutAttributes));
            using (var stream = new MemoryStream())
            {
                s.WriteObject(stream, CreatePersonWithoutAttributes());

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    var xml = reader.ReadToEnd();

                    xml.Should().Contain("<Age>1</Age>");
                    xml.Should().Contain("<Name>j</Name>");
                    xml.Should().Contain("<Id>2</Id>");
                    xml.Should().Contain("<Name>n</Name>");
                    xml.Should().Contain("<Content>,</Content>");
                }

                stream.Seek(0, SeekOrigin.Begin);

                var o = s.ReadObject(stream);

                o.GetType().Should().Be(typeof(PersonWithoutAttributes));

                var person = (PersonWithoutAttributes)o;

                person.Age.Should().Be(1);
                person.Name.Should().Be("j");
                person.Birthday.Should().Be(new DateTime(2000, 1, 1));
                person.Documents.Should().NotBeNull();
                person.Documents.Should().NotBeEmpty();
                person.Documents.First().Id.Should().Be(2);
                person.Documents.First().Name.Should().Be("n");
                person.Documents.First().Content.Should().Be(",");
                person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
            }
        }

        [Fact]
        public void DataContractJsonSerializerSerializesCorrectly()
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(PersonWithoutAttributes));
            using (var stream = new MemoryStream())
            {
                s.WriteObject(stream, CreatePersonWithoutAttributes());

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    var json = reader.ReadToEnd();

                    json.Should().Contain("Age\":1");
                    json.Should().Contain("Name\":\"j");
                    json.Should().Contain("Id\":2");
                    json.Should().Contain("Name\":\"n");
                    json.Should().Contain("Content\":\",");
                }

                stream.Seek(0, SeekOrigin.Begin);

                var o = s.ReadObject(stream);

                o.GetType().Should().Be(typeof(PersonWithoutAttributes));

                var person = (PersonWithoutAttributes)o;

                person.Age.Should().Be(1);
                person.Name.Should().Be("j");
                person.Birthday.Should().Be(new DateTime(2000, 1, 1));
                person.Documents.Should().NotBeNull();
                person.Documents.Should().NotBeEmpty();
                person.Documents.First().Id.Should().Be(2);
                person.Documents.First().Name.Should().Be("n");
                person.Documents.First().Content.Should().Be(",");
                person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
            }
        }

        [Fact]
        public void BinaryFormatterSerializesCorrectly()
        {
            BinaryFormatter s = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                s.Serialize(stream, CreatePerson());

                stream.Seek(0, SeekOrigin.Begin);

                var o = s.Deserialize(stream);

                o.GetType().Should().Be(typeof(Person));

                var person = (Person)o;

                person.Age.Should().Be(1);
                person.Name.Should().Be("j");
                person.Birthday.Should().Be(new DateTime(2000, 1, 1));
                person.Documents.Should().NotBeNull();
                person.Documents.Should().NotBeEmpty();
                person.Documents.First().Id.Should().Be(2);
                person.Documents.First().Name.Should().Be("n");
                person.Documents.First().Content.Should().Be(",");
                person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
            }
        }

        [Fact]
        public void ProtobufSerializesCorrectly()
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, CreatePersonProtobuf());

                stream.Seek(0, SeekOrigin.Begin);

                var person = ProtoBuf.Serializer.Deserialize<PersonProtobuf>(stream);

                person.GetType().Should().Be(typeof(PersonProtobuf));

                person.Age.Should().Be(1);
                person.Name.Should().Be("j");
                person.Birthday.Should().Be(new DateTime(2000, 1, 1));
                person.Documents.Should().NotBeNull();
                person.Documents.Should().NotBeEmpty();
                person.Documents.First().Id.Should().Be(2);
                person.Documents.First().Name.Should().Be("n");
                person.Documents.First().Content.Should().Be(",");
                person.Documents.First().ExpirationDate.Should().Be(new DateTime(2001, 1, 1));
            }
        }

        [Fact]
        public void PersonBirthdayShouldStoreCorrectly()
        {
            Person p = new Person()
            {
                Birthday = new DateTime(2002, 1, 2)
            };
            p.Birthday.Should().Be(new DateTime(2002, 1, 2));
        }

        [Fact]
        public void DocumentExpirationDateShouldStoreCorrectly()
        {
            Document p = new Document()
            {
                ExpirationDate = new DateTime(2002, 1, 2)
            };
            p.ExpirationDate.Should().Be(new DateTime(2002, 1, 2));
        }

        [Fact]
        public void DatetimeParseJsonDataCorrectly()
        {
            string jsonDate = "/Date(1490452166591)/";
            var epochDate = new DateTime(1970, 1, 1);
            long.TryParse(jsonDate.Replace("/Date(", string.Empty).Replace(")/", string.Empty), out long millisec).Should().Be(true);
            millisec.Should().Be(1490452166591);
            var date = epochDate.AddMilliseconds(millisec);

            DateTime checkDate = Newtonsoft.Json.JsonConvert.DeserializeObject<DateTime>($"\"{jsonDate}\"");

            date.Should().Be(checkDate);
        }
    }
}

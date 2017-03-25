using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SerializerComparison
{
    class Program
    {
        static void Main(string[] args)
        {
            DataContractSerializer xmldcs = new DataContractSerializer(typeof(PersonWithoutAttributes));
            var settings = new DataContractJsonSerializerSettings();
            settings.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ssK");
            DataContractJsonSerializer jsondcs = new DataContractJsonSerializer(typeof(Person), settings);
            BinaryFormatter bf = new BinaryFormatter();
            Newtonsoft.Json.JsonSerializer NewtonJson = new Newtonsoft.Json.JsonSerializer();

            Jil.JSON.SetDefaultOptions(new Jil.Options(unspecifiedDateTimeKindBehavior: Jil.UnspecifiedDateTimeKindBehavior.IsUTC));

            TestRunner.RunTestSerialize((p) =>
            {
                Jil.JSON.Serialize(p);
            }, TestRunner.CreatePersonWithoutAttributes(), "Jil");

            /*TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    Jil.JSON.Serialize(p, writer);
                }
            }, TestRunner.CreatePersonWithoutAttributes(), "Jil Stream");

            TestRunner.RunTestSerialize((p) =>
            {
                NetJSON.NetJSON.Serialize(p);
            }, TestRunner.CreatePersonWithoutAttributes(), "NetJSON");

            TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    NetJSON.NetJSON.Serialize(p, writer);
                }
            }, TestRunner.CreatePersonWithoutAttributes(), "NetJSON Stream");

            TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    NewtonJson.Serialize(writer, p);
                }
            }, TestRunner.CreatePersonWithoutAttributes(), "Newtonsoft.Json Stream");

            TestRunner.RunTestSerialize((PersonProtobuf p, Stream stream) =>
            {
                ProtoBuf.Serializer.Serialize(stream, p);
            }, TestRunner.CreatePersonProtobuf(), "ProtoBuf stream");

            TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                xmldcs.WriteObject(stream, p);
            }, TestRunner.CreatePersonWithoutAttributes(), "DataContractSerializer stream");

            TestRunner.RunTestSerialize((Person p, Stream stream) =>
            {
                jsondcs.WriteObject(stream, p);
            }, TestRunner.CreatePerson(), "DataContractJsonSerializer stream");

            TestRunner.RunTestSerialize((Person p, Stream stream) =>
            {
                bf.Serialize(stream, p);
            }, TestRunner.CreatePerson(), "BinaryFormatter stream");

            TestRunner.RunTestDeserialize((json) =>
            {
                Jil.JSON.Deserialize<PersonWithoutAttributes>(json);
            }, "Jil");

            TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    Jil.JSON.Deserialize<PersonWithoutAttributes>(reader);
                }
            }, "Jil Stream");

            TestRunner.RunTestDeserialize((json) =>
            {
                NetJSON.NetJSON.Deserialize<PersonWithoutAttributes>(json);
            }, "NetJSON");

            TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    NetJSON.NetJSON.Deserialize<PersonWithoutAttributes>(reader);
                }
            }, "NetJSON Stream");

            TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    NewtonJson.Deserialize(reader, typeof(PersonWithoutAttributes));
                }
            }, "Newtonsoft.Json Stream");

            TestRunner.RunTestDeserialize((Stream stream) =>
            {
                ProtoBuf.Serializer.Deserialize<PersonProtobuf>(stream);
            }, "ProtoBuf stream", TestRunner.FormatType.ProtobufFormat);

            TestRunner.RunTestDeserialize((stream) =>
            {
                xmldcs.ReadObject(stream);
            }, "DataContractSerializer", TestRunner.FormatType.XmlFormat);

            TestRunner.RunTestDeserialize((stream) =>
            {
                jsondcs.ReadObject(stream);
            }, "DataContractJsonSerializer");

            TestRunner.RunTestDeserialize((stream) =>
            {
                bf.Deserialize(stream);
            }, "BinaryFormatter", TestRunner.FormatType.BinaryFormat);*/

            Console.ReadKey();
        }
    }
}

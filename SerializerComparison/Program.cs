using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace SerializerComparison
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var process = Process.GetCurrentProcess();
            process.ProcessorAffinity = (System.IntPtr)1;
            process.PriorityClass = ProcessPriorityClass.High;

            DataContractSerializer xmldcs = new DataContractSerializer(typeof(PersonWithoutAttributes));
            var settings = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ssK")
            };
            DataContractJsonSerializer jsondcs = new DataContractJsonSerializer(typeof(Person), settings);
            BinaryFormatter bf = new BinaryFormatter();
            Newtonsoft.Json.JsonSerializer NewtonJson = new Newtonsoft.Json.JsonSerializer();
            var msgpack = MsgPack.Serialization.SerializationContext.Default.GetSerializer<PersonWithoutAttributes>();
            var hyperion = new Hyperion.Serializer(new Hyperion.SerializerOptions(false, false));

            Jil.JSON.SetDefaultOptions(new Jil.Options(unspecifiedDateTimeKindBehavior: Jil.UnspecifiedDateTimeKindBehavior.IsUTC));

            logger.Info("Starting measurements");

            Thread.Sleep(500);

            var measurements = TestRunner.RunTestSerialize((p) =>
            {
                Jil.JSON.Serialize(p);
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "Jil Serialization");
            logger.Debug($"Jil Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((p) =>
            {
                Jil.JSON.Serialize(p, new Jil.Options(unspecifiedDateTimeKindBehavior: Jil.UnspecifiedDateTimeKindBehavior.IsUTC));
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "Jil With Options Serialization");
            logger.Debug($"Jil With Options Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    Jil.JSON.Serialize(p, writer);
                }
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "Jil Stream Serialization");
            logger.Debug($"Jil Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StringWriter())
                {
                    Jil.JSON.Serialize(p, writer);
                }
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "Jil StringWriter Serialization");
            logger.Debug($"Jil StringWriter Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((p) =>
            {
                NetJSON.NetJSON.Serialize(p);
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "NetJSON Serialization");
            logger.Debug($"NetJSON Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    NetJSON.NetJSON.Serialize(p, writer);
                }
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "NetJSON Stream Serialization");
            logger.Debug($"NetJSON Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 512, true))
                {
                    NewtonJson.Serialize(writer, p);
                }
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "Newtonsoft.Json Stream Serialization");
            logger.Debug($"Newtonsoft.Json Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonProtobuf p, Stream stream) =>
            {
                ProtoBuf.Serializer.Serialize(stream, p);
            }, TestRunner.CreatePersonProtobuf());

            TestRunner.PrintMeasurements(measurements, "ProtoBuf Stream Serialization");
            logger.Debug($"ProtoBuf Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                msgpack.Pack(stream, p);
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "MsgPack Stream Serialization");
            logger.Debug($"MsgPack Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((PersonWithoutAttributes p, Stream stream) =>
            {
                xmldcs.WriteObject(stream, p);
            }, TestRunner.CreatePersonWithoutAttributes());

            TestRunner.PrintMeasurements(measurements, "DataContractSerializer Stream Serialization");
            logger.Debug($"DataContractSerializer Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((Person p, Stream stream) =>
            {
                jsondcs.WriteObject(stream, p);
            }, TestRunner.CreatePerson());

            TestRunner.PrintMeasurements(measurements, "DataContractJsonSerializer Stream Serialization");
            logger.Debug($"DataContractJsonSerializer Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((Person p, Stream stream) =>
            {
                bf.Serialize(stream, p);
            }, TestRunner.CreatePerson());

            TestRunner.PrintMeasurements(measurements, "BinaryFormatter Stream Serialization");
            logger.Debug($"BinaryFormatter Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((p) =>
            {
                ZeroFormatter.ZeroFormatterSerializer.Serialize(p);
            }, TestRunner.CreatePersonZeroFormatter());

            TestRunner.PrintMeasurements(measurements, "ZeroFormatter Serialization");
            logger.Debug($"ZeroFormatter Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestSerialize((Person p, Stream stream) =>
            {
                hyperion.Serialize(p, stream);
            }, TestRunner.CreatePerson());

            TestRunner.PrintMeasurements(measurements, "Hyperion Stream Serialization");
            logger.Debug($"Hyperion Stream Serialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((json) =>
            {
                Jil.JSON.Deserialize<PersonWithoutAttributes>(json);
            });

            TestRunner.PrintMeasurements(measurements, "Jil Without Attributes Deserialization");
            logger.Debug($"Jil Without Attributes Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((json) =>
            {
                // Doesn't deserialize birthday and expirationdate, making it faster than it should be
                Jil.JSON.Deserialize<Person>(json);
            });

            TestRunner.PrintMeasurements(measurements, "Jil With Attributes Deserialization");
            logger.Debug($"Jil With Attributes Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    Jil.JSON.Deserialize<PersonWithoutAttributes>(reader);
                }
            });

            TestRunner.PrintMeasurements(measurements, "Jil Stream Deserialization");
            logger.Debug($"Jil Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            Type personType = typeof(PersonWithoutAttributes);
            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    Jil.JSON.Deserialize(reader, personType);
                }
            });

            TestRunner.PrintMeasurements(measurements, "Jil Stream Type Without Attributes Deserialization");
            logger.Debug($"Jil Stream Type Without Attributes Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            personType = typeof(Person);
            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    // Doesn't deserialize birthday and expirationdate, making it faster than it should be
                    Jil.JSON.Deserialize(reader, personType);
                }
            });

            TestRunner.PrintMeasurements(measurements, "Jil Stream Type With Attributes Deserialization");
            logger.Debug($"Jil Stream Type With Attributes Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((json) =>
            {
                // Doesn't deserialize datetimes well
                NetJSON.NetJSON.Deserialize<PersonWithoutAttributes>(json);
            });

            TestRunner.PrintMeasurements(measurements, "NetJSON Deserialization");
            logger.Debug($"NetJSON Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    // Doesn't deserialize datetimes well
                    NetJSON.NetJSON.Deserialize<PersonWithoutAttributes>(reader);
                }
            });

            TestRunner.PrintMeasurements(measurements, "NetJSON Stream Deserialization");
            logger.Debug($"NetJSON Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    NewtonJson.Deserialize(reader, typeof(PersonWithoutAttributes));
                }
            });

            TestRunner.PrintMeasurements(measurements, "Newtonsoft.Json Stream Deserialization");
            logger.Debug($"Newtonsoft.Json Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((Stream stream) =>
            {
                ProtoBuf.Serializer.Deserialize<PersonProtobuf>(stream);
            }, TestRunner.FormatType.ProtobufFormat);

            TestRunner.PrintMeasurements(measurements, "ProtoBuf Stream Deserialization");
            logger.Debug($"ProtoBuf Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 512, true))
                {
                    msgpack.Unpack(stream);
                }
            }, TestRunner.FormatType.MsgPackFormat);

            TestRunner.PrintMeasurements(measurements, "MsgPack Stream Deserialization");
            logger.Debug($"MsgPack Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                xmldcs.ReadObject(stream);
            }, TestRunner.FormatType.XmlFormat);

            TestRunner.PrintMeasurements(measurements, "DataContractSerializer Stream Deserialization");
            logger.Debug($"DataContractSerializer Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                // Doesn't deserialize birthday and expirationdate, making it faster than it should be
                jsondcs.ReadObject(stream);
            });

            TestRunner.PrintMeasurements(measurements, "DataContractJsonSerializer Stream Deserialization");
            logger.Debug($"DataContractJsonSerializer Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((stream) =>
            {
                bf.Deserialize(stream);
            }, TestRunner.FormatType.BinaryFormat);

            TestRunner.PrintMeasurements(measurements, "BinaryFormatter Stream Deserialization");
            logger.Debug($"BinaryFormatter Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((byte[] bytes) =>
            {
                ZeroFormatter.ZeroFormatterSerializer.Deserialize<PersonZeroFormatter>(bytes);
            }, TestRunner.FormatType.ZeroFormatterFormat);

            TestRunner.PrintMeasurements(measurements, "ZeroFormatter Deserialization");
            logger.Debug($"ZeroFormatter Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((Stream stream) =>
            {
                ZeroFormatter.ZeroFormatterSerializer.Deserialize<PersonZeroFormatter>(stream);
            }, TestRunner.FormatType.ZeroFormatterFormat);

            TestRunner.PrintMeasurements(measurements, "ZeroFormatter Stream Deserialization");
            logger.Debug($"ZeroFormatter Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            measurements = TestRunner.RunTestDeserialize((Stream stream) =>
            {
                hyperion.Deserialize<Person>(stream);
            }, TestRunner.FormatType.HyperionFormat);

            TestRunner.PrintMeasurements(measurements, "Hyperion Stream Deserialization");
            logger.Debug($"Hyperion Stream Deserialization: {string.Join(":", measurements.Select(m => TestRunner.ConvertToMicroSeconds(m)))}");

            logger.Info("Stopping measurements");
            Console.ReadKey();
        }
    }
}

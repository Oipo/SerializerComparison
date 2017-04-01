using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

namespace SerializerComparison
{
    public static class TestRunner
    {
        public enum FormatType
        {
            JsonFormat,
            XmlFormat,
            BinaryFormat,
            ProtobufFormat,
            MsgPackFormat,
            ZeroFormatterFormat,
            HyperionFormat
        }

        const long reserveGcMem = 1024 * 1024 * 1;
        const int repititions = 250;
        const int documentsCount = 1000;

        public static double ConvertToMicroSeconds(long ticks)
        {
            return ConvertToMicroSeconds((double)ticks);
        }

        public static double ConvertToMicroSeconds(double ticks)
        {
            return ticks / Stopwatch.Frequency * 1_000_000;
        }

        public static void PrintMeasurements(List<long> measurements, string name)
        {
            long min = measurements.Min();
            var max = measurements.Max();
            var avg = measurements.Average();

            Console.WriteLine($"{name} - min {ConvertToMicroSeconds(min):0.##} µs - max {ConvertToMicroSeconds(max):0.##} µs- avg {ConvertToMicroSeconds(avg):0.##} µs");
        }

        public static long Run(Action action)
        {
            if (!GC.TryStartNoGCRegion(reserveGcMem))
            {
                Console.WriteLine("WARNING -- GC running, results probably off");
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            action();

            sw.Stop();
            
            GC.EndNoGCRegion();
            GC.Collect();

            return sw.ElapsedTicks;
        }

        public static List<long> RunTestSerialize<T>(Action<T> serializeAction, T person)
            where T : IPerson
        {
            List<long> measurements = new List<long>
            {
                Capacity = repititions
            };

            // hot run / prime the pump / warmup
            serializeAction(person);

            for (int i = 0; i < repititions; i++)
            {
                measurements.Add(Run(() =>
                {
                    serializeAction(person);
                }));
            }

            return measurements;
        }

        public static List<long> RunTestSerialize<T>(Action<T, Stream> serializeAction, T person)
            where T : IPerson
        {
            List<long> measurements = new List<long>
            {
                Capacity = repititions
            };

            using (var stream = new MemoryStream())
            {
                // hot run / prime the pump / warmup
                serializeAction(person, stream);
                stream.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < repititions; i++)
                {
                    measurements.Add(Run(() =>
                    {
                        serializeAction(person, stream);
                    }));
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            return measurements;
        }

        public static List<long> RunTestDeserialize(Action<string> deserializeAction, FormatType type = FormatType.JsonFormat)
        {
            string input;
            if (type == FormatType.JsonFormat)
            {
                input = File.ReadAllText("PersonJson.txt");
            }
            else if (type == FormatType.XmlFormat)
            {
                input = File.ReadAllText("PersonXml.txt");
            }
            else
            {
                throw new Exception();
            }

            List<long> measurements = new List<long>
            {
                Capacity = repititions
            };

            // hot run / prime the pump / warmup
            deserializeAction(input);

            for (int i = 0; i < repititions; i++)
            {
                measurements.Add(Run(() =>
                {
                    deserializeAction(input);
                }));
            }

            return measurements;
        }

        public static List<long> RunTestDeserialize(Action<byte[]> deserializeAction, FormatType type = FormatType.ZeroFormatterFormat)
        {
            byte[] input;
            if (type != FormatType.ZeroFormatterFormat)
            {
                throw new Exception();
            }
            input = File.ReadAllBytes("PersonZeroFormatter.txt");

            List<long> measurements = new List<long>
            {
                Capacity = repititions
            };

            // hot run / prime the pump / warmup
            deserializeAction(input);

            for (int i = 0; i < repititions; i++)
            {
                measurements.Add(Run(() =>
                {
                    deserializeAction(input);
                }));
            }

            return measurements;
        }

        public static List<long> RunTestDeserialize(Action<Stream> deserializeAction, FormatType type = FormatType.JsonFormat)
        {
            string input;
            byte[] binaryInput;
            if (type == FormatType.JsonFormat)
            {
                input = File.ReadAllText("PersonJson.txt");
                binaryInput = new byte[0];
            }
            else if (type == FormatType.XmlFormat)
            {
                input = File.ReadAllText("PersonXml.txt");
                binaryInput = new byte[0];
            }
            else if (type == FormatType.ProtobufFormat)
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonProtobuf.txt");
            }
            else if(type == FormatType.MsgPackFormat)
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonMsgPack.txt");
            }
            else if(type == FormatType.ZeroFormatterFormat)
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonZeroFormatter.txt");
            }
            else if(type == FormatType.HyperionFormat)
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonHyperion.txt");
            }
            else
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonBinary.txt");
            }

            List<long> measurements = new List<long>
            {
                Capacity = repititions
            };

            using (var stream = new MemoryStream())
            {
                if (type == FormatType.BinaryFormat || type == FormatType.ProtobufFormat || type == FormatType.MsgPackFormat ||
                    type == FormatType.ZeroFormatterFormat || type == FormatType.HyperionFormat)
                {
                    using (var writer = new BinaryWriter(stream, new UTF8Encoding(false), true))
                    {
                        writer.Write(binaryInput);
                    }
                }
                else
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 512, true))
                    {
                        writer.Write(input);
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);

                // hot run / prime the pump / warmup
                deserializeAction(stream);
                stream.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < repititions; i++)
                {
                    measurements.Add(Run(() =>
                    {
                            deserializeAction(stream);
                    }));

                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            return measurements;
        }

        public static PersonProtobuf CreatePersonProtobuf()
        {
            var documents = new List<DocumentProtobuf>
            {
                Capacity = documentsCount
            };

            for (int i = 0; i < documentsCount; i++)
            {
                documents.Add(new DocumentProtobuf
                {
                    Id = i,
                    Name = $"License{i}",
                    Content = "abcdefghijklmnopqrstuvwxyzüäçéèß",
                    ExpirationDate = DateTime.UtcNow.AddDays(i)

                });
            }

            return new PersonProtobuf
            {
                Age = 123,
                Birthday = DateTime.UtcNow,
                Name = "John Doe",
                Documents = documents
            };
        }

        public static PersonZeroFormatter CreatePersonZeroFormatter()
        {
            var documents = new List<DocumentZeroFormatter>
            {
                Capacity = documentsCount
            };

            for (int i = 0; i < documentsCount; i++)
            {
                documents.Add(new DocumentZeroFormatter
                {
                    Id = i,
                    Name = $"License{i}",
                    Content = "abcdefghijklmnopqrstuvwxyzüäçéèß",
                    ExpirationDate = DateTime.UtcNow.AddDays(i)

                });
            }

            return new PersonZeroFormatter
            {
                Age = 123,
                Birthday = DateTime.UtcNow,
                Name = "John Doe",
                Documents = documents
            };
        }

        public static PersonWithoutAttributes CreatePersonWithoutAttributes()
        {
            var documents = new List<DocumentWithoutAttributes>
            {
                Capacity = documentsCount
            };

            for (int i = 0; i < documentsCount; i++)
            {
                documents.Add(new DocumentWithoutAttributes
                {
                    Id = i,
                    Name = $"License{i}",
                    Content = "abcdefghijklmnopqrstuvwxyzüäçéèß",
                    ExpirationDate = DateTime.UtcNow.AddDays(i)

                });
            }

            return new PersonWithoutAttributes
            {
                Age = 123,
                Birthday = DateTime.UtcNow,
                Name = "John Doe",
                Documents = documents
            };
        }

        public static Person CreatePerson()
        {
            var documents = new List<Document>
            {
                Capacity = documentsCount
            };

            for (int i = 0; i < documentsCount; i++)
            {
                documents.Add(new Document
                {
                    Id = i,
                    Name = $"License{i}",
                    Content = "abcdefghijklmnopqrstuvwxyzüäçéèß",
                    ExpirationDate = DateTime.UtcNow.AddDays(i)

                });
            }

            return new Person
            {
                Age = 123,
                Birthday = DateTime.UtcNow,
                Name = "John Doe",
                Documents = documents
            };
        }
    }
}

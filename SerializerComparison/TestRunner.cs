using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace SerializerComparison
{
    public static class TestRunner
    {
        public enum FormatType
        {
            JsonFormat,
            XmlFormat,
            BinaryFormat,
            ProtobufFormat
        }

        const long reserveGcMem = 1024 * 1024;
        const int repititions = 1_000;

        public static long Run(Action action, string name)
        {
            if (!GC.TryStartNoGCRegion(reserveGcMem))
            {
                Console.WriteLine("WARNING -- GC running, results probably off");
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            action();

            sw.Stop();

            try
            {
                GC.EndNoGCRegion();
            }
            catch
            {
                // do nothing
            }

            //Console.WriteLine($"{name} took {sw.ElapsedMilliseconds} ms");
            GC.Collect();
            GC.WaitForFullGCComplete();

            return sw.ElapsedMilliseconds;
        }

        public static List<long> RunTestSerialize<T>(Action<T> serializeAction, T person)
            where T : IPerson
        {
            List<long> measurements = new List<long>();

            for (int i = 0; i < repititions; i++)
            {
                measurements.Add(Run(() =>
                {
                        serializeAction(person);
                }, $"{name} Serialization"));
            }

            return measurements;
        }

        public static void RunTestSerialize<T>(Action<T, Stream> serializeAction, T person, string name)
            where T : IPerson
        {
            using (var stream = new MemoryStream())
            {
                Run(() =>
                {
                    for (int i = 0; i < repititions; i++)
                    {
                        serializeAction(person, stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                }, $"{name} Cold Serialization");

                Run(() =>
                {
                    for (int i = 0; i < repititions; i++)
                    {
                        serializeAction(person, stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                }, $"{name} Hot Serialization");
            }
        }

        public static void RunTestDeserialize(Action<string> deserializeAction, string name, FormatType type = FormatType.JsonFormat)
        {
            string input;
            if (type == FormatType.JsonFormat)
            {
                input = File.ReadAllText("PersonJson.txt");
            }
            else if(type == FormatType.XmlFormat)
            {
                input = File.ReadAllText("PersonXml.txt");
            } else
            {
                throw new Exception();
            }

            Run(() =>
            {
                for (int i = 0; i < repititions; i++)
                {
                    deserializeAction(input);
                }
            }, $"{name} Cold Deserialization");

            Run(() =>
            {
                for (int i = 0; i < repititions; i++)
                {
                    deserializeAction(input);
                }
            }, $"{name} Hot Deserialization");
        }

        public static void RunTestDeserialize(Action<Stream> deserializeAction, string name, FormatType type = FormatType.JsonFormat)
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
            else if(type == FormatType.ProtobufFormat)
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonProtobuf.txt");
            }
            else
            {
                input = "";
                binaryInput = File.ReadAllBytes("PersonBinary.txt");
            }
            using (var stream = new MemoryStream())
            {
                if (type == FormatType.BinaryFormat || type == FormatType.ProtobufFormat)
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

                Run(() =>
                {
                    for (int i = 0; i < repititions; i++)
                    {
                        deserializeAction(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                }, $"{name} Cold Deserialization");

                Run(() =>
                {
                    for (int i = 0; i < repititions; i++)
                    {
                        deserializeAction(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                }, $"{name} Hot Deserialization");
            }
        }

        public static PersonProtobuf CreatePersonProtobuf()
        {
            var documents = new List<DocumentProtobuf>();

            for (int i = 0; i < repititions; i++)
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

        public static PersonWithoutAttributes CreatePersonWithoutAttributes()
        {
            var documents = new List<DocumentWithoutAttributes>();

            for (int i = 0; i < repititions; i++)
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
            var documents = new List<Document>();

            for (int i = 0; i < repititions; i++)
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

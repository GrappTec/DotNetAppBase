#region License

// Copyright(c) 2020 GrappTec
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace DotNetAppBase.Std.Library
{
    public partial class XHelper
    {
        public static class Serializers
        {
            public static class Xml
            {
                public static T Deserialize<T>(string data, params Type[] extraTypes)
                {
                    var serializer = new XmlSerializer(typeof(T), extraTypes);
                    using var reader = new StringReader(data);
                    return (T) serializer.Deserialize(reader);
                }

                public static string Serialize(object obj, params Type[] extraTypes)
                {
                    var serializer = new XmlSerializer(obj.GetType(), extraTypes);
                    using var writer = new StringWriter();
                    serializer.Serialize(writer, obj);

                    return writer.ToString();
                }

                public static void Serialize(object obj, string fileName)
                {
                    var serializer = new XmlSerializer(obj.GetType());
                    using var sw = File.CreateText(fileName);
                    serializer.Serialize(sw, obj);
                    sw.Flush();
                }

                public static string SerializeDataContract(object obj)
                {
                    var serializer = new DataContractSerializer(obj.GetType());
                    using var sw = new StringWriter();
                    using var writer = XmlWriter.Create(sw);
                    serializer.WriteObject(writer, obj);

                    return sw.ToString();
                }

                public static XmlWriter SerializeToXmlWriter(object obj, params Type[] extraTypes)
                {
                    var serializer = new XmlSerializer(obj.GetType(), extraTypes);
                    var writer = XmlWriter.Create(new MemoryStream());
                    serializer.Serialize(writer, obj);

                    return writer;
                }
            }

            public static class DataContract
            {
                public static T Deserialize<T>(XmlReader reader, params Type[] knowTypes)
                {
                    var serializer = new DataContractSerializer(typeof(T), knowTypes);
                    var obj = (T) serializer.ReadObject(reader);
                    return obj;
                }

                public static T Deserialize<T>(string data, params Type[] knowTypes) where T : class
                {
                    if (string.IsNullOrEmpty(data))
                    {
                        return null;
                    }

                    using var reader = XmlReader.Create(new StringReader(data));
                    var obj = Deserialize<T>(reader, knowTypes);
                    return obj;
                }

                public static void Serialize<T>(T obj, XmlWriter writter, params Type[] knowTypes)
                {
                    var serializer = new DataContractSerializer(typeof(T), knowTypes);
                    serializer.WriteObject(writter, obj);
                }

                public static string Serialize<T>(T obj, params Type[] knowTypes)
                {
                    using var textWriter = new StringWriter();
                    using var writter = XmlWriter.Create(textWriter, new XmlWriterSettings
                        {
                            Encoding = Encoding.Unicode,
                            Indent = true,
                            IndentChars = "\t"
                        });
                    Serialize(obj, writter, knowTypes);
                    writter.Flush();
                    writter.Close();
                    return textWriter.ToString();
                }
            }

            public static class Bin
            {
                public static T Deserialize<T>(byte[] data)
                {
                    using var mStream = new MemoryStream(data);
                    var formatter = new BinaryFormatter();
                    var obj = formatter.Deserialize(mStream);

                    return (T) obj;
                }

                public static byte[] Serialize(object obj)
                {
                    byte[] data;
                    using (var mStream = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(mStream, obj);
                        data = mStream.GetBuffer();
                    }

                    return data;
                }
            }

            public static class Json
            {
                private static readonly Lazy<JsonSerializerSettings> LazyDefaultSettings;

                static Json()
                {
                    LazyDefaultSettings = new Lazy<JsonSerializerSettings>(
                        () => new JsonSerializerSettings
                            {
                                ContractResolver = new SerializableContractResolver
                                    {
                                        IgnoreSerializableAttribute = true
                                    }
                            });
                }

                public static JsonSerializerSettings DefaultSettings => LazyDefaultSettings.Value;

                public static T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, DefaultSettings);

                public static string Serialize(object obj, Formatting formatting = Formatting.Indented) => JsonConvert.SerializeObject(obj, formatting, DefaultSettings);

                private class SerializableContractResolver : DefaultContractResolver
                {
                    protected override JsonContract CreateContract(Type objectType)
                    {
                        var contract = base.CreateContract(objectType);

                        if (contract is JsonStringContract)
                        {
                            return CreateObjectContract(objectType);
                        }

                        return contract;
                    }

                    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
                    {
                        var properties = base.CreateProperties(type, memberSerialization);

                        foreach (var p in properties)
                        {
                            p.Ignored = false;
                        }

                        return properties;
                    }
                }
            }
        }
    }
}
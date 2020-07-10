﻿using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using WeaverBuildTools.Enums;
using WeaverCore.Utilities;

namespace WeaverBuildTools.Commands
{
	public static class EmbedResourceCMD
	{
		public static void EmbedResource(string assemblyToEmbedTo, string fileToEmbed,string resourceName, string hash = null,CompressionMethod compression = CompressionMethod.Auto)
		{
			using (var additionStream = File.OpenRead(fileToEmbed))
			{
				EmbedResource(assemblyToEmbedTo, resourceName, additionStream, additionStream.GetHash(), compression);
			}
		}

		public static void EmbedResource(string assemblyToEmbedTo, string resourceName, Stream streamToEmbed, string hash = null, CompressionMethod compression = CompressionMethod.Auto)
		{
			if (hash == null)
			{
				hash = streamToEmbed.GetHash();
			}
			double previousTime = GetTime();
			while (GetTime() - previousTime <= 10.0)
			{
				try
				{
					using (var definition = AssemblyDefinition.ReadAssembly(assemblyToEmbedTo, new ReaderParameters { ReadWrite = true,AssemblyResolver = new MainResolver() }))
					{
						var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
						if (finding != null)
						{
							var metaResource = definition.MainModule.Resources.FirstOrDefault(r => r.Name == (resourceName + "_meta"));
							if (metaResource != null && metaResource is EmbeddedResource)
							{
								var embeddedHash = (EmbeddedResource)metaResource;
								using (var metaStream = embeddedHash.GetResourceStream())
								{
									var meta = ResourceMetaData.FromStream(metaStream);
									if (meta.hash == hash)
									{
										return;
									}
								}
							}
							definition.MainModule.Resources.Remove(finding);
							if (metaResource != null)
							{
								definition.MainModule.Resources.Remove(metaResource);
							}
						}
						if (compression == CompressionMethod.Auto || compression == CompressionMethod.UseCompression)
						{
							using (var compressedStream = new MemoryStream())
							{
								//Console.WriteLine("E");
								using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
								{
									streamToEmbed.CopyToStream(compressionStream);
									compressedStream.Position = 0;

									Stream smallestStream = compressedStream;
									bool actuallyCompressed = true;
									if (streamToEmbed.Length < compressedStream.Length && compression == CompressionMethod.Auto)
									{
										smallestStream = streamToEmbed;
										actuallyCompressed = false;
									}

									var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, smallestStream);
									definition.MainModule.Resources.Add(er);

									using (var metaStream = new ResourceMetaData(actuallyCompressed, hash).ToStream())
									{
										var hashResource = new EmbeddedResource(resourceName + "_meta", ManifestResourceAttributes.Public, metaStream);
										definition.MainModule.Resources.Add(hashResource);
										definition.MainModule.Write();
									}
								}
							}
						}
						else
						{
							var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, streamToEmbed);
							definition.MainModule.Resources.Add(er);
							using (var metaStream = new ResourceMetaData(false, hash).ToStream())
							{
								var hashResource = new EmbeddedResource(resourceName + "_meta", ManifestResourceAttributes.Public, metaStream);
								definition.MainModule.Resources.Add(hashResource);
								definition.MainModule.Write();
							}
						}
					}
					break;
				}
				catch (Exception e)
				{
					if (e.Message.Contains("because it is being used by another process"))
					{
						continue;
					}
					else
					{
						throw;
					}
				}
			}
			if (GetTime() - previousTime > 10.0)
			{
				throw new Exception("Embedding Timeout");
			}
		}

		static double GetTime()
		{
			return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
		}

		public static string GetHelp()
		{
			return	"-----embedresource-----\n" +
					"Embeds a resource into an assembly\n" +
					"\n" +
					"embedresource {assemblyToEmbedTo} {pathToFileToEmbed} {resourceName} [useCompression] [hash]\n\n" +
					"NOTE: If no parameter for [useCompression] is specified, then it will automatically apply compression if it results in a smaller file size. Enter \"true\" or \"false\" to enter manually\n\n" +
					"NOTE: If no hash is specified, then it will calculate the hash based on the file provided in {pathToFileToEmbed}\n\n" +
					"---------------------\n\n";
		}
	}
}

﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// TEMPORARY FILE USED TO SOLVE BOOTSTRAPING ISSUES. WILL BE REMOVED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml.Linq;
using NuGet;
using Microsoft.Framework.Runtime.Compilation;

namespace Microsoft.Framework.Runtime
{
    public class OldEmbeddedResourceProvider : IResourceProvider
    {
        public IList<ResourceDescriptor> GetResources(ICompilationProject project)
        {
            string root = PathUtility.EnsureTrailingSlash(project.ProjectDirectory);

            // Resources have the relative path from the project root
            // and are separated by /. It's always / regardless of the
            // platform.

            return project.Files.ResourceFiles.Select(resourceFile => new ResourceDescriptor()
            {
                Name = PathUtility.GetRelativePath(root, resourceFile)
                           .Replace(Path.DirectorySeparatorChar, '/'),
                StreamFactory = () => new FileStream(resourceFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            })
            .ToList();

        }
    }

    public class OldResxResourceProvider : IResourceProvider
    {
        public IList<ResourceDescriptor> GetResources(ICompilationProject project)
        {
            return Directory.EnumerateFiles(project.ProjectDirectory, "*.resx", SearchOption.AllDirectories)
                            .Select(resxFilePath =>
                                new ResourceDescriptor()
                                {
                                    Name = GetResourceName(project.Name, resxFilePath),
                                    StreamFactory = () => GetResourceStream(resxFilePath)
                                }).ToList();
        }

        private static string GetResourceName(string projectName, string resxFilePath)
        {
            Logger.TraceInformation("[{0}]: Found resource {1}", typeof(ResxResourceProvider).Name, resxFilePath);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resxFilePath);


            if (fileNameWithoutExtension.StartsWith(projectName, StringComparison.OrdinalIgnoreCase))
            {
                return fileNameWithoutExtension + ".resources";
            }

            return projectName + "." + fileNameWithoutExtension + ".resources";
        }

        private static Stream GetResourceStream(string resxFilePath)
        {
            using (var fs = File.OpenRead(resxFilePath))
            {
                var document = XDocument.Load(fs);

                var ms = new MemoryStream();
                var rw = new ResourceWriter(ms);

                foreach (var e in document.Root.Elements("data"))
                {
                    string name = e.Attribute("name").Value;
                    string value = e.Element("value").Value;

                    rw.AddResource(name, value);
                }

                rw.Generate();
                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
        }
    }
}


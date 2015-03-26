﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.Runtime.Compilation;

namespace Microsoft.Framework.Runtime
{
    public class DesignTimeHostProjectCompiler : IProjectCompiler
    {
        private readonly IDesignTimeHostCompiler _compiler;

        public DesignTimeHostProjectCompiler(IDesignTimeHostCompiler compiler)
        {
            _compiler = compiler;
        }

        public IMetadataProjectReference CompileProject(
            ICompilationProject project,
            ILibraryKey target,
            Func<ILibraryExport> referenceResolver,
            Func<IList<ResourceDescriptor>> resourcesResolver)
        {
            // The target framework and configuration are assumed to be correct
            // in the design time process
            var task = _compiler.Compile(project.ProjectDirectory, target);

            return new DesignTimeProjectReference(project, task.Result);
        }
    }
}

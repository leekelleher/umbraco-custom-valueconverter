/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;

#if NET472
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Serialization;
#endif

namespace Umbraco.Community.CustomValueConverter.DataEditor
{
    internal sealed class CustomValueConverterConfigurationConnector : IDataTypeConfigurationConnector
    {
        public IEnumerable<string> PropertyEditorAliases => new[] { CustomValueConverterDataEditor.DataEditorName };

        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public CustomValueConverterConfigurationConnector(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        {
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        }

        public object FromArtifact(IDataType dataType, string configuration)
        {
            var dataTypeConfigurationEditor = dataType.Editor.GetConfigurationEditor();
            return dataTypeConfigurationEditor.FromDatabase(configuration, _configurationEditorJsonSerializer);
        }

        public string ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies)
        {
            if (dataType.Configuration is Dictionary<string, object> config &&
                config.TryGetValueAs(CustomValueConverterConfigurationEditor.DataType, out GuidUdi udi) == true)
            {
                dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Match));
            }

#if NET472
            return ConfigurationEditor.ToDatabase(dataType.Configuration);
#else
            return ConfigurationEditor.ToDatabase(dataType.Configuration, _configurationEditorJsonSerializer);
#endif
        }
    }
}

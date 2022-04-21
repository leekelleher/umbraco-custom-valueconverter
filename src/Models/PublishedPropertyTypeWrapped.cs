/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
#if NET472
using Umbraco.Core.PropertyEditors;
#else
using Umbraco.Cms.Core.PropertyEditors;
#endif

#if NET472
namespace Umbraco.Core.Models.PublishedContent
#else
namespace Umbraco.Cms.Core.Models.PublishedContent
#endif
{
    internal class PublishedPropertyTypeWrapped : IPublishedPropertyType
    {
        private readonly IPublishedPropertyType _propertyType;
        private readonly PublishedDataType _dataType;

        public PublishedPropertyTypeWrapped(IPublishedPropertyType propertyType, PublishedDataType dataType = null)
        {
            _propertyType = propertyType;
            _dataType = dataType;
        }

        public IPublishedContentType ContentType => _propertyType.ContentType;

        public PublishedDataType DataType => _dataType ?? _propertyType.DataType;

        public string Alias => _propertyType.Alias;

        public string EditorAlias => _dataType?.EditorAlias ?? _propertyType.EditorAlias;

        public bool IsUserProperty => _propertyType.IsUserProperty;

        public ContentVariation Variations => _propertyType.Variations;

        public PropertyCacheLevel CacheLevel => _propertyType.CacheLevel;

        public Type ModelClrType => _propertyType.ModelClrType;

        public Type ClrType => _propertyType.ClrType;

        public object ConvertInterToObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) => _propertyType.ConvertInterToObject(owner, referenceCacheLevel, inter, preview);

        public object ConvertInterToXPath(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) => _propertyType.ConvertInterToXPath(owner, referenceCacheLevel, inter, preview);

        public object ConvertSourceToInter(IPublishedElement owner, object source, bool preview) => _propertyType.ConvertSourceToInter(owner, source, preview);

        public bool? IsValue(object value, PropertyValueLevel level) => _propertyType.IsValue(value, level);
    }
}

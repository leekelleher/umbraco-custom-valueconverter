/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;

#if NET472
namespace Umbraco.Core
#else
namespace Umbraco.Extensions
#endif
{
    internal static class DictionaryExtensions
    {
        public static bool TryGetValueAs<TKey, TValue, TValueOut>(this IDictionary<TKey, TValue> config, TKey key, out TValueOut value)
        {
            if (config.TryGetValue(key, out var tmp1) == true)
            {
                if (tmp1 is TValueOut tmp2)
                {
                    value = tmp2;
                    return true;
                }

                var attempt = tmp1.TryConvertTo<TValueOut>();
                if (attempt.Success == true)
                {
                    value = attempt.Result;
                    return attempt.Success;
                }
            }

            value = default;
            return false;
        }
    }
}

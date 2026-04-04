using System;
using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    public class EntityWithProps
    {
        public Prop[] props;

        public string GetProp(string key, string fallback = null)
        {
            Prop prop = props.FirstOrDefault((prop) => prop.key == key && prop.value != null);
            return prop?.value ?? fallback;
        }

        public void SetProp(string key, string value)
        {
            if (GetProp(key) != null)
            {
                props = props.Select((prop) =>
                {
                    if (prop.key == key)
                    {
                        prop.value = value;
                    }

                    return prop;
                }).ToArray();
            }
            else
            {
                props = props.Append(new Prop((key, value))).ToArray();
            }
        }

        public void DeleteProp(string key)
        {
            var prop = props.FirstOrDefault((prop) => prop.key == key) ??
                throw new Exception($"Prop with key {key} does not exist");

            prop.value = null;
        }

        public IReadOnlyList<string> GetPropArray(string key)
        {
            var items = props
                .Where((prop) => prop.key == Prop.ToArrayKey(key) && prop.value != null)
                .Select((prop) => prop.value);

            return items.ToList().AsReadOnly();
        }

        private void EnsurePropArraySentinelRemoved(string key)
        {
            var arrayKey = Prop.ToArrayKey(key);

            var hasSentinel = props.Any((prop) => prop.key == arrayKey && prop.value == null);
            if (hasSentinel)
            {
                props = props.Where((prop) => prop.key != arrayKey).ToArray();
            }
        }

        public void SetPropArray(string key, IEnumerable<string> values)
        {
            var validValues = values.Where((value) => !string.IsNullOrEmpty(value)).Distinct().ToList();

            if (validValues.Count == 0)
            {
                throw new Exception($"Values for prop array {key} must contain at least one non-empty value");
            }

            var arrayKey = Prop.ToArrayKey(key);
            props = props.Where((prop) => prop.key != arrayKey).ToArray();

            props = props.Concat(validValues.Select((value) => new Prop((arrayKey, value)))).ToArray();
        }

        public void DeletePropArray(string key)
        {
            var arrayKey = Prop.ToArrayKey(key);

            if (!props.Any((prop) => prop.key == arrayKey))
            {
                throw new Exception($"Prop array with key {key} does not exist");
            }

            props = props
                .Where((prop) => prop.key != arrayKey)
                // set a single value to null - this ensures the array is cleared
                .Append(new Prop((arrayKey, null)))
                .ToArray();
        }

        public void InsertIntoPropArray(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"Value for prop array {key} cannot be null or empty");
            }

            var arrayKey = Prop.ToArrayKey(key);

            var hasDupe = props.Any((prop) => prop.key == arrayKey && prop.value == value);
            if (!hasDupe)
            {
                EnsurePropArraySentinelRemoved(key);
                props = props.Append(new Prop((arrayKey, value))).ToArray();
            }
        }

        private void EnsurePropArrayHasSentinel(string key)
        {
            var hasItems = props
                .Where((prop) => prop.key == Prop.ToArrayKey(key))
                .Any();

            if (!hasItems)
            {
                props = props.Append(new Prop((Prop.ToArrayKey(key), null))).ToArray();
            }
        }

        public void RemoveFromPropArray(string key, string value)
        {
            var arrayKey = Prop.ToArrayKey(key);
            EnsurePropArraySentinelRemoved(key);

            if (!props.Any((prop) => prop.key == arrayKey && prop.value == value))
            {
                EnsurePropArrayHasSentinel(key);
                throw new Exception($"Value {value} does not exist in prop array {key}");
            }

            props = props.Where((prop) => {
                var isMatchingValue = prop.key == arrayKey && prop.value == value;
                return !isMatchingValue;
            }).ToArray();

            EnsurePropArrayHasSentinel(key);
        }
    }
}

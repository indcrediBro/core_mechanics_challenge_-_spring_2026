using System.Collections;
using System.Collections.Generic;

namespace IncredibleAttributes
{
    /// <summary>
    /// A named list of (display name → value) pairs used with [Dropdown].
    /// Works like a dictionary but preserves insertion order and allows
    /// duplicate display names.
    /// </summary>
    public class DropdownList<T> : IDropdownList, IEnumerable
    {
        private readonly List<string> _keys = new();
        private readonly List<T> _values = new();

        public void Add(string displayName, T value)
        {
            _keys.Add(displayName);
            _values.Add(value);
        }

        public string[] GetDisplayNames() => _keys.ToArray();
        public object[] GetValues()
        {
            var result = new object[_values.Count];
            for (int i = 0; i < _values.Count; i++)
                result[i] = _values[i];
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>Non-generic interface so the editor can read DropdownList without knowing T.</summary>
    public interface IDropdownList
    {
        string[] GetDisplayNames();
        object[] GetValues();
    }
}

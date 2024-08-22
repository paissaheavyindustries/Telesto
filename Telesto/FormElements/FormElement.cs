using System;
using System.Collections.Generic;

namespace Telesto.FormElements
{

    internal abstract class FormElement
    {

        internal Form Owner { get; set; }

        internal Guid Guid { get; set; } = Guid.Empty;
        public string Id { get; set; }
        internal abstract bool Reported { get; }
        public abstract string Value { get; set; }

        public FormElement()
        {
            Guid = Guid.NewGuid();
        }

        internal virtual void Initialize(Dictionary<string, object> d)
        {
            Id = d["id"].ToString();
            if (d.ContainsKey("value") == true)
            {
                Value = d["value"].ToString();
            }
        }

        public abstract void Render();

    }

}

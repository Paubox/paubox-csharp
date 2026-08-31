using System;
using System.Collections.Generic;

namespace Paubox
{
    public class TemplatedMessage : BaseMessage
    {
        public string TemplateName { get; set; }
        public Dictionary<string, object> TemplateValues { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(this.TemplateName)) {
                throw new ArgumentNullException("TemplateName cannot be null.");
            }
        }
    }
}

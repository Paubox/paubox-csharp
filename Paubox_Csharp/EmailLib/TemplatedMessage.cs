using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Paubox
{
    public class TemplatedMessage : BaseMessage
    {
        public string TemplateName { get; set; }
        public Dictionary<string, string> TemplateData { get; set; }
    }
}

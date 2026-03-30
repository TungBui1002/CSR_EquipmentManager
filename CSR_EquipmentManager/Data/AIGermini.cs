using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSR_EquipmentManager.Data
{
    public class AIGermini
    {
        public class GeminiResponse
        {
            public List<Candidate> candidates { get; set; }
        }
        public class Candidate
        {
            public Content content { get; set; }
        }
        public class Content
        {
            public List<Part> parts { get; set; }
        }
        public class Part
        {
            public string text { get; set; }
        }
    }
}
using Ignore;
namespace Martridge.Models.DmodPacker {
    public class DmodIgnoreRuleMetadata {
        public IgnoreRule IgnoreRule { get; set; } = new IgnoreRule("");
        public string RuleDefinition { get; set; } = string.Empty;
        public int RuleLineIndex { get; set; }
        public int RuleIndex { get; set; }

        public override string ToString() => this.RuleDefinition;
    }
}

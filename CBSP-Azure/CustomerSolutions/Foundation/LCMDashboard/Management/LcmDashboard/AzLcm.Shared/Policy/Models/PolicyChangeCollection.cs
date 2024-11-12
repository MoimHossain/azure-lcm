

using System.Text;

namespace AzLcm.Shared.Policy.Models
{
    public class PolicyChangeCollection
    {
        public PolicyChangeCollection()
        {
            Changes = [];
        }

        public List<PolicyChangeInfo> Changes { get; private set; }

        public void AddChange(string propertyName, object newValue, object oldValue)
        {
            Changes.Add(new PolicyChangeInfo(propertyName, newValue, oldValue));
        }

        public bool HasChanges
        {
            get
            {
                return Changes.Count > 0;
            }
        }

        public bool WasGA => Changes.Any(ch => "Preview".Equals(ch.PropertyName, StringComparison.OrdinalIgnoreCase));

        public bool HasDeprecated => Changes.Any(ch => "Deprecated".Equals(ch.PropertyName, StringComparison.OrdinalIgnoreCase));


        public VersionChange VersionChange => GetVersionChange();

        private static readonly char[] separator = ['.'];

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var change in Changes)
            {
                sb.AppendLine(change.ToString());
            }
            return sb.ToString();
        }

        private VersionChange GetVersionChange()
        {
            var kind = VersionChangeKind.None;
            var olderVersion = "-";
            var newVersion = "-";

            var versionProperty = Changes.FirstOrDefault(ch => "Version".Equals(ch.PropertyName, StringComparison.OrdinalIgnoreCase));
            if (versionProperty != null )
            {
                newVersion = $"{versionProperty.NewValue}";
                olderVersion = $"{versionProperty.OldValue}";
                if(!string.IsNullOrWhiteSpace(newVersion) && !string.IsNullOrWhiteSpace(olderVersion))
                {
                    if (newVersion.Equals(olderVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        kind = VersionChangeKind.None;
                    }
                    else 
                    {
                        var newSeq = newVersion.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        var oldSeq = olderVersion.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                        if(newSeq.Length > 1 && oldSeq.Length > 1)
                        {
                            if (!newSeq[0].Equals(oldSeq[0], StringComparison.OrdinalIgnoreCase)) 
                            {
                                kind = VersionChangeKind.Major;
                            }
                            else if (!newSeq[1].Equals(oldSeq[1], StringComparison.OrdinalIgnoreCase))
                            {
                                kind = VersionChangeKind.Minor;
                            }
                            else if (!newSeq[2].Equals(oldSeq[2], StringComparison.OrdinalIgnoreCase))
                            {
                                kind = VersionChangeKind.Patch;
                            }
                        }
                    }
                }
            }
            return new VersionChange(kind, newVersion, olderVersion);
        }
    }

    public enum VersionChangeKind
    {
        None = 0,
        Major = 1,
        Minor = 2,
        Patch = 3
    }

    public record VersionChange(VersionChangeKind VersionChangeKind, string NewVersion, string OldVersion);
}


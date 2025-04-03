using System.Collections.Generic;
using System.IO;
namespace Martridge.Models.DmodPacker
{
    public class DmodPackerDirectoryNode
    {
        public string NameLower { get; }
        public string Name => this.Info.Name;
        
        public DirectoryInfo Info { get; }

        public bool Ignore { get; set; } = false;
        
        public Dictionary<string, DmodPackerFileNode> Files { get; } = new Dictionary<string, DmodPackerFileNode>();
        public Dictionary<string, DmodPackerDirectoryNode> Directories { get; } = new Dictionary<string, DmodPackerDirectoryNode>();

        public DmodPackerDirectoryNode(DirectoryInfo dirInfo)
        {
            this.Info = dirInfo;
            this.NameLower = this.Name.ToLowerInvariant();
        }


        public bool AddChildNode(DmodPackerFileNode node)
        {
            if (this.Files.ContainsKey(node.NameLower)) return false;
            
            this.Files[node.NameLower] = node;

            return true;
        }
        
        public bool AddChildNode(DmodPackerDirectoryNode node)
        {
            if (this.Directories.ContainsKey(node.NameLower)) return false;
            
            this.Directories[node.NameLower] = node;

            return true;
        }
    }
    
    public class DmodPackerFileNode
    {
        public string NameLower { get; }
        public string Name => this.Info.Name;
        
        public FileInfo Info { get; }

        public bool Ignore { get; set; } = false;

        public DmodPackerFileNode(FileInfo fileInfo)
        {
            this.Info = fileInfo;
            this.NameLower = this.Name.ToLowerInvariant();
        }
    }
}

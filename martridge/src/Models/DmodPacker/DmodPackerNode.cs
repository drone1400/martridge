using System;
using System.Collections.Generic;
using System.IO;
namespace Martridge.Models.DmodPacker
{
    public class DmodPackerNode
    {
        public DmodNodeType NodeType { get; }
        public string Name { get; }
        public string FullPath { get; }
        public string RelativePath { get; }
        public string RelativePathLower { get; }
        public DateTime LastModified { get; }
        public bool IsIgnored => this._ignoreRuleMatchesList.Count > 0 && this._ignoreNegateRuleMatchesList.Count == 0;
        public bool IsUnignored => this._ignoreNegateRuleMatchesList.Count > 0;

        public IReadOnlyList<int> IgnoreNegateRuleMatchesList => this._ignoreNegateRuleMatchesList;
        private readonly List<int> _ignoreNegateRuleMatchesList = new List<int>();
        
        public IReadOnlyList<int> IgnoreRuleMatchesList => this._ignoreRuleMatchesList;
        private readonly List<int> _ignoreRuleMatchesList = new List<int>();

        public IReadOnlyDictionary<string, DmodPackerNode> ChildrenDictionary => this._childrenDictionary;
        private readonly Dictionary<string, DmodPackerNode> _childrenDictionary = new Dictionary<string, DmodPackerNode>();
        public IReadOnlyList<DmodPackerNode> ChildrenList => this._childrenList;
        private readonly List<DmodPackerNode> _childrenList = new List<DmodPackerNode>();

        public DmodPackerNode(DirectoryInfo dirInfo, DirectoryInfo baseDirectory)
        {
            this.NodeType = DmodNodeType.Directory;
            this.Name = dirInfo.Name;
            this.FullPath = dirInfo.FullName;
            
            string relativePath = Path.GetRelativePath(baseDirectory.FullName, dirInfo.FullName);
            // NOTE: we need to use '/' as a separator for the gitignore parser to work correctly...
            this.RelativePath = Path.DirectorySeparatorChar != '/' ? relativePath.Replace(Path.DirectorySeparatorChar, '/') : relativePath;
            this.RelativePathLower = this.RelativePath.ToLowerInvariant();
            this.LastModified = dirInfo.LastWriteTime;
        }
        
        public DmodPackerNode(FileInfo fileInfo, DirectoryInfo baseDirectory)
        {
            this.NodeType = GetNodeTypeFromFileExtension(fileInfo.Extension);
            this.Name = fileInfo.Name;
            this.FullPath = fileInfo.FullName;
            
            string relativePath = Path.GetRelativePath(baseDirectory.FullName, fileInfo.FullName);
            // NOTE: we need to use '/' as a separator for the gitignore parser to work correctly...
            this.RelativePath = Path.DirectorySeparatorChar != '/' ? relativePath.Replace(Path.DirectorySeparatorChar, '/') : relativePath;
            this.RelativePathLower = this.RelativePath.ToLowerInvariant();
            this.LastModified = fileInfo.LastWriteTime;
        }


        public bool AddChildNode(DmodPackerNode node) {
            if (this.NodeType != DmodNodeType.Directory)
                return false;
            
            if (!this._childrenDictionary.TryAdd(node.RelativePathLower, node)) return false;
            this._childrenList.Add(node);

            return true;
        }

        public void ResetIgnore() {
            this._ignoreNegateRuleMatchesList.Clear();
            this._ignoreRuleMatchesList.Clear();
        }

        public void Ignore(int ruleIndex) {
            this._ignoreRuleMatchesList.Add(ruleIndex);
        }

        public void NegateIgnore(int ruleIndex) {
            this._ignoreNegateRuleMatchesList.Add(ruleIndex);
        }
        
        public static DmodNodeType GetNodeTypeFromFileExtension(string ext) {
            string extLow = ext.ToLowerInvariant();
            return extLow switch  {
                // TEXT
                ".txt" => DmodNodeType.Text,
                // AUDIO
                ".mp3" => DmodNodeType.Audio,
                ".flac" => DmodNodeType.Audio,
                ".ogg" => DmodNodeType.Audio,
                ".wav" => DmodNodeType.Audio,
                ".mid" => DmodNodeType.Audio,
                // DINKC
                ".c" => DmodNodeType.DinkC,
                // DINKD
                ".d" => DmodNodeType.DinkD,
                // DATA
                ".dat" => DmodNodeType.Data,
                // IMAGE
                ".bmp" => DmodNodeType.Image,
                ".png" => DmodNodeType.Image,
                ".jpg" => DmodNodeType.Image,
                // DIRFF IMAGE COLLECTION [LEGACY]
                ".ff" => DmodNodeType.DirFF,
                // UNKNOWN
                _ => DmodNodeType.Other
            };
        }
    }
}

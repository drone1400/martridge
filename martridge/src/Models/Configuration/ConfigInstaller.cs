﻿using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public class ConfigInstaller {

        public string InstallerName { get; set; } = "";
        public string GameExeName { get; set; } = "";

        public List<ConfigInstallerComponent> InstallerComponents { get; set; } = new List<ConfigInstallerComponent>();

        public static ConfigInstaller GetDefaultInstaller_DinkHD_LatestRTSoft() {
            return new ConfigInstaller() {
                GameExeName = "dink.exe",
                InstallerName = "Dink HD - RTSoft",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "",
                            CheckSha256 = false,
                            Name = "dink_smallwood_hd.exe",
                            Uri = @"https://www.rtsoft.com/dink/DinkSmallwoodHDInstaller.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigInstaller GetDefaultInstaller_DinkHD_193() {
            return new ConfigInstaller() {
                GameExeName = "dink.exe",
                InstallerName = "Dink HD V1.9.3",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "fd451463caaf05c6f321850d6a1c3c2f3dfe2579152934fcce4b441c4ead1efb",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-v1_9_3.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-v1_9_3.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigInstaller GetDefaultInstaller_DinkHD_197() {
            return new ConfigInstaller() {
                GameExeName = "dink.exe",
                InstallerName = "Dink HD V1.97",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "ed3a2243df9dd9e89239206ada77decf2afa32dfb13515670e93099465d7a575",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-1_97.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-1_97.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigInstaller GetDefaultInstaller_DinkHD_198() {
            return new ConfigInstaller() {
                GameExeName = "dink.exe",
                InstallerName = "Dink HD V1.98",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "1e6a76557955cb90f92a4102caf1ee6a8e7cc8f1c0815056e3420ac0bda290d4",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-1_98.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-1_98.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }

        public static ConfigInstaller GetDefaultInstaller_Dink_108() {
            return new ConfigInstaller() {
                GameExeName = "dink.exe",
                InstallerName = "Dink V1.08",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }

        public static ConfigInstaller GetDefaultInstaller_FreeDinkWith108Data() {
            return new ConfigInstaller() {
                GameExeName = "freedink.exe",
                InstallerName = "FreeDink With V1.08 Data",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink",
                            @"island",
                        }
                    },
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "9f42039929251027412dbb5247c299de7c74dff5854f0dd03abc3c10bc3293c2",
                            CheckSha256 = true,
                            Name = "gnu_freedink-109_6.zip",
                            Uri = @"https://www.dinknetwork.com/download/gnu_freedink-109_6.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigInstaller GetDefaultInstaller_FreeDinkWith108DataAndLocalizations() {
            return new ConfigInstaller() {
                GameExeName = "freedink.exe",
                InstallerName = "FreeDink With V1.08 Data (And Localizations)",
                InstallerComponents = new List<ConfigInstallerComponent>() {
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink",
                            @"island",
                        },
                    },
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "9f42039929251027412dbb5247c299de7c74dff5854f0dd03abc3c10bc3293c2",
                            CheckSha256 = true,
                            Name = "gnu_freedink-109_6.zip",
                            Uri = @"https://www.dinknetwork.com/download/gnu_freedink-109_6.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                    new ConfigInstallerComponent() {
                        WebResource = new ConfigWebResource() {
                            Sha256 = "7cb32cae1d88fd15d2c441d6cbfe11e418bab6d66d84e2abe8a1b0c46bbd527a",
                            CheckSha256 = true,
                            Name = "freedink_data-1_08_20190120.zip",
                            Uri = @"https://www.dinknetwork.com/download/freedink_data-1_08_20190120.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink\l10n",
                            @"licenses",
                            @"freedink-data-COPYING.txt",
                            @"freedink-data-NEWS.txt",
                            @"freedink-data-README.txt",
                            @"freedink-data-README-REPLACEMENTS.txt",
                        },
                    },
                },
            };
        }

        public ConfigInstaller Clone() {
            ConfigInstaller cfg = new ConfigInstaller {
                GameExeName = this.GameExeName,
                InstallerName = this.InstallerName,
            };
            for (int i = 0; i < this.InstallerComponents.Count; i++) {
                ConfigInstallerComponent comp = this.InstallerComponents[i];
                cfg.InstallerComponents.Add(comp.Clone());
            }

            return cfg;
        }
    }
}
